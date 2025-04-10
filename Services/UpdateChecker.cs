using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using AfbeeldingenUitzoeken.Models;

namespace AfbeeldingenUitzoeken.Services
{
    public class UpdateChecker
    {
        private readonly string _updateUrl;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Creates a new instance of the UpdateChecker
        /// </summary>
        /// <param name="updateUrl">URL to the version information JSON file</param>
        public UpdateChecker(string updateUrl)
        {
            _updateUrl = updateUrl;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Check if an update is available
        /// </summary>
        /// <returns>True if an update is available, false otherwise</returns>
        public async Task<(bool IsUpdateAvailable, Version LatestVersion, string DownloadUrl)> CheckForUpdateAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync(_updateUrl);
                var versionInfo = JsonSerializer.Deserialize<VersionResponse>(response);

                if (versionInfo == null)
                    return (false, VersionInfo.CurrentVersion, string.Empty);

                var latestVersion = new Version(versionInfo.Version);
                var currentVersion = VersionInfo.CurrentVersion;

                // Compare versions
                bool isUpdateAvailable = latestVersion > currentVersion;

                return (isUpdateAvailable, latestVersion, versionInfo.DownloadUrl);
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the application
                Console.WriteLine($"Error checking for updates: {ex.Message}");
                return (false, VersionInfo.CurrentVersion, string.Empty);
            }
        }
    }

    /// <summary>
    /// Represents the JSON response from the update server
    /// </summary>
    public class VersionResponse
    {
        public string Version { get; set; }
        public string DownloadUrl { get; set; }
        public string ReleaseNotes { get; set; }
        public DateTime ReleaseDate { get; set; }
    }
}
