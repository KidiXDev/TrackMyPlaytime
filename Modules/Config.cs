using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using TMP.NET.WindowUI.ContentWindow;
using VndbSharp.Models.Common;

namespace TMP.NET.Modules
{
    /// <summary>
    /// Stores information for other technical settings and configurations
    /// </summary>
    [Serializable]
    public class Config
    {
        private bool _TimeTracking = true;
        private bool _DisableTextractor;
        private int _TextractorDelay = 3000;
        private bool _EnabledRichPresence = true;
        private bool _AutoCheckUpdate = true;
        private bool _EnabledScreenshot = true;
        private int _ScreenshotApiIndex = 0;
        private FilterConfig _FilterConfig = new FilterConfig();
        private ContentConfig _ContentConfig = new ContentConfig();
        private VndbConfig _VndbConfig = new VndbConfig();
        private string _ScreenshotFolderDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Track My Playtime");
        private double _Width = 1280;
        private double _Height = 720;
        public string x86Directory { get; set; }
        public string x64Directory { get; set; }
        public bool DisableTextractor { get { return _DisableTextractor; } set { _DisableTextractor = value; } }
        public bool TimeTracking { get { return _TimeTracking; } set { _TimeTracking = value; } }
        public bool EnabledRichPresence { get { return _EnabledRichPresence; } set { _EnabledRichPresence = value; } }
        public int SelectedIndex { get; set; }
        public int TextractorDelay { get { return _TextractorDelay; } set { _TextractorDelay = value; } }
        public double Width { get { return _Width; } set { _Width = value; } }
        public double Height { get { return _Height; } set { _Height = value; } }
        public double Top { get; set; }
        public double Left { get; set; }
        public bool Maximized { get; set; }
        public bool AutoCheckUpdate { get { return _AutoCheckUpdate; } set { _AutoCheckUpdate = value; } }
        public bool EnabledScreenshot { get { return _EnabledScreenshot; } set { _EnabledScreenshot = value; } }
        public bool UncompressedArtwork { get; set; }
        public bool LowSpecMode { get; set; }
        public int ScreenshotApiIndex { get { return _ScreenshotApiIndex; } set { _ScreenshotApiIndex = value; } }
        public string ScreenshotFolderDir { get { return _ScreenshotFolderDir; } set { _ScreenshotFolderDir = value; } }
        public FilterConfig filterConfig { get { return _FilterConfig; } set { _FilterConfig = value; } }
        public ContentConfig contentConfig { get { return _ContentConfig; } set { _ContentConfig = value; } }
        public bool FirstTimeInfo { get; set; }
        public VndbConfig vndbConfig { get { return _VndbConfig; } set { _VndbConfig = value; } }

        /// <summary>
        /// Stores information for filter and sort settings in the library view
        /// </summary>
        public class FilterConfig
        {
            private int _FilterIndex = 0;
            private int _SortIndex = 0;
            public int FilterIndex { get { return _FilterIndex; } set { _FilterIndex = value; } }
            public int SortIndex { get { return _SortIndex; } set { _SortIndex = value; } }
        }
        /// <summary>
        /// Stores configutaion information about content page
        /// </summary>
        public class ContentConfig : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            private double _BackgroundBlurValue = 17.0;
            private double _BackgroundOpacityValue = 0.5;
            public double BackgroundBlurValue { 
                get { return _BackgroundBlurValue; } 
                set { 
                    if(value != this._BackgroundBlurValue)
                    {
                        _BackgroundBlurValue = value;
                        NotifyPropertyChanged("BackgroundBlurValue");
                    }
                } 
            }
            public double BackgroundOpacityValue { 
                get { return _BackgroundOpacityValue; } 
                set { 
                    if(value != this._BackgroundOpacityValue)
                    {
                        _BackgroundOpacityValue = value;
                        NotifyPropertyChanged("BackgroundOpacityValue");
                    }
                } 
            }
        }

        public class VndbConfig
        {
            private SpoilerLevel _SpoilerLevel = SpoilerLevel.Major;
            public SpoilerLevel SpoilerSetting { get { return _SpoilerLevel; } set { _SpoilerLevel = value; } }
            public bool ShowSexualTrait { get; set; }
        }
    }
}
