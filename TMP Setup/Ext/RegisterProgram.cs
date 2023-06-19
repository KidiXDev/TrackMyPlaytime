using Microsoft.Win32;
using System;

namespace TMP_Setup.Ext
{
    public class RegisterProgram
    {
        RegistryKey keyCurrentUser = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes").OpenSubKey("tmpdotnet", true);
        RegistryKey keyUninstallerCurrentUser = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("Uninstall").OpenSubKey("Track My Playtime", true);

        public void DebugTest()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true).CreateSubKey("AAATEST");
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                Console.WriteLine($"Error registering CurrentUser: {ex.Message}");
            }
        }

        public void RegisterCurrentUser(string dir)
        {
            try
            {
                if (keyCurrentUser == null)
                {
                    keyCurrentUser = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes", true).CreateSubKey("tmpdotnet", true);
                    keyCurrentUser.SetValue(string.Empty, "URL:tmpdotnet Protocol");
                    keyCurrentUser.SetValue("URL Protocol", string.Empty);

                    RegistryKey shellKey = keyCurrentUser.CreateSubKey("shell", true);
                    RegistryKey openKey = shellKey.CreateSubKey("open", true);
                    RegistryKey commandKey = openKey.CreateSubKey("command", true);
                    commandKey.SetValue(string.Empty, $"\"{dir}\" \"%1\"");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                Console.WriteLine($"Error registering CurrentUser: {ex.Message}");
            }
        }

        public void RegisterAllUser(string dir)
        {
            try
            {
                RegistryKey keyAllUser = Registry.ClassesRoot.OpenSubKey("tmpdotnet");
                //Console.WriteLine(keyAllUser.Name);
                if (keyAllUser == null)
                {
                    //Console.WriteLine(keyAllUser.Name + 2);
                    keyAllUser = Registry.ClassesRoot.CreateSubKey("tmpdotnet", true);
                    keyAllUser.SetValue(string.Empty, "URL:tmpdotnet Protocol");
                    keyAllUser.SetValue("URL Protocol", string.Empty);

                    RegistryKey shellKey = keyAllUser.CreateSubKey("shell");
                    RegistryKey openKey = shellKey.CreateSubKey("open");
                    RegistryKey commandKey = openKey.CreateSubKey("command");
                    commandKey.SetValue(string.Empty, $"\"{dir}\" \"%1\"");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                Console.WriteLine($"Error registering AllUser: {ex.Message}");
            }
        }

        public void RegisterAllUserUninstaller(string executableDir, string installDir, string uninstallDir)
        {
            try
            {
                RegistryKey keyUninstallerAllUser = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("Uninstall", true).OpenSubKey("Track My Playtime", true);
                //Console.WriteLine(keyUninstallerAllUser.Name);
                if (keyUninstallerAllUser == null)
                {
                    Console.WriteLine("LOADING...");
                    keyUninstallerAllUser = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("Uninstall", true).CreateSubKey("Track My Playtime", true);
                    keyUninstallerAllUser.SetValue(string.Empty, string.Empty);
                    keyUninstallerAllUser.SetValue("DisplayIcon", $"\"{executableDir}\"");
                    keyUninstallerAllUser.SetValue("DisplayName", "Track My Playtime");
                    keyUninstallerAllUser.SetValue("InstallLocation", $"\"{installDir}\"");
                    keyUninstallerAllUser.SetValue("DisplayVersion", "latest");
                    keyUninstallerAllUser.SetValue("Publisher", "KidiXDev");
                    keyUninstallerAllUser.SetValue("UninstallString", $"\"{uninstallDir}\"");
                    Console.WriteLine("Done");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering AllUserUninstaller: {ex.Message}");
            }
        }

        public void RegisterCurrentUserUninstaller(string executableDir, string installDir, string uninstallDir)
        {
            try
            {
                if (keyUninstallerCurrentUser == null)
                {
                    keyUninstallerCurrentUser = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("Uninstall", true).CreateSubKey("Track My Playtime", true);
                    keyUninstallerCurrentUser.SetValue(string.Empty, string.Empty);
                    keyUninstallerCurrentUser.SetValue("DisplayIcon", $"\"{executableDir}\"");
                    keyUninstallerCurrentUser.SetValue("DisplayName", "Track My Playtime");
                    keyUninstallerCurrentUser.SetValue("InstallLocation", $"\"{installDir}\"");
                    keyUninstallerCurrentUser.SetValue("DisplayVersion", "latest");
                    keyUninstallerCurrentUser.SetValue("Publisher", "KidiXDev");
                    keyUninstallerCurrentUser.SetValue("UninstallString", $"\"{uninstallDir}\"");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering CurrentUserUninstaller: {ex.Message}");
            }
        }

        public string GetInstallDir()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("Uninstall").OpenSubKey("Track My Playtime"))
                {
                    if (key != null)
                    {
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
    }
}
