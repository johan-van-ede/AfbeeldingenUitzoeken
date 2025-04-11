using System;
using System.IO;
using System.Linq;

namespace AfbeeldingenUitzoeken.Helpers
{
    public static class MediaExtensions
    {
        // Supported image file extensions
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
        
        // Supported video file extensions
        private static readonly string[] VideoExtensions = { ".mp4", ".avi", ".mov", ".wmv", ".mkv", ".flv", ".webm", ".m4v", ".mpg", ".mpeg", ".3gp" };
        
        // All supported media file extensions
        private static readonly string[] AllSupportedExtensions = ImageExtensions.Concat(VideoExtensions).ToArray();
        
        /// <summary>
        /// Checks if a file is a supported image format
        /// </summary>
        public static bool IsImage(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return ImageExtensions.Contains(extension);
        }
        
        /// <summary>
        /// Checks if a file is a supported video format
        /// </summary>
        public static bool IsVideo(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return VideoExtensions.Contains(extension);
        }
        
        /// <summary>
        /// Checks if a file is any supported media format (image or video)
        /// </summary>
        public static bool IsSupportedMedia(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return AllSupportedExtensions.Contains(extension);
        }
    }
}
