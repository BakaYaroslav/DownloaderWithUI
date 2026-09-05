using System;
using System.Windows;
using System.Windows.Controls;

namespace Downloader
{
    public partial class VideoEditorControl : UserControl
    {
        public event Action CloseRequested;
        private VideoInfo _currentVideo;

        public VideoEditorControl()
        {
            InitializeComponent();
        }

        public async void LoadVideo(VideoInfo info, VideoDownloader downloader)
        {
            _currentVideo = info;
            LoadingText.Text = "Loading preview...";
            LoadingText.Visibility = Visibility.Visible;
            previewPlayer.Stop();

            System.Diagnostics.Debug.WriteLine("LoadVideo called with url: " + info?.Url);

            string streamUrl = await downloader.GetPreviewStreamUrl(info.Url);

            System.Diagnostics.Debug.WriteLine("Stream URL: " + (streamUrl ?? "NULL"));

            LoadingText.Visibility = Visibility.Collapsed;

            if (streamUrl == null)
            {
                LoadingText.Text = "Failed to load preview";
                LoadingText.Visibility = Visibility.Visible;
                return;
            }

            previewPlayer.Source = new Uri(streamUrl);
            previewPlayer.Play();
        }

        private void previewPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MediaElement FAILED: " + e.ErrorException?.Message);
            LoadingText.Text = "Playback error: " + e.ErrorException?.Message;
            LoadingText.Visibility = Visibility.Visible;
        }

        private void previewPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MediaElement opened OK, duration: " + previewPlayer.NaturalDuration);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("BackButton_Click FIRED");
            previewPlayer.Stop();
            previewPlayer.Source = null;
            CloseRequested?.Invoke();
        }
    }
}