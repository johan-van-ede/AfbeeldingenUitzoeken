using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using AfbeeldingenUitzoeken.Helpers;
using AfbeeldingenUitzoeken.Models;
using MessageBox = System.Windows.MessageBox;

namespace AfbeeldingenUitzoeken.ViewModels
{
    /// <summary>
    /// Base class for view models that handle media (images and videos)
    /// </summary>
    public abstract class BaseMediaViewModel : INotifyPropertyChanged
    {
        private PictureModel? _currentPicture;
        private BitmapImage? _currentImage;
        private bool _isCurrentItemVideo = false;
        private bool _isVideoPlaying = false;
        private string _videoPlayButtonContent = "▶";

        // Video-related properties
        public bool IsCurrentItemVideo
        {
            get => _isCurrentItemVideo;
            set
            {
                if (_isCurrentItemVideo != value)
                {
                    _isCurrentItemVideo = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public bool IsVideoPlaying
        {
            get => _isVideoPlaying;
            set
            {
                if (_isVideoPlaying != value)
                {
                    _isVideoPlaying = value;
                    VideoPlayButtonContent = value ? "⏸" : "▶";
                    OnPropertyChanged();
                }
            }
        }
        
        public string VideoPlayButtonContent
        {
            get => _videoPlayButtonContent;
            set
            {
                if (_videoPlayButtonContent != value)
                {
                    _videoPlayButtonContent = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public PictureModel? CurrentPicture
        {
            get => _currentPicture;
            set
            {
                _currentPicture = value;
                OnPropertyChanged();
                
                // Reset video state when changing items
                IsVideoPlaying = false;
                
                if (value != null && !string.IsNullOrEmpty(value.FilePath) && File.Exists(value.FilePath))
                {
                    try 
                    {
                        // Set IsCurrentItemVideo based on PictureModel's IsVideo property or using MediaExtensions
                        IsCurrentItemVideo = value.IsVideo || (!string.IsNullOrEmpty(value.FilePath) && MediaExtensions.IsVideo(value.FilePath));
                        
                        if (!IsCurrentItemVideo && !string.IsNullOrEmpty(value.FilePath))
                        {
                            // Handle image file
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.UriSource = new Uri(value.FilePath);
                            bitmap.EndInit();
                            bitmap.Freeze(); // Optimize for UI thread
                            CurrentImage = bitmap;
                        }
                        else
                        {
                            // For videos, we'll load them in the MediaElement in code-behind
                            // The View will handle video loading via the property changed event
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading media: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
