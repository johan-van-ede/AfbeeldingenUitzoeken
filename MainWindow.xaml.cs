using System;
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
        
        // Video playback control methods
        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel && viewModel.CurrentPicture != null && viewModel.CurrentPicture.IsVideo)
            {
                // Update video duration info if available
                if (VideoPlayer.NaturalDuration.HasTimeSpan)
                {
                    viewModel.CurrentPicture.VideoDuration = VideoPlayer.NaturalDuration.TimeSpan;
                }
                
                // Set video dimensions
                viewModel.CurrentPicture.Width = VideoPlayer.NaturalVideoWidth;
                viewModel.CurrentPicture.Height = VideoPlayer.NaturalVideoHeight;
                
                // Start playing video automatically
                VideoPlayer.Play();
                viewModel.IsVideoPlaying = true;
            }
        }
        
        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            // Reset video to beginning when it ends
            VideoPlayer.Position = TimeSpan.Zero;
            
            // Update UI state
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.IsVideoPlaying = false;
            }
        }
        
        private void PlayPauseVideo_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                if (viewModel.IsVideoPlaying)
                {
                    VideoPlayer.Pause();
                    viewModel.IsVideoPlaying = false;
                }
                else
                {
                    VideoPlayer.Play();
                    viewModel.IsVideoPlaying = true;
                }
            }
        }
        
        private void ReplayVideo_Click(object sender, RoutedEventArgs e)
        {
            // Reset video to beginning
            VideoPlayer.Position = TimeSpan.Zero;
            VideoPlayer.Play();
            
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.IsVideoPlaying = true;
            }
        }
        
        // Method to load videos when selected
        public void LoadVideo(string videoPath)
        {
            if (File.Exists(videoPath))
            {
                // Set the source of the MediaElement
                VideoPlayer.Source = new Uri(videoPath);
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
            if (sender is MainViewModel viewModel && e.PropertyName == nameof(MainViewModel.CurrentPicture))
            {
                if (viewModel.CurrentPicture != null && viewModel.CurrentPicture.IsVideo && viewModel.CurrentPicture.FilePath != null)
                {
                    // Load the video when CurrentPicture changes to a video
                    LoadVideo(viewModel.CurrentPicture.FilePath);
                }
                else
                {
                    // Clear video if switching to an image
                    VideoPlayer.Source = null;
                }
            }
        }
    }
}