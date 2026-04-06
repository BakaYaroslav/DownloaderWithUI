using System.Collections.Generic;
using System.ComponentModel;

namespace Downloader
{
    public class VideoInfo : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public string Duration { get; set; }
        public string Thumbnail { get; set; }
        public string Author { get; set; }
        public string Url { get; set; }

        public string VideoId { get; set; }

        public string Format { get; set; }


        private double _fileSizeMb;
        public double FileSizeMb
        {
            get => _fileSizeMb;
            set { _fileSizeMb = value; OnPropertyChanged(nameof(FileSizeMb)); OnPropertyChanged(nameof(FileSizeDisplay)); }
        }

        private string _quality;
        public string Quality
        {
            get => _quality;
            set { _quality = value; OnPropertyChanged(nameof(Quality)); OnPropertyChanged(nameof(FileSizeDisplay)); }
        }

        public string FileSizeDisplay => _fileSizeMb > 0 ? $"{_fileSizeMb:F1} MB - {_quality} - {Format}" : "";

        public string ThumbnailUrl
        {
            get { return "https://img.youtube.com/vi/" + VideoId + "/mqdefault.jpg"; }

        }


        private double _progress;
        public double Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(nameof(Progress)); }
        }

        private string _status;
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }
       
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}