using System.Collections.Generic;
using System.ComponentModel;

namespace Downloader
{
    public class VideoInfo : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public string Duration { get; set; } // длительность видео
        public string Thumbnail { get; set; } // превьюшка видео
        public string Author { get; set; }
        public string Url { get; set; }

        public string VideoId { get; set; }

        public string Format { get; set; }


        private double _fileSizeMb;
        public double FileSizeMb // при получении значения, мы сохраняем его и вызываем событие изменения для FileSizeMb и FileSizeDisplay, чтобы обновить отображение размера файла
        {
            get => _fileSizeMb;
            set { _fileSizeMb = value; OnPropertyChanged(nameof(FileSizeMb)); OnPropertyChanged(nameof(FileSizeDisplay)); } 
        }

        private string _quality;
        public string Quality // так же обновляем качество видео в реальном времени, и при его изменении обновляем отображение размера файла, так как там отображается качество видео
        {
            get => _quality;
            set { _quality = value; OnPropertyChanged(nameof(Quality)); OnPropertyChanged(nameof(FileSizeDisplay)); }
        }

        public string FileSizeDisplay => _fileSizeMb > 0 ? $"{_fileSizeMb:F1} MB - {_quality} - {Format}" : "";

        public string ThumbnailUrl // формируем ссылку на превьюшку видео по его id
        {
            get { return "https://img.youtube.com/vi/" + VideoId + "/mqdefault.jpg"; }

        }


        private double _progress;
        public double Progress // прогрессбар в реальном времени
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(nameof(Progress)); }
        }

        private string _status;
        public string Status // получаем статус загрузки видео, и при его изменении обновляем отображение размера файла, так как там отображается статус видео
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }
       
        public event PropertyChangedEventHandler PropertyChanged; // событие, которое вызывается при изменении свойства, и позволяет обновить интерфейс в реальном времени
        protected void OnPropertyChanged(string name) => // string name - имя свойства, которое изменилось из xaml
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); // рисуем интерфейс в реальном времени при изменении свойства
    }
}