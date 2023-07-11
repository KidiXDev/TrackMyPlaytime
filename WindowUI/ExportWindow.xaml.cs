using Microsoft.Win32;
using System.Windows;

namespace TMP.NET.WindowUI
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {

        private string textList = "Use this feature to save game lists, playing times, " +
            "and other information to files that can be loaded back by this program.\n" +
            "Keep in mind that Background Images cannot be exported\n";

        public string TextList { get { return textList; } }


        public ExportWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "TMPExport";
            dlg.DefaultExt = ".kdb";
            dlg.Filter = "TMP Export File |*.kex";

            if (dlg.ShowDialog() ?? false)
            {
                tbExportDir.Text = dlg.FileName;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
