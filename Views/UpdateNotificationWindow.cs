using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

// Explicitly remove ambiguity by using specific namespaces
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;
using Orientation = System.Windows.Controls.Orientation;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace AfbeeldingenUitzoeken.Views
{
    public partial class UpdateNotificationWindow : System.Windows.Window
    {
        // Text blocks for version display
        private TextBlock? CurrentVersionText { get; set; }
        private TextBlock? NewVersionText { get; set; }
        
        // Property for download URL validation
        private string DownloadUrl { get; set; } = string.Empty;
        
        public UpdateNotificationWindow(string currentVersion, string newVersion, string downloadUrl)
        {
            InitializeComponent();
            
            // Set version information with null checks
            if (CurrentVersionText != null)
                CurrentVersionText.Text = currentVersion;
            
            if (NewVersionText != null)
                NewVersionText.Text = newVersion;
            
            DownloadUrl = downloadUrl;
        }
        
        private void InitializeComponent()
        {
            // Create the window
            this.Title = "Update Available";
            this.Width = 400;
            this.Height = 250;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.ResizeMode = ResizeMode.NoResize;
            this.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30));
            
            // Create the main grid
            Grid mainGrid = new Grid();
            mainGrid.Margin = new Thickness(20);
            this.Content = mainGrid;
            
            // Create rows for the grid
            RowDefinition headerRow = new RowDefinition { Height = GridLength.Auto };
            RowDefinition contentRow = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
            RowDefinition buttonsRow = new RowDefinition { Height = GridLength.Auto };
            mainGrid.RowDefinitions.Add(headerRow);
            mainGrid.RowDefinitions.Add(contentRow);
            mainGrid.RowDefinitions.Add(buttonsRow);
            
            // Add header text
            TextBlock headerText = new TextBlock
            {
                Text = "A new version is available!",
                FontSize = 18,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(224, 224, 224)),
                Margin = new Thickness(0, 0, 0, 15),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };
            Grid.SetRow(headerText, 0);
            mainGrid.Children.Add(headerText);
            
            // Add content
            StackPanel contentPanel = new StackPanel
            {
                Margin = new Thickness(0, 10, 0, 10)
            };
            Grid.SetRow(contentPanel, 1);
            mainGrid.Children.Add(contentPanel);
            
            // Current version
            StackPanel currentVersionPanel = new StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 5)
            };
            contentPanel.Children.Add(currentVersionPanel);
            
            TextBlock currentVersionLabel = new TextBlock
            {
                Text = "Current version: ",
                FontSize = 14,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(224, 224, 224))
            };
            currentVersionPanel.Children.Add(currentVersionLabel);
            
            CurrentVersionText = new TextBlock
            {
                FontSize = 14,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(224, 224, 224))
            };
            currentVersionPanel.Children.Add(CurrentVersionText);
            
            // New version
            StackPanel newVersionPanel = new StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 5)
            };
            contentPanel.Children.Add(newVersionPanel);
            
            TextBlock newVersionLabel = new TextBlock
            {
                Text = "New version: ",
                FontSize = 14,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(224, 224, 224))
            };
            newVersionPanel.Children.Add(newVersionLabel);
            
            NewVersionText = new TextBlock
            {
                FontSize = 14,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(91, 192, 222))
            };
            newVersionPanel.Children.Add(NewVersionText);
            
            // Info text
            TextBlock infoText = new TextBlock
            {
                Text = "Would you like to download the new version now?",
                FontSize = 14,
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(224, 224, 224)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 20, 0, 0)
            };
            contentPanel.Children.Add(infoText);
            
            // Buttons
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };
            Grid.SetRow(buttonPanel, 2);
            mainGrid.Children.Add(buttonPanel);
            
            System.Windows.Controls.Button downloadButton = new System.Windows.Controls.Button
            {
                Content = "Download",
                Width = 100,
                Height = 35,
                Margin = new Thickness(10, 0, 0, 0),
                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(91, 192, 222)),
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(224, 224, 224)),
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            downloadButton.Click += DownloadButton_Click;
            buttonPanel.Children.Add(downloadButton);
            
            System.Windows.Controls.Button remindLaterButton = new System.Windows.Controls.Button
            {
                Content = "Remind Later",
                Width = 100,
                Height = 35,
                Margin = new Thickness(10, 0, 0, 0),
                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(62, 62, 62)),
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(224, 224, 224)),
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            remindLaterButton.Click += RemindLaterButton_Click;
            buttonPanel.Children.Add(remindLaterButton);
        }
        
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open the download URL in the default browser
                if (!string.IsNullOrEmpty(DownloadUrl))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = DownloadUrl,
                        UseShellExecute = true
                    });
                    
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Download URL is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening download link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void RemindLaterButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
