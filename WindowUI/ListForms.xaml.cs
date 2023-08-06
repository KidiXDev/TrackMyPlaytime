using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TMP.NET.Modules;
using TMP.NET.WindowUI.CustomDialogWindow;

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
        public bool isValid;
        private Config setting;
        private GameList _currentSelectedList;
        public uint vnid { get; set; }

        private GUIDGen.ShortcutCreator _gen = new GUIDGen.ShortcutCreator();

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper((Window)sender).Handle;
            var value = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, (int)(value & ~WS_MAXIMIZEBOX));
        }

        public string GetFlowDocumentText(FlowDocument flowDocument)
        {
            StringBuilder stringBuilder = new StringBuilder();

            int totalParagraphs = flowDocument.Blocks.Count;

            for (int i = 0; i < totalParagraphs; i++)
            {
                Block block = flowDocument.Blocks.ElementAt(i);
                if (block is Paragraph paragraph)
                {
                    foreach (Inline inline in paragraph.Inlines)
                    {
                        if (inline is Run run)
                            stringBuilder.Append(run.Text);
                    }

                    if (i < totalParagraphs - 1)
                        stringBuilder.AppendLine();
                }
            }

            return stringBuilder.ToString();
        }


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

        /// <summary>
        /// Check if Image URL is valid
        /// </summary>
        /// <param name="uriToImage"></param>
        /// <returns><see langword="TRUE"/> if <paramref name="uriToImage"/> is valid image url</returns>
        public bool m_CheckImageURL(string uriToImage)
        {
            HttpWebRequest request;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(uriToImage);
                request.Method = "HEAD";
                request.Timeout = 15000;
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
            catch (TimeoutException ex)
            {
                MessageBox.Show($"Failed to check image key\nInfo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                request = null;
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
            if (gl != null && IsEdit)
                v_GL = gl;

            if (IsEdit)
            {
                LoadValue();
                this.Title = "Edit Game";
                cbCreateShortcut.Visibility = Visibility.Hidden;
            }
        }

        private void DeInit()
        {
            imgArtwork.Source = null;
            imgArtwork.UpdateLayout();
            imgOverlay.Source = null;
            imgOverlay.UpdateLayout();
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
                    if (!setting.UncompressedArtwork)
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
            DeInit();
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

            DeInit();


            var wd = new DownloadMetadataDlgWindow(vnid);
            wd.Owner = this;
            wd.ShowDialog();

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
        /// <summary>
        /// Load value
        /// </summary>
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
            rtbDescription.AppendText(v_GL.Description);
            vnid = v_GL.VNID;

            if(v_GL.Tag != null)
                tbTag.Text = string.Join(",", v_GL.Tag);

            if (v_GL.ReleaseDate == DateTime.MinValue)
                datePickerReleaseDate.SelectedDate = null;
            else
                datePickerReleaseDate.SelectedDate = v_GL.ReleaseDate;

            if(v_GL.Website != null)
            {
                string[] webNames = new string[v_GL.Website.Length];
                string[] webUrls = new string[v_GL.Website.Length];
                for(int i = 0; i < v_GL.Website.Length; i++)
                {
                    if (!string.IsNullOrEmpty(v_GL.Website[i]))
                    {
                        string[] parts = v_GL.Website[i].Split(';');

                        if (parts.Length == 2)
                        {
                            webNames[i] = parts[0];
                            webUrls[i] = parts[1];
                        }
                        else
                        {
                            Console.WriteLine("Unsupported format" + i);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(v_GL.Website[0]))
                {
                    tbWebName1.Text = webNames[0];
                    tbWeb1.Text = webUrls[0];
                }
                else
                {
                    tbWebName1.Text = string.Empty;
                    tbWeb1.Text = string.Empty;
                }
                if (!string.IsNullOrEmpty(v_GL.Website[1]))
                {
                    tbWebName2.Text = webNames[1];
                    tbWeb2.Text = webUrls[1];
                }
                else
                {
                    tbWebName2.Text = string.Empty;
                    tbWeb2.Text = string.Empty;
                }
                if (!string.IsNullOrEmpty(v_GL.Website[2]))
                {
                    tbWebName3.Text = webNames[2];
                    tbWeb3.Text = webUrls[2];
                }
                else
                {
                    tbWebName3.Text = string.Empty;
                    tbWeb3.Text = string.Empty;
                }
            }
        }
        #endregion

        private void btnDeleteImage_Click(object sender, RoutedEventArgs e)
        {
            imgDir = null;
            imgBitmap = null;
            imgArtwork.Source = new BitmapImage(new Uri("pack://application:,,,/TMP.NET;component/Resources/no-image.png"));
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

            if(string.IsNullOrEmpty(tbWebName1.Text) && !string.IsNullOrEmpty(tbWeb1.Text) || string.IsNullOrEmpty(tbWebName2.Text) && !string.IsNullOrEmpty(tbWeb2.Text) || string.IsNullOrEmpty(tbWebName3.Text) && !string.IsNullOrEmpty(tbWeb3.Text))
            {
                MessageBox.Show("Please enter website name\t", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!string.IsNullOrEmpty(tbWeb1.Text))
            {
                if (!Uri.IsWellFormedUriString(tbWeb1.Text, UriKind.Absolute))
                {
                    MessageBox.Show($"Website \"{tbWebName1.Text}\" has a invalid url\t", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(tbWeb2.Text))
            {
                if (!Uri.IsWellFormedUriString(tbWeb2.Text, UriKind.Absolute))
                {
                    MessageBox.Show($"Website \"{tbWebName2.Text}\" has a invalid url\t", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(tbWeb3.Text))
            {
                if (!Uri.IsWellFormedUriString(tbWeb3.Text, UriKind.Absolute))
                {
                    MessageBox.Show($"Website \"{tbWebName3.Text}\" has a invalid url\t", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }

        private void btnTagSelector_Click(object sender, RoutedEventArgs e)
        {
            // This function is on development and may not be implement for this time
            TagSelectorWindow form = new TagSelectorWindow();
            form.ShowDialog();
        }

        private void tbWebName1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbWebName1.Text) && !string.IsNullOrEmpty(tbWeb1.Text))
            {
                tbWebName2.IsEnabled = true;
                tbWeb2.IsEnabled = true;

                if (!string.IsNullOrEmpty(tbWebName2.Text) && !string.IsNullOrEmpty(tbWeb2.Text))
                {
                    tbWebName3.IsEnabled = true;
                    tbWeb3.IsEnabled = true;
                }
                else
                {
                    tbWebName3.IsEnabled = false;
                    tbWeb3.IsEnabled = false;
                }
            }
            else
            {
                tbWebName2.IsEnabled = false;
                tbWeb2.IsEnabled = false;
                tbWebName3.IsEnabled = false;
                tbWeb3.IsEnabled = false;
            }
        }

        private void tbWeb1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbWebName1.Text) && !string.IsNullOrEmpty(tbWeb1.Text))
            {
                tbWebName2.IsEnabled = true;
                tbWeb2.IsEnabled = true;

                if (!string.IsNullOrEmpty(tbWebName2.Text) && !string.IsNullOrEmpty(tbWeb2.Text))
                {
                    tbWebName3.IsEnabled = true;
                    tbWeb3.IsEnabled = true;
                }
                else
                {
                    tbWebName3.IsEnabled = false;
                    tbWeb3.IsEnabled = false;
                }
            }
            else
            {
                tbWebName2.IsEnabled = false;
                tbWeb2.IsEnabled = false;
                tbWebName3.IsEnabled = false;
                tbWeb3.IsEnabled = false;
            }
        }

        private void tbWebName2_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbWebName2.Text) && !string.IsNullOrEmpty(tbWeb2.Text))
            {
                tbWebName3.IsEnabled = true;
                tbWeb3.IsEnabled = true;
            }
            else
            {
                tbWebName3.IsEnabled = false;
                tbWeb3.IsEnabled = false;
            }
        }

        private void tbWeb2_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbWebName2.Text) && !string.IsNullOrEmpty(tbWeb2.Text))
            {
                tbWebName3.IsEnabled = true;
                tbWeb3.IsEnabled = true;
            }
            else
            {
                tbWebName3.IsEnabled = false;
                tbWeb3.IsEnabled = false;
            }
        }

        private void btnMetadataDownload_Click(object sender, RoutedEventArgs e)
        {
            var form = new VNDBMetadataDownloaderWindow();
            var res = form.ShowDialog() ?? false;
            if (res)
            {
                tbGameTitle.Text = form.GameTitle;
                tbDeveloper.Text = form.Developer;
                rtbDescription.Document.Blocks.Clear();
                if(!string.IsNullOrEmpty(form.Description))
                    rtbDescription.AppendText(GetFilteredString(RemoveTags(form.Description)));
                tbTag.Text = "Visual Novel";
                imgBitmap = new BitmapImage(new Uri(form.ImageURL));
                imgOverlay.Source = new BitmapImage(new Uri("pack://application:,,,/TMP.NET;component/Resources/overlay2.png"));
                imgArtwork.Source = imgBitmap;
                DateTime rel = DateTime.Parse(form.ReleaseDate);
                Console.WriteLine("RELEASE DATE: " + form.ReleaseDate);
                datePickerReleaseDate.SelectedDate = rel;
                tbWebName1.Text = "VNDB";
                tbWeb1.Text = form.Web1;
                tbWebName2.Text = "Official Web";
                tbWeb2.Text = form.Web2;
                vnid = form.VNID;
            }
        }

        public string GetFilteredString(string input)
        {
            Regex regex = new Regex(@"\[url=([^]]+)\](.*?)\[/url\]");
            string result = regex.Replace(input, "$2");
            return result;
        }

        public string RemoveTags(string input)
        {
            // Define regex pattern for each tag
            string[] tags = { @"\[b\](.*?)\[/b\]",
                          @"\[i\](.*?)\[/i\]",
                          @"\[u\](.*?)\[/u\]",
                          @"\[s\](.*?)\[/s\]",
                          @"\[spoiler\](.*?)\[/spoiler\]",
                          @"\[quote\](.*?)\[/quote\]",
                          @"\[raw\](.*?)\[/raw\]",
                          @"\[code\](.*?)\[/code\]" };

            // Remove all the tags from the input
            foreach (string tag in tags)
            {
                input = Regex.Replace(input, tag, "$1");
            }

            return input;
        }
    }
}
