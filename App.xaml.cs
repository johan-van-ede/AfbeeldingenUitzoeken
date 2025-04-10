using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AfbeeldingenUitzoeken.ViewModels;

namespace AfbeeldingenUitzoeken;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Create and initialize the main window
        var mainWindow = new MainWindow();
        var mainViewModel = mainWindow.DataContext as MainViewModel;
        
        // Check if the config file exists
        var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "config.txt");
        if (File.Exists(configFilePath) && mainViewModel != null)
        {
            var configLines = File.ReadAllLines(configFilePath);
            var settings = configLines.Select(line => line.Split('=')).ToDictionary(parts => parts[0], parts => parts[1]);

            // No need to manually load images here as it's handled in the ViewModel
            // when the properties are set
        }

        // Always show the window, regardless of config file existence
        mainWindow.Show();
    }
}

