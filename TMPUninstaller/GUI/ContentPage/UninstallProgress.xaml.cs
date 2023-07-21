using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TMPUninstaller.Ext;

namespace TMPUninstaller.GUI.ContentPage
{
    /// <summary>
    /// Interaction logic for UninstallProgress.xaml
    /// </summary>
    public partial class UninstallProgress : Page
    {
        private RegistryCommand _reg = new RegistryCommand();
        private string[] fileList =
        {
            "tmp.exe", "DiscordRPC.dll", "handler.exe", "log4net.dll", "Newtonsoft.Json.dll", "TMP.NET.exe.config",
            "tmp.exe.config", "Microsoft.Toolkit.Uwp.Notifications.dll", "System.ValueTuple.dll", "VndbSharp.dll"
        };

        bool keepGameList, keepConfig;
        public UninstallProgress(bool keepGameList, bool keepConfig)
        {
            InitializeComponent();

            this.keepGameList = keepGameList;
            this.keepConfig = keepConfig;

            Task.Run(() => UninstallTask(_reg.GetInstallDir()));
        }

        private void UninstallTask(string installDir)
        {
            try
            {
                string illegalChars = new string(Path.GetInvalidPathChars()) + new string(Path.GetInvalidFileNameChars());
                foreach (char c in illegalChars)
                {
                    if (c != ':' && c != '\\')
                    {
                        installDir = installDir.Replace(c.ToString(), "");
                    }
                }

                foreach (var file in fileList)
                {
                    Console.WriteLine("Combined: " + Path.Combine(installDir, file));
                }

                int totalEntries = fileList.Length;
                int completedEntries = 0;

                // Start operation
                foreach (var file in fileList)
                {
                    if(File.Exists(Path.Combine(installDir, file)))
                        File.Delete(Path.Combine(installDir, file));

                    completedEntries++;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        UpdateProgressBar(totalEntries, completedEntries);
                    });
                }

                _reg.DeleteRegistry();

                string _ListData_n = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KidiXDev\\TrackMyPlaytime\\listdata.kdb");
                string _Config = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KidiXDev\\TrackMyPlaytime\\config.cfg");
                if (!keepGameList)
                {
                    if (File.Exists(_ListData_n))
                    {
                        File.Delete(_ListData_n);
                    }
                }

                if (!keepConfig)
                {
                    if (File.Exists(_Config))
                    {
                        File.Delete(_Config);
                    }
                }

                if (_reg.isCurrentUser)
                {
                    if(File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Track My Playtime.lnk"))
                        File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Track My Playtime.lnk");
                }
                else
                {
                    if(File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory) + "\\Track My Playtime.lnk"))
                        File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory) + "\\Track My Playtime.lnk");
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.NavigationService.Navigate(new UninstallComplete());
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Application.Current.Dispatcher.Invoke(() =>
                {
                this.NavigationService.Navigate(new UninstallFailed(ex.Message));
                });
            }
        }

        private void UpdateProgressBar(int totalEntries, int completedEntries)
        {
            double progressPercentage = (double)completedEntries / totalEntries * 100;
            pbProgress.Value = progressPercentage;
            labelPercent.Content = $"{progressPercentage}%";
        }
    }
}
