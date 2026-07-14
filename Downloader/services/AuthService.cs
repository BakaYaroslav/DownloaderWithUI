using BCrypt.Net;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Downloader.services
{
    public class AuthService
    {
        private int _resetCode;
        private DateTime _codeCreatedAt;

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

                cmd.ExecuteNonQuery(); // Executes insertion query into users table
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

                var result = cmd.ExecuteScalar(); // Executes the query and returns the first column of the first row

                if (result == null)
                    return false;

                string hashedPassword = result.ToString();
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword); // Compares input password with hashed database password
            }
            catch
            {
                return false;
            }
        }

        // Method required by your UI to check email existence, generate code, and handle email dispatch setup
        public bool SendResetPasswordCode(string email)
        {
            try
            {
                using var connection = new MySqlConnection(DatabaseConfig.ConnectionString);
                connection.Open();

                var cmd = new MySqlCommand("SELECT COUNT(*) FROM users WHERE email = @email", connection);
                cmd.Parameters.AddWithValue("@email", email);

                long userExists = Convert.ToInt64(cmd.ExecuteScalar());
                if (userExists == 0)
                    return false; 

              
                Random rnd = new Random();
                _resetCode = rnd.Next(100000, 999999);
                _codeCreatedAt = DateTime.Now;

               
                EmailService emailService = new EmailService();
                bool mailSent = emailService.SendMailMessage(email, _resetCode);

                return mailSent; 
            }
            catch
            {
                return false;
            }
        }

        // Method required by your UI to verify the string pattern input against stored integer parameters
        public bool VerifyResetCode(string email, string code)
        {
            if (DateTime.Now - _codeCreatedAt > TimeSpan.FromMinutes(3))
                return false; // Code expired after 3 minutes

            return code == _resetCode.ToString();
        }

        // Method required by your UI to target updates directly via verified email location
        public bool UpdatePassword(string email, string newPassword)
        {
            try
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                using var connection = new MySqlConnection(DatabaseConfig.ConnectionString);
                connection.Open();

                var cmd = new MySqlCommand("UPDATE users SET password = @password WHERE email = @email", connection);
                cmd.Parameters.AddWithValue("@password", hashedPassword);
                cmd.Parameters.AddWithValue("@email", email);

                cmd.ExecuteNonQuery(); // Updates target row password hash parameter
                return true;
            }
            catch
            {
                return false;
            }
        }

        public int GenerateResetCode()
        {
            Random rnd = new Random();
            int code = rnd.Next(100000, 999999);

            _resetCode = code;
            _codeCreatedAt = DateTime.Now;

            return code;
        }

        public bool VerifyResetCode(int code)
        {
            if (DateTime.Now - _codeCreatedAt > TimeSpan.FromMinutes(3))
                return false; // Code expired
            return code == _resetCode;
        }

        public bool ResetPassword(string login, string newpassword)
        {
            try
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newpassword);
                using var connection = new MySqlConnection(DatabaseConfig.ConnectionString);
                connection.Open();
                var cmd = new MySqlCommand("UPDATE users SET password = @password WHERE login = @login",
                connection);
                cmd.Parameters.AddWithValue("@password", hashedPassword);
                cmd.Parameters.AddWithValue("@login", login);
                cmd.ExecuteNonQuery(); // Updates user password via username reference
                return true;
            }
            catch
            {
                return false;
            }
        }


      
    }
}