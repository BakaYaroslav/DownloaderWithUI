using System.IO;
using System.Windows;

namespace Downloader
{
    public partial class UpdateWindow : Window
    {
        private readonly YtDlpUpdateChecker checker;
        private readonly string downloadUrl;
        private readonly string destinationPath;

        public UpdateWindow(string localVersion, string latestVersion, string downloadUrl, string destinationPath, YtDlpUpdateChecker checker)
        {
            InitializeComponent();

            this.checker = checker;
            this.downloadUrl = downloadUrl;
            this.destinationPath = destinationPath;

            MessageText.Text = "A new version of yt-dlp is available: " + latestVersion + "\nYou have: " + localVersion;
        }

        private void LaterButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateButton.IsEnabled = false;
            LaterButton.IsEnabled = false;
            DownloadProgressBar.Visibility = Visibility.Visible;

            var progress = new Progress<double>(percent =>
            {
                DownloadProgressBar.Value = percent;
            });

            string tempPath = destinationPath + ".new";

            await checker.DownloadFileAsync(downloadUrl, tempPath, progress);

            File.Delete(destinationPath);
            File.Move(tempPath, destinationPath);

            MessageText.Text = "Update complete!";
            DownloadProgressBar.Visibility = Visibility.Collapsed;
            LaterButton.Content = "Close";
            LaterButton.IsEnabled = true;
        }
    }
}