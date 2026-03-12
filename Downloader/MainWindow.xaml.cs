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
            qualityBox.ItemsSource = new List<string> { "1080", "720", "480", "360" };
            qualityBox.SelectedIndex = 0;
            typeBox.ItemsSource = new List<string> { "Video", "Audio" };
            typeBox.SelectedIndex = 0;
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
                try
                {
                    await downloader.Download(url, downFolder, quality, progress);
                    
                   
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
            else if (type == "Audio")
            {
                await downloader.DownloadAudio(url, downFolder, progress);
              
            }
           
            MessageBox.Show(" Downloaded!");
           

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
