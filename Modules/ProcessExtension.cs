using System;
using System.Diagnostics;
using System.Windows;

namespace TMP.NET.Modules
{
    public static class ProcessExtension
    {
        public static bool IsRunning(this Process process)
        {
            if (process == null)
                throw new ArgumentNullException("process");

            try
            {
                Process.GetProcessById(process.Id);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
