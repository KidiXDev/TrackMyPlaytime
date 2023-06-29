using System.Windows;
using System.Windows.Controls;

namespace TMP_Setup.GUI.ContentPage
{
    /// <summary>
    /// Interaction logic for InstallFailed.xaml
    /// </summary>
    public partial class InstallFailed : Page
    {
        string msg;
        public InstallFailed(string msg)
        {
            InitializeComponent();

            this.msg = msg;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            labelErrorMsg.Text = $"Error Message: {msg}";
        }

        private void btnFinish_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
