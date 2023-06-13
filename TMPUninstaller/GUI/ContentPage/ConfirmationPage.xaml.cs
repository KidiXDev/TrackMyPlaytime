using System.Windows;
using System.Windows.Controls;

namespace TMPUninstaller.GUI.ContentPage
{
    /// <summary>
    /// Interaction logic for ConfirmationPage.xaml
    /// </summary>
    public partial class ConfirmationPage : Page
    {
        public ConfirmationPage()
        {
            InitializeComponent();
        }

        private void btnUninstall_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("This action will uninstall Track My Playtime from your computer", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (res == MessageBoxResult.OK)
            {
                this.NavigationService.Navigate(new UninstallProgress(cboxSaveGameList.IsChecked ?? false, cboxSaveConfig.IsChecked ?? false));
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Cancel uninstall track my playtime?", "Question", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (res == MessageBoxResult.OK)
                Application.Current.Shutdown();
        }
    }
}
