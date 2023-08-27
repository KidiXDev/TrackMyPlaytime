﻿using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace TMP.NET.WindowUI
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            labelVersion.Content = $"Version: v{version.Major}.{version.Minor}.{version.Build} beta";
            labelBeta.Content = $"Private Beta Build {version.Revision}";

        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
