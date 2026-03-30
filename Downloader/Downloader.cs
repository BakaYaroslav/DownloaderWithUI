using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Windows;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace Downloader
{
    public class VideoDownloader
    {
        YoutubeDL ytdl = new YoutubeDL(); // это класс с помощью которого мы будем скачивать видео и получать информацию о виде
        public VideoDownloader()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            ytdl.YoutubeDLPath = Path.Combine(baseDir, @"tools\yt-dlp.exe");
            ytdl.FFmpegPath = Path.Combine(baseDir, @"tools\ffmpeg-8.0.1-essentials_build\bin\ffmpeg.exe");


        }

        // async говорит: "этот метод будет ждать не замораживая программу"
        public async Task<bool> Download(string url, string outputFolder, string format, IProgress<double> progress)

        {
            ytdl.OutputFolder = outputFolder;
            var options = new OptionSet(); // это класс, который содержит все настройки для загрузки видео.
            options.Format = format;
            options.AddCustomOption("--merge-output-format", "mp4");
            options.AddCustomOption("--recode-video", "mp4");


            var progressBar = new Progress<DownloadProgress>(p => progress.Report(p.Progress * 100));
            var result = await ytdl.RunVideoDownload(url, overrideOptions: options, progress: progressBar);

            return result.Success;
        }
        public async Task<List<string>> GetVideoFormats(string url)
        {
            var standardQualities = new List<int> { 144, 240, 360, 480, 720, 1080, 1440, 2160 };
            var result = await ytdl.RunVideoDataFetch(url);
            if (!result.Success)
            {
                Console.WriteLine("Failed to get video info");
                return new List<string>();
            }
            try
            {
                var qualities = result.Data.Formats
               .Where(f => f.Height != null)        // только форматы у которых есть высота  .where = foreach с условием { Height: 1080, Ext: "webm" }  → true
               .Where(f => standardQualities.Contains(f.Height.Value)) // только форматы с высотой из списка стандартных  .Where = foreach с условием 1080 → true, 123 → false
               .Select(f => f.Height.Value)         // берём только значение высоты  .Select = это foreach который что-то вытаскивает { Height: 1080, Ext: "webm" }  →  1080
               .Distinct()                          // убираем дубликаты // .Distinct() = 1080, 720, 1080 → 1080, 720
               .OrderByDescending(h => h)           // сортируем от большего к меньшему
               .ToList();
                return qualities.Select(q => q.ToString()).ToList();
            }
            catch (Exception)
            {
                return new List<string>();

            }



        }
        public async Task<VideoInfo> GetInfo(string url)
        {

            var result = await ytdl.RunVideoDataFetch(url);
            if (!result.Success || result.Data == null)
                return null;

            var title = result.Data.Title;
            var duration = TimeSpan.FromSeconds(result.Data.Duration ?? 0).ToString(@"hh\:mm\:ss"); // конвертируем длительность из секунд в формат часы:минуты:секунды
            var author = result.Data.Channel;
            var thumbnail = result.Data.Thumbnail;
            return new VideoInfo
            {
                Title = title,
                Duration = duration,
                Thumbnail = thumbnail,
                Author = author,
            };


        }


        public async Task DownloadAudio(string url, string outputFolder, IProgress<double> progress)
        {
            ytdl.OutputFolder = outputFolder;


            var progressBar = new Progress<DownloadProgress>(p => progress.Report(p.Progress * 100));


            var result = await ytdl.RunAudioDownload(url, AudioConversionFormat.Mp3, progress: progressBar);



        }
    }
}