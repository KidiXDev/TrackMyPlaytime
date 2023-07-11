using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using TMP.NET.Modules;

namespace TMP.NET.WindowUI
{
    /// <summary>
    /// Interaction logic for PropertiesWindow.xaml
    /// </summary>
    public partial class PropertiesWindow : Window
    {
        private GameList gameList { get; }
        public PropertiesWindow(GameList selected)
        {
            InitializeComponent();

            tblockSize.Text = "Calculating...";
            tblockGUID.Text = "Unknown...";
            tblockDate.Text = "Unknown";
            tblockTime.Text = "Unknown";

            gameList = selected;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ExecutableIcon.Source = gameList.IconPath;
            labelGameTitle.Text = gameList.GameName;
            labelGameDev.Text = gameList.GameDev;
            tbGameDir.Text = gameList.GamePath;
            tblockGUID.Text = gameList.GUID;

            if (gameList.DateCreated != DateTime.MinValue)
            {
                tblockDate.Text = gameList.DateCreated.ToString("dd MMMM yyyy");
                tblockTime.Text = gameList.DateCreated.ToString("HH.mm");
            }

            if (gameList.Tracker == DateTime.MinValue)
                tblockPlaytime.Text = "Never";
            else
            {
                var totalHours = gameList.Playtime.TotalHours;
                var hours = (int)Math.Floor(totalHours);
                string formattedHours = hours.ToString("0");
                tblockPlaytime.Text = string.Format("{0}h {1}m {2}s", formattedHours, gameList.Playtime.Minutes, gameList.Playtime.Seconds);
            }

            new Thread(() =>
            {
                long totalSize = DirSize(Directory.GetParent(gameList.GamePath));
                if (totalSize <= -1)
                    return;

                string formattedSize = FormatSize(totalSize);
                Dispatcher.Invoke(() =>
                {
                    tblockSize.Text = formattedSize;
                });
            }).Start();
        }

        private string FormatSize(long size)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int suffixIndex = 0;
            double formattedSize = size;

            while (formattedSize >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                formattedSize /= 1024;
                suffixIndex++;
            }

            return $"{formattedSize:0.##} {suffixes[suffixIndex]}";
        }

        private long DirSize(DirectoryInfo dir)
        {
            try
            {
                long size = 0;

                // Add file sizes
                FileInfo[] fis = dir.GetFiles();
                foreach (FileInfo fi in fis)
                {
                    size += fi.Length;
                }

                // Add subdirectory sizes
                DirectoryInfo[] dis = dir.GetDirectories();
                foreach (DirectoryInfo di in dis)
                {
                    size += DirSize(di);
                }

                return size;
            }
            catch
            {
                Dispatcher.Invoke(() =>
                {
                    tblockSize.Text = $"Failed to calculate.";
                });
                return -1;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dir = Directory.GetParent(tbGameDir.Text);
            Process.Start($"\"{dir}\"");
        }
    }
}
