using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Downloader
{
    public partial class VideoEditorControl : UserControl
    {
        public event Action CloseRequested;
        private VideoInfo _currentVideo;

        private LibVLC _libVLC;
        private LibVLCSharp.Shared.MediaPlayer _mediaPlayer;

        private double _durationSeconds;
        private double _timelineWidth;
        private double _trimStartRatio = 0;
        private double _trimEndRatio = 1;
        private DispatcherTimer _playheadTimer;
        private string _onSaveLogin;
        private bool _isScrubbing = false;

        public VideoEditorControl()
        {
            InitializeComponent();

            Core.Initialize();
            _libVLC = new LibVLC();
            _mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            videoView.MediaPlayer = _mediaPlayer;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_mediaPlayer == null) return;
            _mediaPlayer.Volume = (int)e.NewValue;
        }

        public async void LoadVideo(VideoInfo info, VideoDownloader downloader, string login)
        {
            _currentVideo = info;
            LoadingText.Text = "Loading preview...";
            LoadingText.Visibility = Visibility.Visible;

            var (videoUrl, audioUrl) = await downloader.GetPreviewStreams(info.Url);

            if (videoUrl == null)
            {
                LoadingText.Text = "Failed to load preview";
                return;
            }

            var media = new Media(_libVLC, new Uri(videoUrl));
            if (audioUrl != null)
                media.AddSlave(MediaSlaveType.Audio, 0, audioUrl);

            _mediaPlayer.Play(media);
            LoadingText.Visibility = Visibility.Collapsed;

            _mediaPlayer.LengthChanged += (s, e) =>
            {
                _durationSeconds = e.Length / 1000.0;
                Dispatcher.Invoke(async () => await LoadThumbnails(videoUrl, downloader));
            };

            StartPlayheadTimer();
        }

        private async Task LoadThumbnails(string videoUrl, VideoDownloader downloader)
        {
            var files = await downloader.GenerateThumbnails(videoUrl, _durationSeconds, 12);

            var oldImages = TimelineCanvas.Children.OfType<Image>().ToList();
            foreach (var img in oldImages)
                TimelineCanvas.Children.Remove(img);

            _timelineWidth = TimelineCanvas.ActualWidth;
            double thumbWidth = _timelineWidth / files.Count;

            for (int i = 0; i < files.Count; i++)
            {
                var img = new Image
                {
                    Width = thumbWidth,
                    Height = 60,
                    Stretch = Stretch.UniformToFill,
                    Source = new BitmapImage(new Uri(files[i]))
                };
                Canvas.SetLeft(img, i * thumbWidth);
                Canvas.SetTop(img, 0);
                TimelineCanvas.Children.Insert(0, img);
            }

            UpdateHandlePositions();
        }

        private void TimelineCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _timelineWidth = TimelineCanvas.ActualWidth;
            UpdateHandlePositions();
        }

        private void UpdateHandlePositions()
        {
            if (_timelineWidth <= 0) return;

            double startX = _trimStartRatio * _timelineWidth;
            double endX = _trimEndRatio * _timelineWidth;

            Canvas.SetLeft(StartHandle, startX);
            Canvas.SetLeft(EndHandle, endX - EndHandle.Width);

            Canvas.SetLeft(TrimOverlayLeft, 0);
            TrimOverlayLeft.Width = Math.Max(0, startX);

            Canvas.SetLeft(TrimOverlayRight, endX);
            TrimOverlayRight.Width = Math.Max(0, _timelineWidth - endX);

            UpdateTrimLabel();
        }

        private void StartHandle_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double newX = Canvas.GetLeft(StartHandle) + e.HorizontalChange;
            double endX = _trimEndRatio * _timelineWidth;
            newX = Math.Max(0, Math.Min(newX, endX - 20));

            _trimStartRatio = newX / _timelineWidth;
            UpdateHandlePositions();
        }

        private void EndHandle_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double newX = Canvas.GetLeft(EndHandle) + EndHandle.Width + e.HorizontalChange;
            double startX = _trimStartRatio * _timelineWidth;
            newX = Math.Max(startX + 20, Math.Min(newX, _timelineWidth));

            _trimEndRatio = newX / _timelineWidth;
            UpdateHandlePositions();
        }

        private void UpdateTrimLabel()
        {
            var start = TimeSpan.FromSeconds(_trimStartRatio * _durationSeconds);
            var end = TimeSpan.FromSeconds(_trimEndRatio * _durationSeconds);
            TrimLabel.Text = $"{start:hh\\:mm\\:ss} — {end:hh\\:mm\\:ss}";

            if (_currentVideo != null)
            {
                _currentVideo.TrimStart = start;
                _currentVideo.TrimEnd = end;
            }
        }


        private void StartPlayheadTimer()
        {
            _playheadTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _playheadTimer.Tick += (s, e) =>
            {
                if (_durationSeconds <= 0 || _timelineWidth <= 0) return;
                double ratio = _mediaPlayer.Position;
                double x = ratio * _timelineWidth;
                Canvas.SetLeft(Playhead, x);
            };
            _playheadTimer.Start();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _playheadTimer?.Stop();
            _mediaPlayer.Stop();
            CloseRequested?.Invoke();
        }


        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            var pauseIcon = PlayPauseButton.Template.FindName("pauseIcon", PlayPauseButton) as System.Windows.Shapes.Path;
            var playIcon = PlayPauseButton.Template.FindName("playIcon", PlayPauseButton) as System.Windows.Shapes.Path;

            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Pause();
                pauseIcon.Visibility = Visibility.Collapsed;
                playIcon.Visibility = Visibility.Visible;
            }
            else
            {
                _mediaPlayer.Play();
                pauseIcon.Visibility = Visibility.Visible;
                playIcon.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentVideo == null) return;

          
            services.VideoService.UpdateAllInfo(_onSaveLogin, _currentVideo);

            _playheadTimer?.Stop();
            _mediaPlayer.Stop();
            CloseRequested?.Invoke();
        }



        private void TimelineCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
          
            if (e.OriginalSource is FrameworkElement fe && (fe.Name == "StartHandle" || fe.Name == "EndHandle"))
                return;

            _isScrubbing = true;
            SeekToMousePosition(e.GetPosition(TimelineCanvas).X);
            TimelineCanvas.CaptureMouse();
        }

        private void TimelineCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isScrubbing) return;
            SeekToMousePosition(e.GetPosition(TimelineCanvas).X);
        }

        private void TimelineCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isScrubbing = false;
            TimelineCanvas.ReleaseMouseCapture();
        }

        private void SeekToMousePosition(double x)
        {
            if (_timelineWidth <= 0) return;
            x = Math.Max(0, Math.Min(x, _timelineWidth));
            double ratio = x / _timelineWidth;
            _mediaPlayer.Position = (float)ratio;
            Canvas.SetLeft(Playhead, x);
        }
    }
}