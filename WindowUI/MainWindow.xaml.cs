﻿/*
 *   Status: In-Development
 *   Publishing Status: Un-released
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
 *       Update Features:
 *       - Screenshot
 *       - Global Keyboard Hook
 *       
 *       [4 June 2023]
 *       Update Features:
 *       - Argument Listener at runtime
 *       - Change the GameTitle label to Textblock inside the ViewBox to achieve automatic font re-size
 *       - Add Textractor Launch Delay Setting
 *       
 *       [12 June 2023]
 *       Update Features:
 *       - Make Installer and Uninstaller
 *       
 *       Upcoming Features:
 *       - Unknown
 *
*/

using log4net;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TMP.NET.Modules;
using TMP.NET.WindowUI;

namespace TMP.NET
{
    /// <summary>
    /// All Program and UI logic is on here
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variable & Object Instance

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _ListData_n = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KidiXDev\\TrackMyPlaytime\\listdata.kdb");
        private readonly string _Config = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KidiXDev\\TrackMyPlaytime\\config.cfg");

        private ObservableCollection<GameList> i_List = new ObservableCollection<GameList>();
        public ObservableCollection<GameList> i_listv { get { return i_List; } }

        private List<ImportList> importList;

        private Config setting = new Config();

        private DateTime _dateTime;
        private GUIDGen _gen = new GUIDGen();

        private Modules.DiscordRPC discord = new Modules.DiscordRPC();

        private readonly string[] args = Environment.GetCommandLineArgs();

        private enum AppState
        {
            Idle, Initialize, Running
        }

        private AppState state = AppState.Idle;
        #endregion

        #region Serialize And Deserialize Data
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

        private void SerializeSetting(string filePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }

            File.WriteAllText(filePath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        }

