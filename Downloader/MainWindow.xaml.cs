using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

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
                var formats = await downloader.GetVideoFormats(url);
                qualityBox.ItemsSource = formats;
                qualityBox.SelectedIndex = 0;
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
            if (type == "Video")
            {
               
                    await downloader.Download(url, downFolder, quality, progress);
            }
            else if (type == "Audio")
            {
                await downloader.DownloadAudio(url, downFolder, progress);
              
            }
    }
        private void typeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (typeBox.SelectedItem.ToString() == "Audio")
                qualityBox.Visibility = Visibility.Collapsed;
            else
                qualityBox.Visibility = Visibility.Visible;
        }


    }
}
