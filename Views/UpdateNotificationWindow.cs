using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AfbeeldingenUitzoeken.Views
{
    public partial class UpdateNotificationWindow : Window
    {
        public UpdateNotificationWindow(string currentVersion, string newVersion, string downloadUrl)
        {
            InitializeComponent();
            
            // Set version information
            CurrentVersionText.Text = currentVersion;
            NewVersionText.Text = newVersion;
            DownloadUrl = downloadUrl;
        }
        
        private string DownloadUrl { get; set; }
        
        private void InitializeComponent()
        {
            // Create the window
            this.Title = "Update Available";
            this.Width = 400;
            this.Height = 250;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.ResizeMode = ResizeMode.NoResize;
            this.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
            
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
                Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                Margin = new Thickness(0, 0, 0, 15),
                HorizontalAlignment = HorizontalAlignment.Center
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
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 5)
            };
            contentPanel.Children.Add(currentVersionPanel);
            
            TextBlock currentVersionLabel = new TextBlock
            {
                Text = "Current version: ",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224))
            };
            currentVersionPanel.Children.Add(currentVersionLabel);
            
            CurrentVersionText = new TextBlock
            {
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224))
            };
            currentVersionPanel.Children.Add(CurrentVersionText);
            
            // New version
            StackPanel newVersionPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 5)
            };
            contentPanel.Children.Add(newVersionPanel);
            
            TextBlock newVersionLabel = new TextBlock
            {
                Text = "New version: ",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224))
            };
            newVersionPanel.Children.Add(newVersionLabel);
            
            NewVersionText = new TextBlock
            {
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(91, 192, 222))
            };
            newVersionPanel.Children.Add(NewVersionText);
            
            // Info text
            TextBlock infoText = new TextBlock
            {
                Text = "Would you like to download the new version?",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 20, 0, 0)
            };
            contentPanel.Children.Add(infoText);
            
            // Buttons
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };
            Grid.SetRow(buttonPanel, 2);
            mainGrid.Children.Add(buttonPanel);
            
            Button downloadButton = new Button
            {
                Content = "Download",
                Width = 100,
                Height = 35,
                Margin = new Thickness(10, 0, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(91, 192, 222)),
                Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            downloadButton.Click += DownloadButton_Click;
            buttonPanel.Children.Add(downloadButton);
            
            Button remindLaterButton = new Button
            {
                Content = "Remind Later",
                Width = 100,
                Height = 35,
                Margin = new Thickness(10, 0, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(62, 62, 62)),
                Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            remindLaterButton.Click += RemindLaterButton_Click;
            buttonPanel.Children.Add(remindLaterButton);
        }
        
        // Text blocks for version display
        private TextBlock CurrentVersionText { get; set; }
        private TextBlock NewVersionText { get; set; }
        
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open the download URL in the default browser
                Process.Start(new ProcessStartInfo
                {
                    FileName = DownloadUrl,
                    UseShellExecute = true
                });
                
                this.DialogResult = true;
                this.Close();
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
