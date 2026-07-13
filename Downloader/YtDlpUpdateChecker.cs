using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace Downloader
{
    public class YtDlpUpdateChecker
    {
        private readonly HttpClient httpClient = new HttpClient();

        public YtDlpUpdateChecker()
        {
            httpClient.DefaultRequestHeaders.Add("User-Agent", "MyDownloaderApp");
        }

        public async Task<(string version, string downloadUrl)> GetLatestReleaseInfoAsync()
        {
            string url = "https://api.github.com/repos/yt-dlp/yt-dlp/releases/latest";
            string json = await httpClient.GetStringAsync(url);

            JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;

            string version = root.GetProperty("tag_name").GetString();

            JsonElement assets = root.GetProperty("assets");
            string downloadUrl = null;

            foreach (JsonElement asset in assets.EnumerateArray())
            {
                string assetName = asset.GetProperty("name").GetString();
                if (assetName == "yt-dlp.exe")
                {
                    downloadUrl = asset.GetProperty("browser_download_url").GetString();
                    break;
                }
            }

            return (version, downloadUrl);
        }

        public async Task<string> GetLocalVersionAsync(string ytdlpPath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = ytdlpPath;
            startInfo.Arguments = "--version";
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            Process process = Process.Start(startInfo);
            string output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return output.Trim();
        }

        public bool IsUpdateAvailable(string localVersion, string latestVersion)
        {
            int comparison = string.Compare(localVersion, latestVersion, StringComparison.Ordinal);
            return comparison < 0;
        }

        public async Task DownloadFileAsync(string url, string destinationPath, IProgress<double> progress)
        {
            using HttpResponseMessage response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            long totalBytes = response.Content.Headers.ContentLength ?? -1;

            using Stream downloadStream = await response.Content.ReadAsStreamAsync();
            using FileStream fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);

            byte[] buffer = new byte[8192];
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await downloadStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalRead += bytesRead;

                if (totalBytes > 0)
                {
                    double percent = (double)totalRead / totalBytes * 100;
                    progress.Report(percent);
                }
            }
        }
    }
}