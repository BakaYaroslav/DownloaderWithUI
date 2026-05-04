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
            string SessionFile = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Downloader",
                "session.txt"
            );
            Directory.CreateDirectory(Path.GetDirectoryName(SessionFile));

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
