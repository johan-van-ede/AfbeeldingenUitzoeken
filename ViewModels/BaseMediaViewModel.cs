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

                IsVideoPlaying = false;

                if (value != null && !string.IsNullOrEmpty(value.FilePath) && File.Exists(value.FilePath))
                {
                    try 
                    {                        
                        IsCurrentItemVideo = value.IsVideo || (!string.IsNullOrEmpty(value.FilePath) && MediaExtensions.IsVideo(value.FilePath));                        
                        if (!IsCurrentItemVideo && !string.IsNullOrEmpty(value.FilePath))
                        {
                            // Load full-res image if not already loaded
                            if (value.Image == null)
                            {
                                value.Image = ImageLoader.LoadImage(value.FilePath);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading media: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }        

        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
