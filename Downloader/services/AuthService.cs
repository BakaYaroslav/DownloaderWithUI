using BCrypt.Net;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Downloader.services
{
    public class AuthService
    {

        public bool Register(string login, string password, string email)
        {
            try
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                using var connection = new MySqlConnection(DatabaseConfig.ConnectionString);
                connection.Open();

                var cmd = new MySqlCommand("INSERT INTO users (login, password, email) " +
                                            "VALUES (@login, @password, @email)",
                connection);

                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@password", hashedPassword);
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
                var cmd = new MySqlCommand(
            "SELECT password FROM users WHERE login = @login",
            connection);

                cmd.Parameters.AddWithValue("@login", login);

                var result = cmd.ExecuteScalar(); // выполняет запрос и возвращает одно значение (первую колонку первой строки)

                if (result == null)
                    return false;

                string hashedPassword = result.ToString();
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword); // Сравнивает введенный пароль с хешированным паролем из базы данных по первой записи
            }
            catch
            {
                return false;
            }

        }


       
    }
}

