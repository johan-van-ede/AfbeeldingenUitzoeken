using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ListBox = System.Windows.Controls.ListBox;
using MediaElement = System.Windows.Controls.MediaElement;
using System.Windows.Controls;
using AfbeeldingenUitzoeken.ViewModels;
using AfbeeldingenUitzoeken.Views;
using AfbeeldingenUitzoeken.Models;

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
            var viewModel = new MainViewModel();
            DataContext = viewModel;
            
            // Subscribe to the navigation event to ensure selected item stays visible
            viewModel.OnItemNavigated += (item) => EnsureItemVisible(item);
            
            // Add keyboard handling at the window level to ensure it works everywhere
            this.KeyDown += MainWindow_KeyDown;
            
            // Give focus to the ListBox when the window loads
            this.Loaded += (s, e) => {
                var imagesListBox = FindVisualChild<ListBox>(this, "ImagesListBox");
                if (imagesListBox != null) {
                    imagesListBox.Focus();
                }
            };
        }
        
        // Button handlers for navigation
        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel && viewModel.CanGoPrevious)
            {
                viewModel.NavigateTo(moveForward: false);
                System.Diagnostics.Debug.WriteLine("Previous button clicked - Using ViewModel.NavigateTo");
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel && viewModel.CanGoNext)
            {
                viewModel.NavigateTo(moveForward: true);
                System.Diagnostics.Debug.WriteLine("Next button clicked - Using ViewModel.NavigateTo");
            }
        }

        // Show popup for keep subfolder selection
        private void KeepButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel && !string.IsNullOrEmpty(viewModel.KeepFolderPath))
            {
                var subfolderVM = new ViewModels.KeepSubfolderViewModel(viewModel.KeepFolderPath);
                var popup = new Views.KeepSubfolderPopup(subfolderVM.Subfolders);
                popup.Owner = this;
                if (popup.ShowDialog() == true && !string.IsNullOrEmpty(popup.SelectedSubfolder))
                {
                    // Move picture to selected subfolder
                    var subfolderPath = System.IO.Path.Combine(viewModel.KeepFolderPath, popup.SelectedSubfolder);
                    viewModel.MoveCurrentPictureTo(subfolderPath);
                }
            }
        }
        
        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Only handle keys if they haven't been handled by another control
            if (!e.Handled && DataContext is MainViewModel viewModel)
            {
                switch (e.Key) 
                {                
                    case System.Windows.Input.Key.Up:
                    case System.Windows.Input.Key.Left:
                        // Navigate to previous image directly using the ViewModel's NavigateTo method (MVVM)
                        if (viewModel.CanGoPrevious)
                        {
                            // Use the ViewModel's navigation method for proper MVVM
                            viewModel.NavigateTo(moveForward: false);
                            e.Handled = true;
                        }
                        break;
                          
                    case System.Windows.Input.Key.Down:
                    case System.Windows.Input.Key.Right:
                        // Navigate to next image directly using the ViewModel's NavigateTo method (MVVM)
                        if (viewModel.CanGoNext)
                        {
                            // Use the ViewModel's navigation method for proper MVVM
                            viewModel.NavigateTo(moveForward: true);
                            e.Handled = true;
                        }
                        break;
                }
            }
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
            
            // No longer loading from config.txt - the settings view model will handle loading 
            // the proper folder set or empty values if none is selected
            
            settingsView.ShowDialog();     
        }
        
        // Video playback control methods
        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (sender is MediaElement videoPlayer && 
                DataContext is MainViewModel viewModel && 
                viewModel.CurrentPicture != null && 
                viewModel.CurrentPicture.IsVideo)
            {
                // Update video duration info if available
                if (videoPlayer.NaturalDuration.HasTimeSpan)
                {
                    viewModel.CurrentPicture.VideoDuration = videoPlayer.NaturalDuration.TimeSpan;
                }
                
                // Set video dimensions
                viewModel.CurrentPicture.Width = videoPlayer.NaturalVideoWidth;
                viewModel.CurrentPicture.Height = videoPlayer.NaturalVideoHeight;
                
                // Start playing video (we don't auto-play anymore, only when play button is clicked)
                // Instead, let the PlayPauseVideo_Click method handle playing
            }
        }
        
        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (sender is MediaElement videoPlayer)
            {
                // Reset video to beginning when it ends
                videoPlayer.Position = TimeSpan.Zero;
                
                // Update UI state
                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.IsVideoPlaying = false;
                }
            }
        }
        
        private void PlayPauseVideo_Click(object sender, RoutedEventArgs e)
        {
            // Find the VideoPlayer in the visual tree
            var videoPlayer = FindVisualChild<MediaElement>(this, "VideoPlayer");
            
            if (DataContext is MainViewModel viewModel && videoPlayer != null)
            {
                if (viewModel.IsVideoPlaying)
                {
                    videoPlayer.Pause();
                    viewModel.IsVideoPlaying = false;
                }
                else
                {
                    videoPlayer.Play();
                    viewModel.IsVideoPlaying = true;
                }
            }
        }
        
        private void ReplayVideo_Click(object sender, RoutedEventArgs e)
        {
            // Find the VideoPlayer in the visual tree
            var videoPlayer = FindVisualChild<MediaElement>(this, "VideoPlayer");
            
            if (videoPlayer != null)
            {
                // Reset video to beginning
                videoPlayer.Position = TimeSpan.Zero;
                videoPlayer.Play();
                
                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.IsVideoPlaying = true;
                }
            }
        }
        
        // Method to load videos when selected
        public void LoadVideo(string videoPath)
        {
            // Find the VideoPlayer in the visual tree
            var videoPlayer = FindVisualChild<MediaElement>(this, "VideoPlayer");
            
            if (File.Exists(videoPath) && videoPlayer != null)
            {
                // Set the source of the MediaElement
                videoPlayer.Source = new Uri(videoPath);
            }
        }
        
        // Override OnPropertyChanged to catch when the CurrentPicture changes
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            
            if (DataContext is MainViewModel viewModel && e.Property == DataContextProperty)
            {
                // Update when DataContext changes
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }
        
        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var videoPlayer = FindVisualChild<MediaElement>(this, "VideoPlayer");
            
            if (sender is MainViewModel viewModel && 
                e.PropertyName == nameof(MainViewModel.CurrentPicture) && 
                videoPlayer != null)
            {
                if (viewModel.CurrentPicture != null && 
                    viewModel.CurrentPicture.IsVideo && 
                    viewModel.CurrentPicture.FilePath != null)
                {
                    // Load the video when CurrentPicture changes to a video
                    LoadVideo(viewModel.CurrentPicture.FilePath);
                }
                else
                {
                    // Clear video if switching to an image
                    videoPlayer.Source = null;
                }
            }
        }

        // Handle arrow key navigation in the image list        
        // // Handle arrow key navigation in the image list        
        private void ImagesListBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Make all arrow keys work consistently for navigation
            if (DataContext is MainViewModel viewModel)
            {
                switch (e.Key) 
                {
                    case System.Windows.Input.Key.Left:
                    case System.Windows.Input.Key.Up:
                        // Navigate to previous image using the centralized method
                        if (viewModel.CanGoPrevious)
                        {
                            // Use the centralized navigation method for consistency
                            NavigateToImage(forward: false);
                            e.Handled = true;
                        }
                        break;
                        
                    case System.Windows.Input.Key.Right:
                    case System.Windows.Input.Key.Down:
                        // Navigate to next image using the centralized method
                        if (viewModel.CanGoNext)
                        {
                            // Use the centralized navigation method for consistency
                            NavigateToImage(forward: true);
                            e.Handled = true;
                        }
                        break;
                }
            }
        }
          // Ensure the selected item is visible in the list
        private void EnsureItemVisible(object? item)
        {
            if (item != null)
            {
                // Find the ListBox in the visual tree
                var imagesListBox = FindVisualChild<ListBox>(this, "ImagesListBox");
                
                if (imagesListBox != null)
                {
                    // Ensure the item is scrolled into view
                    imagesListBox.ScrollIntoView(item);
                    
                    // Additional debugging to verify it's being called
                    System.Diagnostics.Debug.WriteLine($"Ensuring item visible: {(item as PictureModel)?.FileName}");
                }
            }
        }        
        // Flag to prevent reentrant calls to SelectionChanged
        private bool _isProcessingSelectionChange = false;
        
        // When an item is selected in the list, update the main view
        private void ImagesListBox_SelectionChanged (object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Guard against reentrant calls - prevents the method from being called while it's already executing
                if (_isProcessingSelectionChange)
                {
                    return;
                }
                
                _isProcessingSelectionChange = true;
                
                // First capture all variables safely
                var listBox = sender as ListBox;
                if (listBox == null)
                {
                    return;
                }
                
                var viewModel = DataContext as MainViewModel;
                if (viewModel == null)
                {
                    return;
                }
                
                // Get the selected index directly from the ListBox
                int selectedIndex = listBox.SelectedIndex;
                
                // Debug info
                System.Diagnostics.Debug.WriteLine($"ListBox selection changed: SelectedIndex={selectedIndex}, CurrentIndex={viewModel.CurrentIndex}");
                
                // If the selected index is valid and different from the current index
                if (selectedIndex >= 0 && selectedIndex != viewModel.CurrentIndex)
                {
                    // Update model directly based on the selected index
                    viewModel.CurrentIndex = selectedIndex;
                    
                    // Always use the ViewModel's method to set the current picture and load the full image
                    if (selectedIndex < viewModel.PicturesQueue.Count)
                    {
                        var selectedPicture = viewModel.PicturesQueue[selectedIndex];
                        viewModel.SetCurrentPictureWithFullImage(selectedPicture);
                    }
                    
                    // Update navigation state
                    viewModel.UpdateNavigationState();
                    
                    // Ensure selected item is visible
                    EnsureItemVisible(viewModel.CurrentPicture);
                    
                    System.Diagnostics.Debug.WriteLine($"Updated from selection: CurrentIndex={viewModel.CurrentIndex}, Picture={viewModel.CurrentPicture?.FileName}");
                    System.Diagnostics.Debug.WriteLine($"---------------------------------------------------------------");

                    // Make sure commands are reevaluated
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions for debugging
                System.Diagnostics.Debug.WriteLine($"Error in ImagesListBox_SelectionChanged: {ex.Message}");
            }
            finally
            {
                // Always reset the flag when done
                _isProcessingSelectionChange = false;
            }
        }
        
        // Helper method to find a named element in the visual tree
        private static T? FindVisualChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            // If parent is null, there's no visual child to be found
            if (parent == null) return null;
            
            // Check if the parent is the element we're looking for
            if (parent is FrameworkElement frameworkElement && frameworkElement.Name == childName && parent is T result)
            {
                return result;
            }
            
            // Iterate through all the children of the parent
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                
                // If the child is a FrameworkElement and has the name we're looking for, return it
                if (child is FrameworkElement element && element.Name == childName && child is T match)
                {
                    return match;
                }
                
                // Recursively check the children of this child
                var foundChild = FindVisualChild<T>(child, childName);
                if (foundChild != null)
                {
                    return foundChild;
                }
            }
            
            // If we get here, we didn't find the element we're looking for
            return null;
        }  
        
        // Centralized navigation method for all navigation operations throughout the application
        private void NavigateToImage(bool forward, bool forceToIndex = false, int targetIndex = -1)
        {
            if (DataContext is MainViewModel viewModel)
            {
                // Debug current state
                System.Diagnostics.Debug.WriteLine("--------------");
                System.Diagnostics.Debug.WriteLine($"NavigateToImage: Forward={forward}, ForceToIndex={forceToIndex}, TargetIndex={targetIndex}, CurrentIndex={viewModel.CurrentIndex}");
                System.Diagnostics.Debug.WriteLine("--------------");

                // Option 1: Force navigation to a specific index
                if (forceToIndex && targetIndex >= 0 && targetIndex < viewModel.PicturesQueue.Count)
                {
                    // Update index and picture directly
                    viewModel.CurrentIndex = targetIndex;
                    viewModel.CurrentPicture = viewModel.PicturesQueue[targetIndex];
                    viewModel.UpdateNavigationState();
                    
                    System.Diagnostics.Debug.WriteLine($"Forced navigation to index: {targetIndex}");
                }
                // Option 2: Navigate forward/backward
                else if ((forward && viewModel.CanGoNext) || (!forward && viewModel.CanGoPrevious))
                {
                    // Use the view model's NavigateTo method for actual navigation
                    viewModel.NavigateTo(forward);
                    System.Diagnostics.Debug.WriteLine($"Normal navigation: Direction={forward}, New Index={viewModel.CurrentIndex}");
                }
                
                // Always synchronize the ListBox selection after any navigation
                var imagesListBox = FindVisualChild<ListBox>(this, "ImagesListBox");
                if (imagesListBox != null && viewModel.CurrentPicture != null)
                {
                    // Set flag to prevent reentrant calls when updating the selection
                    _isProcessingSelectionChange = true;
                    try
                    {
                        imagesListBox.SelectedItem = viewModel.CurrentPicture;
                        EnsureItemVisible(viewModel.CurrentPicture);
                        System.Diagnostics.Debug.WriteLine($"UI synchronized to: {viewModel.CurrentPicture.FileName}");
                    }
                    finally
                    {
                        _isProcessingSelectionChange = false;
                    }
                }
                
                // Ensure commands are up-to-date
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}
