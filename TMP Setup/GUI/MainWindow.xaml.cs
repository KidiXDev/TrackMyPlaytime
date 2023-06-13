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
using TMP_Setup.Ext;
using TMP_Setup.GUI.ContentPage;

namespace TMP_Setup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] args = Environment.GetCommandLineArgs();
        public MainWindow()
        {
            InitializeComponent();

            foreach (string arg in args)
            {
                if (arg.Equals("-admin"))
                {
                    ContentFrame.NavigationService.Navigate(new LicenseAgreementPage(false));
                    return;
                }
            }

            ContentFrame.NavigationService.Navigate(new Page1());
        }
    }
}
