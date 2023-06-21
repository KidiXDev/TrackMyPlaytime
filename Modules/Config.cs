using System;

namespace TMP.NET.Modules
{
    [Serializable]
    public class Config
    {
        private bool _TimeTracking = true;
        private bool _DisableTextractor;
        private int _TextractorDelay = 3000;
        private bool _EnabledRichPresence = true;
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
    }
}
