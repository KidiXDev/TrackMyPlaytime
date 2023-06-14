using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace TMP.NET.Modules
{
    public class UpdateChecker
    {
        private bool updateAvailable = false;
        private const string repoURL = "https://api.github.com/repos/KidiXDev/TrackMyPlaytime/releases/latest";

        public async Task CheckForUpdate()
        {
            if (updateAvailable)
            {
                var res = MessageBox.Show("An update is available, would you like to visit the download page?", "Notification", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                    OpenUpdateLink();

                return;
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)");
                try
                {
                    HttpResponseMessage hrm = await client.GetAsync(repoURL);
                    hrm.EnsureSuccessStatusCode();

                    string responseBody = await hrm.Content.ReadAsStringAsync();
                    dynamic release = JsonConvert.DeserializeObject(responseBody);

                    string latestVersionString = release.tag_name;
                    string currentVersionString = Assembly.GetExecutingAssembly().GetName().Version.ToString();

                    Version latestVersion = Version.Parse(latestVersionString.TrimStart('v'));
                    Version currentVersion = Version.Parse(currentVersionString);

                    if (latestVersion > currentVersion)
                    {
                        updateAvailable = true;
                        var res = MessageBox.Show("An update is available, would you like to visit the download page?", "Notification", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if(res == MessageBoxResult.Yes)
                            OpenUpdateLink();
                    }
                    else
                    {
                        MessageBox.Show("You are already on the latest version", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Update check failed\nInfo: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private static void OpenUpdateLink()
        {
            string repoUrl = "https://github.com/KidiXDev/TrackMyPlaytime/releases";
            Process.Start(new ProcessStartInfo { FileName = repoUrl, UseShellExecute = true });
        }
    }
}
