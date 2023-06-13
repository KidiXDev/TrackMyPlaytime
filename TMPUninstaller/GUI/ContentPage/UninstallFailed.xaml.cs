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

namespace TMPUninstaller.GUI.ContentPage
{
    /// <summary>
    /// Interaction logic for UninstallFailed.xaml
    /// </summary>
    public partial class UninstallFailed : Page
    {
        string msg;
        public UninstallFailed(string msg)
        {
            InitializeComponent();

            this.msg = msg;
        }

        private void btnFinish_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            labelErrorMsg.Content = $"Error Message: {msg}";
        }
    }
}
