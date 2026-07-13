using Downloader.services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Text.Json;
using System.Linq;

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
           
            if (PasswordPanel.Visibility == Visibility.Visible || CodePanel.Visibility == Visibility.Visible || NewPassPanel.Visibility == Visibility.Visible)
                return;

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

            if (password.Length < 6)
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

       
        private void EmailTextBox_pass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendCode_Click(sender, e);
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            RegPanel.Visibility = Visibility.Collapsed;
            PasswordPanel.Visibility = Visibility.Collapsed;
            CodePanel.Visibility = Visibility.Collapsed;
            NewPassPanel.Visibility = Visibility.Collapsed;

            LoginTextBox.Visibility = Visibility.Visible;
            firstPassword.Visibility = Visibility.Visible;
            ActionBtn.Visibility = Visibility.Visible;
            ForgotPasswordBtn.Visibility = Visibility.Collapsed; 

            ActionBtn.Content = "Log In";
            var color = (Color)ColorConverter.ConvertFromString("#33FFFFFF");
            LoginBtn.Background = new SolidColorBrush(color);
            RegBtn.Background = Brushes.Transparent;

            ClearAllErrorTexts();
        }

        private void RegBtn_Click(object sender, RoutedEventArgs e)
        {
            RegPanel.Visibility = Visibility.Visible;
            PasswordPanel.Visibility = Visibility.Collapsed;
            CodePanel.Visibility = Visibility.Collapsed;
            NewPassPanel.Visibility = Visibility.Collapsed;

            LoginTextBox.Visibility = Visibility.Visible;
            firstPassword.Visibility = Visibility.Visible;
            ActionBtn.Visibility = Visibility.Visible;
            ForgotPasswordBtn.Visibility = Visibility.Collapsed;

            ActionBtn.Content = "Sign In";
            LoginBtn.Background = Brushes.Transparent;
            var color = (Color)ColorConverter.ConvertFromString("#33FFFFFF");
            RegBtn.Background = new SolidColorBrush(color);

            ClearAllErrorTexts();
        }

        private void ActionBtn_Click(object sender, RoutedEventArgs e)
        {
           
            if (PasswordPanel.Visibility == Visibility.Visible || CodePanel.Visibility == Visibility.Visible || NewPassPanel.Visibility == Visibility.Visible)
                return;

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
                        var mainWindow = Application.Current.Windows
                            .OfType<MainWindow>()
                            .FirstOrDefault();

                        if (mainWindow != null)
                        {
                            mainWindow.SwitchToAccount(login);
                            mainWindow.Activate();
                        }

                        this.Close();
                    }
                    else
                    {
                        var mainWindow = new MainWindow(login);
                        mainWindow.Show();
                        this.Close();
                    }
                }
                else
                {
                    PasswordError.Text = "Invalid login or password";
                    PasswordError.Visibility = Visibility.Visible;
                    ForgotPasswordBtn.Visibility = Visibility.Visible; // Показуємо кнопку ТІЛЬКИ після помилки
                }
            }
            else if (ActionBtn.Content.ToString() == "Sign In")
            {
                Button_Reg_Click(sender, e);
            }
        }

        private void ForgotPasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            ClearAllErrorTexts();

            RegPanel.Visibility = Visibility.Collapsed;
            LoginTextBox.Visibility = Visibility.Collapsed;
            firstPassword.Visibility = Visibility.Collapsed;
            ForgotPasswordBtn.Visibility = Visibility.Collapsed;
            ActionBtn.Visibility = Visibility.Collapsed;

            PasswordPanel.Visibility = Visibility.Visible;
        }

        private void CodeBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox != null && textBox.Text.Length == 1)
            {
                if (textBox == C1) C2.Focus();
                else if (textBox == C2) C3.Focus();
                else if (textBox == C3) C4.Focus();
                else if (textBox == C4) C5.Focus();
                else if (textBox == C5) C6.Focus();
            }
        }

        private void VerifyBtn_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox_pass.Text.Trim();
            string fullCode = C1.Text + C2.Text + C3.Text + C4.Text + C5.Text + C6.Text;

            CodeError.Visibility = Visibility.Collapsed;

            bool isCorrect = authService.VerifyResetCode(email, fullCode);

            if (isCorrect)
            {
                CodeError.Visibility = Visibility.Collapsed;
                CodePanel.Visibility = Visibility.Collapsed;
                NewPassPanel.Visibility = Visibility.Visible;
            }
            else
            {
                CodeError.Text = "Incorrect or expired code.";
                CodeError.Visibility = Visibility.Visible;
            }
        }

        private void CreatePassword_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox_pass.Text.Trim();
            string newPass = newfirstPassword.Password.Trim();
            string confirmPass = newsecondPassword.Password.Trim();

            newPasswordError.Visibility = Visibility.Collapsed;
            newPasswordError2.Visibility = Visibility.Collapsed;

            if (newPass.Length < 6)
            {
                newPasswordError.Text = "Password must be at least 6 characters long.";
                newPasswordError.Visibility = Visibility.Visible;
                return;
            }
            else if (!newPass.Any(char.IsUpper) || !newPass.Any(char.IsLower) || !newPass.Any(char.IsDigit))
            {
                newPasswordError.Text = "Must contain uppercase, lowercase and digits.";
                newPasswordError.Visibility = Visibility.Visible;
                return;
            }

            if (newPass != confirmPass)
            {
                newPasswordError2.Text = "Passwords do not match.";
                newPasswordError2.Visibility = Visibility.Visible;
                return;
            }

            bool success = authService.UpdatePassword(email, newPass);

            if (success)
            {
                MessageBox.Show("Password changed successfully! Please log in.");

                ClearAllErrorTexts();

                NewPassPanel.Visibility = Visibility.Collapsed;
                LoginTextBox.Visibility = Visibility.Visible;
                firstPassword.Visibility = Visibility.Visible;
                ActionBtn.Visibility = Visibility.Visible;
                ForgotPasswordBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Error updating password.");
            }
        }

        private void SendCode_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox_pass.Text.Trim();

            EmailError_pass.Visibility = Visibility.Collapsed;

            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                EmailError_pass.Text = "Please enter a valid email.";
                EmailError_pass.Visibility = Visibility.Visible;
                return;
            }

            bool codeSent = authService.SendResetPasswordCode(email);

            if (codeSent)
            {
                EmailError_pass.Visibility = Visibility.Collapsed;

                PasswordPanel.Visibility = Visibility.Collapsed;
                CodePanel.Visibility = Visibility.Visible;
            }
            else
            {
                EmailError_pass.Text = "User with this email not found.";
                EmailError_pass.Visibility = Visibility.Visible;
            }
        }

     
        private void ClearAllErrorTexts()
        {
            LoginError.Text = ""; LoginError.Visibility = Visibility.Collapsed;
            PasswordError.Text = ""; PasswordError.Visibility = Visibility.Collapsed;
            PasswordError2.Text = ""; PasswordError2.Visibility = Visibility.Collapsed;
            EmailError.Text = ""; EmailError.Visibility = Visibility.Collapsed;
            EmailError_pass.Text = ""; EmailError_pass.Visibility = Visibility.Collapsed;
            CodeError.Text = ""; CodeError.Visibility = Visibility.Collapsed;
            newPasswordError.Text = ""; newPasswordError.Visibility = Visibility.Collapsed;
            newPasswordError2.Text = ""; newPasswordError2.Visibility = Visibility.Collapsed;
        }
    }
}