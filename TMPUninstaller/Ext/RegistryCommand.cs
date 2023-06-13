using Microsoft.Win32;

namespace TMPUninstaller.Ext
{
    internal class RegistryCommand
    {
        public bool isCurrentUser = false;
        public string GetInstallDir()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("Uninstall").OpenSubKey("Track My Playtime"))
                {
                    if (key != null)
                    {
                        isCurrentUser = true;
                        return key.GetValue("InstallLocation").ToString();
                    }
                    else
                    {
                        using (RegistryKey key2 = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("Uninstall").OpenSubKey("Track My Playtime"))
                        {
                            if (key2 != null)
                            {
                                return key2.GetValue("InstallLocation").ToString();
                            }
                        }
                    }

                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public void DeleteRegistry()
        {
            if(isCurrentUser)
            {
                using (var key = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("Uninstall", true))
                {
                    if (key != null)
                        key.DeleteSubKeyTree("Track My Playtime");

                    using (var key2 = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes", true))
                    {
                        if (key2 != null)
                            key2.DeleteSubKeyTree("tmpdotnet");
                    }
                }
            }
            else
            {
                using (var key = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("Uninstall", true))
                {
                    if(key != null)
                        key.DeleteSubKeyTree("Track My Playtime");

                    using (var key2 = Registry.ClassesRoot.OpenSubKey("tmpdotnet", true))
                    {
                        if (key2 != null)
                            Registry.ClassesRoot.DeleteSubKeyTree("tmpdotnet");
                    }
                }
            }
        }
    }
}
