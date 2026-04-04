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
                "INSERT INTO videos (login, title, url) VALUES (@login, @title, @url)",
                connection);
            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@title", video.Title);
            cmd.Parameters.AddWithValue("@url", video.Url);
            cmd.ExecuteNonQuery();
        }

        public static List<VideoInfo> LoadVideos(string login)
        {
            var list = new List<VideoInfo>();
            using var connection = new MySqlConnection(DatabaseConfig.ConnectionString);
            connection.Open();
            var cmd = new MySqlCommand(
                "SELECT title, url FROM videos WHERE login = @login ORDER BY created_at DESC",
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
                    VideoId = url.Split("v=")[1].Split("&")[0]
                });
            }return list;
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