using System.Windows;
using System.Windows.Controls;

namespace TMP_Setup.GUI.ContentPage
{
    /// <summary>
    /// Interaction logic for LicenseAgreementPage.xaml
    /// </summary>
    public partial class LicenseAgreementPage : Page
    {

        bool type;

        private string licenseText = @" Thank you for trying this program, this program is free and 
 can be downloaded by everyone.

 Official Links: https://github.com/KidiXDev/TrackMyPlaytime
 Website: https://trackmyplaytime.netlify.app
 Contact Support: kidixdev@gmail.com
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
            if (cbAgree.IsChecked ?? false)
            {
                this.NavigationService.Navigate(new DirectoryPage(type));
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("The program will not be installed", "Cancel Setup?", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
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
