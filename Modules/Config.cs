﻿using System;

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
        private FilterConfig _FilterConfig = new FilterConfig();
        public string x86Directory { get; set; }
        public string x64Directory { get; set; }
        public bool DisableTextractor { get { return _DisableTextractor; } set { _DisableTextractor = value; } }
        public bool TimeTracking { get { return _TimeTracking; } set { _TimeTracking = value; } }
        public bool EnabledRichPresence { get { return _EnabledRichPresence; } set { _EnabledRichPresence = value; } }
        public int SelectedIndex { get; set; }
        public int TextractorDelay { get { return _TextractorDelay; } set { _TextractorDelay = value; } }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Top { get; set; }
        public double Left { get; set; }
        public bool Maximized { get; set; }
        public bool AutoCheckUpdate { get { return _AutoCheckUpdate; } set { _AutoCheckUpdate = value; } }
        public bool EnabledScreenshot { get { return _EnabledScreenshot; } set { _EnabledScreenshot = value; } }
        public bool UncompressedArtwork { get; set; }
        public FilterConfig filterConfig { get { return _FilterConfig; } set { _FilterConfig = value; } }
        /// <summary>
        /// Stores information for filter and sort settings in the library view
        /// </summary>
        public class FilterConfig
        {
            private int _FilterIndex = 0;
            private int _SortIndex = 0;
            public int FilterIndex { get { return _FilterIndex; } set { _FilterIndex = value; } }
            public int SortIndex { get { return _SortIndex;} set { _SortIndex = value; } }
        }
    }
}
