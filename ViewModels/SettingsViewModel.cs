using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Text.Json;
using Microsoft.Win32;
using AfbeeldingenUitzoeken.Models;
using AfbeeldingenUitzoeken.Views;
// Explicitly use Windows MessageBox and Application to avoid ambiguity
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Application;

namespace AfbeeldingenUitzoeken.ViewModels
{    public class SettingsViewModel : INotifyPropertyChanged
    {
        private string _libraryPath = string.Empty;
        private string _keepFolderPath = string.Empty;
        private string _binFolderPath = string.Empty;
        private string _checkLaterFolderPath = string.Empty;
        private bool _isSaveButtonVisible;
        private string _versionInfo;
        private bool _isLibraryPathInvalid;
        private bool _isKeepFolderPathInvalid;
        private bool _isBinFolderPathInvalid;
        private bool _isCheckLaterFolderPathInvalid;
        private ObservableCollection<FolderSet> _folderSets = new ObservableCollection<FolderSet>();
        private FolderSet? _selectedFolderSet;
        private string _newFolderSetName = string.Empty;
        private readonly string _folderSetsConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "foldersets.json");
        private bool _isFolderSetNameValid = false;
        private string _currentFolderSetName = string.Empty;        public string LibraryPath
        {
            get => _libraryPath;
            set
            {
                _libraryPath = value;
                IsLibraryPathInvalid = !string.IsNullOrEmpty(value) && !Directory.Exists(value);
                OnPropertyChanged();
                UpdateSaveButtonVisibility();
            }
        }

        public ObservableCollection<FolderSet> FolderSets
        {
            get => _folderSets;
            set
            {
                _folderSets = value;
                OnPropertyChanged();
            }
        }        public FolderSet? SelectedFolderSet
        {
            get => _selectedFolderSet;
            set
            {
                _selectedFolderSet = value;
                OnPropertyChanged();
                
                if (value != null)
                {
                    // Update the current paths from the selected folder set
                    LibraryPath = value.LibraryPath;
                    KeepFolderPath = value.KeepFolderPath;
                    BinFolderPath = value.BinFolderPath;
                    CheckLaterFolderPath = value.CheckLaterFolderPath;
                    CurrentFolderSetName = value.Name;
                    
                    // Also update the NewFolderSetName to allow editing the current selection
                    _newFolderSetName = value.Name; // Directly set the field to avoid validation
                    OnPropertyChanged(nameof(NewFolderSetName));
                    
                    // Update validation since we're editing an existing set
                    UpdateFolderSetNameValidation();
                }
            }
        }        public string NewFolderSetName
        {
            get => _newFolderSetName;
            set
            {
                _newFolderSetName = value;
                UpdateFolderSetNameValidation();
                OnPropertyChanged();
            }
        }

        private void UpdateFolderSetNameValidation()
        {
            // Allow the name if it's not empty and either:
            // 1. It's the same as the currently selected folder set, or
            // 2. It doesn't match any existing folder set name
            IsFolderSetNameValid = !string.IsNullOrWhiteSpace(_newFolderSetName) && 
                                   ((_selectedFolderSet != null && _selectedFolderSet.Name == _newFolderSetName) ||
                                    !FolderSets.Any(fs => fs.Name.Equals(_newFolderSetName, StringComparison.OrdinalIgnoreCase)));
            OnPropertyChanged(nameof(IsFolderSetNameValid));
        }

        public bool IsFolderSetNameValid
        {
            get => _isFolderSetNameValid;
            set
            {
                _isFolderSetNameValid = value;
                OnPropertyChanged();
            }
        }

        public string CurrentFolderSetName
        {
            get => _currentFolderSetName;
            set
            {
                _currentFolderSetName = value;
                OnPropertyChanged();
            }
        }

