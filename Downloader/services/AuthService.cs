using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;


namespace Downloader.services
{
    public class AuthService
    {

        public bool Register(string login, string password, string email)
        {
            try
            {
                using var connection = new MySqlConnection(DatabaseConfig.ConnectionString);
                connection.Open();

                var cmd = new MySqlCommand("INSERT INTO users (login, password, email) " +
                                            "VALUES (@login, @password, @email)",
                connection);

                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@email", email);

                cmd.ExecuteNonQuery(); // Выполняет запрос на вставку данных в таблицу users
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Login(string login, string password)
        {
            try
            {
                using var connection = new MySqlConnection(DatabaseConfig.ConnectionString);
                connection.Open();
                var cmd = new MySqlCommand("SELECT COUNT(*) FROM users " +
                                            "WHERE login = @login AND password = @password",
                connection);
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@password", password);
                long count = (long)cmd.ExecuteScalar(); // проверяет, существует ли пользователь с таким логином и паролем
                cmd.ExecuteNonQuery();
                return count > 0; // если count больше 0, значит пользователь с таким логином и паролем существует, и мы возвращаем true, иначе false
            }
            catch
            {
                return false;
            }

        }
    }
}

