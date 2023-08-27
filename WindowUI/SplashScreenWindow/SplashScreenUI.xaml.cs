using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using TMP.NET.Modules;
using TMP.NET.Modules.Ext;
using VndbSharp;
using VndbSharp.Models.Dumps;

namespace TMP.NET.WindowUI.SplashScreenWindow
{
    /// <summary>
    /// Interaction logic for SplashScreenUI.xaml
    /// </summary>
    public partial class SplashScreenUI : Window
    {
        private readonly string GameLibraryDir = PathFinderManager.GameLibraryDir;
        private readonly string ConfigFileDir = PathFinderManager.AppConfigDir;

        string[] args;

        private ObservableCollection<GameList> i_List = new ObservableCollection<GameList>();
        private Config setting = new Config();
        private Config.FilterConfig filterSetting = new Config.FilterConfig();
        private Config.ContentConfig cc = new Config.ContentConfig();
        private Config.VndbConfig vndb = new Config.VndbConfig();

        /// <summary>
        /// Used to load <see langword="GameList"/> or <see langword="Library"/> from specified directory <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath"></param>
        /// <remarks><see langword="GameList"/> is <see cref="i_List"/>.</remarks>
        private async Task DeserializeData(string filePath)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        using (var m_FStream = new FileStream(filePath, FileMode.Open))
                        {
                            var m_BFormatter = new BinaryFormatter();
                            i_List = (ObservableCollection<GameList>)m_BFormatter.Deserialize(m_FStream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load Game List\nIt is possible that it is caused by a corrupted file\nInfo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    MainWindow.log.Error("Failed to load Game List", ex);
                }
            });
        }
        /// <summary>
        /// Used to load <see langword="Configuration"/> from specified directory <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath"></param>
        /// <remarks>See also <see cref="Config"/>.</remarks>
        private async Task DeserializeSetting(string filePath)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        setting = JsonConvert.DeserializeObject<Config>(File.ReadAllText(filePath));

                        filterSetting = setting.filterConfig;
                        cc = setting.contentConfig;
                        vndb = setting.vndbConfig;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load Setting\nIt is possible that it is caused by a corrupted file\nInfo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    MainWindow.log.Error("Failed load setting", ex);
                }
            });
        }

        public SplashScreenUI(string[] args)
        {
            InitializeComponent();
            this.args = args;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            labelVersion.Content = $"Version: v{version.Major}.{version.Minor}.{version.Build}";

            var fadeInStoryboard = FindResource("FadeIn") as Storyboard;
            if (fadeInStoryboard != null)
                BeginStoryboard(fadeInStoryboard);

            Task.Run(async () =>
            {
                /*this.Dispatcher.Invoke(() => labelProgress.Content = "Verifying file integrity...");
                await VerifyFileIntegrity();*/

                this.Dispatcher.Invoke(() => labelProgress.Content = "Loading Configuration...");
                await LoadingSetting();

                this.Dispatcher.Invoke(() => labelProgress.Content = "Checking for updates...");
                if (setting.AutoCheckUpdate)
                    await CheckForUpdate(); // Check for available update

                this.Dispatcher.Invoke(() => labelProgress.Content = "Checking for trait dumps update...");
                await CheckForTraitDumps(); // Check if trait dumps updates every 3 days after last file is modified

                this.Dispatcher.Invoke(() => labelProgress.Content = "Loading Data...");
                await LoadingData(); // Loading game library data

                this.Dispatcher.Invoke(() =>
                {
                    labelProgress.Content = "Load Complete.";
                    Console.WriteLine($"Spoiler Level: {vndb.SpoilerSetting} Explicit Content: {vndb.ShowExplicitContent}");
                    var window = new MainWindow(i_List, setting, filterSetting, cc, vndb);
                    MainWindow.HandleParameter(args);
                    Application.Current.MainWindow = window;
                    window.Show(); // show MainWindow

                    this.Close();
                });
            });
        }

        private async Task CheckForTraitDumps()
        {
            var trait = await GetTraitDumps();
            if (trait != null)
                await SerializeTraitDumps(trait);
        }

        private async Task<List<Trait>> GetTraitDumps()
        {
            var traitDumpDir = PathFinderManager.TraitDumpsDir;

            if (!File.Exists(traitDumpDir) || CheckTraitDumpOutOfDateStatus(traitDumpDir))
            {
                Console.WriteLine("Updating Trait Dumps...");
                this.Dispatcher.Invoke(() => labelProgress.Content = "Updating trait dumps...");
                return (await VndbUtils.GetTraitsDumpAsync()).ToList();
            }

            return null;
        }

        private bool CheckTraitDumpOutOfDateStatus(string filePath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                DateTime lastModified = fileInfo.LastWriteTime;
                DateTime currentDate = DateTime.Now;
                TimeSpan difference = currentDate - lastModified;

                // Checks whether the file has passed 3 days since it was last created
                if (difference.TotalDays >= 3)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return true;
            }
        }

        private async Task SerializeTraitDumps(List<Trait> traits)
        {
            string fileTraitDumps = PathFinderManager.TraitDumpsDir;

            try
            {
                if(!Directory.Exists(Path.GetDirectoryName(fileTraitDumps)))
                    Directory.CreateDirectory(Path.GetDirectoryName(fileTraitDumps));

                string jsonTraitsData = JsonConvert.SerializeObject(traits, Formatting.Indented);

                using (StreamWriter file = new StreamWriter(fileTraitDumps))
                {
                    await file.WriteAsync(jsonTraitsData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task CheckForUpdate()
        {
            UpdateChecker update = new UpdateChecker();
            if (await update.CheckForUpdateOnBackground())
            {
                this.Dispatcher.Invoke(() => { labelProgress.Content = "Downloading Update..."; pbProgress.IsIndeterminate = false; });
                //await DownloadFileAsync("https://github.com/KidiXDev/TrackMyPlaytime/releases/latest/download/TMP.Setup.exe", "D:\\TMPNET\\Track My Playtime\\New folder\\TMP.Setup.exe");
            }
        }

        private Task DownloadFileAsync(string url, string outputPath)
        {
            WebClient webClient = new WebClient();

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            webClient.DownloadProgressChanged += (s, e) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    pbProgress.Value = e.ProgressPercentage;
                });
            };

            webClient.DownloadFileCompleted += (s, e) =>
            {
                if (e.Error != null)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        labelProgress.Content = "Update Failed...";
                        pbProgress.Value = 0;
                        pbProgress.IsIndeterminate = true;
                    });
                    tcs.SetException(e.Error);
                }
                else if (e.Cancelled)
                {
                    tcs.SetCanceled();
                }
                else
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        labelProgress.Content = "Update Complete...";
                        pbProgress.Value = 0;
                        pbProgress.IsIndeterminate = true;
                    });
                    tcs.SetResult(true);
                }
            };

            webClient.DownloadFileAsync(new Uri(url), outputPath);

            return tcs.Task;
        }

        private async Task LoadingData()
        {
            await DeserializeData(GameLibraryDir);
        }

        private async Task LoadingSetting()
        {
            await DeserializeSetting(ConfigFileDir);
        }

        private async Task VerifyFileIntegrity()
        {
            CheckIntegrity chk = new CheckIntegrity();
            var missingFile = await chk.VerifyFileAsync();

            if (missingFile.Count > 0)
            {
                Console.WriteLine(missingFile.Count + " Missing File Found!");
                this.Dispatcher.Invoke(() => labelProgress.Content = "Repairing file integrity...");
                await chk.RepairingMissingFile(missingFile, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KidiXDev\\TrackMyPlaytime\\orig.zip"), AppDomain.CurrentDomain.BaseDirectory);

                this.Dispatcher.Invoke(() => labelProgress.Content = "Verifying file integrity...");
                await VerifyFileIntegrity();
                this.Dispatcher.Invoke(() =>
                {
                    ProcessStartInfo Info = new ProcessStartInfo();
                    Info.Arguments = "/C choice /C Y /N /D Y /T 2 & START \"\" \"" + Assembly.GetEntryAssembly().Location + "\"";
                    Info.WindowStyle = ProcessWindowStyle.Hidden;
                    Info.CreateNoWindow = true;
                    Info.FileName = "cmd.exe";
                    Process.Start(Info);
                    Application.Current.Shutdown();
                });
            }
        }
    }
}
