/*
 *   Status: Released
 *   Publishing Status: Public Released
 *
 *   Project Started
 *   ===============
 *   [16 April 2023]
 *
 *   Changelog:
 *   
 *       [18 May 2023]
 *       =============
 *       v1.0.0a
 *       =======
 *       Updated Features:
 *       - Discord Rich Presence
 *       - Game Shortcut
 *       - More Settings
 *       
 *       [1 June 2023]
 *       Updated Features:
 *       - Import GameList From TMP Java Version Support
 *       - Optimize Performance
 *       - Fixed Some Bug
 *       - Logging Feature
 *       
 *       [3 June 2023]
 *       Updated Features:
 *       - Screenshot
 *       - Global Keyboard Hook
 *       
 *       [4 June 2023]
 *       Updated Features:
 *       - Argument Listener at runtime
 *       - Change the GameTitle label to Textblock inside the ViewBox to achieve automatic font re-size
 *       - Add Textractor Launch Delay Setting
 *       
 *       [12 June 2023]
 *       Updated Features:
 *       - Make Installer and Uninstaller
 *       
 *       [14 June 2023]
 *       ==============
 *       v1.0.0
 *       ======
 *       - First Public Release
 *       
 *       [19 June 2023]
 *       ==============
 *       v1.1.0
 *       ======
 *       Updated Features:
 *       - Optimized Performance
 *       - Fixed crash issue #1
 *       - Added about window
 *       - Fullscreen Support
 *       
 *       [27 June 2023]
 *       ==============
 *       v1.2.0
 *       ======
 *       Updated Features:
 *       - Added automatic update checker
 *       - Change notification system
 *       - Added screenshots sound effect
 *       - Change UI Update Before and After game launch
 *       
 *       [29 June 2023]
 *       ==============
 *       v1.2.1
 *       ======
 *       Bug Fixed:
 *       - Fixed word wrap on installer
 *       - Some Typo and bug fixes
 *       - Installer optimization
 *       - Added a small detail to the main menu
 *       
 *       [11 July 2023]
 *       ==============
 *       v1.3.0
 *       =======
 *       New Features:
 *       - New GameList ContextMenu feature
 *       - Search menu
 *       - Added new detail animation on some UI Element
 *       - Properties window
 *       - Sort Filter option
 *       - Add more UI Improvement
 *       - Enable Screenshot Toogle
 *       
 *       Bug fixed:
 *       - Typo on crash log fixed
 *       - Typo message box on crash info
 *       - WordWrap issue on playtime label
 *       - Fixed minor issue when program required restart after change Discord RPC setting
 *       
 *       Temporary Removed Feature:
 *       - Import GameList
 *       
 *       [12 July 2023]
 *       ==============
 *       v1.3.1
 *       =======
 *       Bug fixed:
 *       - Shortcut issue (running the game via the shortcut doesn't work as it should,
 *       could cause the game list data to be lost that had been included in the library).
 *       - Incorrectly selected index right after changing sort type
 *       - Delete game from library issue when you force to delete using context menu while your game is still running
 *       
 *       UI Improvement:
 *       - Change setting UI layout (again)
 *       - Removed Create shortcut and Delete button from Edit Game Window
 *       
 *       New Minor Feature:
 *       - Added new Date Created sort menu
 *       
 *       [21 July 2023]
 *       ==============
 *       v2.0.0 beta
 *       ============
 *       MAJOR UPDATE
 *       ============
 *       New feature:
 *       - New Home Screen UI Rework
 *       You can now provide a description, game tag, release date, and link to a specific website, then display it to the library
 *       - The level of opcity and the intensity of the blur on background image can be fully customized via the settings
 *       - VNDB integrity support (Work in progress)
 *       - Difference screenshot API
 *       - Custom screenshot output directory
 *       - Changed image artwork type from landscape to portrait
 *       - Performance improvement
 *       - Setting UI Rework
 *       - Game setting UI Rework
*/

using log4net;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using TMP.NET.Modules;
using TMP.NET.WindowUI;
using TMP.NET.WindowUI.ContentWindow;

namespace TMP.NET
{
    /// <summary>
    /// All Program and UI logic is on here
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variable & Object Instance

        public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

#if DEBUG
        private readonly string _ListData_n = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KidiXDev\\TrackMyPlaytime\\Debug\\listdata.kdb");
        private readonly string _Config = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KidiXDev\\TrackMyPlaytime\\Debug\\config.cfg");
#else
        private readonly string _ListData_n = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KidiXDev\\TrackMyPlaytime\\listdata.kdb");
        private readonly string _Config = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KidiXDev\\TrackMyPlaytime\\config.cfg");
#endif

        //private ObservableCollection<GameListContentControl> _contentControll = new ObservableCollection<GameListContentControl>();
        private GameListContentControl _contentControl;
        private ObservableCollection<GameList> i_List;
        public ObservableCollection<GameList> i_listv { get { return i_List; } }

        private readonly WindowExtension _wext;

        private List<ImportList> importList;

        private Config setting;
        private Config.FilterConfig filterSetting;
        private Config.ContentConfig cc;

        private DateTime _dateTime;
        private readonly GUIDGen _gen = new GUIDGen();

        private Modules.DiscordRPC discord = new Modules.DiscordRPC();

        /// <summary>
        /// Get CommandLine Argument
        /// </summary>
        private readonly string[] args = Environment.GetCommandLineArgs();

        private GameList _currentSelectedList;
        private GameList _lastSelectedList;
        private GameList _runningGame;

        public enum AppState
        {
            Idle, Initialize, Running
        }

        public AppState state = AppState.Idle;
        #endregion

        #region Serialize And Deserialize Data
        /// <summary>
        /// Used to save <see langword="GameList"/> or <see langword="Library"/> to specified directory <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath"></param>
        /// <remarks><see langword="GameList"/> is <see cref="i_List"/>.</remarks>
        private void SerializeData(string filePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }

