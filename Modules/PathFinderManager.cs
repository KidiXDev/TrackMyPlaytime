using System;
using System.IO;

namespace TMP.NET.Modules
{
    public class PathFinderManager
    {
#if DEBUG
        public static readonly string VndbDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"KidiXDev\\TrackMyPlaytime\\Debug\\data\\vndb");
        public static readonly string TraitDumpsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"KidiXDev\\TrackMyPlaytime\\Debug\\data\\traitdumps.kdm");
        public static readonly string GameLibraryDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KidiXDev\\TrackMyPlaytime\\Debug\\listdata.kdb");
        public static readonly string AppConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KidiXDev\\TrackMyPlaytime\\Debug\\config.cfg");
#else
        public static readonly string VndbDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"KidiXDev\\TrackMyPlaytime\\data\\vndb");
        public static readonly string TraitDumpsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"KidiXDev\\TrackMyPlaytime\\data\\traitdumps.kdm");
        public static readonly string GameLibraryDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KidiXDev\\TrackMyPlaytime\\listdata.kdb");
        public static readonly string AppConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KidiXDev\\TrackMyPlaytime\\config.cfg");
#endif
    }
}