        public string KeepFolderPath
        {
            get => _keepFolderPath;
            set
            {
                _keepFolderPath = value;
                IsKeepFolderPathInvalid = !string.IsNullOrEmpty(value) && !Directory.Exists(value);
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
                IsBinFolderPathInvalid = !string.IsNullOrEmpty(value) && !Directory.Exists(value);
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
                IsCheckLaterFolderPathInvalid = !string.IsNullOrEmpty(value) && !Directory.Exists(value);
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

        public bool IsLibraryPathInvalid
        {
            get => _isLibraryPathInvalid;
            private set
            {
                _isLibraryPathInvalid = value;
                OnPropertyChanged();
            }
        }

        public bool IsKeepFolderPathInvalid
        {
            get => _isKeepFolderPathInvalid;
            private set
            {
                _isKeepFolderPathInvalid = value;
                OnPropertyChanged();
            }
        }

        public bool IsBinFolderPathInvalid
        {
            get => _isBinFolderPathInvalid;
            private set
            {
                _isBinFolderPathInvalid = value;
                OnPropertyChanged();
            }
        }

        public bool IsCheckLaterFolderPathInvalid
        {
            get => _isCheckLaterFolderPathInvalid;
            private set
            {
                _isCheckLaterFolderPathInvalid = value;
                OnPropertyChanged();
            }
        }        public ICommand SelectLibraryCommand { get; }
        public ICommand SelectKeepFolderCommand { get; }
        public ICommand SelectBinFolderCommand { get; }
        public ICommand SelectCheckLaterFolderCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SaveFolderSetCommand { get; }
        public ICommand RemoveFolderSetCommand { get; }        public SettingsViewModel()
        {
            _versionInfo = AfbeeldingenUitzoeken.Models.VersionInfo.VersionWithLabel;

            SelectLibraryCommand = new RelayCommand(_ => SelectFolder(path => LibraryPath = path));
            SelectKeepFolderCommand = new RelayCommand(_ => SelectFolder(path => KeepFolderPath = path));
            SelectBinFolderCommand = new RelayCommand(_ => SelectFolder(path => BinFolderPath = path));
            SelectCheckLaterFolderCommand = new RelayCommand(_ => SelectFolder(path => CheckLaterFolderPath = path));
            SaveCommand = new RelayCommand(_ => SaveSettings());
            SaveFolderSetCommand = new RelayCommand(_ => SaveFolderSet(), _ => IsFolderSetNameValid);
            RemoveFolderSetCommand = new RelayCommand(_ => RemoveFolderSet(), _ => SelectedFolderSet != null);

            // Load folder sets from config if available
            LoadFolderSets();
            
            // Load settings from config if available
            LoadConfigSettings();
        }        private void LoadFolderSets()
        {
            try
            {
                if (File.Exists(_folderSetsConfigPath))
                {
                    var json = File.ReadAllText(_folderSetsConfigPath);
                    var folderSets = JsonSerializer.Deserialize<ObservableCollection<FolderSet>>(json);
                    if (folderSets != null)
                    {
                        FolderSets = folderSets;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading folder sets: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveFolderSets()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(FolderSets, options);
                File.WriteAllText(_folderSetsConfigPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving folder sets: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }        private void LoadConfigSettings()
        {
            // With config.txt removed, we initialize with empty paths
            // Paths will only be populated when a folder set is selected
            LibraryPath = string.Empty;
            KeepFolderPath = string.Empty;
            BinFolderPath = string.Empty;
            CheckLaterFolderPath = string.Empty;
            CurrentFolderSetName = string.Empty;
            
            // No automatic selection of folder sets at startup
            // User must explicitly select a folder set
        }

        private void RemoveFolderSet()
        {
            if (SelectedFolderSet == null)
                return;

            var result = MessageBox.Show(
                $"Are you sure you want to remove the folder set '{SelectedFolderSet.Name}'?", 
                "Confirm Removal", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                FolderSets.Remove(SelectedFolderSet);
                SaveFolderSets();
                SelectedFolderSet = null;
                CurrentFolderSetName = string.Empty;
            }
        }        private void SaveFolderSet()
        {
            if (string.IsNullOrWhiteSpace(NewFolderSetName))
                return;

            // Check if a folder set with this name already exists
            var existingFolderSet = FolderSets.FirstOrDefault(fs => 
                fs.Name.Equals(NewFolderSetName, StringComparison.OrdinalIgnoreCase));
            
            // If a folder set with this name exists, update it; otherwise create a new one
            var folderSet = existingFolderSet ?? new FolderSet();
            
            // Update the folder set properties
            folderSet.Name = NewFolderSetName; // Keep the case the user entered
            folderSet.LibraryPath = LibraryPath;
            folderSet.KeepFolderPath = KeepFolderPath;
            folderSet.BinFolderPath = BinFolderPath;
            folderSet.CheckLaterFolderPath = CheckLaterFolderPath;

            // Add to collection if it's a new folder set
            if (existingFolderSet == null)
            {
                FolderSets.Add(folderSet);
            }
            
            // Save to file
            SaveFolderSets();
            
            // Select the folder set
            SelectedFolderSet = folderSet;
            CurrentFolderSetName = folderSet.Name;
            
            // Update the main application with the new folder set
            UpdateMainViewModelWithCurrentFolderSet();
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
        }        private void SaveSettings()
        {
            try
            {
                // No longer saving to config.txt - the folder paths are only saved as part of folder sets
                // and stored in foldersets.json
                
                // Update the MainViewModel if accessible
                var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (mainWindow?.DataContext is MainViewModel mainViewModel)
                {
                    // First, update all the non-Library paths which don't trigger reloads
                    mainViewModel.KeepFolderPath = KeepFolderPath;
                    mainViewModel.BinFolderPath = BinFolderPath;
                    mainViewModel.CheckLaterFolderPath = CheckLaterFolderPath;
                    
                    // Set CurrentFolderSetName to update the display in main window
                    mainViewModel.CurrentFolderSetName = CurrentFolderSetName;
                    
                    // Check if library path changed
                    bool libraryPathChanged = mainViewModel.LibraryPath != LibraryPath;
                    
                    // Set the library path, which will trigger image reload if changed
                    if (libraryPathChanged)
                    {
                        mainViewModel.LibraryPath = LibraryPath;  // This will automatically reload images
                    }
                    else
                    {
                        // Force a reload of images even if the path didn't change
                        // This ensures the images are updated when switching folder sets
                        mainViewModel.ForceReloadImages();
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
        }

        /// <summary>
        /// Updates the MainViewModel with the currently selected folder set
        /// to ensure changes are immediately applied to the main application
        /// </summary>
        private void UpdateMainViewModelWithCurrentFolderSet()
        {
            // Find the main window and its view model
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow?.DataContext is MainViewModel mainViewModel)
            {
                // Update the folder set name first to update UI display
                mainViewModel.CurrentFolderSetName = CurrentFolderSetName;
                
                // Update paths except library path
                mainViewModel.KeepFolderPath = KeepFolderPath;
                mainViewModel.BinFolderPath = BinFolderPath;
                mainViewModel.CheckLaterFolderPath = CheckLaterFolderPath;
                
                // Check if library path has changed
                bool libraryPathChanged = mainViewModel.LibraryPath != LibraryPath;
                
                // Set library path last (this will automatically reload images if the path changed)
                if (libraryPathChanged)
                {
                    mainViewModel.LibraryPath = LibraryPath;
                }
                else
                {
                    // Force reload even if path is the same
                    mainViewModel.ForceReloadImages();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
