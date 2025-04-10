using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using AfbeeldingenUitzoeken.Models;
using AfbeeldingenUitzoeken.Views;
// Explicitly use Windows MessageBox and Application to avoid ambiguity
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Application;

namespace AfbeeldingenUitzoeken.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private string _libraryPath;
        private string _keepFolderPath;
        private string _binFolderPath;
        private string _checkLaterFolderPath;
        private bool _isSaveButtonVisible;
        private string _versionInfo;

        public string LibraryPath
        {
            get => _libraryPath;
            set
            {
                _libraryPath = value;
                OnPropertyChanged();
                UpdateSaveButtonVisibility();
            }
        }

        public string KeepFolderPath
        {
            get => _keepFolderPath;
            set
            {
                _keepFolderPath = value;
                OnPropertyChanged();
                UpdateSaveButtonVisibility();
            }
        }

        public string BinFolderPath
        {
            get => _binFolderPath;
            set
            {
                _binFolderPath = value;
                OnPropertyChanged();
                UpdateSaveButtonVisibility();
            }
        }

        public string CheckLaterFolderPath
        {
            get => _checkLaterFolderPath;
            set
            {
                _checkLaterFolderPath = value;
                OnPropertyChanged();
                UpdateSaveButtonVisibility();
            }
        }

        public bool IsSaveButtonVisible
        {
            get => _isSaveButtonVisible;
            set
            {
                _isSaveButtonVisible = value;
                OnPropertyChanged();
            }
        }

        public string VersionInfo
        {
            get => _versionInfo;
            private set
            {
                _versionInfo = value;
                OnPropertyChanged();
            }
        }

        public ICommand SelectLibraryCommand { get; }
        public ICommand SelectKeepFolderCommand { get; }
        public ICommand SelectBinFolderCommand { get; }
        public ICommand SelectCheckLaterFolderCommand { get; }
        public ICommand SaveCommand { get; }

        public SettingsViewModel()
        {            // Initialize fields to avoid null reference exceptions
            _libraryPath = string.Empty;
            _keepFolderPath = string.Empty;
            _binFolderPath = string.Empty;
            _checkLaterFolderPath = string.Empty;
            _versionInfo = AfbeeldingenUitzoeken.Models.VersionInfo.VersionWithLabel;

            SelectLibraryCommand = new RelayCommand(_ => SelectFolder(path => LibraryPath = path));
            SelectKeepFolderCommand = new RelayCommand(_ => SelectFolder(path => KeepFolderPath = path));
            SelectBinFolderCommand = new RelayCommand(_ => SelectFolder(path => BinFolderPath = path));
            SelectCheckLaterFolderCommand = new RelayCommand(_ => SelectFolder(path => CheckLaterFolderPath = path));
            SaveCommand = new RelayCommand(_ => SaveSettings());

            // Load settings from config if available
            LoadConfigSettings();
        }        private void LoadConfigSettings()
        {
            var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "config.txt");
            if (File.Exists(configFilePath))
            {
                var configLines = File.ReadAllLines(configFilePath);
                var settings = configLines.Select(line => line.Split('=')).ToDictionary(parts => parts[0], parts => parts[1]);

                LibraryPath = settings.ContainsKey("LibraryPath") ? settings["LibraryPath"] : string.Empty;
                KeepFolderPath = settings.ContainsKey("KeepFolderPath") ? settings["KeepFolderPath"] : string.Empty;
                BinFolderPath = settings.ContainsKey("BinFolderPath") ? settings["BinFolderPath"] : string.Empty;
                CheckLaterFolderPath = settings.ContainsKey("CheckLaterFolderPath") ? settings["CheckLaterFolderPath"] : string.Empty;
            }
        }
          private void SelectFolder(Action<string> setPathAction)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                Description = "Select a folder"
            };
            
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string folderPath = dialog.SelectedPath;
                if (!string.IsNullOrWhiteSpace(folderPath))
                {
                    setPathAction(folderPath);
                }
            }
        }

        private void SaveSettings()
        {
            try
            {
                // Save the selected paths to a configuration file
                var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "config.txt");
                File.WriteAllLines(configFilePath, new[]
                {
                    $"LibraryPath={LibraryPath}",
                    $"KeepFolderPath={KeepFolderPath}",
                    $"BinFolderPath={BinFolderPath}",
                    $"CheckLaterFolderPath={CheckLaterFolderPath}"
                });                // Update the MainViewModel if accessible
                var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (mainWindow?.DataContext is MainViewModel mainViewModel)
                {
                    // Update folder paths without triggering image reload
                    bool libraryPathChanged = mainViewModel.LibraryPath != LibraryPath;
                    
                    // Set the non-Library paths directly as they don't trigger reloads
                    mainViewModel.KeepFolderPath = KeepFolderPath;
                    mainViewModel.BinFolderPath = BinFolderPath;
                    mainViewModel.CheckLaterFolderPath = CheckLaterFolderPath;
                      // Only reload the library if the path has actually changed
                    if (libraryPathChanged)
                    {
                        mainViewModel.LibraryPath = LibraryPath;
                        // Make sure to refresh the image counts even if the library path is the same
                        mainViewModel.RefreshImageCounts();
                    }
                }

                // Close the settings window
                Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w is SettingsView)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSaveButtonVisibility()
        {
            IsSaveButtonVisible = !string.IsNullOrEmpty(LibraryPath) &&
                                  !string.IsNullOrEmpty(KeepFolderPath) &&
                                  !string.IsNullOrEmpty(BinFolderPath) &&
                                  !string.IsNullOrEmpty(CheckLaterFolderPath);
        }        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
