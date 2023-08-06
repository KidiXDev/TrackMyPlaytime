using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using TMP.NET.Modules;

namespace TMP.NET.WindowUI.ContentWindow
{
    /// <summary>
    /// Interaction logic for GameListContentControl.xaml
    /// </summary>
    public partial class GameListContentControl : UserControl, INotifyPropertyChanged
    {
        private GameList gl;
        private Config.ContentConfig cc;
        private Config setting;
        FileSystemWatcher watcher;

        private string folderPath;

        public bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public double BGOpacity { get { return cc.BackgroundOpacityValue; } }
        public double BGBlur { get { return cc.BackgroundBlurValue; } }

        private void CcInstance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BackgroundBlurValue")
            {
                OnPropertyChanged("BGBlur");
            }
            else if (e.PropertyName == "BackgroundOpacityValue")
            {
                BackgroundImg.BeginAnimation(Image.OpacityProperty, null);
                OnPropertyChanged("BGOpacity");
            }
        }

        public GameListContentControl(Config setting, Config.ContentConfig cc)
        {
            InitializeComponent();

            this.setting = setting;
            this.cc = cc;
            cc.PropertyChanged += CcInstance_PropertyChanged;

            DataContext = this;
        }

        public void Init(GameList gl)
        {
            this.gl = gl;

            tblockDescription.Text = string.IsNullOrEmpty(gl.Description) ? "No Description" : gl.Description;

            if (gl.BackgroundPath() == null)
            {
                CoverImg.Source = new BitmapImage(new Uri("pack://application:,,,/TMP.NET;component/Resources/no-img-added.png"));
                BackgroundImg.Source = new BitmapImage(new Uri("pack://application:,,,/TMP.NET;component/Resources/no-img-added.png"));
            }
            else
            {
                CoverImg.Source = gl.BackgroundPath();
                BackgroundImg.Source = gl.BackgroundPath();
            }

            if (gl.ReleaseDate == DateTime.MinValue)
                tblockReleaseDate.Text = "Unknown";
            else
                tblockReleaseDate.Text = gl.ReleaseDate.Date.ToString("dd MMM, yyyy");

            if (gl.Tag != null)
            {
                tblockTag.Text = string.Empty;

                List<string> tagList = new List<string>();

                foreach (var tag in gl.Tag)
                {
                    tagList.Add(tag.ToString());
                }

                if (tagList.Count > 0)
                {
                    StringBuilder tagTextBuilder = new StringBuilder();

                    tagTextBuilder.Append(tagList[0]);

                    for (int i = 1; i < tagList.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(tagList[i]) && !char.IsWhiteSpace(tagList[i][0]) && !char.IsWhiteSpace(tagTextBuilder[tagTextBuilder.Length - 1]))
                            tagTextBuilder.Append(", ");
                        else
                            tagTextBuilder.Append(",");

                        tagTextBuilder.Append(tagList[i]);
                    }

                    if (string.IsNullOrEmpty(tagTextBuilder.ToString()))
                        tblockTag.Text = "No Tag";
                    else
                        tblockTag.Text = tagTextBuilder.ToString();
                }
            }
            else
                tblockTag.Text = "No Tag";


            if (gl.Website != null)
            {
                string[] webnames;
                string[] webUrls;

                webnames = new string[gl.Website.Length];
                webUrls = new string[gl.Website.Length];

                for (int i = 0; i < gl.Website.Length; i++)
                {
                    if (!string.IsNullOrEmpty(gl.Website[i]))
                    {
                        string[] parts = gl.Website[i].Split(';');

                        if (parts.Length == 2)
                        {
                            webnames[i] = parts[0];
                            webUrls[i] = parts[1];
                        }
                        else
                        {
                            Console.WriteLine("Unsupported format" + i);
                        }
                    }
                }

                if (string.IsNullOrEmpty(gl.Website[0]))
                {
                    web1Text.Text = "None";
                    web1link.NavigateUri = null;
                }
                if (string.IsNullOrEmpty(gl.Website[1]))
                {
                    web2Text.Text = string.Empty;
                    web2link.NavigateUri = null;
                }
                if (string.IsNullOrEmpty(gl.Website[2]))
                {
                    web3Text.Text = string.Empty;
                    web3link.NavigateUri = null;
                }

                InitWebUrl(gl, webUrls, webnames);
            }
            else
            {
                web1Text.Text = "None";
                web1link.NavigateUri = null;
                web2Text.Text = string.Empty;
                web3Text.Text = string.Empty;
            }

            if (!Directory.Exists(Path.Combine(setting.ScreenshotFolderDir, ValidGameName(gl.GameName))))
            {
                Directory.CreateDirectory(Path.Combine(setting.ScreenshotFolderDir, ValidGameName(gl.GameName)));
            }

            folderPath = Path.Combine(setting.ScreenshotFolderDir, ValidGameName(gl.GameName));
            watcher = new FileSystemWatcher(folderPath);
            watcher.EnableRaisingEvents = true;
            watcher.Created += OnImageCreated;
            OnImageCreated(null, null);

            if (IsDirectoryEmpty(folderPath))
                screenshotGrid.Visibility = Visibility.Collapsed;
            else
                screenshotGrid.Visibility = Visibility.Visible;

