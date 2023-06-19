using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using IWshRuntimeLibrary;

namespace TMP.NET.Modules
{
    public class GUIDGen
    {
        public string GenerateGUID(int leng, ObservableCollection<GameList> list)
        {
            string guid = GenerateRandomString(leng);
            while (IsGuidExist(guid, list))
            {
                guid = GenerateRandomString(leng);
            }
            return guid;
        }

        private string GenerateRandomString(int leng)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            byte[] randomBytes = new byte[leng];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            StringBuilder sb = new StringBuilder(leng);
            foreach (byte b in randomBytes)
            {
                sb.Append(chars[b % (chars.Length)]);
            }
            return sb.ToString();
        }


        private bool IsGuidExist(string guid, ObservableCollection<GameList> list)
        {
            foreach (GameList game in list)
            {
                if (game.GUID.Equals(guid))
                    return true;
            }
            return false;
        }

        public class ShortcutCreator
        {
            public void CreateShortcut(string targetExePath, string shortcutPath, string iconPath, string args)
            {
                var shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = targetExePath;
                shortcut.Arguments = args;
                shortcut.IconLocation = iconPath;
                shortcut.Save();
            }

            public void CreateURLShortcut(String url, string shortcutPath, string iconPath)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(shortcutPath, false, Encoding.Unicode))
                    {
                        writer.WriteLine("[InternetShortcut]");
                        writer.WriteLine("URL=" + url);
                        writer.WriteLine("IconFile=" + iconPath);
                        writer.WriteLine("IconIndex=0");
                    }
                }
                catch (Exception ex)
                {
                    MainWindow.log.Error("Failed to create shortcut", ex);
                    MessageBox.Show($"Failed to create shortcut\nInfo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
