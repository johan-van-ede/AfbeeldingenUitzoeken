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
{    public class MainViewModel : BaseMediaViewModel
    {
        private string _libraryPath = string.Empty;
        private string _keepFolderPath = string.Empty;
        private string _binFolderPath = string.Empty;
        private string _checkLaterFolderPath = string.Empty;
        private string _currentFolderSetName = string.Empty;
        private int _currentIndex = 0;
        private bool _canGoNext = false;
        private bool _canGoPrevious = false;
        private readonly BackgroundImageLoader _backgroundLoader;
        private ObservableCollection<Models.FolderSet> _availableFolderSets = new ObservableCollection<Models.FolderSet>();
        private Models.FolderSet? _selectedFolderSet;

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

        private int _imagesLoadedCount = 0;
        public int ImagesLoadedCount
        {
            get => _imagesLoadedCount;
            set
            {
                if (_imagesLoadedCount != value)
                {
                    _imagesLoadedCount = value;
                    OnPropertyChanged(nameof(ImagesLoadedCount));
                }
            }
        }

        private string _loadingProgressText = string.Empty;
        public string LoadingProgressText
        {
            get => _loadingProgressText;
            set
            {
                if (_loadingProgressText != value)
                {
                    _loadingProgressText = value;
                    OnPropertyChanged(nameof(LoadingProgressText));
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

        public string CurrentFolderSetName
        {
            get => _currentFolderSetName;
            set
            {
                if (_currentFolderSetName != value)
                {
                    _currentFolderSetName = value;
                    OnPropertyChanged();

                    // When folder set changes, we should update the UI to clearly show which set is active
                    OnPropertyChanged(nameof(FolderSetDisplay));
                }
            }
        }

        // Property for displaying folder set information in the main window
        public string FolderSetDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(CurrentFolderSetName))
                    return "No folder set selected";

                return $"Folder Set: {CurrentFolderSetName}";
            }
        }
        public ObservableCollection<Models.FolderSet> AvailableFolderSets
        {
            get => _availableFolderSets;
            set
            {
                _availableFolderSets = value;
                OnPropertyChanged();
            }
        }

        public Models.FolderSet? SelectedFolderSet
        {
            get => _selectedFolderSet;
            set
            {
                if (_selectedFolderSet != value)
                {
                    _selectedFolderSet = value;
                    OnPropertyChanged();

                    if (value != null)
                    {
                        // Update paths from the selected folder set
                        LibraryPath = value.LibraryPath;
                        KeepFolderPath = value.KeepFolderPath;
                        BinFolderPath = value.BinFolderPath;
                        CheckLaterFolderPath = value.CheckLaterFolderPath;
                        CurrentFolderSetName = value.Name;

                        // Force reload of images
                        ForceReloadImages();
                    }
                }
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

            NextPictureCommand = new RelayCommand(_ => NextPicture());
            PreviousPictureCommand = new RelayCommand(_ => PreviousPicture());
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
                    // Check if this picture already exists in the queue (prevent duplicates)
                    bool isDuplicate = PicturesQueue.Any(p => p.FilePath == picture.FilePath);

                    if (!isDuplicate)
                    {
                        PicturesQueue.Add(picture);
                        ImagesLoadedCount = PicturesQueue.Count;
                        LoadingProgressText = $"Loaded {PicturesQueue.Count} of {TotalImageCount} images...";
                        // Update counters
                        OnPropertyChanged(nameof(RemainingImageCount));
                        OnPropertyChanged(nameof(ProcessedPercentage));
                        OnPropertyChanged(nameof(LoadingProgressText));
                        // If this is the first image and we don't have a current picture yet,
                        // set it as the current picture
                        if (PicturesQueue.Count == 1 && CurrentPicture == null)
                        {
                            CurrentIndex = 0;
                            CurrentPicture = picture;
                            UpdateNavigationState();
                        }
                        else
                        {
                            // Make sure navigation state is updated after each image is added
                            UpdateNavigationState();
                        }
                    }
                    else
                    {
                        // If we found a duplicate, we should still count it as "processed"
                        // so our progress percentage calculation remains accurate
                        System.Diagnostics.Debug.WriteLine($"Skipped duplicate file: {picture.FilePath}");
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
                    SetCurrentPictureWithFullImage(picture);
                    UpdateNavigationState();
                }
            }
        }
        private void NextPicture()
        {
            // Use the centralized navigation method to move forward
            NavigateTo(true);
        }

        private void PreviousPicture()
        {
            // Use the centralized navigation method to move backward
            NavigateTo(false);
        }

        /// <summary>
        /// Centralized navigation method that ensures all types of navigation are synchronized
        /// </summary>
        /// <param name="moveForward">True to move to next image, False to move to previous image</param>
        /// <param name="forceUpdate">If true, update the current image even if the index hasn't changed (for direct selection)</param>
        public void NavigateTo(bool moveForward, bool forceUpdate = false)
        {
            // Update navigation state first to ensure can-navigate flags are current
            UpdateNavigationState();

            if (moveForward && CanGoNext && CurrentIndex < PicturesQueue.Count - 1)
            {
                CurrentIndex++;
                SetCurrentPictureWithFullImage(PicturesQueue[CurrentIndex]);
                if (CurrentPicture != null)
                    OnItemNavigated?.Invoke(CurrentPicture);
                // Debug
                System.Diagnostics.Debug.WriteLine($"NavigateTo(next) executed: CurrentIndex={CurrentIndex}, Picture={CurrentPicture?.FileName}");
            }
            else if (!moveForward && CanGoPrevious && CurrentIndex > 0)
            {
                CurrentIndex--;
                SetCurrentPictureWithFullImage(PicturesQueue[CurrentIndex]);
                if (CurrentPicture != null)
                    OnItemNavigated?.Invoke(CurrentPicture);
                // Debug
                System.Diagnostics.Debug.WriteLine($"NavigateTo(previous) executed: CurrentIndex={CurrentIndex}, Picture={CurrentPicture?.FileName}");
            }
            // Handle the case where we want to force an update for the current index (e.g. direct gallery selection)
            else if (forceUpdate && CurrentIndex >= 0 && CurrentIndex < PicturesQueue.Count)
            {
                SetCurrentPictureWithFullImage(PicturesQueue[CurrentIndex]);
                if (CurrentPicture != null)
                    OnItemNavigated?.Invoke(CurrentPicture);
                // Debug
                System.Diagnostics.Debug.WriteLine($"NavigateTo(force) executed: CurrentIndex={CurrentIndex}, Picture={CurrentPicture?.FileName}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"NavigateTo skipped: CanGoNext={CanGoNext}, CanGoPrevious={CanGoPrevious}, CurrentIndex={CurrentIndex}, Count={PicturesQueue.Count}, ForceUpdate={forceUpdate}");
            }

            // Always update navigation state after navigation
            UpdateNavigationState();
        }

        // Buffer size: number of images before/after the current to keep in memory
        private const int ImageBufferSize = 2;

        // Helper to ensure full image and thumbnail are loaded for the main view and buffer
        public void SetCurrentPictureWithFullImage(PictureModel picture)
        {
            CurrentPicture = picture;
            int current = PicturesQueue.IndexOf(picture);
            // Buffer window: [current-ImageBufferSize, current+ImageBufferSize]
            for (int i = 0; i < PicturesQueue.Count; i++)
            {
                var pic = PicturesQueue[i];
                if (Math.Abs(i - current) <= ImageBufferSize)
                {
                    // Load image if not loaded and not a video
                    if (pic.Image == null && !pic.IsVideo && !string.IsNullOrEmpty(pic.FilePath))
                    {
                        pic.Image = AfbeeldingenUitzoeken.Helpers.ImageLoader.LoadImage(pic.FilePath);
                    }
                    // Load thumbnail if not loaded
                    if (pic.Thumbnail == null && !string.IsNullOrEmpty(pic.FilePath))
                    {
                        if (pic.IsVideo)
                            pic.Thumbnail = AfbeeldingenUitzoeken.Helpers.ImageLoader.LoadVideoThumbnail(pic.FilePath, 150);
                        else
                            pic.Thumbnail = AfbeeldingenUitzoeken.Helpers.ImageLoader.LoadImage(pic.FilePath, 150, 0, true);
                    }
                }
                else
                {
                    // Unload image to free memory
                    if (pic.Image != null)
                    {
                        pic.Image = null;
                    }
                    // Optionally unload thumbnail if you want to save even more memory
                    // pic.Thumbnail = null;
                }
            }
            // Ensure current picture is loaded
            if (picture.Image == null && !picture.IsVideo && !string.IsNullOrEmpty(picture.FilePath))
            {
                picture.Image = AfbeeldingenUitzoeken.Helpers.ImageLoader.LoadImage(picture.FilePath);
                // Set resolution from full image when loaded and notify UI
                if (picture.Image != null)
                {
                    picture.Width = picture.Image.PixelWidth;
                    picture.Height = picture.Image.PixelHeight;
                }
            }
            // Ensure current thumbnail is loaded
            if (picture.Thumbnail == null && !string.IsNullOrEmpty(picture.FilePath))
            {
                if (picture.IsVideo)
                    picture.Thumbnail = AfbeeldingenUitzoeken.Helpers.ImageLoader.LoadVideoThumbnail(picture.FilePath, 150);
                else
                    picture.Thumbnail = AfbeeldingenUitzoeken.Helpers.ImageLoader.LoadImage(picture.FilePath, 150, 0, true);
            }
            OnPropertyChanged(nameof(CurrentPicture));
        }

        // Event to notify the view when navigation occurs
        public event Action<PictureModel>? OnItemNavigated;

        /// <summary>
        /// Updates the navigation state based on the current index and queue size.
        /// This method is called whenever the current index changes.
        /// </summary>

        public void UpdateNavigationState()
        {
            // Fixed implementation to make sure the navigation buttons work correctly
            bool newCanGoNext = PicturesQueue.Count > 0 && CurrentIndex < PicturesQueue.Count - 1;
            bool newCanGoPrevious = PicturesQueue.Count > 0 && CurrentIndex > 0;

            // Only update if values actually changed to avoid unnecessary property change events
            if (CanGoNext != newCanGoNext)
                CanGoNext = newCanGoNext;

            if (CanGoPrevious != newCanGoPrevious)
                CanGoPrevious = newCanGoPrevious;

            // Force command manager to re-evaluate all commands
            CommandManager.InvalidateRequerySuggested();

            // Debug output to troubleshoot navigation buttons
            System.Diagnostics.Debug.WriteLine($"Navigation state updated: Queue items={PicturesQueue.Count}, CurrentIndex={CurrentIndex}, CanGoNext={CanGoNext}, CanGoPrevious={CanGoPrevious}");
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
            // Reset all paths since no folder set is selected at startup
            LibraryPath = string.Empty;
            KeepFolderPath = string.Empty;
            BinFolderPath = string.Empty;
            CheckLaterFolderPath = string.Empty;
            CurrentFolderSetName = string.Empty;

            // Try to load the last selected folder set from foldersets.json
            LoadLastSelectedFolderSet();

            // Show settings if no valid paths are configured
            ShowSettingsIfInvalidPaths();
        }
        private void LoadLastSelectedFolderSet()
        {
            try
            {
                string folderSetsConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "foldersets.json");
                if (File.Exists(folderSetsConfigPath))
                {
                    var json = File.ReadAllText(folderSetsConfigPath);
                    var folderSets = System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<Models.FolderSet>>(json);

                    if (folderSets != null && folderSets.Count > 0)
                    {
                        // Populate the available folder sets
                        AvailableFolderSets = folderSets;

                        // Automatically select the first folder set at startup
                        // This ensures users don't need to manually select a folder set each time
                        SelectedFolderSet = folderSets.FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading folder sets: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    OnPropertyChanged(nameof(LoadingProgressText));
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

        /// <summary>
        /// Forces the application to reload images from the current library path.
        /// Used when a new folder set is selected but the path remains the same.
        /// </summary>
        public void ForceReloadImages()
        {
            if (!string.IsNullOrEmpty(LibraryPath) && Directory.Exists(LibraryPath))
            {
                // Reload images from the library path
                LoadImagesFromLibrary();
            }
        }

        private async void LoadImagesFromLibrary()
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

            // Create all PictureModels with empty thumbnails
            var pictureModels = new List<PictureModel>();
            for (int i = 0; i < mediaFiles.Count; i++)
            {
                var file = mediaFiles[i];
                var isVideo = MediaExtensions.IsVideo(file);
                var fileInfo = new FileInfo(file);
                PictureModel pic = new PictureModel
                {
                    FilePath = file,
                    FileName = Path.GetFileName(file),
                    Image = null, // Only load when in buffer
                    Thumbnail = null, // Will be loaded async
                    CreationDate = fileInfo.LastWriteTime,
                    FileSize = fileInfo.Length,
                    Width = 0,
                    Height = 0,
                    IsVideo = isVideo,
                    VideoDuration = isVideo ? TimeSpan.Zero : null
                };
                pictureModels.Add(pic);
            }
            // Order by date (oldest first)
            foreach (var pic in pictureModels.OrderBy(p => p.CreationDate))
            {
                PicturesQueue.Add(pic);
            }

            // Set first image as current
            if (PicturesQueue.Count > 0)
            {
                CurrentIndex = 0;
                SetCurrentPictureWithFullImage(PicturesQueue[0]);
            }

            // Load thumbnails in the background
            await System.Threading.Tasks.Task.Run(() =>
            {
                for (int i = 0; i < PicturesQueue.Count; i++)
                {
                    var pic = PicturesQueue[i];
                    if (pic.Thumbnail == null && !string.IsNullOrEmpty(pic.FilePath))
                    {
                        BitmapImage? thumb = null;
                        if (pic.IsVideo)
                            thumb = AfbeeldingenUitzoeken.Helpers.ImageLoader.LoadVideoThumbnail(pic.FilePath, 150);
                        else
                            thumb = AfbeeldingenUitzoeken.Helpers.ImageLoader.LoadImage(pic.FilePath, 150, 0, true);
                        if (thumb != null)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                pic.Thumbnail = thumb;
                                // Do not set resolution from thumbnail; only set from full image when loaded
                                // Notify UI of thumbnail update
                                var idx = PicturesQueue.IndexOf(pic);
                                if (idx >= 0)
                                    PicturesQueue[idx] = pic;
                            });
                        }
                    }
                }
            });

            IsLoading = false;
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

                    SetCurrentPictureWithFullImage(PicturesQueue[CurrentIndex]);
                }
                else
                {
                    // If no more images, clear the display
                    CurrentPicture = null;
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
            }              // Clear the thumbnail cache when application is closing to free memory
            ImageLoader.ClearCache();
        }

        // Move current picture to a specific subfolder in the keep folder
        public void MoveCurrentPictureTo(string subfolderPath)
        {
            if (CurrentPicture != null && !string.IsNullOrEmpty(subfolderPath))
            {
                MovePicture(CurrentPicture, subfolderPath);
            }
        }      
    }
}
