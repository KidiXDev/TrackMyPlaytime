using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace TMP.NET.Modules
{
    public static class CustomProtocolHandler
    {
        private const int WM_COPYDATA = 0x004A;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        public static void ProcessURL(string url)
        {
            MessageBox.Show($"URL masuk: {url}");
        }

        public static void SendURL(string url)
        {
            Process[] processes = Process.GetProcessesByName("TMP.NET");

            if (processes.Length > 0)
            {
                IntPtr hWnd = processes[0].MainWindowHandle;

                COPYDATASTRUCT cds = new COPYDATASTRUCT();
                cds.dwData = IntPtr.Zero;
                cds.lpData = Marshal.StringToHGlobalAnsi(url);
                cds.cbData = Encoding.Default.GetBytes(url).Length + 1;

                SendMessage(hWnd, WM_COPYDATA, IntPtr.Zero, ref cds);

                Marshal.FreeHGlobal(cds.lpData);
            }
        }
    }
}
