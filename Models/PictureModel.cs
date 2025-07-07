using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace AfbeeldingenUitzoeken.Models
{
    public class PictureModel : System.ComponentModel.INotifyPropertyChanged
    {
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        private BitmapImage? _image;
        public BitmapImage? Image
        {
            get => _image;
            set
            {
                if (_image != value)
                {
                    _image = value;
                    if (_image != null)
                    {
                        Width = _image.PixelWidth;
                        Height = _image.PixelHeight;
                    }
                    OnPropertyChanged(nameof(Image));
                }
            }
        }
        public BitmapImage? Thumbnail { get; set; }
        public DateTime CreationDate { get; set; }
        public long FileSize { get; set; } // In bytes
        private int _width;
        public int Width
        {
            get => _width;
            set { if (_width != value) { _width = value; OnPropertyChanged(nameof(Width)); OnPropertyChanged(nameof(Resolution)); } }
        }
        private int _height;
        public int Height
        {
            get => _height;
            set { if (_height != value) { _height = value; OnPropertyChanged(nameof(Height)); OnPropertyChanged(nameof(Resolution)); } }
        }
        public bool IsVideo { get; set; } // Flag to indicate if this is a video file
        public TimeSpan? VideoDuration { get; set; } // Duration for video files

        // Formatted properties for display
        public string FileSizeFormatted => FormatFileSize(FileSize);
        public string Resolution => $"{Width} x {Height}";
        public string DateFormatted => CreationDate.ToString("yyyy-MM-dd HH:mm:ss");
        public string DurationFormatted => VideoDuration.HasValue ?
            $"{VideoDuration.Value.Hours:D2}:{VideoDuration.Value.Minutes:D2}:{VideoDuration.Value.Seconds:D2}" :
            string.Empty;

        private string FormatFileSize(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB" };
            if (byteCount == 0)
                return "0 " + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return $"{(Math.Sign(byteCount) * num)} {suf[place]}";
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
}
