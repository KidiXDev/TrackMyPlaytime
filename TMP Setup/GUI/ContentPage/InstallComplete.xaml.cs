using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System;

namespace TMP_Setup.GUI.ContentPage
{
    /// <summary>
    /// Interaction logic for InstallComplete.xaml
    /// </summary>
    public partial class InstallComplete : Page
    {
        string dir;

        public InstallComplete(string dir)
        {
            InitializeComponent();

            this.dir = dir;
        }

        private void btnFinish_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboxLaunch.IsChecked ?? true)
                    Process.Start(dir + "\\tmp.exe");
                if (cboxReadme.IsChecked ?? true)
                    Process.Start("notepad.exe", dir + "\\ReadMe.txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Somthing wrong\n" + ex.Message);
            }

            Application.Current.Shutdown();
        }
    }
}
