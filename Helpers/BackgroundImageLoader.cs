using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using AfbeeldingenUitzoeken.Helpers;
using AfbeeldingenUitzoeken.Models;

namespace AfbeeldingenUitzoeken.Helpers
{
    /// <summary>
    /// Provides background loading capabilities for image galleries
    /// </summary>
    public class BackgroundImageLoader
    {        private readonly ConcurrentQueue<string> _pendingFiles = new ConcurrentQueue<string>();
        private readonly ConcurrentDictionary<string, Task<PictureModel?>> _runningTasks = new ConcurrentDictionary<string, Task<PictureModel?>>();
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _isRunning;
        private readonly int _maxConcurrentTasks;
        
        public event Action<PictureModel>? ImageLoaded;
        
        public BackgroundImageLoader(int maxConcurrentTasks = 3)
        {
            _maxConcurrentTasks = maxConcurrentTasks;
        }
        
        /// <summary>
        /// Queue a file for loading as a PictureModel
        /// </summary>
        public void QueueFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;
                
            _pendingFiles.Enqueue(filePath);
            
            if (!_isRunning)
                StartProcessing();
        }
        
        /// <summary>
        /// Queue multiple files for processing
        /// </summary>
        public void QueueFiles(IEnumerable<string> filePaths)
        {
            foreach (var path in filePaths)
                _pendingFiles.Enqueue(path);
                
            if (!_isRunning)
                StartProcessing();
        }
        
        /// <summary>
        /// Start processing the queue in the background
        /// </summary>
        private void StartProcessing()
        {
            if (_isRunning)
                return;
                
            _isRunning = true;
            _cts = new CancellationTokenSource();
            
            Task.Run(() => ProcessQueue(_cts.Token));
        }
        
        /// <summary>
        /// Stop all background processing
        /// </summary>
        public void Stop()
        {
            _cts.Cancel();
            _isRunning = false;
        }
        
        private async Task ProcessQueue(CancellationToken token)
        {
            while (!token.IsCancellationRequested && (_pendingFiles.Count > 0 || _runningTasks.Count > 0))
            {
                // Start new tasks if we have capacity
                while (_pendingFiles.Count > 0 && _runningTasks.Count < _maxConcurrentTasks)
                {                    if (_pendingFiles.TryDequeue(out string filePath))
                    {
                        var task = Task.Run(() => LoadPictureModel(filePath), token);
                        _runningTasks.TryAdd(filePath, task);
                    }
                }
                
                // Wait for any task to complete
                Task<Task<PictureModel?>>? completedTask = null;
                  if (_runningTasks.Count > 0)
                {
                    try
                    {
                        completedTask = Task.WhenAny(_runningTasks.Values);
                        var result = await completedTask;
                        var pictureModel = await result;
                        
                        // Remove the completed task
                        var completedKey = _runningTasks.FirstOrDefault(x => x.Value == result).Key;
                        if (!string.IsNullOrEmpty(completedKey))
                            _runningTasks.TryRemove(completedKey, out _);
                            
                        // Notify listeners
                        if (pictureModel != null)
                            ImageLoaded?.Invoke(pictureModel);
                    }                    catch (Exception)
                    {
                        // Handle task failures - just remove the task and continue
                        if (completedTask?.Result != null)
                        {
                            var failedTask = completedTask.Result;
                            var failedKey = _runningTasks.FirstOrDefault(x => x.Value == failedTask).Key;
                            if (!string.IsNullOrEmpty(failedKey))
                                _runningTasks.TryRemove(failedKey, out _);
                        }
                    }
                }
                else
                {
                    await Task.Delay(50, token); // Brief pause if no tasks are running
                }
            }
            
            _isRunning = false;
        }
          private PictureModel? LoadPictureModel(string filePath)
        {
            try
            {
                bool isVideo = MediaExtensions.IsVideo(filePath);
                var fileInfo = new System.IO.FileInfo(filePath);
                
                // Load thumbnail with caching
                var thumbnail = ImageLoader.LoadImage(filePath, maxWidth: 150, isThumbnail: true);
                
                if (thumbnail == null)
                    return null;
                    
                int width = 0;
                int height = 0;
                
                if (!isVideo)
                {
                    width = thumbnail.PixelWidth;
                    height = thumbnail.PixelHeight;
                }
                
                return new PictureModel
                {
                    FilePath = filePath,
                    FileName = System.IO.Path.GetFileName(filePath),
                    Thumbnail = thumbnail,
                    CreationDate = fileInfo.CreationTime,
                    FileSize = fileInfo.Length,
                    Width = width,
                    Height = height,
                    IsVideo = isVideo,
                    VideoDuration = isVideo ? TimeSpan.Zero : null
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading image {filePath}: {ex.Message}");
                return null;
            }
        }
    }
}
