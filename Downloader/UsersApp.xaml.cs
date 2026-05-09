using Downloader.services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Text.Json;

namespace Downloader
{
   
    public partial class UsersApp : Window
    {
        private AuthService authService = new AuthService();
        
        public bool IsAddingAccount { get; set; } = false;
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
                  
                    var mainWindow = new MainWindow(login);
                    mainWindow.Show();
                    this.Close();
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
            {
              
                if (RegPanel.Visibility == Visibility.Visible)
                    secondPassword.Focus();
                
                else
                    ActionBtn_Click(sender, e);
                    
            }
          

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

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            RegPanel.Visibility = Visibility.Collapsed;
            ActionBtn.Content = "Log In";
            var color = (Color)ColorConverter.ConvertFromString("#33FFFFFF");
            LoginBtn.Background = new SolidColorBrush(color);
            RegBtn.Background = Brushes.Transparent;
            LoginError.Text = "";
            PasswordError.Text = "";
            PasswordError2.Text = "";
            EmailError.Text = "";
        }

        private void RegBtn_Click(object sender, RoutedEventArgs e)
        {
            RegPanel.Visibility = Visibility.Visible;
            ActionBtn.Content = "Sign In";
            LoginBtn.Background = Brushes.Transparent;
            var color = (Color)ColorConverter.ConvertFromString("#33FFFFFF");
            RegBtn.Background = new SolidColorBrush(color);
            LoginError.Text = "";
            PasswordError.Text = "";
            PasswordError2.Text = "";
            EmailError.Text = "";


        }
        private void ActionBtn_Click(object sender, RoutedEventArgs e)
        {
            string SessionFile = System.IO.Path.Combine(
                 Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                 "Downloader",
                 "session.txt"
             );
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(SessionFile));

            if (ActionBtn.Content.ToString() == "Log In")
            {
                string login = LoginTextBox.Text.Trim();
                string password = firstPassword.Password.Trim();

                bool success = authService.Login(login, password);

                if (success)
                {
                    SessionService.AddAccount(login);

                    if (IsAddingAccount)
                    {
                        // Знаходимо існуючий MainWindow і переключаємо акаунт
                        var mainWindow = Application.Current.Windows
                            .OfType<MainWindow>()
                            .FirstOrDefault();

                        if (mainWindow != null)
                        {
                            mainWindow.SwitchToAccount(login);
                            mainWindow.Activate(); // повертаємо фокус
                        }

                        this.Close(); // просто закриваємо вікно логіну
                    }
                    else
                    {
                        // Звичайний логін — відкриваємо новий MainWindow
                        var mainWindow = new MainWindow(login);
                        mainWindow.Show();
                        this.Close();
                    }
                }
                else
                {
                    PasswordError.Text = "Invalid login or password";
                    PasswordError.Visibility = Visibility.Visible;
                }
            }
            else if (ActionBtn.Content.ToString() == "Sign In")
            {
                Button_Reg_Click(sender, e);
            }
        }
    }
}


