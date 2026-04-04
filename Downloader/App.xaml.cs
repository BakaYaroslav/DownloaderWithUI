using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace Downloader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            string SessionFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "session.txt");
           
            base.OnStartup(e);

            if (File.Exists(SessionFile))
            {
                string login = File.ReadAllText(SessionFile);
                new MainWindow(login).Show();
            }
            else
            {
                new UsersApp().Show();
            }
        }
    }

}
