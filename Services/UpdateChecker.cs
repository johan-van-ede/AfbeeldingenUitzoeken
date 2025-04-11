using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using AfbeeldingenUitzoeken.Models;
using AfbeeldingenUitzoeken.Views;

namespace AfbeeldingenUitzoeken.Services
{
    public class UpdateChecker
    {
        private readonly string _updateServerUrl;
        private readonly HttpClient _httpClient;

        // Update model class
        public class UpdateInfo
        {
            public string? Version { get; set; }
            public string? DownloadUrl { get; set; }
            public string? ReleaseNotes { get; set; }
            public DateTime ReleaseDate { get; set; }
        }

        public UpdateChecker(string updateServerUrl)
        {
            _updateServerUrl = updateServerUrl;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Checks if an update is available by comparing the current version with the latest version from the server
        /// </summary>
        /// <returns>Tuple containing: isUpdateAvailable, latestVersion, and downloadUrl</returns>
        public async Task<(bool isUpdateAvailable, string latestVersion, string downloadUrl)> CheckForUpdateAsync()
        {
            try
            {
                // Get JSON from the update server
                var response = await _httpClient.GetStringAsync(_updateServerUrl);
                
                // Parse the JSON response
                using var jsonDoc = JsonDocument.Parse(response);
                var root = jsonDoc.RootElement;
                
                // Extract version and download URL
                var latestVersionString = root.GetProperty("Version").GetString() ?? string.Empty;
                var downloadUrl = root.GetProperty("DownloadUrl").GetString() ?? string.Empty;
                
                // Parse the server version and compare with current version
                if (Version.TryParse(latestVersionString, out var latestVersion) && !string.IsNullOrEmpty(downloadUrl))
                {
                    var currentVersion = Models.VersionInfo.CurrentVersion;
                    
                    // If server version is greater than current version, an update is available
                    return (latestVersion > currentVersion, latestVersionString, downloadUrl);
                }
                
                return (false, string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking for updates: {ex.Message}");
                return (false, string.Empty, string.Empty);
            }
        }

        /// <summary>
        /// Shows the update notification window with the version information
        /// </summary>
        public void ShowUpdateNotification(string currentVersion, string newVersion, string downloadUrl)
        {
            if (string.IsNullOrEmpty(downloadUrl))
            {
                return; // Don't show notification if downloadUrl is empty
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var notificationWindow = new UpdateNotificationWindow(
                    currentVersion,
                    newVersion,
                    downloadUrl);
                
                notificationWindow.Show();
            });
        }
    }
}
