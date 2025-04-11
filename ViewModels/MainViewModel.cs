using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AfbeeldingenUitzoeken.Models;
using AfbeeldingenUitzoeken.Views;
using AfbeeldingenUitzoeken.Helpers;
// Explicitly use Windows MessageBox and Application to avoid ambiguity
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Application;

namespace AfbeeldingenUitzoeken.ViewModels
{
    public class MainViewModel : BaseMediaViewModel
    {
        private string _libraryPath = string.Empty;
        private string _keepFolderPath = string.Empty;
        private string _binFolderPath = string.Empty;
        private string _checkLaterFolderPath = string.Empty;
        private int _currentIndex = 0;
        private bool _canGoNext = false;
        private bool _canGoPrevious = false;
        private readonly BackgroundImageLoader _backgroundLoader;

        public ObservableCollection<PictureModel> PicturesQueue { get; set; } = new ObservableCollection<PictureModel>();
        
        // Loading indicator properties
        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                if (_currentIndex != value)
                {
                    _currentIndex = value;
                    OnPropertyChanged();
                    UpdateNavigationState();
                }
            }
        }
        
        public bool CanGoNext
        {
            get => _canGoNext;
            private set
            {
                _canGoNext = value;
                OnPropertyChanged();
            }
        }

        public bool CanGoPrevious
        {
            get => _canGoPrevious;
            private set
            {
                _canGoPrevious = value;
                OnPropertyChanged();
            }
        }

        public string LibraryPath
        {
            get => _libraryPath;
            set
            {
                bool pathChanged = _libraryPath != value;
                _libraryPath = value;
                OnPropertyChanged();
                
                if (pathChanged)
                {
                    LoadImagesFromLibrary();
                }
            }
        }

        public string KeepFolderPath
        {
            get => _keepFolderPath;
            set
            {
                _keepFolderPath = value;
                OnPropertyChanged();
            }
        }

        public string BinFolderPath
        {
            get => _binFolderPath;
            set
            {
                _binFolderPath = value;
                OnPropertyChanged();
            }
        }

        public string CheckLaterFolderPath
        {
            get => _checkLaterFolderPath;
            set
            {
                _checkLaterFolderPath = value;
                OnPropertyChanged();
            }
        }

        public ICommand KeepCommand { get; }
        public ICommand ThrowAwayCommand { get; }
        public ICommand CheckLaterCommand { get; }
        public ICommand NextPictureCommand { get; }
        public ICommand PreviousPictureCommand { get; }
        public ICommand SelectImageCommand { get; }

        public MainViewModel()
        {
            PicturesQueue = new ObservableCollection<PictureModel>();

            KeepCommand = new RelayCommand(_ => KeepPicture());
            ThrowAwayCommand = new RelayCommand(_ => ThrowAwayPicture());
            CheckLaterCommand = new RelayCommand(_ => CheckLaterPicture());
            NextPictureCommand = new RelayCommand(_ => NextPicture(), _ => CanGoNext);
            PreviousPictureCommand = new RelayCommand(_ => PreviousPicture(), _ => CanGoPrevious);
            SelectImageCommand = new RelayCommand(param => SelectImage(param as PictureModel));

            // Initialize the background image loader with event handling
            _backgroundLoader = new BackgroundImageLoader();
            _backgroundLoader.ImageLoaded += OnImageLoaded;

            LoadConfigSettings();
        }
        
        // Event handler for when images are loaded in the background
        private void OnImageLoaded(PictureModel picture)
        {
            // Since this event comes from a background thread, we need to use the Dispatcher
            // to safely update the UI collection
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (picture != null)
                {
                    PicturesQueue.Add(picture);
                    
                    // Update counters
                    OnPropertyChanged(nameof(RemainingImageCount));
                    OnPropertyChanged(nameof(ProcessedPercentage));
                    
                    // If this is the first image and we don't have a current picture yet,
                    // set it as the current picture
                    if (PicturesQueue.Count == 1 && CurrentPicture == null)
                    {
                        CurrentIndex = 0;
                        CurrentPicture = picture;
                        UpdateNavigationState();
                    }
                }
                
                // If we've loaded all expected images, turn off the loading indicator
                if (PicturesQueue.Count >= _totalImageCount)
                {
                    IsLoading = false;
                }
            });
        }
        
        private void SelectImage(PictureModel? picture)
        {
            if (picture != null)
            {
                int index = PicturesQueue.IndexOf(picture);
                if (index >= 0)
                {
                    CurrentIndex = index;
                    CurrentPicture = picture;
                    UpdateNavigationState();
                }
            }
        }
        
        private void NextPicture()
        {
            if (CurrentIndex < PicturesQueue.Count - 1)
            {
                CurrentIndex++;
                CurrentPicture = PicturesQueue[CurrentIndex];
            }
        }
        
        private void PreviousPicture()
        {
            if (CurrentIndex > 0)
            {
                CurrentIndex--;
                CurrentPicture = PicturesQueue[CurrentIndex];
            }
        }
        
        private void UpdateNavigationState()
        {
            CanGoNext = PicturesQueue.Count > 0 && CurrentIndex < PicturesQueue.Count - 1;
            CanGoPrevious = PicturesQueue.Count > 0 && CurrentIndex > 0;
        }

        public bool HasInvalidPaths()
        {
            bool hasInvalid = false;
            
            if (!string.IsNullOrEmpty(LibraryPath) && !Directory.Exists(LibraryPath))
                hasInvalid = true;
                
            if (!string.IsNullOrEmpty(KeepFolderPath) && !Directory.Exists(KeepFolderPath))
                hasInvalid = true;
                
            if (!string.IsNullOrEmpty(BinFolderPath) && !Directory.Exists(BinFolderPath))
                hasInvalid = true;
                
            if (!string.IsNullOrEmpty(CheckLaterFolderPath) && !Directory.Exists(CheckLaterFolderPath))
                hasInvalid = true;

            return hasInvalid;
        }
        
        private void ShowSettingsIfInvalidPaths()
        {
            if (HasInvalidPaths())
            {
                var settingsWindow = new SettingsView();
                // Only set Owner if MainWindow is already loaded
                if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsLoaded)
                {
                    settingsWindow.Owner = Application.Current.MainWindow;
                }
                settingsWindow.ShowDialog();
            }
        }

        public void LoadConfigSettings()
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

                ShowSettingsIfInvalidPaths();
            }
            else
            {
                ShowSettingsIfInvalidPaths();
            }
        }

        private int _totalImageCount;
        private int _initialImageCount;
        
        public int TotalImageCount
        {
            get => _totalImageCount;
            set
            {
                if (_totalImageCount != value)
                {
                    _totalImageCount = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ProcessedPercentage));
                }
            }
        }
        
        public int RemainingImageCount => PicturesQueue?.Count ?? 0;
        
        public string ProcessedPercentage
        {
            get
            {
                if (_initialImageCount == 0) return "0%";
                
                // When the queue is empty but we're loading new images, return 0%
                if (RemainingImageCount == 0 && RemainingImageCount < _initialImageCount)
                {
                    // If queue is empty but we have a non-zero initial count,
                    // we're either at 100% completion or just loading a new folder
                    // Check if we've actually processed any images
                    return _initialImageCount == _totalImageCount ? "0%" : "100%";
                }
                
                int processed = _initialImageCount - RemainingImageCount;
                double percentage = Math.Round((double)processed / _initialImageCount * 100, 1);
                return $"{percentage}%";
            }
        }
        
        private void UpdateImageCounts(List<string> mediaFiles)
        {
            // Reset both counts to ensure percentage starts at 0% for a new folder
            _initialImageCount = mediaFiles.Count;
            TotalImageCount = mediaFiles.Count;
            
            // Ensure progress percentage is updated
            OnPropertyChanged(nameof(RemainingImageCount));
            OnPropertyChanged(nameof(ProcessedPercentage));
        }
        
        public void RefreshImageCounts()
        {
            if (string.IsNullOrEmpty(LibraryPath) || !Directory.Exists(LibraryPath))
                return;
                
            // Get all supported media files using our MediaExtensions helper
            var mediaFiles = Directory.GetFiles(LibraryPath, "*.*", SearchOption.AllDirectories)
                .Where(file => MediaExtensions.IsSupportedMedia(file))
                .ToList();
                
            UpdateImageCounts(mediaFiles);
        }
        
        private void LoadImagesFromLibrary()
        {
            if (string.IsNullOrEmpty(LibraryPath) || !Directory.Exists(LibraryPath))
                return;

            // Stop any previous loading process
            _backgroundLoader.Stop();
            
            // Clear existing pictures
            PicturesQueue.Clear();
            
            // Set loading indicator
            IsLoading = true;
            
            // Get all supported media files using our MediaExtensions helper
            var mediaFiles = Directory.GetFiles(LibraryPath, "*.*", SearchOption.AllDirectories)
                .Where(file => MediaExtensions.IsSupportedMedia(file))
                .ToList();
                
            // Update the counters for the new media library
            UpdateImageCounts(mediaFiles);
            
            // Ensure progress percentage is updated
            OnPropertyChanged(nameof(RemainingImageCount));
            OnPropertyChanged(nameof(ProcessedPercentage));

            // Queue all files for background loading
            _backgroundLoader.QueueFiles(mediaFiles);
        }

        private void KeepPicture()
        {
            if (CurrentPicture != null && !string.IsNullOrEmpty(KeepFolderPath))
            {
                MovePicture(CurrentPicture, KeepFolderPath);
            }
        }
        
        private void ThrowAwayPicture()
        {
            if (CurrentPicture != null && !string.IsNullOrEmpty(BinFolderPath))
            {
                MovePicture(CurrentPicture, BinFolderPath);
            }
        }
        
        private void CheckLaterPicture()
        {
            if (CurrentPicture != null && !string.IsNullOrEmpty(CheckLaterFolderPath))
            {
                MovePicture(CurrentPicture, CheckLaterFolderPath);
            }
        }
          private void MovePicture(PictureModel picture, string destinationFolder)
        {
            if (picture == null || string.IsNullOrEmpty(destinationFolder)) return;
            if (string.IsNullOrEmpty(picture.FilePath) || !File.Exists(picture.FilePath)) 
            {
                MessageBox.Show("The file does not exist or the path is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }

                // Ensure filename is not null
                string fileName = picture.FileName ?? Path.GetFileName(picture.FilePath);
                if (string.IsNullOrEmpty(fileName))
                {
                    MessageBox.Show("Invalid file name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string destinationPath = Path.Combine(destinationFolder, fileName);
                int counter = 1;

                // If file already exists, add a number to the filename to make it unique
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                string extension = Path.GetExtension(fileName);
                
                while (File.Exists(destinationPath))
                {
                    fileName = $"{fileNameWithoutExtension}_{counter}{extension}";
                    destinationPath = Path.Combine(destinationFolder, fileName);
                    counter++;
                }

                // Move the file
                File.Move(picture.FilePath, destinationPath);
                
                // Save current index before removing the picture
                int currentIndexBeforeRemoval = CurrentIndex;
                
                // Remove the picture from the queue
                PicturesQueue.Remove(picture);
                
                // Update remaining count display
                OnPropertyChanged(nameof(RemainingImageCount));
                OnPropertyChanged(nameof(ProcessedPercentage));

                // If there are more images, show the next one
                if (PicturesQueue.Count > 0)
                {
                    // If we removed the last image, move to the previous one
                    // If we removed an item before the current index, we need to adjust the index
                    if (currentIndexBeforeRemoval >= PicturesQueue.Count)
                    {
                        CurrentIndex = PicturesQueue.Count - 1;
                    }
                    
                    CurrentPicture = PicturesQueue[CurrentIndex];
                }
                else
                {
                    // If no more images, clear the display
                    CurrentPicture = null;
                    CurrentImage = null;
                    // Reset video state
                    IsVideoPlaying = false;
                    IsCurrentItemVideo = false;
                }
                
                UpdateNavigationState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error moving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        public new event PropertyChangedEventHandler? PropertyChanged;
        
        protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            base.OnPropertyChanged(propertyName);
        }
        
        public void CheckAndPromptToEmptyBinFolder()
        {
            if (!string.IsNullOrEmpty(BinFolderPath) && Directory.Exists(BinFolderPath))
            {
                // Find all media files in the bin folder using our MediaExtensions helper
                var mediaFiles = Directory.GetFiles(BinFolderPath, "*.*", SearchOption.AllDirectories)
                    .Where(file => MediaExtensions.IsSupportedMedia(file))
                    .ToList();
                
                if (mediaFiles.Count > 0)
                {
                    var result = MessageBox.Show(
                        $"There are {mediaFiles.Count} media files in the bin folder. Would you like to delete them permanently?",
                        "Empty Bin Folder",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        // Delete all files
                        foreach (var file in mediaFiles)
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error deleting file {file}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            
            // Clear the thumbnail cache when application is closing to free memory
            ImageLoader.ClearCache();
        }
    }
}
