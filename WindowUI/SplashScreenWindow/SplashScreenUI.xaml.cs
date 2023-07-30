using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TMP.NET.Modules;
using System;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMP.NET.Properties;
using System.Net;
using System.Windows.Media.Animation;

namespace TMP.NET.WindowUI.SplashScreenWindow
{
    /// <summary>
    /// Interaction logic for SplashScreenUI.xaml
    /// </summary>
    public partial class SplashScreenUI : Window
    {
#if DEBUG
        private readonly string _ListData_n = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KidiXDev\\TrackMyPlaytime\\Debug\\listdata.kdb");
        private readonly string _Config = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KidiXDev\\TrackMyPlaytime\\Debug\\config.cfg");
#else
        private readonly string _ListData_n = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KidiXDev\\TrackMyPlaytime\\listdata.kdb");
        private readonly string _Config = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KidiXDev\\TrackMyPlaytime\\config.cfg");
#endif

        string[] args;

        private ObservableCollection<GameList> i_List = new ObservableCollection<GameList>();
        private Config setting = new Config();
        private Config.FilterConfig filterSetting = new Config.FilterConfig();
        private Config.ContentConfig cc = new Config.ContentConfig();

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
                this.Dispatcher.Invoke(() => labelProgress.Content = "Loading Configuration...");
                await Task.Delay(300);
                await LoadingSetting();

                this.Dispatcher.Invoke(() => labelProgress.Content = "Checking for updates...");
                if (setting.AutoCheckUpdate)
                {
                    await CheckForUpdate(); // Check for available update
                }

                this.Dispatcher.Invoke(() => labelProgress.Content = "Loading Data..." );
                await LoadingData(); // Loading game library data

                this.Dispatcher.Invoke(() =>
                {
                    labelProgress.Content = "Load Complete.";
                    var window = new MainWindow(i_List, setting, filterSetting, cc);
                    MainWindow.HandleParameter(args);
                    Application.Current.MainWindow = window;
                    window.Show(); // show MainWindow

                    this.Close();
                });
            });
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
                    this.Dispatcher.Invoke(() => {
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
            await DeserializeData(_ListData_n);
        }

        private async Task LoadingSetting()
        {
            await DeserializeSetting(_Config);
        }
    }
}
