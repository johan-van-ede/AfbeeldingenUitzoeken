using System.Windows.Input;
using System.Windows;
using AfbeeldingenUitzoeken.ViewModels;
using Button = System.Windows.Controls.Button;

namespace AfbeeldingenUitzoeken.Helpers
{
    /// <summary>
    /// Helper class to assist with navigation between images
    /// </summary>
    public static class NavigationHelper
    {
        /// <summary>
        /// Setup navigation buttons with direct event handlers
        /// </summary>
        public static void SetupNavigationButtons(MainWindow window, Button nextButton, Button prevButton)
        {
            // Add click handlers for navigation buttons
            nextButton.Click += (sender, e) =>
            {
                if (window.DataContext is MainViewModel vm)
                {
                    NavigateNext(vm);
                    // Update button enabled/disabled states
                    nextButton.IsEnabled = vm.CanGoNext;
                    prevButton.IsEnabled = vm.CanGoPrevious;
                }
            };
            
            prevButton.Click += (sender, e) =>
            {
                if (window.DataContext is MainViewModel vm)
                {
                    NavigatePrevious(vm);
                    // Update button enabled/disabled states
                    nextButton.IsEnabled = vm.CanGoNext;
                    prevButton.IsEnabled = vm.CanGoPrevious;
                }
            };
            
            // Force initial button state update
            if (window.DataContext is MainViewModel vm)
            {
                // Set initial button enabled states
                nextButton.IsEnabled = vm.CanGoNext;
                prevButton.IsEnabled = vm.CanGoPrevious;
                
                // Listen for changes to the CanGoNext/CanGoPrevious properties
                vm.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(vm.CanGoNext))
                        nextButton.IsEnabled = vm.CanGoNext;
                    else if (e.PropertyName == nameof(vm.CanGoPrevious))
                        prevButton.IsEnabled = vm.CanGoPrevious;
                };
            }
        }
        
        /// <summary>
        /// Navigate to next image if possible
        /// </summary>
        public static bool NavigateNext(MainViewModel viewModel)
        {
            if (viewModel.PicturesQueue.Count > 0 && viewModel.CurrentIndex < viewModel.PicturesQueue.Count - 1)
            {
                // Update index and current picture
                viewModel.CurrentIndex++;
                viewModel.CurrentPicture = viewModel.PicturesQueue[viewModel.CurrentIndex];
                
                // Let the view model update its navigation state (this updates CanGoNext/CanGoPrevious)
                viewModel.UpdateNavigationState();
                
                // Debug message
                System.Diagnostics.Debug.WriteLine($"NavigateNext executed: CurrentIndex={viewModel.CurrentIndex}");
                return true;
            }
            
            System.Diagnostics.Debug.WriteLine($"NavigateNext skipped: CurrentIndex={viewModel.CurrentIndex}, Count={viewModel.PicturesQueue.Count}");
            return false;
        }
        
        /// <summary>
        /// Navigate to previous image if possible
        /// </summary>
        public static bool NavigatePrevious(MainViewModel viewModel)
        {
            if (viewModel.PicturesQueue.Count > 0 && viewModel.CurrentIndex > 0)
            {
                // Update index and current picture
                viewModel.CurrentIndex--;
                viewModel.CurrentPicture = viewModel.PicturesQueue[viewModel.CurrentIndex];
                
                // Let the view model update its navigation state (this updates CanGoNext/CanGoPrevious)
                viewModel.UpdateNavigationState();
                
                // Debug message
                System.Diagnostics.Debug.WriteLine($"NavigatePrevious executed: CurrentIndex={viewModel.CurrentIndex}");
                return true;
            }
            
            System.Diagnostics.Debug.WriteLine($"NavigatePrevious skipped: CurrentIndex={viewModel.CurrentIndex}");
            return false;
        }
    }
}
