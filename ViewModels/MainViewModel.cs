using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AfbeeldingenUitzoeken.Models;
using AfbeeldingenUitzoeken.Views; // Add reference to Views namespace
// Explicitly use Windows MessageBox and Application to avoid ambiguity
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Application;

namespace AfbeeldingenUitzoeken.ViewModels
{    public class MainViewModel : INotifyPropertyChanged
    {
        private PictureModel? _currentPicture;
        private BitmapImage? _currentImage;
        private string _libraryPath = string.Empty;
        private string _keepFolderPath = string.Empty;
        private string _binFolderPath = string.Empty;
        private string _checkLaterFolderPath = string.Empty;
        private int _currentIndex = 0;
        private bool _canGoNext = false;
        private bool _canGoPrevious = false;

        public ObservableCollection<PictureModel> PicturesQueue { get; set; } = new ObservableCollection<PictureModel>();
        
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
        }        public bool CanGoPrevious
        {
            get => _canGoPrevious;
            private set
            {
                _canGoPrevious = value;
                OnPropertyChanged();
            }
        }

        public PictureModel? CurrentPicture
        {
            get => _currentPicture;
            set
            {
                _currentPicture = value;
                OnPropertyChanged();
                if (value != null && File.Exists(value.FilePath))
                {
                    try 
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.UriSource = new Uri(value.FilePath);
                        bitmap.EndInit();
                        bitmap.Freeze(); // Optimize for UI thread
                        CurrentImage = bitmap;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        public BitmapImage? CurrentImage
        {
            get => _currentImage;
            set
            {
                _currentImage = value;
                OnPropertyChanged();
            }
        }        public string LibraryPath
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
            {                _binFolderPath = value;
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

            LoadConfigSettings();
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
        }        private void ShowSettingsIfInvalidPaths()
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
        
        private void UpdateImageCounts(List<string> imageFiles)
        {
            // Reset both counts to ensure percentage starts at 0% for a new folder
            _initialImageCount = imageFiles.Count;
            TotalImageCount = imageFiles.Count;
            
            // Ensure progress percentage is updated
            OnPropertyChanged(nameof(RemainingImageCount));
            OnPropertyChanged(nameof(ProcessedPercentage));
        }
        
        public void RefreshImageCounts()
        {
            if (string.IsNullOrEmpty(LibraryPath) || !Directory.Exists(LibraryPath))
                return;
                
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
            var imageFiles = Directory.GetFiles(LibraryPath, "*.*", SearchOption.AllDirectories)
                .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLower()))
                .ToList();
                
            UpdateImageCounts(imageFiles);
        }
        
        private void LoadImagesFromLibrary()
        {
            if (string.IsNullOrEmpty(LibraryPath) || !Directory.Exists(LibraryPath))
                return;

            PicturesQueue.Clear();
            
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
            var imageFiles = Directory.GetFiles(LibraryPath, "*.*", SearchOption.AllDirectories)
                .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLower()))
                .ToList();
                
            // Update the counters for the new image library
            UpdateImageCounts(imageFiles);
            
            // Ensure progress percentage is updated
            OnPropertyChanged(nameof(RemainingImageCount));
            OnPropertyChanged(nameof(ProcessedPercentage));

