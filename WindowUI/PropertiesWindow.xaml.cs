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
        private GameList gl { get; }
        public bool VnidChecker{ get { return (gl.VNID != 0); }
        }
        public PropertiesWindow(GameList selected)
        {
            InitializeComponent();

            this.DataContext = this;

            tblockSize.Text = "Calculating...";
            tblockGUID.Text = "Unknown...";
            tblockDate.Text = "Unknown";
            tblockTime.Text = "Unknown";

            gl = selected;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ExecutableIcon.Source = gl.IconPath;
            labelGameTitle.Text = gl.GameName;
            labelGameDev.Text = gl.GameDev;
            tbGameDir.Text = gl.GamePath;
            tblockGUID.Text = gl.GUID;
            tblockVNID.Text = (gl.VNID == 0) ? null : gl.VNID.ToString();

            if (gl.DateCreated != DateTime.MinValue)
            {
                tblockDate.Text = gl.DateCreated.ToString("dd MMMM yyyy");
                tblockTime.Text = gl.DateCreated.ToString("HH.mm");
            }

            if (gl.Tracker == DateTime.MinValue)
                tblockPlaytime.Text = "Never";
            else
            {
                var totalHours = gl.Playtime.TotalHours;
                var hours = (int)Math.Floor(totalHours);
                string formattedHours = hours.ToString("0");
                tblockPlaytime.Text = string.Format("{0}h {1}m {2}s", formattedHours, gl.Playtime.Minutes, gl.Playtime.Seconds);
            }

            new Thread(() =>
            {
                long totalSize = DirSize(Directory.GetParent(gl.GamePath));
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
