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

    // Online update checking has been removed as per user request.

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
