using Microsoft.Win32;
using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TMP.NET.Modules;
using System.Windows.Interop;
using System.Linq;

namespace TMP.NET.WindowUI
{
    /// <summary>
    /// Add and Edit Game List
    /// </summary>
    public partial class ListForms : Window
    {
        public BitmapImage imgBitmap;
        public string imgDir;
        public string v_ProgramType;
        public string[] v_LaunchParameter;
        private string lastImageSelected;
        public bool isDeleted, isValid;
        private Config setting;
        private GameList _currentSelectedList;

        private GUIDGen.ShortcutCreator _gen = new GUIDGen.ShortcutCreator();

        public Bitmap ResizeImage(Image image, int width, int height)
        {
            try
            {
                var destRect = new Rectangle(0, 0, width, height);
                var destImage = new Bitmap(width, height);

                destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }

                return destImage;
            }
            catch (Exception ex)
            {
                MainWindow.log.Error("Failed to Resize Image", ex);
                return null;
            }
        }

        public void CreateShortcut(GameList gl)
        {
            var shortcutName = gl.GameName;
            string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char invalidChar in invalidChars)
            {
                shortcutName = shortcutName.Replace(invalidChar.ToString(), "");
            }

            var shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + $"\\{shortcutName}.url";
            var iconPath = gl.GamePath;
            _gen.CreateURLShortcut($"tmpdotnet://launch/{gl.GUID}", shortcutPath, iconPath);
        }

        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            try
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                    enc.Save(outStream);
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                    return new Bitmap(bitmap);
                }
            }
            catch (Exception ex)
            {
                MainWindow.log.Error("Failed to Convert BitmapImage2Bitmap", ex);
                return null;
            }
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            try
            {
                using (var memory = new MemoryStream())
                {
                    bitmap.Save(memory, ImageFormat.Png);
                    memory.Position = 0;

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                MainWindow.log.Error("Failed to Convert Bitmap2BitmapImage", ex);
                return null;
            }
        }

        private void ValidateImageKey(bool onButton)
        {
            if (m_CheckImageURL(tbImageKey.Text))
            {
                isValid = true;

                if (onButton)
                    MessageBox.Show("Your image key URL is valid.\t\t", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                isValid = false;
                MessageBox.Show("Your image URL is invalid\t\t", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public string m_ConvertIMG2Base64(ImageSource image)
        {
            try
            {
                if (image == null)
                    return null;

                var bitmapImage = image as BitmapImage;

                var encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    return Convert.ToBase64String(stream.ToArray());
                }
            }
            catch (Exception ex)
            {
                MainWindow.log.Warn("Exception Thrown when convert img to base64 string", ex);
                return null;
            }
        }

        public bool m_CheckImageURL(string uriToImage)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uriToImage);
                request.Method = "HEAD";
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK && response.ContentType == "image/jpeg" ||
                        response.StatusCode == HttpStatusCode.OK && response.ContentType == "image/png")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Convert image icon to base-64 string
        /// </summary>
        /// <param name="imageSource"></param>
        /// <returns>Base64String</returns>
        public string IconToBase64String(ImageSource imageSource)
        {
            var bitmapSource = (BitmapSource)imageSource;

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            using (var memoryStream = new MemoryStream())
            {
                encoder.Save(memoryStream);
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
        /// <summary>
        /// Re-convert icon from base-64 string to bitmap image
        /// </summary>
        /// <param name="GamePath"></param>
        /// <returns>ImageSource of the icon of an executable file</returns>
        public ImageSource IconPathMethod(string GamePath)
        {
            try
            {
                if (File.Exists(GamePath))
                {
                    System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(GamePath);
                    return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
                else { return null; }
            }
            catch
            {
                Console.WriteLine("Failed to extract icon from executable files...");
                return null;
            }
        }

        public ListForms(bool IsEdit, GameList gl, GameList currentSelectedList, Config setting)
        {
            InitializeComponent();
            this.setting = setting;
            _currentSelectedList = currentSelectedList;
            System.Windows.Controls.Image deleteImage = (System.Windows.Controls.Image)FindResource("DeleteImage");
            btnDelete.Content = deleteImage;
            btnDelete.Click -= btnDelete_Click;
            btnDelete.Click += btnDeleteImage_Click;
            btnDeleteImage.Visibility = Visibility.Collapsed;
            btnShortcut.Visibility = Visibility.Hidden;
            if (gl != null && IsEdit)
                v_GL = gl;

            if (IsEdit)
            {
                System.Windows.Controls.Image trashIcon = (System.Windows.Controls.Image)FindResource("TrashIcon");
                btnDelete.Content = trashIcon;
                LoadValue();
                this.Title = "Edit Game";
                btnDelete.Visibility = Visibility.Visible;
                btnShortcut.Visibility = Visibility.Visible;
                btnDeleteImage.Visibility = Visibility.Visible;
                btnDelete.Click -= btnDeleteImage_Click;
                btnDelete.Click += btnDelete_Click;
                cbCreateShortcut.Visibility = Visibility.Hidden;
                var window = (MainWindow)Application.Current.MainWindow;
                if (window.state == MainWindow.AppState.Running)
                {
                    if(gl == currentSelectedList)
                        btnDelete.IsEnabled = false;
                }
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

            if (imgBitmap == null)
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            else
            {
                if (!string.IsNullOrEmpty(imgDir))
                    ofd.InitialDirectory = Path.GetDirectoryName(imgDir);
                else if (!string.IsNullOrEmpty(lastImageSelected))
                    ofd.InitialDirectory = Path.GetDirectoryName(lastImageSelected);
                else
                    ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }

            if (ofd.ShowDialog() == true)
            {
                var chk = new BitmapImage(new Uri(ofd.FileName));
                if (chk.PixelWidth > 1920 || chk.PixelHeight > 1080)
                {
                    if(!setting.UncompressedArtwork)
                        imgBitmap = ToBitmapImage(ResizeImage(BitmapImage2Bitmap(chk), 1280, 720));
                    else
                        imgBitmap = chk;
                }
                else
                    imgBitmap = chk;

                imgDir = ofd.FileName;
                imgArtwork.Source = imgBitmap;
                imgOverlay.Source = new BitmapImage(new Uri("pack://application:,,,/TMP.NET;component/Resources/overlay2.png"));
                lastImageSelected = ofd.FileName;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Title = Title + " | Busy...";
            if (!FieldChecker())
            {
                Title = Title.Substring(0, Title.IndexOf('|')).Trim();
                return;
            }

            if (rbX64.IsChecked ?? false)
                v_ProgramType = "x64";
            else
                v_ProgramType = "x86";

            v_LaunchParameter = tbLaunchParameter.Text.Split(' ');
            this.DialogResult = true;
        }

        private void btnGameDir_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Executable (*.exe)|*.exe";

            if (string.IsNullOrEmpty(tbGameDir.Text))
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            else
                ofd.InitialDirectory = Path.GetDirectoryName(tbGameDir.Text);

            if (ofd.ShowDialog() == true)
            {
                tbGameDir.Text = ofd.FileName;
            }
        }

        private void btnValidate_Click(object sender, RoutedEventArgs e)
        {
            ValidateImageKey(true);
        }

        private GameList v_GL;

        #region EDIT
        private void LoadValue()
        {
            tbGameTitle.Text = v_GL.GameName;
            tbDeveloper.Text = v_GL.GameDev;
            tbGameDir.Text = v_GL.GamePath;
            imgDir = v_GL.BackgroundDir;

            if (v_GL.BackgroundBase64 != null)
            {
                imgBitmap = v_GL.BackgroundPath();
                imgArtwork.Source = imgBitmap;
                imgOverlay.Source = new BitmapImage(new Uri("pack://application:,,,/TMP.NET;component/Resources/overlay2.png"));
            }

            if (v_GL.ProgramType.Equals("x64"))
                rbX64.IsChecked = true;
            else
                rbX86.IsChecked = true;

            tbLaunchParameter.Text = string.Join(" ", v_GL.LaunchParameter == null || v_GL.LaunchParameter.Length == 0 ? Array.Empty<string>() : v_GL.LaunchParameter);
            cbDisableTextractor.IsChecked = v_GL.DisableTextractor;
            cbRunAsAdmin.IsChecked = v_GL.RunAsAdmin;
            cbUseLauncherHandler.IsChecked = v_GL.UseLauncherHandler;
            tbImageKey.Text = v_GL.ImageKey;
            cbHideGameTitle.IsChecked = v_GL.HideGameTitle;
            cbHideImageKey.IsChecked = v_GL.HideImageKey;
        }
        #endregion

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Title = Title + " | Busy...";
            string dateAdded = "Unknown";
            if (v_GL.DateCreated != DateTime.MinValue)
                dateAdded = v_GL.DateCreated.ToString("dd-MMMM-yyyy");

            var res = MessageBox.Show($"Are you sure want to delete this game from the library?\n\nTitle: {v_GL.GameName}\nDeveloper: {v_GL.GameDev}\nDate Added: {dateAdded}", "Are you sure?", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (res == MessageBoxResult.OK)
            {
                isDeleted = true;
                this.DialogResult = false;
            }

            try
            {
                var shortcutName = v_GL.GameName;
                string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                foreach (char invalidChar in invalidChars)
                {
                    shortcutName = shortcutName.Replace(invalidChar.ToString(), "");
                }

                var shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + $"\\{shortcutName}.url";

                if (File.Exists(shortcutPath))
                    File.Delete(shortcutPath);
            }
            catch(Exception ex)
            {
                MainWindow.log.Error("Exception Thrown when Delete GameList", ex);
                MessageBox.Show($"Exception thrown.\nInfo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnShortcut_Click(object sender, RoutedEventArgs e)
        {
            CreateShortcut(v_GL);
        }

        private void btnDeleteImage_Click(object sender, RoutedEventArgs e)
        {
            imgDir = null;
            imgBitmap = null;
            imgArtwork.Source = new BitmapImage(new Uri("pack://application:,,,/TMP.NET;component/Resources/add-image.png"));
            imgOverlay.Source = new BitmapImage(new Uri("pack://application:,,,/TMP.NET;component/Resources/overlay.png"));
        }

        private bool FieldChecker()
        {
            if (tbGameTitle.Text.Length > 64 || tbDeveloper.Text.Length > 64)
            {
                MessageBox.Show("The maximum character limit allowed is no more than 64 characters.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrEmpty(tbGameTitle.Text) || string.IsNullOrEmpty(tbGameDir.Text))
            {
                MessageBox.Show("Please enter all field correctly.\t\t", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!File.Exists(tbGameDir.Text))
            {
                MessageBox.Show("Please enter the correct executable directory\t", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!string.IsNullOrEmpty(tbImageKey.Text))
            {
                ValidateImageKey(false);
                if (!isValid)
                {
                    MessageBox.Show("Please validate image key first\t", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }
    }
}
