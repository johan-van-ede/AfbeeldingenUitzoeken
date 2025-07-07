using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace AfbeeldingenUitzoeken.Helpers
{
    /// <summary>
    /// Simple video thumbnail extractor that uses FFmpeg to extract thumbnails
    /// This avoids the compatibility issues between System.Drawing and WPF
    /// </summary>
    internal static class VideoThumbnailExtractor
    {
        /// <summary>
        /// Get a thumbnail from a video file
        /// </summary>
        /// <param name="videoFilePath">Path to the video file</param>
        /// <param name="maxWidth">Maximum width for the thumbnail</param>
        /// <param name="maxHeight">Maximum height for the thumbnail</param>
        /// <returns>A bitmap image of the video thumbnail or null if extraction fails</returns>
        public static BitmapImage? ExtractThumbnail(string videoFilePath, int maxWidth = 0, int maxHeight = 0)
        {
            if (!File.Exists(videoFilePath))
                return null;
                
            try
            {                // First, try to find and use a system-generated thumbnail if available
                // Try to get the Windows shell thumbnail directly
                var shellThumbnail = WindowsThumbnailProvider.GetThumbnail(videoFilePath, 
                    maxWidth > 0 ? maxWidth : 320, 
                    maxHeight > 0 ? maxHeight : 240);
                
                if (shellThumbnail != null)
                {
                    // Convert the shell thumbnail to a BitmapImage
                    var bitmapImage = new BitmapImage();
                    var encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(shellThumbnail));
                    
                    using (var memoryStream = new MemoryStream())
                    {
                        encoder.Save(memoryStream);
                        memoryStream.Position = 0;
                        
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = memoryStream;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                    }
                    
                    return bitmapImage;
                }
                
                // Fall back to checking common thumbnail locations
                string thumbnailPath = Path.Combine(
                    Path.GetDirectoryName(videoFilePath) ?? string.Empty,
                    "Thumbs",
                    Path.GetFileNameWithoutExtension(videoFilePath) + ".jpg");
                
                if (File.Exists(thumbnailPath))
                {
                    // Use the existing thumbnail if available
                    return LoadThumbnailImage(thumbnailPath, maxWidth, maxHeight);
                }
                
                // If no system thumbnail is available, try to extract a frame using MediaPlayer
                try
                {
                    // Create a temporary file for the thumbnail
                    string tempThumbPath = Path.Combine(
                        Path.GetTempPath(),
                        $"vidthumb_{Path.GetFileNameWithoutExtension(videoFilePath)}_{Guid.NewGuid()}.jpg");
                    
                    var mediaPlayer = new System.Windows.Media.MediaPlayer();
                    
                    // Set up event completion to know when the media is loaded
                    var mediaOpened = new System.Threading.ManualResetEvent(false);
                    
                    mediaPlayer.MediaOpened += (s, e) => mediaOpened.Set();
                    mediaPlayer.MediaFailed += (s, e) => mediaOpened.Set();
                    
                    // Open the media file
                    mediaPlayer.Open(new Uri(videoFilePath));
                    
                    // Wait for the media to be opened with a timeout
                    if (mediaOpened.WaitOne(3000))
                    {
                        if (mediaPlayer.NaturalDuration.HasTimeSpan)
                        {
                            // Set position to 10% of the video to avoid black frames at the beginning
                            TimeSpan targetPos = TimeSpan.FromMilliseconds(
                                mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds * 0.1);
                            mediaPlayer.Position = targetPos;
                            
                            // Give it time to seek and render the frame
                            System.Threading.Thread.Sleep(300);
                            
                            // For WPF, we can use a VideoDrawing and DrawingVisual to capture the frame
                            var videoDrawing = new System.Windows.Media.VideoDrawing
                            {
                                Player = mediaPlayer,
                                Rect = new System.Windows.Rect(0, 0, 
                                    maxWidth > 0 ? maxWidth : 320, 
                                    maxHeight > 0 ? maxHeight : 240)
                            };
                            
                            var drawingVisual = new System.Windows.Media.DrawingVisual();
                            using (var drawingContext = drawingVisual.RenderOpen())
                            {
                                drawingContext.DrawDrawing(videoDrawing);
                            }
                            
                            var frameRenderTarget = new System.Windows.Media.Imaging.RenderTargetBitmap(
                                maxWidth > 0 ? maxWidth : 320,
                                maxHeight > 0 ? maxHeight : 240,
                                96, 96,
                                System.Windows.Media.PixelFormats.Pbgra32);
                            
                            frameRenderTarget.Render(drawingVisual);
                            
                            // Save to the temp file
                            var jpegEncoder = new System.Windows.Media.Imaging.JpegBitmapEncoder();
                            jpegEncoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(frameRenderTarget));
                            
                            using (var fileStream = new FileStream(tempThumbPath, FileMode.Create))
                            {
                                jpegEncoder.Save(fileStream);
                            }
                            
                            // Now load this thumbnail
                            if (File.Exists(tempThumbPath))
                            {
                                var thumbnailImage = LoadThumbnailImage(tempThumbPath, maxWidth, maxHeight);
                                
                                // Delete the temp file after loading
                                try { File.Delete(tempThumbPath); } catch { }
                                
                                return thumbnailImage;
                            }
                        }
                    }
                    
                    mediaPlayer.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to extract video frame: {ex.Message}");
                    // Continue to fallback method
                }
                
                // If thumbnail extraction failed, return null
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting video thumbnail: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Load an image from file with optional size constraints
        /// </summary>
        private static BitmapImage? LoadThumbnailImage(string filePath, int maxWidth, int maxHeight)
        {
            if (!File.Exists(filePath))
                return null;
                
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
                
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading thumbnail image: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Creates a fallback thumbnail for a video when extraction fails
        /// </summary>
        public static BitmapImage CreateFallbackThumbnail(int width = 150, int height = 150)
        {
            // Create a simple blue background as fallback
            var drawingVisual = new System.Windows.Media.DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                // Draw a blue rectangle
                drawingContext.DrawRectangle(
                    System.Windows.Media.Brushes.DodgerBlue,
                    null,
                    new System.Windows.Rect(0, 0, width, height));
            }
            
            // Render to bitmap
            var renderTarget = new System.Windows.Media.Imaging.RenderTargetBitmap(
                width, height, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
            
            renderTarget.Render(drawingVisual);
            
            // Convert to BitmapImage
            var thumbnailImage = new BitmapImage();
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
            
            return thumbnailImage;
        }
    }
}