        private void DeserializeData(string filePath)
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
                log.Error("Failed to load Game List", ex);
            }
        }

        private void DeserializeSetting(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    setting = JsonConvert.DeserializeObject<Config>(File.ReadAllText(filePath));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load Setting\nIt is possible that it is caused by a corrupted file\nInfo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                log.Error("Failed load setting", ex);
            }
        }

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

        private void FirstLoad()
        {
            DeserializeData(_ListData_n);
            DeserializeSetting(_Config);

            if (setting.EnabledRichPresence)
                discord.Initialize();
        }

        private void ScreenshotExec()
        {
            try
            {
                if (!CaptureHandler.IsWindowFocused(proc))
                    return;

                string imagePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\Track My Playtime";

                if (!Directory.Exists(imagePath))
                    Directory.CreateDirectory(imagePath);

                string fileName = $"{proc.ProcessName} {DateTime.Now.ToString("dd-MMMM-yyyy HH-mm-ss")}.png";
                string combined = Path.Combine(imagePath, fileName);
                var img = CaptureHandler.TakeScreenshot(proc);
                img.Save(combined, ImageFormat.Png);

                ShowNotification("Screenshot Taken!", "Saved in: " + combined);
            }
            catch (Exception ex)
            {
                ShowNotification("Failed take screenshot", $"Info: {ex.Message}");
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
                        Console.WriteLine("ingfo");
                        mainWindow.StateChecker();
                        if (mainWindow.state == AppState.Initialize)
                        {
                            mainWindow.StartProcess();
                        }
                    }
                    Console.WriteLine($"Launch Id: {launchId}");
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
                    LV_List.ScrollIntoView(LV_List.Items[setting.SelectedIndex]);
                }
            }
        }

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

        private void ShowNotification(string baloonMsg, string message)
        {
            System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Icon = SystemIcons.Information;

            HwndSource source = (HwndSource)HwndSource.FromVisual(this);

            IntPtr handle = source.Handle;

            notifyIcon.BalloonTipTitle = baloonMsg;
            notifyIcon.BalloonTipText = message;
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(3000);

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 3000;
            timer.Tick += (sender, e) =>
            {
                notifyIcon.Dispose();
                timer.Stop();
            };
            timer.Start();
        }

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
            {
                MainWindow.WindowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
                HwndSource.FromHwnd(MainWindow.WindowHandle)?.AddHook(new HwndSourceHook(HandleMessages));
            };
            Modules.Keyboard.KeyboardHook.KeyCombinationPressed += ScreenshotExec;
            //devConsole.Show();

            DataContext = this;
            FirstLoad();
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
            SerializeSetting(_Config);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                LoadShortcut();
            }));
        }

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
                    discord.updatePresence(null, null, "tmp_logo", "Track My Playtime");

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

        #region UI Update & Process Logic
        private void updateTracker(GameList game, object selected) // Update Date and Playtime UI
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
                                label_LastPlayed.Content = game.Tracker.Date.ToString("dd-MM-yyyy");

                            var totalHours = game.Playtime.TotalHours;
                            var hours = (int)Math.Floor(totalHours);
                            string formattedHours = hours.ToString("0");
                            label_Playtime.Content = string.Format("{0}h {1}m {2}s", formattedHours, game.Playtime.Minutes, game.Playtime.Seconds);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    log.Error("Failed to update tracker", ex);
                }
            });
        }

        private void updatePresenceOnPlay(GameList game)
        {
            if (!setting.EnabledRichPresence)
                return;

            string gameName = game.HideGameTitle ? "a game" : game.GameName;
            string imageKey = game.HideImageKey || string.IsNullOrEmpty(game.ImageKey) ? "tmp_logo" : game.ImageKey;
            string trackMyPlaytime = "Track My Playtime";

            if (game.HideGameTitle && game.HideImageKey)
            {
                discord.updatePresence("Playing a game", null, "tmp_logo", null);
            }
            else if (game.HideGameTitle)
            {
                discord.updatePresence("Playing a game", null, imageKey, null);
            }
            else if (game.HideImageKey)
            {
                discord.updatePresence($"Playing {gameName}", null, "tmp_logo", null);
            }
            else
            {
                if (!string.IsNullOrEmpty(game.ImageKey))
                    discord.updatePresence($"Playing {gameName}", null, imageKey, gameName, "tmp_logo", trackMyPlaytime);
                else
                    discord.updatePresence($"Playing {gameName}", null, "tmp_logo", gameName);
            }
        }

        private Process proc = new Process();

        private void textractorExec(GameList game)
        {
            try
            {
                new Thread(() =>
                {
                    if (!setting.DisableTextractor && !game.DisableTextractor)
                    {
                        Thread.Sleep(setting.TextractorDelay);
                        var textractorProc = new Process();
                        if (game.ProgramType.Equals("x86"))
                        {
                            textractorProc.StartInfo.FileName = setting.x86Directory;
                            textractorProc.StartInfo.WorkingDirectory = Path.GetDirectoryName(setting.x86Directory);
                            textractorProc.Start();
                        }
                        else
                        {
                            textractorProc.StartInfo.FileName = setting.x64Directory;
                            textractorProc.StartInfo.WorkingDirectory = Path.GetDirectoryName(setting.x64Directory);
                            textractorProc.Start();
                        }
                    }
                }).Start();
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed launch Textractor\nInfo{e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                log.Error("Failed launch Textractor", e);
            }
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

                    proc.Start();
                    timer.Start();
                    this.Dispatcher.Invoke(() =>
                    {
                        Title = $"Track My Playtime | Running: {list.GameName}";
                        updatePresenceOnPlay(list);

                    });
                    state = AppState.Running;

                    textractorExec(list);

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
                    return;
                }

                TimeSpan ts = timer.Elapsed;
                _dateTime = DateTime.Now;

                list.Playtime += ts;
                list.Tracker = _dateTime;

                // Update UI
                updateTracker(list, selected);

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

                    proc.Start();
                    timer.Start();
                    this.Dispatcher.Invoke(() =>
                    {
                        Title = $"Track My Playtime | Running: {l_gameList.GameName}";
                        updatePresenceOnPlay(l_gameList);

                    });
                    state = AppState.Running;

                    textractorExec(l_gameList);

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
                    log.Error("Exception thrown when launch game", ex);
                    MessageBox.Show($"An error occurred while executing the program\nInfo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                TimeSpan ts = timer.Elapsed;
                _dateTime = DateTime.Now;

                l_gameList.Playtime += ts;
                l_gameList.Tracker = _dateTime;

                // Update UI
                updateTracker(l_gameList, selected);

                // Save changes
                SerializeData(_ListData_n);
            }).Start();
        }

        private void StartProcess()
        {
            if (LV_List.SelectedItem != null)
            {
                Title = $"Track My Playtime | Initializing...";
                var l_gameList = (GameList)LV_List.SelectedItem;
                var selected = LV_List.SelectedItem;
                var timer = new Stopwatch();

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
                var l_gameList = (GameList)LV_List.SelectedItem;
                var selectedItem = LV_List.SelectedIndex;
                labelGameTitle.Text = l_gameList.GameName;
                label_DevName.Content = l_gameList.GameDev;

                var g_Brush = GridImg.Background as ImageBrush;

                if (l_gameList.BackgroundPath() == null)
                    g_Brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/TMP.NET;component/Resources/no-image.png"));
                else
                    g_Brush.ImageSource = l_gameList.BackgroundPath();

                updateTracker(l_gameList, LV_List.SelectedItem);
                setting.SelectedIndex = selectedItem;
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (LV_List.SelectedItems != null)
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
            var form = new ListForms(false, null); // Show form window
            var result = form.ShowDialog() ?? false; // save dialog result
            if (result)  // if dialog result is true then save all information and save it to Listview
            {
                var gl = new GameList();
                gl.GameName = form.tbGameTitle.Text;
                gl.GamePath = form.tbGameDir.Text;
                gl.GameDev = form.tbDeveloper.Text;
                gl.BackgroundBase64 = form.m_ConvertIMG2Base64(form.imgBitmap);
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

                i_List.Add(gl);

                // Refresh ListView
                i_List = new ObservableCollection<GameList>(i_List.OrderBy(item => item.GameName));
                LV_List.ItemsSource = i_List;
                CollectionViewSource.GetDefaultView(i_List).Refresh();
                LV_List.SelectedItem = gl;

                SerializeData(_ListData_n);

                if(form.cbCreateShortcut.IsChecked ?? false)
                    form.CreateShortcut(gl);
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (LV_List.SelectedItem != null)
            {
                var form = new ListForms(true, (GameList)LV_List.SelectedItem);
                var res = form.ShowDialog() ?? false;
                if (res)
                {
                    var gl = (GameList)LV_List.SelectedItem;
                    gl.GameName = form.tbGameTitle.Text;
                    gl.GamePath = form.tbGameDir.Text;
                    gl.GameDev = form.tbDeveloper.Text;
                    gl.BackgroundBase64 = form.m_ConvertIMG2Base64(form.imgBitmap);
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

                    // Refresh ListView
                    i_List = new ObservableCollection<GameList>(i_List.OrderBy(item => item.GameName));
                    LV_List.ItemsSource = i_List;
                    CollectionViewSource.GetDefaultView(i_List).Refresh();

                    labelGameTitle.Text = gl.GameName;
                    label_DevName.Content = gl.GameDev;

                    var g_Brush = GridImg.Background as ImageBrush;

                    if (gl.BackgroundPath() == null)
                        g_Brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/TMP.NET;component/Resources/no-image.png"));
                    else
                        g_Brush.ImageSource = gl.BackgroundPath();

                    SerializeData(_ListData_n);
                }
                else
                {
                    if (form.isDeleted)
                    {
                        if (LV_List.SelectedItem is GameList selected)
                            i_List.Remove(selected);

                        CollectionViewSource.GetDefaultView(i_List).Refresh();

                        labelGameTitle.Text = null;
                        label_DevName.Content = null;
                        label_Playtime.Content = "0h 0m 0s";
                        label_LastPlayed.Content = "Never";

                        var g_Brush = GridImg.Background as ImageBrush;
                        g_Brush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/TMP.NET;component/Resources/no-image.png"));

                        SerializeData(_ListData_n);
                        return;
                    }
                }
            }
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow form = new SettingWindow(setting);
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
                        SerializeSetting(_Config);

                        Process.Start(Application.ResourceAssembly.Location);
                        Application.Current.Shutdown();
                    }

                }

                setting.x86Directory = form.tbTextractorDirx86.Text;
                setting.x64Directory = form.tbTextractorDirx64.Text;
                setting.DisableTextractor = form.cbDisableTextractor.IsChecked ?? false;
                setting.TimeTracking = form.cbTimeTracking.IsChecked ?? true;
                setting.EnabledRichPresence = form.cbEnableRichPresence.IsChecked ?? true;
                setting.TextractorDelay = Convert.ToInt32(form.tbTextractorDelay.Text);

                if (!setting.EnabledRichPresence)
                    discord.Deinitialize();

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
            /*var gl1 = (GameList)LV_List.SelectedItem;
            Console.WriteLine(gl1.Playtime.TotalHours);*/

            Bitmap img = CaptureHandler.TakeScreenshot(proc);
            img.Save("ss.png", ImageFormat.Png);

            // Menampilkan notifikasi
            string imagePath = Environment.CurrentDirectory + "\\ss.png";
            ShowNotification("Screenshot Taken!", "Saved in: " + imagePath);

            Console.WriteLine("Captured!");
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "TMP Export File (*.kex)|*.kex";
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

                    // Refresh ListView
                    i_List = new ObservableCollection<GameList>(i_List.OrderBy(item => item.GameName));
                    LV_List.ItemsSource = i_List;
                    CollectionViewSource.GetDefaultView(i_List).Refresh();
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
    }
}
