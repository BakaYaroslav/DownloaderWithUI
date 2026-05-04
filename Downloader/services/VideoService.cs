using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Security.Policy;

namespace Downloader.services
{
    public class VideoService
    {
        public static void SaveVideo(string login, VideoInfo video)
        {
            using var connection = new MySqlConnection(DatabaseConfig.ConnectionString);
            connection.Open();
            var cmd = new MySqlCommand(
    @"INSERT INTO videos (login, title, url, duration, author, file_size_mb, quality, format) 
      VALUES (@login, @title, @url, @duration, @author, @fileSizeMb, @quality, @format)",
    connection);
            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@title", video.Title);
            cmd.Parameters.AddWithValue("@url", video.Url);
            cmd.Parameters.AddWithValue("@duration", video.Duration ?? "");
            cmd.Parameters.AddWithValue("@author", video.Author ?? "");
            cmd.Parameters.AddWithValue("@format", video.Format ?? "");
            cmd.Parameters.AddWithValue("@quality", video.Quality ?? "");
            cmd.Parameters.AddWithValue("@fileSizeMb", video.FileSizeMb);
            cmd.ExecuteNonQuery();
        }

        public static List<VideoInfo> LoadVideos(string login)
        {
            var list = new List<VideoInfo>();
            using var connection = new MySqlConnection(DatabaseConfig.ConnectionString);
            connection.Open();
            var cmd = new MySqlCommand(
                "SELECT title, url, duration, author, file_size_mb, quality, format FROM videos WHERE login = @login ORDER BY created_at DESC",
                 connection);
            cmd.Parameters.AddWithValue("@login", login);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var url = reader.GetString("url");
                list.Add(new VideoInfo
                {
                    Title = reader.GetString("title"),
                    Url = url,
                    VideoId = url.Split("v=")[1].Split("&")[0],
                    Duration = reader.IsDBNull(reader.GetOrdinal("duration")) ? "" : reader.GetString("duration"),
                    Author = reader.IsDBNull(reader.GetOrdinal("author")) ? "" : reader.GetString("author"),
                    Quality = reader.IsDBNull(reader.GetOrdinal("quality")) ? "" : reader.GetString("quality"),
                    Format = reader.IsDBNull(reader.GetOrdinal("format")) ? "" : reader.GetString("format"),
                    FileSizeMb = reader.IsDBNull(reader.GetOrdinal("file_size_mb")) ? 0 : reader.GetDouble("file_size_mb")

                });
            } return list;
        }
        public static void UpdateAllInfo(string login, VideoInfo video)
        {
            using var connection = new MySqlConnection(DatabaseConfig.ConnectionString);
            connection.Open();
            var cmd = new MySqlCommand(
                @"UPDATE videos 
          SET file_size_mb = @size, quality = @quality, format = @format
          WHERE login = @login AND url = @url",
                connection);
            cmd.Parameters.AddWithValue("@size", video.FileSizeMb);
            cmd.Parameters.AddWithValue("@quality", video.Quality ?? "");
            cmd.Parameters.AddWithValue("@format", video.Format ?? "");
            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@url", video.Url);
            cmd.ExecuteNonQuery();
        }
        public static void DeleteVideo(string login, VideoInfo video)
        {
            using var connection = new MySqlConnection(DatabaseConfig.ConnectionString);
            connection.Open();
            var cmd = new MySqlCommand(
                "DELETE FROM videos WHERE login = @login AND url = @url",
                connection);
            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@url", video.Url);
            cmd.ExecuteNonQuery();
        }
    }
}