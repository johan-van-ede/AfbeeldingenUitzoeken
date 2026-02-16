using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AfbeeldingenUitzoeken.Models;
using AfbeeldingenUitzoeken.Services;
using AfbeeldingenUitzoeken.ViewModels;
using AfbeeldingenUitzoeken.Views;

namespace AfbeeldingenUitzoeken;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{    // Update server URL - this should point to a JSON file on your server
    // that contains version information
    private const string UpdateServerUrl = "https://github.com/johan-van-ede/AfbeeldingenUitzoeken/releases/latest/download/version.json";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Create and initialize the main window
        var mainWindow = new MainWindow();
        var mainViewModel = mainWindow.DataContext as MainViewModel;

        // Show the window first so it can be used as an owner for dialogs
        mainWindow.Show();
        
        // Now check for invalid paths and show settings if needed
        if (mainViewModel != null)
        {
            // LoadConfigSettings will validate paths and show the settings window if needed
            mainViewModel.LoadConfigSettings();
        }
        
        // Check for updates after a short delay to allow the app to finish loading
        Task.Delay(2000).ContinueWith(_ => CheckForUpdatesAsync(mainWindow));
    }
    
    /// <summary>
    /// Checks for application updates in the background
    /// </summary>
    private async Task CheckForUpdatesAsync(Window ownerWindow)
    {
    // Online update check removed; method now does nothing.
    }
}

