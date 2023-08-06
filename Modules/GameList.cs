using System;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TMP.NET.Modules
{
    /// <summary>
    /// Stores all information on games that have been added to the library
    /// </summary>
    [Serializable]
    public class GameList
    {
        private string _programType;
        public string GameName { get; set; }
        public string GameDev { get; set; }
        public ImageSource IconPath { get { return IconPathMethod(); } }

        #region Method
        /// <summary>
        /// Convert image icon to base-64 string
        /// </summary>
        /// <param name="imageSource"></param>
        /// <returns>Base64String of the icon image</returns>
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
        /// Extract icon from executable file
        /// </summary>
        /// <param name="GamePath"></param>
        /// <returns>ImageSource of the icon of an executable file</returns>
        public ImageSource ExtractIconFromExe(string GamePath)
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
        /// <summary>
        /// Re-convert icon from base-64 string to bitmap image
        /// </summary>
        /// <returns>ImageSource of the icon of an executable file</returns>
        public BitmapImage IconPathMethod()
        {
            if (IconBase64 == null)
            {
                try
                {
                    IconBase64 = IconToBase64String(ExtractIconFromExe(GamePath));
                }
                catch
                {
                    return null;
                }
            }

            if (IconBase64 == null)
                return null;

            byte[] bytes = Convert.FromBase64String(IconBase64);
            using (var stream = new MemoryStream(bytes))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                bytes = null;
                return image;
            }
        }
        /// <summary>
        /// Re-converts background image from base-64 string to bitmap image
        /// </summary>
        /// <returns>The BitmapImage that was converted to a base-64 string</returns>
        public BitmapImage BackgroundPath()
        {
            if (BackgroundBase64 == null)
                return null;

            byte[] bytes = Convert.FromBase64String(BackgroundBase64);
            using (var stream = new MemoryStream(bytes))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                bytes = null;
                return image;
            }
        }

        #endregion

        public string GamePath { get; set; }
        public string BackgroundBase64 { get; set; }
        public DateTime Tracker { get; set; }
        public string ImageKey { get; set; }
        public string ProgramType { get { return _programType ?? "x86"; } set { _programType = value; } }
        public string[] LaunchParameter { get; set; }
        public bool DisableTextractor { get; set; }
        public bool RunAsAdmin { get; set; }
        public bool UseLauncherHandler { get; set; }
        public bool HideGameTitle { get; set;}
        public bool HideImageKey { get; set; }
        public TimeSpan Playtime { get; set; }
        public string GUID { get; set; }
        public string BackgroundDir { get; set; }
        public string IconBase64 { get; set; }
        public DateTime DateCreated { get; set; }
        public Version DatabaseVersion { get; set; }
        public string Description { get; set; }
        public string[] Tag { get; set; }
        public string[] Website { get; set; }
        public DateTime ReleaseDate { get; set; }
        public uint VNID { get; set; }
    }
}
