using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Interop;

namespace TMP.NET.Modules
{
    internal class NotificationHandler
    {
        /*// Buat instance dari NotifyIcon
        private NotifyIcon notifyIcon;

        // Method untuk menampilkan notifikasi
        private void ShowNotification(string message)
        {
            NotifyIcon notifyIcon = new NotifyIcon();
            notifyIcon.Icon = SystemIcons.Information;

            // Membuat HwndSource dari window utama
            HwndSource source = (HwndSource)HwndSource.FromVisual(this);

            // Mendapatkan handle window utama
            IntPtr handle = source.Handle;

            // Mengaktifkan notifikasi balon
            notifyIcon.BalloonTipTitle = "Screen Captured!";
            notifyIcon.BalloonTipText = message;
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(3000);

            // Membersihkan notifikasi setelah beberapa waktu
            Timer timer = new Timer();
            timer.Interval = 3000;
            timer.Tick += (sender, e) =>
            {
                notifyIcon.Dispose();
                timer.Stop();
            };
            timer.Start();
        }*/
    }
}
