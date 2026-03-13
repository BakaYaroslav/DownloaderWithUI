using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Downloader
{
   
    public partial class MainWindow : Window
    {
        VideoDownloader downloader = new VideoDownloader();
        public MainWindow()
        {
            InitializeComponent();
            typeBox.ItemsSource = new List<string> { "Video", "Audio" };
            typeBox.SelectedIndex = 0;
        }
        private async void urlTable_TextChanged(object sender, TextChangedEventArgs e)
        {
            string url = urlTable.Text.Trim();

            if (url.Contains("youtube.com/watch"))
            {
                urlLabel.Content = "Paste link here:";
                urlLabel.Foreground = Brushes.DarkGray;
                var formats = await downloader.GetVideoFormats(url);
                qualityBox.ItemsSource = formats;
                qualityBox.SelectedIndex = 0;
                var info = await downloader.GetInfo(url);

                videoTitle.Text = info.Title;
                videoInfo.Text = $"{info.Duration} · {info.Author}";

                var bmp = new BitmapImage(new Uri(info.Thumbnail)); // превюшка видео
                thumbnail.Source = bmp;

                previewCard.Visibility = Visibility.Visible;
            }
            else
            {
                urlLabel.Content = "Invalid link and YOU!";
                urlLabel.Foreground = Brushes.Red;
            }

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {


            string downFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string url = urlTable.Text;
            string type = typeBox.SelectedItem.ToString();
            string quality = $"bestvideo[height={qualityBox.SelectedItem}]+bestaudio";

            void updateProgress(double value)
            {
                progressBar.Value = value;
                progressLabel.Content = $"{value:F1}%";
            }

            var progress = new Progress<double>(updateProgress);
           
            if (type == "Video")
            {
                await downloader.Download(url, downFolder, quality, progress);
            }
            else if (type == "Audio")
            {
                await downloader.DownloadAudio(url, downFolder, progress);
              
            }

            bool success = await downloader.Download(url, downFolder, quality, progress);
            if (success)
                progressLabel.Content = "Video Downloaded!";
            else
                progressLabel.Content = "Failed";
        }
    }
}