            using (var m_FStream = new FileStream(filePath, FileMode.Create))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(m_FStream, i_List);
            }
        }
        /// <summary>
        /// Used to save <see langword="Configuration"/> to specified directory <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath"></param>
        /// <remarks>See also <see cref="Config"/>.</remarks>
        private void SerializeSetting(string filePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }

            setting.filterConfig = filterSetting;
            setting.contentConfig = cc;

            File.WriteAllText(filePath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        }
        /// <summary>
        /// Used to load <see langword="GameList"/> or <see langword="Library"/> from specified directory <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath"></param>
        /// <remarks><see langword="GameList"/> is <see cref="i_List"/>.</remarks>

        private bool DeserializeImportList(string dir)
        {
            try
            {
                if (File.Exists(dir))
                {
                    importList = JsonConvert.DeserializeObject<List<ImportList>>(File.ReadAllText(dir));
                    string text = $"{importList.Count} Game titles detected, do you want to add them to the game list?";
                    var result = MessageBox.Show(text, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes) return true;
                    else return false;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to Import GameList\nIt is possible that it is caused by a corrupted file\nInfo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                log.Info("Failed to import", ex);
                return false;
            }
        }
        #endregion

        /// <summary>
        /// Used to load configuration data and game libraries.
        /// </summary>
        /// <remarks>This method used to run before <see cref="Window_Loaded(object, RoutedEventArgs)"/>.</remarks>
        private void FirstLoad()
        {
            //DeserializeData(_ListData_n);
            //DeserializeSetting(_Config);
            _contentControl = new GameListContentControl(setting, cc);

            this.Width = setting.Width;
            this.Height = setting.Height;
            this.Top = setting.Top;
            this.Left = setting.Left;
            if (setting.Maximized)
                this.WindowState = WindowState.Maximized;

            if (setting.EnabledRichPresence)
                discord.Initialize();

            if (setting.LowSpecMode)
                SolidCommandBrush.Opacity = 1;
        }

        /// <summary>
        /// Execute screenshot process.
        /// </summary>
        /// <remarks>See also <seealso cref="CaptureHandler.TakeScreenshot(Process)"/></remarks>
        private void ScreenshotExec()
        {
            try
            {
                // If screenshot is disabled via setting
                if (!setting.EnabledScreenshot)
                    return;

                // If game window is not focused, return.
                if (!CaptureHandler.IsWindowFocused(proc))
                    return;

                // Check if game title has invalid char or not
                var title = _runningGame.GameName;
                string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                foreach (char invalidChar in invalidChars)
                {
                    title = title.Replace(invalidChar.ToString(), "");
                }

                string imagePath = Path.Combine(setting.ScreenshotFolderDir, title);

                if (!Directory.Exists(imagePath))
                    Directory.CreateDirectory(imagePath);

                // Save captured image.
                string fileName = $"{proc.ProcessName} {DateTime.Now.ToString("dd MMM, yyyy HH.mm.ss")}.png";
                string combined = Path.Combine(imagePath, fileName);

                // Capture screen based on what API is used
                if (setting.ScreenshotApiIndex == 0)
                {
                    var img = CaptureHandler.TakeScreenshot(proc);
                    img.Save(combined, ImageFormat.Png);
                }
                else if (setting.ScreenshotApiIndex == 1)
                {
                    var img = ScreenCapture.CaptureDesktop();
                    img.Save(combined, ImageFormat.Png);
                }
                else
                {
                    var img = ScreenCapture.CaptureActiveWindow();
                    img.Save(combined, ImageFormat.Png);
                }


                /*var img = ScreenCapture.CaptureWindow(proc.MainWindowHandle);
                img.Save(combined, ImageFormat.Png);*/

                /*var img = DXScreenCapture.CaptureProcessScreenshot(proc);
                img.Save(combined, ImageFormat.Png);*/

                // Play capture sfx.
                var str = Application.GetResourceStream(new Uri("pack://application:,,,/TMP.NET;component/Resources/capture-sfx.wav")).Stream;
                SoundPlayer sp = new SoundPlayer(str);
                sp.Play();

                ShowScreenshotNotification("Screenshot Taken!", $"Saved in: {combined}", combined);
            }
            catch (Exception ex)
            {
                log.Warn("Failed take screenshot", ex);
            }
        }

        #region ParameterPasser
        public static IntPtr WindowHandle { get; private set; }

        internal static void HandleParameter(string[] args)
        {
            if (Application.Current?.MainWindow is MainWindow mainWindow)
            {
                if (args != null && args.Length > 0 && args[0]?.IndexOf("tmpdotnet://launch", StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    var url = new Uri(args[0]);
                    var segments = url.Segments;
                    var launchId = segments[segments.Length - 1].TrimEnd('/');

                    if (mainWindow.state == AppState.Running)
                    {
                        MessageBox.Show("Other programs are still running.\nInfo: Please close other programs first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (mainWindow.ShortcutURLStarter(launchId))
                    {
                        mainWindow.StateChecker();
                        if (mainWindow.state == AppState.Initialize)
                        {
                            mainWindow.StartProcess();
                        }
                    }
                }
            }
        }

        private static IntPtr HandleMessages(IntPtr handle, int message, IntPtr wParameter, IntPtr lParameter, ref Boolean handled)
        {
            if (handle != MainWindow.WindowHandle)
                return IntPtr.Zero;

            var data = UnsafeNative.GetMessage(message, lParameter);

            if (data != null)
            {
                if (Application.Current.MainWindow == null)
                    return IntPtr.Zero;

                if (Application.Current.MainWindow.WindowState == WindowState.Minimized)
                    Application.Current.MainWindow.WindowState = WindowState.Normal;

                UnsafeNative.SetForegroundWindow(new WindowInteropHelper(Application.Current.MainWindow).Handle);

                var args = data.Split(' ');
                HandleParameter(args);
                handled = true;
            }

            return IntPtr.Zero;
        }
        #endregion


        #region Shortcut Utilize
        /// <summary>
        /// Check if <see langword="StartShortcut"/> is <see langword="True"/> or not, if <see langword="True"/> then process to <see cref="StartProcess"/>
        /// </summary>
        /// <remarks>See also: <br/> <seealso cref="StartShortcut"/> <br/> <seealso cref="StartProcess"/></remarks>
        private void LoadShortcut()
        {
            if (StartShortcut())
            {
                StateChecker();
                if (state == AppState.Initialize)
                    StartProcess();
            }
            else
            {
                if (setting.SelectedIndex >= 0 && LV_List.Items.Count > 0)
                {
                    LV_List.SelectedIndex = setting.SelectedIndex;
                    if (LV_List.Items.Count >= setting.SelectedIndex)
                        LV_List.ScrollIntoView(LV_List.Items[setting.SelectedIndex]);
                }
            }
        }

        /// <summary>
        /// Check if argument contains "launch/" or not.
        /// </summary>
        /// <returns><see langword="True"/> if contains defined sentences.</returns>
        private bool StartShortcut()
        {
            if (args.Length >= 1)
            {
                foreach (var launch in args)
                {
                    if (launch.Contains("launch/"))
                    {
                        string key = launch.Substring(launch.LastIndexOf('/') + 1);
                        foreach (var l in i_List)
                        {
                            if (l.GUID != null && l.GUID.Equals(key))
                            {
                                LV_List.SelectedItem = l;
                                LV_List.ScrollIntoView(l);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Check if <paramref name="guid"/> is exist on <see cref="i_List"/>
        /// </summary>
        /// <param name="guid"></param>
        /// <returns><see langword="True"/> if exist and process to <see cref="HandleParameter"/> <br/> otherwise is <see langword="False"/>.</returns>
        private bool ShortcutURLStarter(string guid)
        {
            foreach (var l in i_List)
            {
                if (l.GUID != null && l.GUID.Equals(guid))
                {
                    LV_List.SelectedItem = l;
                    LV_List.ScrollIntoView(l);
                    return true;
                }
            }
            return false;
        }
        #endregion

        /// <summary>
        /// Show screenshot toast notification.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="imgDir"></param>
        private void ShowScreenshotNotification(string title, string message, string imgDir)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                new ToastContentBuilder()
                    .AddArgument("openss", imgDir)
                    .AddText(title)
                    .AddText(message)
                    .AddHeroImage(new Uri(imgDir))
                    .Show();
            }));
        }

        private void DatabaseUpdater()
        {
            foreach (var item in i_List)
            {
                if (item.DatabaseVersion == null)
                {
                    item.DateCreated = DateTime.Now;
                    item.DatabaseVersion = new Version(1, 0, 0);
                }
            }
            SerializeData(_ListData_n);
        }

        private void Window_SourceInitialized(object sender, EventArgs ea)
        {
            HwndSource hwndSource = (HwndSource)HwndSource.FromVisual((Window)sender);
            hwndSource.AddHook(_wext.DragHook);

            _wext.aspectRatio = this.Width / this.Height;
        }

        private bool UserFilter(object item)
        {
            if (string.IsNullOrEmpty(tbSearch.Text))
                return true;
            else
                return ((item as GameList).GameName.IndexOf(tbSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public MainWindow(ObservableCollection<GameList> list, Config setting, Config.FilterConfig filterSetting, Config.ContentConfig cc)
        {
            InitializeComponent();

            this.i_List = list;
            this.setting = setting;
            this.filterSetting = filterSetting;
            this.cc = cc;

            this.Loaded += (s, e) =>
            {
                MainWindow.WindowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
                HwndSource.FromHwnd(MainWindow.WindowHandle)?.AddHook(new HwndSourceHook(HandleMessages));
            };
            _wext = new WindowExtension();
            Modules.Keyboard.KeyboardHook.KeyCombinationPressed += ScreenshotExec;
            this.SourceInitialized += Window_SourceInitialized;

            DataContext = this;

            FirstLoad();

            DatabaseUpdater();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (state == AppState.Running)
            {
                e.Cancel = true;
                MessageBox.Show("Please close running programs first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (setting.EnabledRichPresence)
                discord.Deinitialize();

            Modules.Keyboard.KeyboardHook.Stop();

            // Save window state
            if (WindowState == WindowState.Maximized)
            {
                setting.Width = RestoreBounds.Width;
                setting.Height = RestoreBounds.Height;
                setting.Top = RestoreBounds.Top;
                setting.Left = RestoreBounds.Left;
                setting.Maximized = true;
            }
            else
            {
                setting.Width = this.Width;
                setting.Height = this.Height;
                setting.Top = this.Top;
                setting.Left = this.Left;
                setting.Maximized = false;
            }

            setting.SelectedIndex = LV_List.SelectedIndex;
            SerializeSetting(_Config);
            ToastNotificationManagerCompat.Uninstall();
            Application.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // Sort GameList
                FilterSort();

                LoadShortcut();

                if (LV_List.SelectedItem == null)
                    btnEdit.IsEnabled = false;
                else
                    btnEdit.IsEnabled = true;

                if (LV_List.SelectedItem == null)
                {
                    _contentControl.DeInit();
                    _contentControl.FreshInit();
                }

                ContentArea.Content = _contentControl;

                if (!setting.FirstTimeInfo)
                {
                    MessageBox.Show("You are using the beta version of \"Track My Playtime\",\nIf you encounter a problem, you can immediately report it to the issues section.\n\nYour feedback has a big impact here...", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    setting.FirstTimeInfo = true;
                }
            }));
        }

        /// <summary>
        /// Check current state of program.
        /// </summary>
        private void StateChecker()
        {
            if (state == AppState.Running)
            {
                btnPlay.Content = "Play";
                btnPlay.Background = System.Windows.Media.Brushes.Green;
                state = AppState.Idle;
                Title = "Track My Playtime";
                Modules.Keyboard.KeyboardHook.Stop();

                if (setting.EnabledRichPresence)
                    discord.ClearCurrentPresence();

                if (ProcessExtension.IsRunning(proc))
                    proc.Kill();
            }
            else if (state == AppState.Idle)
            {
                btnPlay.Content = "Stop";
                btnPlay.Background = System.Windows.Media.Brushes.Red;
                state = AppState.Initialize;
                Modules.Keyboard.KeyboardHook.Start();
            }
        }

        public void FilterSort()
        {
            FilterController fc = new FilterController(filterSetting, i_List, LV_List);
            fc.FilterControl();

            FilterInit();
        }

        private void FilterInit()
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(LV_List.ItemsSource);
            view.Filter = UserFilter;
        }

        #region UI Update & Process Logic
        private void UpdateTracker(GameList game, object selected) // Update Date and Playtime UI
        {
            this.Dispatcher.Invoke(() =>
            {
                try
                {
                    if (game.Tracker == DateTime.MinValue)
                    {
                        label_LastPlayed.Content = "Never";

                        if (game.Playtime == TimeSpan.MinValue)
                        {
                            label_Playtime.Content = "0h 0m 0s";
                        }
                        else
                        {
                            var totalHours = game.Playtime.TotalHours;
                            var hours = (int)Math.Floor(totalHours);
                            string formattedHours = hours.ToString("0");

                            label_Playtime.Content = string.Format("{0}h {1}m {2}s", formattedHours, game.Playtime.Minutes, game.Playtime.Seconds);
                        }
                    }
                    else
                    {
                        if (LV_List.SelectedItem != selected)
                            return;

                        else
                        {
                            if (game.Tracker.Date.Equals(DateTime.Today))
                                label_LastPlayed.Content = "Today";

                            else if (game.Tracker.Date.Equals(DateTime.Today.AddDays(-1)))
                                label_LastPlayed.Content = "Yesterday";

                            else
                                label_LastPlayed.Content = game.Tracker.Date.ToString("dd MMM, yyyy");

                            var totalHours = game.Playtime.TotalHours;
                            var hours = (int)Math.Floor(totalHours);
                            string formattedHours = hours.ToString("0");
                            label_Playtime.Content = string.Format("{0}h {1}m {2}s", formattedHours, game.Playtime.Minutes, game.Playtime.Seconds);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Failed to update tracker", ex);
                }
            });
        }

        private void UpdatePresenceOnPlay(GameList game)
        {
            if (!setting.EnabledRichPresence)
                return;

            string gameName = game.HideGameTitle ? "a game" : game.GameName;
            string imageKey = game.HideImageKey || string.IsNullOrEmpty(game.ImageKey) ? "tmp_logo" : game.ImageKey;
            string trackMyPlaytime = "Track My Playtime";

            if (game.HideGameTitle && game.HideImageKey)
            {
                discord.UpdatePresence("Playing a game", null, "tmp_logo", null);
            }
            else if (game.HideGameTitle)
            {
                discord.UpdatePresence("Playing a game", null, imageKey, null, "tmp_logo", "Track My Playtime");
            }
            else if (game.HideImageKey)
            {
                discord.UpdatePresence($"Playing {gameName}", null, "tmp_logo", null);
            }
            else
            {
                if (!string.IsNullOrEmpty(game.ImageKey))
                    discord.UpdatePresence($"Playing {gameName}", null, imageKey, gameName, "tmp_logo", trackMyPlaytime);
                else
                    discord.UpdatePresence($"Playing {gameName}", null, "tmp_logo", gameName);
            }
        }

        private Process proc = new Process();

        private void TextractorExec(GameList game)
        {
            new Thread(() =>
            {
                try
                {
                    if (!setting.DisableTextractor && !game.DisableTextractor)
                    {
                        if (!File.Exists(setting.x86Directory))
                            throw new FileNotFoundException("Textractor Executable not found\n\nPlease make sure you enter the correct directory, or if you don't want to use Textractor it's a good idea to disable it via Settings");

                        if (!File.Exists(setting.x64Directory))
                            throw new FileNotFoundException("Textractor Executable not found\n\nPlease make sure you enter the correct directory, or if you don't want to use Textractor it's a good idea to disable it via Settings");

                        Thread.Sleep(setting.TextractorDelay);
                        var textractorProc = new Process();
                        if (game.ProgramType.Equals("x86"))
                        {
                            if (string.IsNullOrEmpty(setting.x86Directory))
                                return;

                            textractorProc.StartInfo.FileName = setting.x86Directory;
                            textractorProc.StartInfo.WorkingDirectory = Path.GetDirectoryName(setting.x86Directory);
                            textractorProc.Start();
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(setting.x64Directory))
                                return;

                            textractorProc.StartInfo.FileName = setting.x64Directory;
                            textractorProc.StartInfo.WorkingDirectory = Path.GetDirectoryName(setting.x64Directory);
                            textractorProc.Start();
                        }
                    }
                }
                catch (FileNotFoundException e)
                {
                    MessageBox.Show($"Failed launch Textractor\nInfo: {e.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    log.Warn("Failed launch Textractor", e);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Failed launch Textractor\nInfo: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    log.Error("Failed launch Textractor", e);
                }
            }).Start();

        }

        private void StartProcessWithLauncherHandler(GameList list, Stopwatch timer, object selected)
        {
            string param = string.Empty;
            if (list.LaunchParameter.Length > 0)
            {
                param = string.Join(" ", list.LaunchParameter);
            }
            string[] handlerArgument = { $"\"{list.GamePath}\"", list.RunAsAdmin.ToString(), param };
            string launcherHandlerDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "handler.exe");
            proc.StartInfo.FileName = launcherHandlerDir;
            proc.StartInfo.WorkingDirectory = Path.GetDirectoryName(launcherHandlerDir);
            proc.StartInfo.Arguments = string.Join(" ", handlerArgument);
            proc.StartInfo.Verb = string.Empty;
            Modules.Keyboard.KeyboardHook.Stop();
            new Thread(() =>
            {
                try
                {
                    if (!File.Exists(list.GamePath))
                        throw new FileNotFoundException($"Removed or Missing Program Directory:\n{list.GamePath}");

                    if (!File.Exists(launcherHandlerDir))
                        throw new FileNotFoundException($"Launcher Handler Executable not found.");

                    proc.Start();

                    _runningGame = list;
                    _dateTime = DateTime.Now;
                    list.Tracker = _dateTime;
                    UpdateTracker(list, selected);

                    timer.Start();
                    this.Dispatcher.Invoke(() =>
                    {
                        Title = $"Track My Playtime | Running: {list.GameName}";
                        UpdatePresenceOnPlay(list);

                    });
                    state = AppState.Running;

                    TextractorExec(list);

                    proc.WaitForExit();
                    timer.Stop();
                    this.Dispatcher.Invoke(() =>
                    {
                        if (state == AppState.Idle)
                            return;
                        StateChecker();
                    });
                }
                catch (Exception ex)
                {
                    timer.Stop();
                    timer.Reset();
                    state = AppState.Running;
                    this.Dispatcher.Invoke(() =>
                    {
                        StateChecker();
                    });
                    log.Error("Exception thrown when launch with launcher handler", ex);
                    MessageBox.Show($"An error occurred while executing the program\nInfo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                TimeSpan ts = timer.Elapsed;
                _dateTime = DateTime.Now;

                list.Tracker = _dateTime;

                if (setting.TimeTracking)
                    list.Playtime += ts;

                // Update UI
                UpdateTracker(list, selected);

                // Save changes
                SerializeData(_ListData_n);
            }).Start();
        }

        private void StartNormal(GameList l_gameList, Stopwatch timer, object selected)
        {
            new Thread(() =>
            {
                try
                {
                    if (!File.Exists(l_gameList.GamePath))
                        throw new FileNotFoundException($"Removed or Missing Program Directory:\n{l_gameList.GamePath}");

                    if (l_gameList.RunAsAdmin)
                        proc.StartInfo.Verb = "runas";
                    else
                        proc.StartInfo.Verb = string.Empty;

                    // start process
                    proc.Start();

                    _runningGame = l_gameList;
                    _dateTime = DateTime.Now;
                    l_gameList.Tracker = _dateTime;
                    UpdateTracker(l_gameList, selected);

                    timer.Start(); // start time tracking
                    this.Dispatcher.Invoke(() =>
                    {
                        Title = $"Track My Playtime | Running: {l_gameList.GameName}";
                        UpdatePresenceOnPlay(l_gameList);

                    });
                    state = AppState.Running;

                    // launch textractor
                    TextractorExec(l_gameList);

                    proc.WaitForExit();

                    var procByName = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(l_gameList.GamePath));
                    if (procByName.Length > 0)
                    {
                        proc = procByName.FirstOrDefault();
                        proc.WaitForExit();
                    }
                    else
                    {
                        // some vn games sometimes not run with the main executable process, but with a child of the main executable with the ".log" extension
                        // so in order to keep track of playtime, the process needs to be found and then wait for it to exit
                        // if it still doesn't work then a Launcher Handler is needed to keep the time tracked
                        var another = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(l_gameList.GamePath) + ".log");
                        if (another.Length > 0)
                        {
                            proc = another.FirstOrDefault();
                            proc.WaitForExit(); // wait for process to exit
                        }
                    }

                    timer.Stop(); // stop time tracking

                    this.Dispatcher.Invoke(() =>
                    {
                        if (state == AppState.Idle)
                            return;
                        StateChecker();
                    });

                    // dispose to clear memory
                    proc.Dispose();
                }
                catch (Exception ex)
                {
                    timer.Stop();
                    state = AppState.Running;
                    this.Dispatcher.Invoke(() =>
                    {
                        StateChecker();
                    });
                    log.Error("Exception thrown when launch game", ex);
                    MessageBox.Show($"An error occurred while executing the program\nInfo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                TimeSpan ts = timer.Elapsed;
                _dateTime = DateTime.Now;

                l_gameList.Tracker = _dateTime;

                if (setting.TimeTracking)
                    l_gameList.Playtime += ts;

                // Update UI
                UpdateTracker(l_gameList, selected);

                // Save changes
                SerializeData(_ListData_n);
            }).Start();
        }

        private void StartProcess()
        {
            if (_lastSelectedList != null)
            {
                Title = $"Track My Playtime | Initializing...";
                var l_gameList = _lastSelectedList;
                var selected = LV_List.SelectedItem;
                var timer = new Stopwatch();

                _currentSelectedList = l_gameList;
                proc = new Process();

                if (!l_gameList.UseLauncherHandler)
                {
                    proc.StartInfo.FileName = l_gameList.GamePath;
                    proc.StartInfo.WorkingDirectory = Path.GetDirectoryName(l_gameList.GamePath);

                    if (l_gameList.LaunchParameter.Length > 0)
                        proc.StartInfo.Arguments = string.Join(" ", l_gameList.LaunchParameter);

                    StartNormal(l_gameList, timer, selected);
                }
                else
                {
                    StartProcessWithLauncherHandler(l_gameList, timer, selected);
                }
            }
        }
        #endregion

        #region UI Logic

        private void LV_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (LV_List.SelectedItem != null)
            {
                btnEdit.IsEnabled = true;
                var l_gameList = (GameList)LV_List.SelectedItem;
                var selectedItem = LV_List.SelectedIndex;

                labelGameTitle.Text = l_gameList.GameName;
                label_DevName.Text = l_gameList.GameDev;

                _contentControl.DeInit();
                _contentControl.Init(l_gameList);

                UpdateTracker(l_gameList, LV_List.SelectedItem);
                _lastSelectedList = l_gameList;
                setting.SelectedIndex = selectedItem;
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (_lastSelectedList != null)
            {
                if (i_List.Count <= 0)
                    return;

                if (state == AppState.Running)
                {
                    var result = MessageBox.Show("Stop running programs? all unsaved progress will be lost", "Stop program?", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Cancel)
                        return;
                }

                StateChecker();
                if (state == AppState.Initialize)
                    StartProcess();
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var form = new ListForms(false, null, null, setting); // Show form window
            form.Owner = this;
            form.Width = 650;
            form.Height = 620;
            var result = form.ShowDialog() ?? false; // save dialog result
            if (result)  // if dialog result is true then save all information and save it to Listview
            {
                var gl = new GameList();
                gl.GameName = form.tbGameTitle.Text;
                gl.GamePath = form.tbGameDir.Text;
                gl.GameDev = string.IsNullOrEmpty(form.tbDeveloper.Text) ? "Unknown" : form.tbDeveloper.Text;
                gl.BackgroundBase64 = form.m_ConvertIMG2Base64(form.imgBitmap);
                gl.IconBase64 = form.IconToBase64String(form.IconPathMethod(form.tbGameDir.Text));
                gl.ProgramType = form.v_ProgramType;
                gl.LaunchParameter = new string[form.v_LaunchParameter.Length];
                Array.Copy(form.v_LaunchParameter, gl.LaunchParameter, form.v_LaunchParameter.Length);
                gl.DisableTextractor = form.cbDisableTextractor.IsChecked ?? false;
                gl.RunAsAdmin = form.cbRunAsAdmin.IsChecked ?? false;
                gl.UseLauncherHandler = form.cbUseLauncherHandler.IsChecked ?? false;
                gl.ImageKey = form.tbImageKey.Text;
                gl.HideGameTitle = form.cbHideGameTitle.IsChecked ?? false;
                gl.HideImageKey = form.cbHideImageKey.IsChecked ?? false;
                gl.BackgroundDir = form.imgDir;
                gl.GUID = _gen.GenerateGUID(6, i_List);
                gl.DateCreated = DateTime.Now;
                gl.ReleaseDate = form.datePickerReleaseDate.SelectedDate ?? DateTime.MinValue;
                gl.Description = form.GetFlowDocumentText(form.rtbDescription.Document);
                gl.Tag = form.tbTag.Text.Split(',');
                gl.Website = new string[3];

                if (!string.IsNullOrEmpty(form.tbWebName1.Text) && !string.IsNullOrEmpty(form.tbWeb1.Text))
                {
                    gl.Website[0] = form.tbWebName1.Text + ";" + form.tbWeb1.Text;
                }

                if (!string.IsNullOrEmpty(form.tbWebName2.Text) && !string.IsNullOrEmpty(form.tbWeb2.Text))
                {
                    gl.Website[1] = form.tbWebName2.Text + ";" + form.tbWeb2.Text;
                }

                if (!string.IsNullOrEmpty(form.tbWebName3.Text) && !string.IsNullOrEmpty(form.tbWeb3.Text) && form.tbWeb3.IsEnabled)
                {
                    gl.Website[2] = form.tbWebName3.Text + ";" + form.tbWeb3.Text;
                }

                i_List.Add(gl);

                // Sort GameList
                FilterSort();


                LV_List.SelectedItem = gl;

                if (!btnEdit.IsEnabled)
                    btnEdit.IsEnabled = true;

                // Save
                SerializeData(_ListData_n);

                if (form.cbCreateShortcut.IsChecked ?? false)
                    form.CreateShortcut(gl);

                // Clear memory
                form.Close();
                form = null;
                System.GC.Collect();
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_lastSelectedList != null)
            {
                var form = new ListForms(true, _lastSelectedList, _currentSelectedList, setting);
                form.Owner = this;
                form.Width = 650;
                form.Height = 620;
                var res = form.ShowDialog() ?? false;
                if (res)
                {
                    var gl = _lastSelectedList;
                    gl.GameName = form.tbGameTitle.Text;
                    gl.GamePath = form.tbGameDir.Text;
                    gl.GameDev = string.IsNullOrEmpty(form.tbDeveloper.Text) ? "Unknown" : form.tbDeveloper.Text;
                    gl.BackgroundBase64 = form.m_ConvertIMG2Base64(form.imgBitmap);
                    gl.IconBase64 = form.IconToBase64String(form.IconPathMethod(form.tbGameDir.Text));
                    gl.ProgramType = form.v_ProgramType;
                    gl.LaunchParameter = new string[form.v_LaunchParameter.Length];
                    Array.Copy(form.v_LaunchParameter, gl.LaunchParameter, form.v_LaunchParameter.Length);
                    gl.DisableTextractor = form.cbDisableTextractor.IsChecked ?? false;
                    gl.RunAsAdmin = form.cbRunAsAdmin.IsChecked ?? false;
                    gl.UseLauncherHandler = form.cbUseLauncherHandler.IsChecked ?? false;
                    gl.ImageKey = form.tbImageKey.Text;
                    gl.HideGameTitle = form.cbHideGameTitle.IsChecked ?? false;
                    gl.HideImageKey = form.cbHideImageKey.IsChecked ?? false;
                    gl.BackgroundDir = form.imgDir;
                    gl.ReleaseDate = form.datePickerReleaseDate.SelectedDate ?? DateTime.MinValue;
                    gl.Description = form.GetFlowDocumentText(form.rtbDescription.Document);
                    gl.Tag = form.tbTag.Text.Split(',');
                    gl.Website = new string[3];

                    if (!string.IsNullOrEmpty(form.tbWebName1.Text) && !string.IsNullOrEmpty(form.tbWeb1.Text))
                    {
                        gl.Website[0] = form.tbWebName1.Text + ";" + form.tbWeb1.Text;
                    }

                    if (!string.IsNullOrEmpty(form.tbWebName2.Text) && !string.IsNullOrEmpty(form.tbWeb2.Text))
                    {
                        gl.Website[1] = form.tbWebName2.Text + ";" + form.tbWeb2.Text;
                    }

                    if (!string.IsNullOrEmpty(form.tbWebName3.Text) && !string.IsNullOrEmpty(form.tbWeb3.Text) && form.tbWeb3.IsEnabled)
                    {
                        gl.Website[2] = form.tbWebName3.Text + ";" + form.tbWeb3.Text;
                    }

                    // Sort GameList
                    FilterSort();

                    labelGameTitle.Text = gl.GameName;
                    label_DevName.Text = gl.GameDev;

                    // Refresh Content
                    _contentControl.DeInit();
                    _contentControl.Init(_lastSelectedList);

                    // Save
                    SerializeData(_ListData_n);

                    // Clear memory
                    form.Close();
                    form = null;
                    System.GC.Collect();
                }
            }
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow form = new SettingWindow(setting, cc);
            form.Owner = this;
            form.Height = 550;
            form.Width = 450;
            var res = form.ShowDialog() ?? false;
            if (res)
            {
                bool isChk = form.cbEnableRichPresence.IsChecked ?? true;
                if (!setting.EnabledRichPresence && isChk)
                {
                    var a = MessageBox.Show("Restarting the application is required for the changes to take effect.\nWant to restart now?", "Restart?", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                    if (a == MessageBoxResult.OK)
                    {
                        setting.x86Directory = form.tbTextractorDirx86.Text;
                        setting.x64Directory = form.tbTextractorDirx64.Text;
                        setting.DisableTextractor = form.cbDisableTextractor.IsChecked ?? false;
                        setting.TimeTracking = form.cbTimeTracking.IsChecked ?? true;
                        setting.EnabledRichPresence = form.cbEnableRichPresence.IsChecked ?? true;
                        setting.TextractorDelay = Convert.ToInt32(form.tbTextractorDelay.Text);
                        setting.AutoCheckUpdate = form.cbAutoCheckUpdate.IsChecked ?? true;
                        setting.UncompressedArtwork = form.cbUncompressedArtwork.IsChecked ?? false;
                        setting.EnabledScreenshot = form.cbEnableScreenshot.IsChecked ?? true;
                        setting.LowSpecMode = form.cbLowSpecMode.IsChecked ?? false;
                        setting.ScreenshotFolderDir = form.tbScreenshotDir.Text;
                        setting.ScreenshotApiIndex = form.cboxScreenshotApiMethod.SelectedIndex;

                        SerializeSetting(_Config);

                        ProcessStartInfo Info = new ProcessStartInfo();
                        Info.Arguments = "/C choice /C Y /N /D Y /T 2 & START \"\" \"" + Assembly.GetEntryAssembly().Location + "\"";
                        Info.WindowStyle = ProcessWindowStyle.Hidden;
                        Info.CreateNoWindow = true;
                        Info.FileName = "cmd.exe";
                        Process.Start(Info);
                        Application.Current.Shutdown();
                    }

                }

                setting.x86Directory = form.tbTextractorDirx86.Text;
                setting.x64Directory = form.tbTextractorDirx64.Text;
                setting.DisableTextractor = form.cbDisableTextractor.IsChecked ?? false;
                setting.TimeTracking = form.cbTimeTracking.IsChecked ?? true;
                setting.EnabledRichPresence = form.cbEnableRichPresence.IsChecked ?? true;
                setting.TextractorDelay = Convert.ToInt32(form.tbTextractorDelay.Text);
                setting.AutoCheckUpdate = form.cbAutoCheckUpdate.IsChecked ?? true;
                setting.UncompressedArtwork = form.cbUncompressedArtwork.IsChecked ?? false;
                setting.EnabledScreenshot = form.cbEnableScreenshot.IsChecked ?? true;
                setting.LowSpecMode = form.cbLowSpecMode.IsChecked ?? false;
                setting.ScreenshotFolderDir = form.tbScreenshotDir.Text;
                setting.ScreenshotApiIndex = form.cboxScreenshotApiMethod.SelectedIndex;

                if (!setting.EnabledRichPresence)
                    discord.Deinitialize();

                if (setting.LowSpecMode)
                    SolidCommandBrush.Opacity = 1;
                else
                    SolidCommandBrush.Opacity = 0.6;

                SerializeSetting(_Config);
            }
        }

        private void btnDebug_Click(object sender, RoutedEventArgs e)
        {
            var item = (GameList)LV_List.SelectedItem;
            Console.WriteLine($"GUID: {item.GUID}");
            Console.WriteLine($"Selected INDEX: {LV_List.SelectedIndex}");
        }

        private void btnDebug2_Click(object sender, RoutedEventArgs e)
        {
            /*var item = (GameList)LV_List.SelectedItem;
            item.GUID = _gen.GenerateGUID(6, i_List);
            SerializeData(_ListData_n);*/

            foreach (var items in i_List)
            {
                items.GUID = _gen.GenerateGUID(6, i_List);
                Console.WriteLine($"Game Title: {items.GameName}\nGUID: {items.GUID}\n");
            }
            SerializeData(_ListData_n);
        }

        private void btnDebug3_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(args[1]);
            foreach (var item in i_List)
            {
                if (item.GUID == args[1])
                {
                    LV_List.SelectedItem = item;
                    break;
                }
            }
        }

        private void btnDebug4_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 100; i++)
            {
                GameList gl = new GameList();
                gl.GameName = $"DEBUG {i}";
                gl.GamePath = "";
                gl.GUID = _gen.GenerateGUID(6, i_List);
                gl.DateCreated = DateTime.Now;
                i_List.Add(gl);
            }
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "TMP Export File (*.kex)|*.kex";
            ofd.Title = "Import GameList";
            if (ofd.ShowDialog() == true)
            {
                if (DeserializeImportList(ofd.FileName))
                {
                    List<GameList> gl2Add = new List<GameList>();
                    List<GameList> item2Remove = new List<GameList>();

                    foreach (var item in importList)
                    {
                        bool continued = false;
                        foreach (var item2 in i_List)
                        {
                            if (item2.GamePath == item.gameDir)
                            {
                                var res = MessageBox.Show($"Duplicate game on list detected...\n\n" +
                                    $"Game Title: {item.gameName}\nDirectory: {item.gameDir}\n\nDo you want to merge current playtime? " +
                                    $"Selecting \"No\" will cause the existing game to be overwritten",
                                    "Information", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                if (res == MessageBoxResult.Yes)
                                {
                                    item2.Playtime += TimeSpan.FromSeconds(item.playtime);
                                    continued = true;
                                    break;
                                }
                                else
                                {
                                    item2Remove.Add(item2);
                                }
                            }
                        }

                        if (continued)
                            continue;

                        GameList gl = new GameList();
                        gl.GameName = item.gameName;
                        gl.GameDev = item.developerName;
                        gl.GamePath = item.gameDir;
                        gl.LaunchParameter = new string[item.launchArgument.Length];
                        Array.Copy(item.launchArgument, gl.LaunchParameter, item.launchArgument.Length);
                        gl.ProgramType = item.programType;
                        gl.Playtime = TimeSpan.FromSeconds(item.playtime);
                        gl.ImageKey = item.imageKey;
                        gl.GUID = _gen.GenerateGUID(6, i_List);

                        Console.WriteLine($"Game: {gl.GameName} Playtime: {gl.Playtime} Total Hours: {gl.Playtime.TotalHours} GUID: {gl.GUID}");

                        gl2Add.Add(gl);
                    }

                    foreach (var item in gl2Add)
                    {
                        i_List.Add(item);
                    }

                    foreach (var deleted in item2Remove)
                    {
                        i_List.Remove(deleted);
                    }

                    // Sort GameList
                    FilterSort();

                    MessageBox.Show("Import Successfully!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    SerializeData(_ListData_n);
                }
                else
                {
                    importList = null;
                }
            }
        }
        #endregion

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            new ExportWindow().ShowDialog();
        }

        private void btnPlay_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                To = 55,
                Duration = new Duration(TimeSpan.FromSeconds(0.1)),
                EasingFunction = new QuadraticEase()
            };

            ElasticEase elasticEase = new ElasticEase
            {
                EasingMode = EasingMode.EaseOut,
                Oscillations = 1,
                Springiness = 10
            };
            Storyboard.SetTarget(animation, btnPlay);
            Storyboard.SetTargetProperty(animation, new PropertyPath("FontSize"));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Completed += (s, _) => btnPlay.FontSize = 55;
            storyboard.Begin();
        }

        private void btnPlay_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                To = 50,
                Duration = new Duration(TimeSpan.FromSeconds(0.1)),
                EasingFunction = new QuadraticEase()
            };

            ElasticEase elasticEase = new ElasticEase
            {
                EasingMode = EasingMode.EaseOut,
                Oscillations = 1,
                Springiness = 10
            };
            Storyboard.SetTarget(animation, btnPlay);
            Storyboard.SetTargetProperty(animation, new PropertyPath("FontSize"));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Completed += (s, _) => btnPlay.FontSize = 50;
            storyboard.Begin();
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (LV_List.ItemsSource != null)
                    CollectionViewSource.GetDefaultView(LV_List.ItemsSource).Refresh();

                LV_List.SelectedItem = _lastSelectedList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            var window = new FilterWindow(filterSetting)
            {
                Width = 350,
                Height = 250
            };

            var buttonLocation = btnFilter.PointToScreen(new System.Windows.Point(0, 0));

            double windowLeft = buttonLocation.X + btnFilter.ActualWidth + 10;
            double windowTop = buttonLocation.Y;

            // Add offside
            window.Left = windowLeft + 4.3;
            window.Top = windowTop - 10;

            window.Show();
        }

        private void ctxDelete_Click(object sender, RoutedEventArgs e)
        {
            if (LV_List.SelectedItems != null)
            {
                var gl = (GameList)LV_List.SelectedItem;

                if (gl == _currentSelectedList && state == AppState.Running)
                {
                    MessageBox.Show($"This game is still running, you can't delete it at this time", "Failed to delete", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string dateAdded = "Unknown";
                if (gl.DateCreated != DateTime.MinValue)
                    dateAdded = gl.DateCreated.ToString("dd-MMMM-yyyy");

                var res = MessageBox.Show($"Are you sure want to delete this game from the library?\n\nTitle: {gl.GameName}\nDeveloper: {gl.GameDev}\nDate Added: {dateAdded}", "Are you sure?", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (res == MessageBoxResult.Cancel)
                    return;

                if (LV_List.SelectedItem is GameList selected)
                {
                    try
                    {
                        var shortcutName = selected.GameName;
                        string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                        foreach (char invalidChar in invalidChars)
                        {
                            shortcutName = shortcutName.Replace(invalidChar.ToString(), "");
                        }

                        var shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + $"\\{shortcutName}.url";

                        if (File.Exists(shortcutPath))
                            File.Delete(shortcutPath);

                        i_List.Remove(selected);
                    }
                    catch (Exception ex)
                    {
                        MainWindow.log.Error("Exception Thrown when Delete GameList", ex);
                    }
                }
                else return;

                // Sort GameList
                FilterSort();

                // Refresh content page
                _contentControl.FreshInit();

                labelGameTitle.Text = "Game Title";
                label_DevName.Text = "Developer Name";
                label_Playtime.Content = "0h 0m 0s";
                label_LastPlayed.Content = "Never";

                btnEdit.IsEnabled = false;

                SerializeData(_ListData_n);
                return;
            }
        }

        private void ctxProperties_Click(object sender, RoutedEventArgs e)
        {
            if (LV_List.SelectedItems != null)
            {
                var window = new PropertiesWindow((GameList)LV_List.SelectedItem);
                window.Show();
            }
        }

        private void ctxShortcut_Click(object sender, RoutedEventArgs e)
        {
            var win = new ListForms(false, null, null, null);
            win.CreateShortcut((GameList)LV_List.SelectedItem);
        }
    }
}
