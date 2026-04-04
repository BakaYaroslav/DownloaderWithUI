using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Collections.ObjectModel;
using Downloader.services;
namespace Downloader
{

    public partial class MainWindow : Window
    {
        VideoDownloader downloader = new VideoDownloader();
        ObservableCollection<VideoInfo> videos = new ObservableCollection<VideoInfo>();
        VideoInfo video = new VideoInfo();

        public string CurrentLogin { get; set; }

        public MainWindow(string login)
        {
            InitializeComponent();
            CurrentLogin = login;
            typeBox.ItemsSource = new List<string> { "Video", "Audio" };
            typeBox.SelectedIndex = 0;
            videoList.ItemsSource = videos;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        
            var saved = VideoService.LoadVideos(CurrentLogin);
            foreach (var v in saved)
                videos.Add(v);
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
             
                urlLabel.Content = "Paste link here: ";
               
                info.VideoId = url.Split("v=")[1].Split("&")[0];

                VideoService.SaveVideo(CurrentLogin, info);
                videos.Add(info);

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

            string quality = $"bestvideo[height<={qualityBox.SelectedItem}][ext=mp4]+bestaudio[ext=m4a]/bestvideo[height<={qualityBox.SelectedItem}]+bestaudio/best";
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

        private void RemoveCard_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag is VideoInfo item)
            {
                var list = videoList.ItemsSource as ObservableCollection<VideoInfo>;
                list?.Remove(item);
                VideoService.DeleteVideo(CurrentLogin, item);
            }
        }
    }
}