            PlayBGAnim();
        }

        public void DeInit()
        {
            if (watcher != null)
            {
                watcher.Dispose();
                watcher = null;
            }

            ssArea1.Source = null;
            ssArea1.UpdateLayout();
            ssArea2.Source = null;
            ssArea2.UpdateLayout();
            ssArea3.Source = null;
            ssArea3.UpdateLayout();

            CoverImg.Source = null;
            CoverImg.UpdateLayout();
            BackgroundImg.Source = null;
            BackgroundImg.UpdateLayout();

            gl = null;
        }

        public void FreshInit()
        {
            CoverImg.Source = new BitmapImage(new Uri("pack://application:,,,/TMP.NET;component/Resources/no-img-added.png"));
            BackgroundImg.Source = new BitmapImage(new Uri("pack://application:,,,/TMP.NET;component/Resources/no-img-added.png"));
            tblockTag.Text = "No Tag";
            tblockReleaseDate.Text = "Unknown";
            screenshotGrid.Visibility = Visibility.Collapsed;
            tblockDescription.Text = "No Description";
            web1Text.Text = "None";
            web1link.NavigateUri = null;
            web2Text.Text = string.Empty;
            web2link.NavigateUri = null;
            web3Text.Text = string.Empty;
            web3link.NavigateUri = null;

        }

        private string ValidGameName(string gl)
        {
            var nem = gl;
            string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char invalidChar in invalidChars)
            {
                nem = nem.Replace(invalidChar.ToString(), "");
            }

            return nem;
        }

        private void PlayBGAnim()
        {
            if (setting.LowSpecMode)
                return;

            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = cc.BackgroundOpacityValue,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new CubicEase(),
            };

            BackgroundImg.BeginAnimation(Image.OpacityProperty, opacityAnimation);
        }

        private void InitWebUrl(GameList gl, string[] webUrl, string[] webNames)
        {
            if (!string.IsNullOrEmpty(webUrl[0]))
            {
                web1link.NavigateUri = new Uri(webUrl[0]);
                web1Text.Text = webNames[0];
            }

            if (!string.IsNullOrEmpty(webUrl[1]))
            {
                web2link.NavigateUri = new Uri(webUrl[1]);
                web2Text.Text = webNames[1];
            }

            if (!string.IsNullOrEmpty(webUrl[2]))
            {
                web3link.NavigateUri = new Uri(webUrl[2]);
                web3Text.Text = webNames[2];
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            if (e.Uri == null)
            {
                e.Handled = true;
                return;
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void TextBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(Path.Combine(setting.ScreenshotFolderDir, ValidGameName(gl.GameName)));
        }

        private void OnImageCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                var pngFiles = new DirectoryInfo(folderPath).GetFiles("*.png");

                if (pngFiles.Length > 0)
                {
                    var sortedFiles = pngFiles.OrderByDescending(f => f.CreationTime);

                    FileInfo newestFile = sortedFiles.ElementAtOrDefault(0);
                    FileInfo secondNewestFile = sortedFiles.ElementAtOrDefault(1);
                    FileInfo thirdNewestFile = sortedFiles.ElementAtOrDefault(2);

                    string newestFilePath = newestFile?.FullName;
                    string secondNewestFilePath = secondNewestFile?.FullName;
                    string thirdNewestFilePath = thirdNewestFile?.FullName;

                    this.Dispatcher.Invoke(() =>
                    {
                        ssArea1.Source = null;
                        ssArea2.Source = null;
                        ssArea3.Source = null;

                        if (newestFile == null && secondNewestFile == null && thirdNewestFile == null)
                            screenshotGrid.Visibility = Visibility.Collapsed;
                        else
                            screenshotGrid.Visibility = Visibility.Visible;

                        if (secondNewestFile == null && thirdNewestFile == null)
                            ssArea1.Source = LoadImage(newestFilePath);
                        else if (thirdNewestFile == null)
                        {
                            ssArea1.Source = LoadImage(secondNewestFilePath);
                            ssArea2.Source = LoadImage(newestFilePath);
                        }
                        else if (newestFile != null && secondNewestFile != null && thirdNewestFile != null)
                        {
                            ssArea1.Source = LoadImage(thirdNewestFilePath);
                            ssArea2.Source = LoadImage(secondNewestFilePath);
                            ssArea3.Source = LoadImage(newestFilePath);
                        }
                    });

                    // Free resources
                    pngFiles = null;
                    newestFile = null;
                    secondNewestFile = null;
                    thirdNewestFile = null;
                }
            }
            catch (Exception ex)
            {
                MainWindow.log.Error("Error occurred when loading image on content page", ex);
                MessageBox.Show("Error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private System.Windows.Media.ImageSource LoadImage(string imagePath)
        {
            return new BitmapImage(new Uri(imagePath));
        }

        private void tblockVndbChars_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Application.Current?.MainWindow is MainWindow mainWindow)
            {
                mainWindow.ContentArea.Content = new WikiContentControl(gl);
                mainWindow.CommandBar.Visibility = Visibility.Collapsed;

            }
        }
    }
}
