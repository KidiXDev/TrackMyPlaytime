using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace TMP.NET.Modules
{
    internal class CaptureHandler
    {

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static Bitmap TakeScreenshot(Process process)
        {
            IntPtr handle = process.MainWindowHandle;

            RECT rect;
            GetWindowRect(handle, out rect);

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            Bitmap screenshot = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(screenshot);

            IntPtr hdc = graphics.GetHdc();
            int PW_CLIENTONLY = 0x1; int PW_RENDERFULLCONTENT = 0x2;
            PrintWindow(handle, hdc, PW_CLIENTONLY | PW_RENDERFULLCONTENT);
            graphics.ReleaseHdc(hdc);
            graphics.Dispose();

            return screenshot;
        }

        public static bool IsWindowFocused(Process proc)
        {
            IntPtr foregroundWindow = GetForegroundWindow();
            uint foregroundProcessId;
            GetWindowThreadProcessId(foregroundWindow, out foregroundProcessId);

            return foregroundProcessId == proc.Id;
        }
    }
}

