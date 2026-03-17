using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Collections.ObjectModel;
namespace Downloader
{

    public partial class MainWindow : Window
    {
        VideoDownloader downloader = new VideoDownloader();
        ObservableCollection<VideoInfo> videos = new ObservableCollection<VideoInfo>();
        VideoInfo video = new VideoInfo();
        public MainWindow()
        {
            InitializeComponent();
            typeBox.ItemsSource = new List<string> { "Video", "Audio" };
            typeBox.SelectedIndex = 0;
            videoList.ItemsSource = videos;
        }
        private async void urlTable_TextChanged(object sender, TextChangedEventArgs e)
        {
            string url = urlTable.Text.Trim();

            if (!url.Contains("youtube.com/watch?v="))
            {
                urlLabel.Content = "Invalid URL or YOU!";
                urlLabel.Foreground = Brushes.IndianRed;
                return;

            }
            try
            {
                urlLabel.Content = "Wait...";
                urlLabel.Foreground = Brushes.DarkGray;
                var formats = await downloader.GetVideoFormats(url);
                qualityBox.ItemsSource = formats;
                qualityBox.SelectedIndex = 0;
                var info = await downloader.GetInfo(url);
                if (info == null) return;
                info.Url = url;
                videos.Add(info);
                urlLabel.Content = "Paste link here: ";
            }
            catch (Exception ex)
            {
                urlLabel.Content = "Invalid URL or YOU!";
                urlLabel.Foreground = Brushes.Red;


            }
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var info = button?.DataContext as VideoInfo;

            string downFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string url = info.Url;

            string quality = $"bestvideo[height<={qualityBox.SelectedItem}]+bestaudio/bestvideo+bestaudio/best";
            string type = typeBox.SelectedItem.ToString();

           

            var progress = new Progress<double>(value =>
            {
                if (value == 0) return;
                {
                    info.Progress = value;
                    info.Status = $"{value:F1}%";
                }
            }); 
            if (type == "Video")
            {
                bool success = await downloader.Download(url, downFolder, quality, progress);
                info.Status = success ? "Downloaded!" : "Failed";
            }
            else if (type == "Audio")
            {
                await downloader.DownloadAudio(url, downFolder, progress);
                info.Status = "Downloaded!";
            }

        }
    }
}