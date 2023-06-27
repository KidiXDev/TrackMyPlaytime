using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI.Notifications;

namespace TMP.NET.Modules
{
    public class UpdateChecker
    {
        public static bool updateAvailable = false;
        public bool cooldown;
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
                        if (res == MessageBoxResult.Yes)
                            OpenUpdateLink();
                    }
                    else
                    {
                        MessageBox.Show("You are already on the latest version", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    await Task.Delay(5000);
                    cooldown = false;
                }
                catch (Exception ex)
                {
                    MainWindow.log.Warn("Checking update failed", ex);
                    MessageBox.Show($"Update check failed\nInfo: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        public async Task CheckForUpdateOnBackground()
        {
            if (updateAvailable)
            {
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
                        ShowUpdateNotification("Update is available", "Click this notification to visit the download page");
                    }
                }
                catch (Exception ex)
                {
                    MainWindow.log.Warn("Checking update failed", ex);
                }
            }
        }

        public void OpenUpdateLink()
        {
            string repoUrl = "https://trackmyplaytime.netlify.app/";
            Process.Start(new ProcessStartInfo { FileName = repoUrl, UseShellExecute = true });
        }

        private void ShowUpdateNotification(string baloonMsg, string message)
        {
            new ToastContentBuilder()
                .AddArgument("openweb", "web")
                .AddText("Update is available")
                .AddText("Click this notification to visit the download page")
                .Show();
        }

        private void ToastActivated(ToastNotification sender, object args)
        {
            Console.WriteLine("Sus");
            OpenUpdateLink();
        }
    }
}
