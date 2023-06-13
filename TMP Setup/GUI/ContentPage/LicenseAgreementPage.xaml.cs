using System;
using System.Collections.Generic;
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

namespace TMP_Setup.GUI.ContentPage
{
    /// <summary>
    /// Interaction logic for LicenseAgreementPage.xaml
    /// </summary>
    public partial class LicenseAgreementPage : Page
    {

        bool type;

        private string licenseText = @"
 This program is completely free, if any party sells it, 
 it is certain that you have been scam.
 Because I don't charge anything

 I made this program entirely myself, if there are other 
 parties who claim that this is theirs, please let me know immediately.

 It is strictly forbidden to sell, re-upload, change any content from
 this program without my permission.
";


        public LicenseAgreementPage(bool type)
        {
            InitializeComponent();
            this.type = type;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            tblockLicense.Text = licenseText;
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if(cbAgree.IsChecked ?? false)
            {
                this.NavigationService.Navigate(new DirectoryPage(type));
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("The program will not be installed", "Cancel Setup?", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if(result == MessageBoxResult.OK)
            {
                Application.Current.Shutdown();
            }
        }

        private void cbAgree_Click(object sender, RoutedEventArgs e)
        {
            btnNext.IsEnabled = cbAgree.IsChecked ?? false;
        }
    }
}
