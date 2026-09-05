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
        YtDlpUpdateChecker updateChecker = new YtDlpUpdateChecker();
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
            InitialsText.Text = GetInitial(login);

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            EditorControl.CloseRequested += CloseEditor;

            LoadVideosForCurrentUser();
            CheckYtDlpUpdate();

        }
 
        private string GetInitial(string login)
        {
            if (string.IsNullOrEmpty(login)) return "?";
            return login[0].ToString().ToUpper();
        }
        private void LoadVideosForCurrentUser()
        {
            videos.Clear(); 
            var saved = VideoService.LoadVideos(CurrentLogin);
            foreach (var v in saved)
                videos.Add(v);
        }

        public void SwitchToAccount(string login)
        {
            CurrentLogin = login;
            SessionService.SwitchAccount(login); // сохраняем session.json
            LoadVideosForCurrentUser();         
                                            
        }
        public void RemoveCurrentAccount()
        {
            bool noAccountsLeft = SessionService.RemoveAccount(CurrentLogin);

            if (noAccountsLeft)
            {
                new UsersApp().Show();
                this.Close();
                return;
            }

            // если аккаунты остались, переключаемся на другой
            var data = SessionService.Load();
            SwitchToAccount(data.Active);
        }
        private void ProfileBtn_Click(object sender, RoutedEventArgs e)
        {
            CurrentLoginText.Text = CurrentLogin;

            var data = SessionService.Load();
            AccountsList.ItemsSource = data.Saved.Where(a => a != CurrentLogin).ToList();

            var allVideos = VideoService.LoadVideos(CurrentLogin);
            double totalMb = allVideos.Sum(v => v.FileSizeMb);
            StatsText.Text = $"{allVideos.Count} downloaded videos · {totalMb:F1} MB";

            ProfilePopup.IsOpen = !ProfilePopup.IsOpen;
        }

        private void SwitchAccount_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            string login = btn?.Tag?.ToString();
            if (login == null) return;

            ProfilePopup.IsOpen = false;
            SwitchToAccount(login);
            InitialsText.Text = GetInitial(login);
        }

        private void AddAccount_Click(object sender, RoutedEventArgs e)
        {
            ProfilePopup.IsOpen = false;

            var existing = Application.Current.Windows
                .OfType<UsersApp>()
                .FirstOrDefault();

            if (existing != null)
            {
                existing.Activate();
                return;
            }

            var loginWindow = new UsersApp();
            loginWindow.IsAddingAccount = true; 
            loginWindow.Show();
        }

        private void RemoveAccount_Click(object sender, RoutedEventArgs e)
        {
            ProfilePopup.IsOpen = false;
            RemoveCurrentAccount();
        }

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            SessionService.SwitchAccount("");
            new UsersApp().Show();
            this.Close();
        }

       
        private async void urlTable_TextChanged(object sender, TextChangedEventArgs e)
        {
            string url = urlTable.Text.Trim();

            if (!url.Contains("youtube.com/watch"))
            {
                urlLabel.Text = "Invalid URL or YOU!";
                urlLabel.Foreground = Brushes.IndianRed;
                return;

            }
            try
            {
                urlLabel.Text = "Get video info...";
                urlLabel.Foreground = Brushes.DarkGray;
                var formats = await downloader.GetVideoFormats(url);
                qualityBox.ItemsSource = formats;
                qualityBox.SelectedIndex = 0;
                var info = await downloader.GetInfo(url);
                if (info == null) return;
                info.Url = url;
             
                urlLabel.Text = "Paste link here: ";
               
                info.VideoId = url.Split("v=")[1].Split("&")[0];

                VideoService.SaveVideo(CurrentLogin, info);
                videos.Insert(0, info);

            }
            catch (Exception ex)
            {
                urlLabel.Text = "Invalid URL or YOU!";
                urlLabel.Foreground = Brushes.Red;
                MessageBox.Show("URL processing failed: " + ex.Message + "\n\nInner: " + ex.InnerException?.Message);
            }
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var info = button?.DataContext as VideoInfo;

            // если уже скачивается — это клик по Cancel
            if (info.CancellationTokenSource != null)
            {
                info.CancellationTokenSource.Cancel();
                return;
            }

            info.CancellationTokenSource = new CancellationTokenSource();
            info.Status = "Downloading...";

            string downFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string url = info.Url;

            string selectedQuality = qualityBox.SelectedItem?.ToString() ?? "1080";
            string quality = $"bestvideo[height<={selectedQuality}][ext=mp4]+bestaudio[ext=m4a]/bestvideo[height<={selectedQuality}]+bestaudio/best";
            info.Quality = selectedQuality + "p";
            string type = typeBox.SelectedItem.ToString();
            info.Format = type == "Video" ? "mp4" : "mp3";

            var progress = new Progress<double>(value =>
            {
                if (value == 0) return;
                info.Progress = value;
                info.Status = $"{value:F1}%";
            });

            try
            {
                if (type == "Video")
                {
                    bool success = await downloader.Download(url, downFolder, quality, progress, info.CancellationTokenSource.Token);
                    if (success)
                    {
                        var file = Directory.GetFiles(downFolder, "*.mp4")
                            .Select(f => new FileInfo(f))
                            .OrderByDescending(f => f.LastWriteTime)
                            .FirstOrDefault();

                        info.FileSizeMb = file != null ? file.Length / (1024.0 * 1024.0) : 0;
                        VideoService.UpdateAllInfo(CurrentLogin, info);
                        info.Status = "Downloaded!";
                    }
                    else info.Status = "Failed";
                }
                else if (type == "Audio")
                {
                    await downloader.DownloadAudio(url, downFolder, progress);
                    info.Status = "Downloaded!";
                }
            }
            catch (OperationCanceledException)
            {
                info.Status = "Cancelled";
                info.Progress = 0;
            }
            finally
            {
                info.CancellationTokenSource = null;
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

        private async void CheckYtDlpUpdate()
        {
            string local = await updateChecker.GetLocalVersionAsync(downloader.YtDlpPath);
            var (latest, downloadUrl) = await updateChecker.GetLatestReleaseInfoAsync();

            if (updateChecker.IsUpdateAvailable(local, latest))
            {
                var updateWindow = new UpdateWindow(local, latest, downloadUrl, downloader.YtDlpPath, updateChecker);
                updateWindow.Owner = this;
                updateWindow.ShowDialog();
            }
        }

        private void EditCard_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag is VideoInfo item)
            {
                OpenEditor(item);
            }
        }

        private void OpenEditor(VideoInfo info)
        {
            MainView.Visibility = Visibility.Collapsed;
            EditorView.Visibility = Visibility.Visible;
            EditorControl.LoadVideo(info, downloader); // метод внутри UserControl, который сам всё загрузит
        }

        private void CloseEditor()
        {
            EditorView.Visibility = Visibility.Collapsed;
            MainView.Visibility = Visibility.Visible;
        }

       
    }

}