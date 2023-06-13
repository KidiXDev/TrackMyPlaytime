using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TMP_Setup.Ext
{
    internal class AdminChecker
    {
        public bool IsUserAdministrator()
        {
            bool isAdmin;
            try
            {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception)
            {
                isAdmin = false;
            }
            return isAdmin;
        }

        public bool CheckWritePermission(string folderPath)
        {
            try
            {
                string testDirectory = Path.Combine(folderPath, Guid.NewGuid().ToString());
                Directory.CreateDirectory(testDirectory);
                Directory.Delete(testDirectory);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking folder permission: {ex.Message}");
                return false;
            }
        }
    }
}
