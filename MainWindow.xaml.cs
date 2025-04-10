using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using AfbeeldingenUitzoeken.ViewModels;
using AfbeeldingenUitzoeken.Views;

namespace AfbeeldingenUitzoeken
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
        
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            
            // Check if there are images in the bin folder and prompt to empty it
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.CheckAndPromptToEmptyBinFolder();
            }
        }
        
        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsView = new Views.SettingsView();
            var settingsViewModel = settingsView.DataContext as SettingsViewModel;
            
            // Load existing paths from the config file if it exists
            var configFilePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.txt");
            if (File.Exists(configFilePath))
            {
                var configLines = File.ReadAllLines(configFilePath);
                var settings = configLines.Select(line => line.Split('=')).ToDictionary(parts => parts[0], parts => parts[1]);

                if (settingsViewModel != null)
                {
                    settingsViewModel.LibraryPath = settings.ContainsKey("LibraryPath") ? settings["LibraryPath"] : string.Empty;
                    settingsViewModel.KeepFolderPath = settings.ContainsKey("KeepFolderPath") ? settings["KeepFolderPath"] : string.Empty;
                    settingsViewModel.BinFolderPath = settings.ContainsKey("BinFolderPath") ? settings["BinFolderPath"] : string.Empty;
                    settingsViewModel.CheckLaterFolderPath = settings.ContainsKey("CheckLaterFolderPath") ? settings["CheckLaterFolderPath"] : string.Empty;
                }
            }

            settingsView.ShowDialog();
        }
    }
}