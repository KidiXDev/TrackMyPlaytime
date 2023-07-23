using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VndbSharp;
using VndbSharp.Models;
using VndbSharp.Models.Common;
using VndbSharp.Models.Release;

namespace TMP.NET.WindowUI
{
    /// <summary>
    /// Interaction logic for VNDBMetadataDownloaderWindow.xaml
    /// </summary>
    public partial class VNDBMetadataDownloaderWindow : Window, INotifyPropertyChanged
    {
        public string GameTitle { get; set; }
        public string Developer { get; set; }
        public string ImageURL { get; set; }
        public string Description { get; set; }
        public string Web1 { get; set; }
        public string Web2 { get; set; }
        public string Web3 { get; set; }
        public string ReleaseDate { get; set; }

        private enum ProgState
        {
            Idle,
            Searching,
            Downloading
        }

        private ProgState _state = ProgState.Idle;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<VNResult> _result = new ObservableCollection<VNResult>();
        private Vndb _client;

        private string _CurrentStatus;
        public string CurrentStatus
        {
            get { return _CurrentStatus; }
            set
            {
                if (value != this._CurrentStatus)
                {
                    _CurrentStatus = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public VNDBMetadataDownloaderWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            lv_list.ItemsSource = _result;
            _client = new Vndb();

            CurrentStatus = "Welcome";
        }

        public class VNResult
        {
            private string _Title;
            private string _OriginalTitle;
            private string _VNID;
            private string _CoverURL;

            public uint vnid { get; set; }

            public string Title { get { return _Title; } set { _Title = value; } }
            public string VNID { get { return _VNID; } set { _VNID = value; } }
            public string OriginalTitle { get { return _OriginalTitle; } set { _OriginalTitle = value; } }
            public ImageSource CoverURL { get { return new BitmapImage(new Uri(_CoverURL)); } }
            public string SetCoverURL { set { _CoverURL = value; } }
        }

        private async void btnFind_Click(object sender, RoutedEventArgs e)
        {
            await FindVN();
        }

        private async Task FindVN()
        {
            Timer timer = new Timer(200);
            try
            {
                CurrentStatus = "Searching...";

                if (_state != ProgState.Idle)
                    return;

                _state = ProgState.Searching;

                _result.Clear();

                int dotCount = 0;
                const int maxDotCount = 3;

                timer.Elapsed += (sender, e) =>
                {
                    dotCount = (dotCount + 1) % (maxDotCount + 1);
                    string dots = new string('.', dotCount);
                    Dispatcher.Invoke(() => CurrentStatus = $"Searching{dots}"); // Animate
                };

                timer.Start();

                var vns = await _client.GetVisualNovelAsync(VndbFilters.Search.Fuzzy(tbSearch.Text), VndbFlags.FullVisualNovel);

                timer.Stop();

                if (vns == null)
                {
                    CurrentStatus = "Something went wrong...";
                    _state = ProgState.Idle;
                    return;
                }

                if (vns.Count == 0)
                {
                    CurrentStatus = "No Visual Novels Found!";
                    _state = ProgState.Idle;
                    return;
                }

                foreach (var v in vns)
                {
                    VNResult res = new VNResult();
                    res.Title = "Title: " + v?.Name;
                    res.VNID = "ID: " + v?.Id.ToString();
                    res.vnid = v.Id;
                    res.OriginalTitle = "Original: " + v?.OriginalName;
                    res.SetCoverURL = v?.Image;
                    _result.Add(res);
                }
                CurrentStatus = $"Found {vns.Count} Result";

                _state = ProgState.Idle;
            }
            catch (Exception ex)
            {
                _state = ProgState.Idle;
                timer.Stop();
                CurrentStatus = "Something went wrong...";
                MainWindow.log.Error("Exception thrown when searching", ex);

                MessageBox.Show($"Search failed\nInfo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeInit(bool status)
        {
            if (_client != null)
                _client.Dispose();

            this.DialogResult = status;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DeInit(false);
        }

        private void lv_list_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as VNResult;
            if (item != null)
            {
                DownloadMetadata(item);
            }
        }

        private async Task GetProducers (VNResult item)
        { 
            var vnp = await _client.GetReleaseAsync(VndbFilters.VisualNovel.Equals(item.vnid), VndbFlags.Producers);
            if (vnp == null)
            {
                Console.WriteLine("null");
                return;
            }

            var pro = vnp.SelectMany(x => x.Producers);

            Developer = CombineDeveloperNames(pro);
        }

        private string CombineDeveloperNames(IEnumerable<ProducerRelease> producers)
        {
            HashSet<string> developerNamesSet = new HashSet<string>();
            List<string> developerNames = new List<string>();

            foreach (var producer in producers)
            {
                if (producer.IsDeveloper && !string.IsNullOrEmpty(producer.Name) && !developerNamesSet.Contains(producer.Name))
                {
                    developerNamesSet.Add(producer.Name);
                    developerNames.Add(producer.Name);
                }
            }

            return string.Join(" & ", developerNames);
        }

        private async Task BasicItem(VNResult item)
        {
            Timer timer = new Timer(200);
            try
            {
                int dotCount = 0;
                const int maxDotCount = 3;

                timer.Elapsed += (sender, e) =>
                {
                    dotCount = (dotCount + 1) % (maxDotCount + 1);
                    string dots = new string('.', dotCount);
                    Dispatcher.Invoke(() => CurrentStatus = $"Downloading Metadata{dots}"); // Animate
                };

                timer.Start();

                var resVn = await _client.GetVisualNovelAsync(VndbFilters.Id.Equals(item.vnid), VndbFlags.FullVisualNovel);
                var resRelease = await _client.GetReleaseAsync(VndbFilters.VisualNovel.Equals(item.vnid), VndbFlags.FullRelease);
                await GetProducers(item);

                timer.Stop();

                if (resVn == null)
                {
                    CurrentStatus = "Something went wrong...";
                    _state = ProgState.Idle;
                    return;
                }

                if (resRelease == null)
                {
                    CurrentStatus = "Something went wrong...";
                    _state = ProgState.Idle;
                    return;
                }

                var title = resVn.Select(x => x.Name).FirstOrDefault();
                var description = resVn.Select(x => x.Description).FirstOrDefault();
                var releaseDate = resVn.Select(x => x.Released).FirstOrDefault();
                var vndbUrl = "https://vndb.org/v" + resVn.Select(x => x.Id).FirstOrDefault();
                var officialWeb = resRelease.Select(x => x.Website).FirstOrDefault();
                var imgUrl = resVn.Select(x => x.Image).FirstOrDefault();

                this.GameTitle = title;
                this.Description = description;
                this.ReleaseDate = releaseDate.ToString();
                this.ImageURL = imgUrl;
                this.Web1 = $"{vndbUrl}";
                this.Web2 = $"{officialWeb}";

                CurrentStatus = "Done";

                _state = ProgState.Idle;

                DeInit(true);
            }
            catch (Exception ex)
            {
                _state = ProgState.Idle;
                timer.Stop();
                CurrentStatus = "Something went wrong...";
                MainWindow.log.Error("Exception thrown when searching", ex);

                MessageBox.Show($"Search failed\nInfo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DownloadMetadata(VNResult item)
        {
            if (_state != ProgState.Idle)
                return;

            _state = ProgState.Downloading;

            CurrentStatus = "Downloading Metadata...";
            await BasicItem(item);
        }

        private void ctxSelect_Click(object sender, RoutedEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as VNResult;
            if (item != null)
            {
                DownloadMetadata(item);
            }
        }
    }
}
