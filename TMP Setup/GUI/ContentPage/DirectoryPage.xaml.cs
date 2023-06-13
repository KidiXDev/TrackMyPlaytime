using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TMP_Setup.Ext;

namespace TMP_Setup.GUI.ContentPage
{
    /// <summary>
    /// Interaction logic for DirectoryPage.xaml
    /// </summary>
    public partial class DirectoryPage : Page, INotifyPropertyChanged
    {
        private AdminChecker _chk = new AdminChecker();

        bool currentUser;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Console.WriteLine(InstallDir);
        }

        private string _installDir;

        public string InstallDir
        {
            get { return _installDir; }
            set
            {
                if (_installDir != value)
                {
                    _installDir = value;
                    OnPropertyChanged("InstallDir");
                }
            }
        }

        public DirectoryPage(bool type)
        {
            InitializeComponent();

            this.DataContext = this;
            this.currentUser = type;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!currentUser)
                InstallDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Track My Playtime";
            else
                InstallDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Track My Playtime";
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderPicker();
            dlg.InputPath = Path.GetDirectoryName(InstallDir);

            dlg.Multiselect = false;
            dlg.Title = "Select Installation Directory";
            dlg.OkButtonLabel = "Select";
            if (dlg.ShowDialog() == true)
            {
                if (!_chk.CheckWritePermission(dlg.ResultPath))
                {
                    MessageBox.Show("This directory cannot be used, it needs administrator privileges to install it in this directory", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                InstallDir = dlg.ResultPath + @"\Track My Playtime";
            }
        }

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new InstallProgress(InstallDir, currentUser, cboxCreateShortcut.IsChecked ?? true));
        }
    }
}
