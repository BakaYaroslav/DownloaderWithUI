using Downloader.services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Downloader
{
   
    public partial class UsersApp : Window
    {
        private AuthService authService = new AuthService();
        public UsersApp()
        {
            InitializeComponent();
        }

        private void Button_Reg_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = firstPassword.Password.Trim();
            string confirmPassword = secondPassword.Password.Trim();
            string email = EmailTextBox.Text.ToLower();

            LoginError.Visibility = Visibility.Collapsed;
            PasswordError.Visibility = Visibility.Collapsed;
            PasswordError2.Visibility = Visibility.Collapsed;
            EmailError.Visibility = Visibility.Collapsed;

            bool hasError = false;

            if (login.Length < 4)
            {
                LoginError.Text = "Login must be at least 4 characters long.";
                LoginError.Visibility = Visibility.Visible;
                hasError = true;
            }

            if (password.Length < 6 )
            {
                PasswordError.Text = "Password must be at least 6 characters long.";
                PasswordError.Visibility = Visibility.Visible;
                hasError = true;
            }
            else if (!password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
            {
                PasswordError.Text = "Must contain uppercase, lowercase and digits.";
                PasswordError.Visibility = Visibility.Visible;
                hasError = true;
            }
            if (password != confirmPassword)
            {
                PasswordError2.Text = "Passwords do not match.";
                PasswordError2.Visibility = Visibility.Visible;
                hasError = true;
            }
             if (!email.Contains("@") || !email.Contains(".") || email.Length < 7)
            {
                EmailError.Text = "Invalid email format.";
                EmailError.Visibility = Visibility.Visible;
                hasError = true;
            }
            if (!hasError)
            {

               
               bool success = authService.Register(login, password, email);

                if (success)
                {
                    MessageBox.Show("Registration successful!");
                }
                else
                {
                    MessageBox.Show("Error! This username or email already be taken.");
                }
            }
        }

        private void LoginTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                firstPassword.Focus();
        }

        private void firstPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                secondPassword.Focus();
        }

        private void secondPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                EmailTextBox.Focus();
        }

        private void EmailTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Button_Reg_Click(sender, e); 
        }
    }
}

