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
            LoadingText.Visibility = Visibility.Visible;
            previewPlayer.Stop();

            string streamUrl = await downloader.GetPreviewStreamUrl(info.Url);

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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            previewPlayer.Stop();
            previewPlayer.Source = null;
            CloseRequested?.Invoke();
        }
    }
}