            foreach (var file in imageFiles)
            {
                try
                {
                    // Create thumbnail for gallery
                    var thumbnail = new BitmapImage();
                    thumbnail.BeginInit();
                    thumbnail.CacheOption = BitmapCacheOption.OnLoad;
                    thumbnail.UriSource = new Uri(file);
                    thumbnail.DecodePixelWidth = 150; // Constrain width for thumbnails
                    thumbnail.EndInit();
                    thumbnail.Freeze(); // Optimize for UI thread
                    
                    // Get file information
                    var fileInfo = new FileInfo(file);
                    
                    // Get image resolution by loading the full image
                    var fullImage = new BitmapImage();
                    fullImage.BeginInit();
                    fullImage.CacheOption = BitmapCacheOption.OnLoad;
                    fullImage.UriSource = new Uri(file);
                    fullImage.EndInit();
                    
                    PicturesQueue.Add(new PictureModel
                    {
                        FilePath = file,
                        FileName = Path.GetFileName(file),
                        Thumbnail = thumbnail,
                        CreationDate = fileInfo.CreationTime,
                        FileSize = fileInfo.Length,
                        Width = fullImage.PixelWidth,
                        Height = fullImage.PixelHeight
                    });                }
                catch (Exception ex)
                {
                    // Skip files that can't be loaded as images
                    MessageBox.Show($"Error loading image {file}: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            
            if (PicturesQueue.Count > 0)
            {
                CurrentIndex = 0;
                CurrentPicture = PicturesQueue[0];
                UpdateNavigationState();
            }
            else
            {
                // If no images found, clear the current image display
                CurrentPicture = null;
                CurrentImage = null;
                CurrentIndex = 0;
                UpdateNavigationState();
            }
        }

        private void KeepPicture()
        {
            if (CurrentPicture != null && !string.IsNullOrEmpty(KeepFolderPath))
            {
                MovePicture(CurrentPicture, KeepFolderPath);
            }
        }        private void ThrowAwayPicture()
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

            try
            {                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }

                // Ensure filename is not null
                string fileName = picture.FileName ?? Path.GetFileName(picture.FilePath ?? string.Empty);
                string filePath = picture.FilePath ?? string.Empty;
                
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) 
                {
                    MessageBox.Show("Cannot move file: Source file path is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                string destinationPath = Path.Combine(destinationFolder, fileName);
                File.Copy(filePath, destinationPath, true);
                File.Delete(filePath);
                
                int currentIdx = PicturesQueue.IndexOf(picture);
                PicturesQueue.Remove(picture);
                
                // Notify that remaining count has changed
                OnPropertyChanged(nameof(RemainingImageCount));
                OnPropertyChanged(nameof(ProcessedPercentage));
                
                // If we removed the current picture, we need to adjust CurrentIndex
                if (PicturesQueue.Count > 0)
                {
                    // If we removed the last picture in the queue, go to the new last picture
                    if (currentIdx >= PicturesQueue.Count)
                    {
                        CurrentIndex = PicturesQueue.Count - 1;
                    }
                    else
                    {
                        // Stay at the same index (which is now the next picture)
                        CurrentIndex = currentIdx;
                    }
                    
                    CurrentPicture = PicturesQueue[CurrentIndex];
                }                else
                {
                    // If no pictures left, reset everything
                    CurrentPicture = null;
                    CurrentImage = null;
                    CurrentIndex = 0;
                    
                    // If there are no more pictures in the queue, prompt to empty the bin folder
                    if (destinationFolder == BinFolderPath)
                    {
                        // Don't prompt immediately since we might be in the process of finishing up
                        // Wait a moment to ensure the UI has updated
                        System.Threading.Tasks.Task.Delay(100).ContinueWith(_ => 
                        {
                            Application.Current.Dispatcher.Invoke(() => CheckAndPromptToEmptyBinFolder());
                        });
                    }
                    else
                    {
                        // Check if any images were moved to the bin folder during this session
                        if (Directory.Exists(BinFolderPath) && 
                            Directory.GetFiles(BinFolderPath, "*.*").Any(file => 
                                new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" }
                                    .Contains(Path.GetExtension(file).ToLower())))
                        {
                            System.Threading.Tasks.Task.Delay(100).ContinueWith(_ => 
                            {
                                Application.Current.Dispatcher.Invoke(() => CheckAndPromptToEmptyBinFolder());
                            });
                        }
                    }
                }
                
                // Update navigation button states
                UpdateNavigationState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error moving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool CheckAndPromptToEmptyBinFolder()
        {
            // Check if bin folder exists and has files
            if (!string.IsNullOrEmpty(BinFolderPath) && Directory.Exists(BinFolderPath))
            {
                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
                var imageFiles = Directory.GetFiles(BinFolderPath, "*.*", SearchOption.AllDirectories)
                    .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLower()))
                    .ToList();

                if (imageFiles.Count > 0)
                {
                    // Ask the user if they want to empty the bin folder
                    var result = MessageBox.Show(
                        $"There are {imageFiles.Count} images in the Throw away folder. Would you like to delete them?",
                        "Empty Throw Away Folder",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            foreach (var file in imageFiles)
                            {
                                File.Delete(file);
                            }
                            MessageBox.Show(
                                "Throw away folder has been emptied successfully.",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                $"Error emptying Throw away folder: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }
            }
            return false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
