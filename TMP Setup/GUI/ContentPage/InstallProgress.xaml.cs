using IWshRuntimeLibrary;
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TMP_Setup.Ext;

namespace TMP_Setup.GUI.ContentPage
{
    /// <summary>
    /// Interaction logic for InstallProgress.xaml
    /// </summary>
    public partial class InstallProgress : Page
    {
        string dir;
        bool type;
        bool createShortcut;

        private RegisterProgram _reg = new RegisterProgram();

        public InstallProgress(string dir, bool type, bool createDesktop)
        {
            InitializeComponent();

            this.DataContext = this;
            this.dir = dir;
            this.type = type;
            this.createShortcut = createDesktop;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => ExtractZip(this.dir));
        }

        public void CreateShortcut(string targetExePath, string shortcutPath, string iconPath, string args)
        {
            var shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetExePath;
            shortcut.Arguments = args;
            shortcut.IconLocation = iconPath;
            shortcut.Save();
        }

        private void ExtractZip(string extractPath)
        {
            try
            {
                if(!Directory.Exists(Path.GetDirectoryName(dir)))
                {
                    throw new DirectoryNotFoundException("Target directory not found");
                }

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream resourceStream = assembly.GetManifestResourceStream("TMP_Setup.Resources.tmp.zip"))
                {
                    using (ZipArchive archive = new ZipArchive(resourceStream))
                    {
                        int totalEntries = archive.Entries.Count;
                        int completedEntries = 0;

                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            string entryPath = Path.Combine(extractPath, entry.FullName);
                            if (entryPath.EndsWith("/")) // Membuat direktori jika diperlukan
                            {
                                Directory.CreateDirectory(entryPath);
                                completedEntries++; // Mengupdate kemajuan

                                // Menggunakan Dispatcher.Invoke untuk memperbarui ProgressBar di UI thread
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    UpdateProgressBar(totalEntries, completedEntries);
                                });

                                continue;
                            }

                            using (Stream entryStream = entry.Open())
                            {
                                using (FileStream fileStream = new FileStream(entryPath, FileMode.Create))
                                {
                                    entryStream.CopyTo(fileStream);
                                }
                            }

                            completedEntries++; // Mengupdate kemajuan

                            // Menggunakan Dispatcher.Invoke untuk memperbarui ProgressBar di UI thread
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                UpdateProgressBar(totalEntries, completedEntries);
                            });
                        }
                    }
                }

                if (type)
                {
                    _reg.RegisterCurrentUser(dir + "\\tmp.exe");
                    _reg.RegisterCurrentUserUninstaller(dir + "\\tmp.exe", dir, dir + "\\uninstall.exe");
                }
                else
                {
                    _reg.RegisterAllUser(dir + "\\tmp.exe");
                    _reg.RegisterAllUserUninstaller(dir + "\\tmp.exe", dir, dir + "\\uninstall.exe");
                }

                if(createShortcut)
                {
                    string targetExePath = dir + "\\tmp.exe";
                    string shortcutPath;
                    string iconPath = dir + "\\tmp.exe";
                    if (type)
                        shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Track My Playtime.lnk";
                    else
                        shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory) + "\\Track My Playtime.lnk";

                    CreateShortcut(targetExePath, shortcutPath, iconPath, null);
                }

                Dispatcher.Invoke(() =>
                {
                    this.NavigationService.Navigate(new InstallComplete(dir));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Dispatcher.Invoke(() =>
                {
                    this.NavigationService?.Navigate(new InstallFailed(ex.Message));
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
