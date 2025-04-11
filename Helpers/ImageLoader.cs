using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AfbeeldingenUitzoeken.Helpers
{
    /// <summary>
    /// Helper class for efficient image loading with caching
    /// </summary>
    public static class ImageLoader
    {
        // Thread-safe cache to prevent reloading the same thumbnails
        private static readonly ConcurrentDictionary<string, BitmapImage> ThumbnailCache = new ConcurrentDictionary<string, BitmapImage>();

        /// <summary>
        /// Loads an image with the specified dimensions, caching thumbnail results
        /// </summary>
        /// <param name="filePath">Path to the image file</param>
        /// <param name="maxWidth">Maximum width (0 for no constraint)</param>
        /// <param name="maxHeight">Maximum height (0 for no constraint)</param>
        /// <param name="isThumbnail">Whether this is a thumbnail that should be cached</param>
        /// <returns>A BitmapImage of the requested size</returns>
        public static BitmapImage? LoadImage(string filePath, int maxWidth = 0, int maxHeight = 0, bool isThumbnail = false)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return null;
            }            // For thumbnails, check the cache first
            string? cacheKey = isThumbnail ? GenerateCacheKey(filePath, maxWidth, maxHeight) : null;

            if (isThumbnail && !string.IsNullOrEmpty(cacheKey) && ThumbnailCache.TryGetValue(cacheKey, out var cachedImage))
            {
                return cachedImage;
            }

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(filePath);

                if (maxWidth > 0)
                    bitmap.DecodePixelWidth = maxWidth;

                if (maxHeight > 0)
                    bitmap.DecodePixelHeight = maxHeight;

                bitmap.EndInit();
                bitmap.Freeze(); // Optimize for UI thread

                // Cache thumbnails for better performance
                if (isThumbnail && !string.IsNullOrEmpty(cacheKey))
                {
                    ThumbnailCache[cacheKey] = bitmap;
                }

                return bitmap;
            }            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading image {filePath}: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Asynchronously loads an image with the specified dimensions
        /// </summary>
        /// <param name="filePath">Path to the image file</param>
        /// <param name="maxWidth">Maximum width (0 for no constraint)</param>
        /// <param name="maxHeight">Maximum height (0 for no constraint)</param>
        /// <param name="isThumbnail">Whether this is a thumbnail that should be cached</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the loaded BitmapImage.</returns>
        public static Task<BitmapImage?> LoadImageAsync(string filePath, int maxWidth = 0, int maxHeight = 0, bool isThumbnail = false)
        {
            return Task.Run(() => LoadImage(filePath, maxWidth, maxHeight, isThumbnail));
        }

        /// <summary>
        /// Clears the thumbnail cache to free memory
        /// </summary>
        public static void ClearCache()
        {
            ThumbnailCache.Clear();
        }

        /// <summary>
        /// Generates a unique cache key for the given parameters
        /// </summary>
        /// <param name="filePath">Path to the image file</param>
        /// <param name="maxWidth">Maximum width</param>
        /// <param name="maxHeight">Maximum height</param>
        /// <returns>A unique cache key</returns>
        private static string GenerateCacheKey(string filePath, int maxWidth, int maxHeight)
        {
            var sb = new StringBuilder(filePath);
            sb.Append("_").Append(maxWidth).Append("_").Append(maxHeight);
            return sb.ToString();
        }
    }
}
