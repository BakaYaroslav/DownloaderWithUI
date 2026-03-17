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