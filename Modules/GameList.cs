using System;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TMP.NET.Modules
{
    [Serializable]
    public class GameList
    {
        private string _programType;
        public string GameName { get; set; }
        public string GameDev { get; set; }
        public ImageSource IconPath { get { return IconPathMethod(); } }
        public ImageSource IconPathMethod()
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
    }
}
