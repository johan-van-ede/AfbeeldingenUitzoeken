using System;
using System.Collections.Concurrent;
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
            }

            // For thumbnails, check the cache first
            string? cacheKey = isThumbnail ? GenerateCacheKey(filePath, maxWidth, maxHeight) : null;
            if (isThumbnail && !string.IsNullOrEmpty(cacheKey) && ThumbnailCache.TryGetValue(cacheKey, out var cachedImage))
            {
                return cachedImage;
            }

            try
            {
                // Load image into memory stream to allow metadata access
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    
                    var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    BitmapFrame frame = decoder.Frames[0];

                    // Check for EXIF orientation
                    BitmapMetadata? metadata = frame.Metadata as BitmapMetadata;
                    int orientation = 1;
                    if (metadata != null && metadata.ContainsQuery("/app1/ifd/{ushort=274}"))
                    {
                        object? o = metadata.GetQuery("/app1/ifd/{ushort=274}");
                        if (o != null)
                            orientation = Convert.ToInt32(o);
                    }

                    // Apply rotation/flip if needed
                    TransformedBitmap? transformed = null;
                    var transform = new System.Windows.Media.TransformGroup();
                        switch (orientation)
                        {
                            case 2: // Flip horizontal
                                transform.Children.Add(new System.Windows.Media.ScaleTransform(-1, 1, 0.5, 0.5));
                                break;
                            case 3: // Rotate 180
                                transform.Children.Add(new System.Windows.Media.RotateTransform(180));
                                break;
                            case 4: // Flip vertical
                                transform.Children.Add(new System.Windows.Media.ScaleTransform(1, -1, 0.5, 0.5));
                                break;
                            case 5: // Transpose
                                transform.Children.Add(new System.Windows.Media.RotateTransform(90));
                                transform.Children.Add(new System.Windows.Media.ScaleTransform(1, -1, 0.5, 0.5));
                                break;
                            case 6: // Rotate 90
                                transform.Children.Add(new System.Windows.Media.RotateTransform(90));
                                break;
                            case 7: // Transverse
                                transform.Children.Add(new System.Windows.Media.RotateTransform(270));
                                transform.Children.Add(new System.Windows.Media.ScaleTransform(1, -1, 0.5, 0.5));
                                break;
                            case 8: // Rotate 270
                                transform.Children.Add(new System.Windows.Media.RotateTransform(270));
                                break;
                        
                    }

                    BitmapSource source = frame;
                    if (transform.Children.Count > 0)
                    {
                        transformed = new TransformedBitmap(frame, transform);
                        transformed.Freeze();
                        source = transformed;
                    }

                    // Optionally scale for thumbnail, preserving aspect ratio
                    if (maxWidth > 0 || maxHeight > 0)
                    {
                        double scaleX = 1.0, scaleY = 1.0;
                        double origWidth = source.PixelWidth;
                        double origHeight = source.PixelHeight;
                        if (maxWidth > 0 && maxHeight > 0)
                        {
                            double ratioX = (double)maxWidth / origWidth;
                            double ratioY = (double)maxHeight / origHeight;
                            double scale = Math.Min(ratioX, ratioY);
                            scaleX = scaleY = scale;
                        }
                        else if (maxWidth > 0)
                        {
                            scaleX = scaleY = (double)maxWidth / origWidth;
                        }
                        else if (maxHeight > 0)
                        {
                            scaleX = scaleY = (double)maxHeight / origHeight;
                        }
                        var scaled = new TransformedBitmap(source, new System.Windows.Media.ScaleTransform(scaleX, scaleY));
                        scaled.Freeze();
                        source = scaled;
                    }

                    // Convert to BitmapImage for compatibility
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(source));
                    using (var ms = new MemoryStream())
                    {
                        encoder.Save(ms);
                        ms.Position = 0;
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        if (isThumbnail && !string.IsNullOrEmpty(cacheKey))
                        {
                            ThumbnailCache[cacheKey] = bitmap;
                        }
                        return bitmap;
                    }
                }
            }
            catch (Exception ex)
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

        /// <summary>
        /// Loads or creates a thumbnail for a video file
        /// </summary>
        /// <param name="filePath">Path to the video file</param>
        /// <param name="maxWidth">Maximum width (0 for no constraint)</param>
        /// <param name="maxHeight">Maximum height (0 for no constraint)</param>
        /// <returns>A BitmapImage to use as video thumbnail</returns>
        public static BitmapImage? LoadVideoThumbnail(string filePath, int maxWidth = 0, int maxHeight = 0)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return null;
            }
            
            // Check if we already have this video thumbnail cached
            string cacheKey = "video_" + GenerateCacheKey(filePath, maxWidth, maxHeight);
            if (ThumbnailCache.TryGetValue(cacheKey, out var cachedImage))
            {
                return cachedImage;
            }

            try
            {
                // Try to extract an actual frame from the video
                BitmapImage? thumbnailImage = null;
                
                // First, try to find and use a system-generated thumbnail if available
                string thumbnailPath = Path.Combine(
                    Path.GetDirectoryName(filePath) ?? string.Empty,
                    "Thumbs",
                    Path.GetFileNameWithoutExtension(filePath) + ".jpg");
                  
                if (File.Exists(thumbnailPath))
                {
                    // Use the existing thumbnail if available
                    thumbnailImage = LoadImage(thumbnailPath, maxWidth, maxHeight, true);
                }
                
                // If no system thumbnail is available, try to extract a frame using VideoThumbnailExtractor
                if (thumbnailImage == null)
                {
                    try
                    {
                        // Use our VideoThumbnailExtractor helper to get a thumbnail
                        thumbnailImage = VideoThumbnailExtractor.ExtractThumbnail(filePath, maxWidth, maxHeight);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to extract video thumbnail: {ex.Message}");
                    }
                }
                
                // If we couldn't get a thumbnail, create a fallback blue rectangle
                if (thumbnailImage == null)
                {
                    // Create a simple blue background as fallback
                    var drawingVisual = new System.Windows.Media.DrawingVisual();
                    using (var drawingContext = drawingVisual.RenderOpen())
                    {
                        // Draw a blue rectangle
                        drawingContext.DrawRectangle(
                            System.Windows.Media.Brushes.DodgerBlue,
                            null,
                            new System.Windows.Rect(0, 0, maxWidth > 0 ? maxWidth : 150, maxHeight > 0 ? maxHeight : 150));
                    }
                    
                    // Render to bitmap
                    var renderTarget = new System.Windows.Media.Imaging.RenderTargetBitmap(
                        maxWidth > 0 ? maxWidth : 150,
                        maxHeight > 0 ? maxHeight : 150,
                        96, 96,
                        System.Windows.Media.PixelFormats.Pbgra32);
                    
                    renderTarget.Render(drawingVisual);
                    
                    // Convert to BitmapImage
                    thumbnailImage = new BitmapImage();
                    var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                    encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(renderTarget));
                    
                    using (var memoryStream = new MemoryStream())
                    {
                        encoder.Save(memoryStream);
                        memoryStream.Position = 0;
                        
                        thumbnailImage.BeginInit();
                        thumbnailImage.CacheOption = BitmapCacheOption.OnLoad;
                        thumbnailImage.StreamSource = memoryStream;
                        thumbnailImage.EndInit();
                        thumbnailImage.Freeze();
                    }
                }
                
                // Now add a semi-transparent play icon overlay on top of the thumbnail
                var finalDrawingVisual = new System.Windows.Media.DrawingVisual();
                using (var drawingContext = finalDrawingVisual.RenderOpen())
                {
                    // First draw the thumbnail image
                    drawingContext.DrawImage(thumbnailImage, 
                        new System.Windows.Rect(0, 0, 
                            maxWidth > 0 ? maxWidth : thumbnailImage.PixelWidth, 
                            maxHeight > 0 ? maxHeight : thumbnailImage.PixelHeight));
                    
                    // Calculate dimensions for a smaller play icon (30% of the image size)
                    double iconWidth = (maxWidth > 0 ? maxWidth : thumbnailImage.PixelWidth) * 0.3;
                    double iconHeight = (maxHeight > 0 ? maxHeight : thumbnailImage.PixelHeight) * 0.3;
                    double centerX = (maxWidth > 0 ? maxWidth : thumbnailImage.PixelWidth) / 2;
                    double centerY = (maxHeight > 0 ? maxHeight : thumbnailImage.PixelHeight) / 2;
                    
                    // Create a semi-transparent blue play triangle
                    var playIcon = new System.Windows.Media.PathGeometry();
                    var figure = new System.Windows.Media.PathFigure();
                    
                    // Create a triangle
                    figure.StartPoint = new System.Windows.Point(centerX - iconWidth/2, centerY - iconHeight/2);
                    figure.Segments.Add(new System.Windows.Media.LineSegment(
                        new System.Windows.Point(centerX - iconWidth/2, centerY + iconHeight/2), true));
                    figure.Segments.Add(new System.Windows.Media.LineSegment(
                        new System.Windows.Point(centerX + iconWidth/2, centerY), true));
                    figure.Segments.Add(new System.Windows.Media.LineSegment(
                        new System.Windows.Point(centerX - iconWidth/2, centerY - iconHeight/2), true));
                    
                    playIcon.Figures.Add(figure);
                    
                    // Semi-transparent blue brush (60% opacity)
                    var playIconBrush = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromArgb(153, 30, 144, 255)); // 60% opacity DodgerBlue
                    
                    // Draw the play icon with the semi-transparent blue color
                    drawingContext.DrawGeometry(playIconBrush, null, playIcon);
                }
                
                // Render the final image with the play icon overlay
                var finalRenderTarget = new System.Windows.Media.Imaging.RenderTargetBitmap(
                    maxWidth > 0 ? maxWidth : thumbnailImage.PixelWidth,
                    maxHeight > 0 ? maxHeight : thumbnailImage.PixelHeight,
                    96, 96,
                    System.Windows.Media.PixelFormats.Pbgra32);
                
                finalRenderTarget.Render(finalDrawingVisual);
                
                // Create a BitmapImage from the rendered bitmap
                var finalBitmap = new BitmapImage();
                var finalEncoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                finalEncoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(finalRenderTarget));
                
                using (var memoryStream = new MemoryStream())
                {
                    finalEncoder.Save(memoryStream);
                    memoryStream.Position = 0;
                    
                    finalBitmap.BeginInit();
                    finalBitmap.CacheOption = BitmapCacheOption.OnLoad;
                    finalBitmap.StreamSource = memoryStream;
                    finalBitmap.EndInit();
                    finalBitmap.Freeze();
                }
                
                // Cache the final thumbnail
                ThumbnailCache[cacheKey] = finalBitmap;
                
                return finalBitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to create video thumbnail: {ex.Message}");
                return null;
            }
        }
    }
}
