using System;
using System.Windows;
using TMP.NET.Modules;

namespace TMP.NET.WindowUI
{
    /// <summary>
    /// Interaction logic for FilterWindow.xaml
    /// </summary>
    public partial class FilterWindow : Window
    {
        private readonly string[] SortType = { "Ascending", "Descending" };
        private readonly string[] Filter = { "Alphabet", "Playtime", "Last Played", "Date Added" };

        public Config.FilterConfig _filterSetting;

        public FilterWindow(Config.FilterConfig filterSetting)
        {
            InitializeComponent();
            this._filterSetting = filterSetting;

            foreach (var type in SortType)
            {
                cboxSortType.Items.Add(type);
            }

            foreach (var type in Filter)
            {
                cboxFilter.Items.Add(type);
            }

            cboxSortType.SelectedIndex = _filterSetting.SortIndex;
            cboxFilter.SelectedIndex = _filterSetting.FilterIndex;
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            _filterSetting.FilterIndex = cboxFilter.SelectedIndex;
            _filterSetting.SortIndex = cboxSortType.SelectedIndex;

            var window = (MainWindow)Application.Current.MainWindow;
            window.FilterSort();

            this.Hide();

            window.Activate();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
