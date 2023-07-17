using Microsoft.Win32;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using TMP.NET.Modules;

namespace TMP.NET.WindowUI
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        // Height: 530
        // Weight: 500

        Config setting;
        AboutWindow aboutWindow;
        UpdateChecker _chk = new UpdateChecker();
        public SettingWindow(Config setting)
        {
            InitializeComponent();
            this.setting = setting;
            FirstLoad();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            onApply();
        }

        private void onApply()
        {
            try
            {
                if (Convert.ToInt32(tbTextractorDelay.Text) > 60000)
                {
                    MessageBox.Show("The maximum allowable texttractor delay time is no more than 60000ms", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                MainWindow.log.Error("Failed to apply textractor delay setting", ex);
                Console.WriteLine(ex.Message);
            }

            if (!string.IsNullOrEmpty(tbTextractorDirx86.Text))
            {
                if (!File.Exists(tbTextractorDirx86.Text))
                {
                    MessageBox.Show("Invalid Textractor Directory!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning); return;
                }
            }
            else if (!string.IsNullOrEmpty(tbTextractorDirx64.Text))
            {
                if (!File.Exists(tbTextractorDirx64.Text))
                {
                    MessageBox.Show("Invalid Textractor Directory!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning); return;
                }
            }

            this.DialogResult = true;
        }

        private void FirstLoad()
        {
            tbTextractorDirx86.Text = setting.x86Directory;
            tbTextractorDirx64.Text = setting.x64Directory;
            cbDisableTextractor.IsChecked = setting.DisableTextractor;
            cbTimeTracking.IsChecked = setting.TimeTracking;
            cbEnableRichPresence.IsChecked = setting.EnabledRichPresence;
            tbTextractorDelay.Text = setting.TextractorDelay.ToString();
            cbAutoCheckUpdate.IsChecked = setting.AutoCheckUpdate;
            cbUncompressedArtwork.IsChecked = setting.UncompressedArtwork;
            cbEnableScreenshot.IsChecked = setting.EnabledScreenshot;
        }

        private void btnBrowseX86_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Executable (*.exe)|*.exe";
            ofd.Title = "Select Textractor Executable";
            if(!string.IsNullOrEmpty(tbTextractorDirx86.Text) && File.Exists(tbTextractorDirx86.Text))
                ofd.InitialDirectory = tbTextractorDirx86.Text;

            if (ofd.ShowDialog() == true)
            {
                tbTextractorDirx86.Text = ofd.FileName;
            }
        }

        private void btnBrowseX64_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Executable (*.exe)|*.exe";
            ofd.Title = "Select Textractor Executable";
            if (!string.IsNullOrEmpty(tbTextractorDirx64.Text) && File.Exists(tbTextractorDirx64.Text))
                ofd.InitialDirectory = tbTextractorDirx64.Text;

            if (ofd.ShowDialog() == true)
            {
                tbTextractorDirx64.Text = ofd.FileName;
            }
        }

        private void tbTextractorDelay_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Label_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            if (regex.IsMatch((String)e.DataObject.GetData(typeof(String))))
                e.CancelCommand();
            else
                e.CancelCommand();
        }

        private async void btnCheckforUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (UpdateChecker.updateAvailable)
            {
                var res = MessageBox.Show("An update is available, would you like to visit the download page?", "Notification", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                    _chk.OpenUpdateLink();

                return;
            }

            if (_chk.cooldown)
            {
                MessageBox.Show("Please wait a while before checking for available updates again", "Cooldown...", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _chk.cooldown = true;
            await _chk.CheckForUpdate();
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            if (aboutWindow == null)
            {
                aboutWindow = new AboutWindow() { Owner = this };
                aboutWindow.Closed += (obj, eArg) => { aboutWindow = null; };
                aboutWindow.Show();
            }
            else
            {
                if (aboutWindow.WindowState == WindowState.Minimized)
                    aboutWindow.WindowState = WindowState.Normal;
                aboutWindow.Focus();
            }
        }
    }
}
