using System;
using System.ComponentModel;
using System.Diagnostics;
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
        private AdminChecker _chk;
        private RegisterProgram _program;

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
            _chk = new AdminChecker();
            this.DataContext = this;
            this.currentUser = type;
            _program = new RegisterProgram();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_program.GetInstallDir()))
            {
                if (!currentUser)
                    InstallDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\Track My Playtime";
                else
                    InstallDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Track My Playtime";
            }
            else
            {
                label_isInstalled.Visibility = Visibility.Visible;
                btnBrowse.IsEnabled = false;
                tbInstallDir.IsReadOnly = true;

                string insDir = _program.GetInstallDir();
                string illegalChars = new string(Path.GetInvalidPathChars()) + new string(Path.GetInvalidFileNameChars());
                foreach (char c in illegalChars)
                {
                    if (c != ':' && c != '\\')
                    {
                        insDir = insDir.Replace(c.ToString(), "");
                    }
                }
                InstallDir = insDir;
            }
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
            if (!ProcessChecker())
                return;
            this.NavigationService.Navigate(new InstallProgress(InstallDir, currentUser, cboxCreateShortcut.IsChecked ?? true));
        }

        private bool ProcessChecker()
        {
            var process = Process.GetProcessesByName("tmp");
            if (process.Length > 0)
            {
                MessageBox.Show("Please close \"Track My Playtime\" First.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}
