using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace AfbeeldingenUitzoeken.Models
{
    public class PictureModel
    {
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public BitmapImage? Thumbnail { get; set; }
        public DateTime CreationDate { get; set; }
        public long FileSize { get; set; } // In bytes
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsVideo { get; set; } // Flag to indicate if this is a video file
        public TimeSpan? VideoDuration { get; set; } // Duration for video files
        
        // Formatted properties for display
        public string FileSizeFormatted => FormatFileSize(FileSize);
        public string ResolutionFormatted => $"{Width} x {Height}";
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
    }
}
