using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TMP_Setup.Ext;

namespace TMP_Setup.GUI.ContentPage
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        AdminChecker _chk = new AdminChecker();

        public Page1()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if(rbCurrentUser.IsChecked ?? true)
            {
                this.NavigationService.Navigate(new LicenseAgreementPage(true));
            }
            else
            {
                if(_chk.IsUserAdministrator())
                    this.NavigationService.Navigate(new LicenseAgreementPage(false));
                else
                {
                    RestartAsAdministrator("-admin");
                }
            }
        }

        private void rbCurrentUser_Click(object sender, RoutedEventArgs e)
        {
            labelInfo.Visibility = Visibility.Collapsed;
        }

        private void rbAllUser_Click(object sender, RoutedEventArgs e)
        {
            labelInfo.Visibility = Visibility.Visible;
        }

        private void RestartAsAdministrator(string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
            startInfo.Arguments = arguments;
            startInfo.Verb = "runas";
            try
            {
                Process.Start(startInfo);
                Application.Current.Shutdown();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("To install the program on all users you need to allow the program to use administrator privileges so that the process can continue.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
