using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using TMP.NET.Modules;

namespace TMP.NET.WindowUI.ContentWindow
{
    /// <summary>
    /// Interaction logic for WikiContentControl.xaml
    /// </summary>
    public partial class WikiContentControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        string dataPath;
        private Config.VndbConfig vndbConfig;
        private GameList gl;
        private ObservableCollection<CharacterMetadata> _cList;
        private List<CustomStaffMetadata> _staffData;
        private List<CustomTraits> _traitDumps;
        public List<CustomStaffMetadata> StaffData
        {
            get { return _staffData; }
            set
            {
                _staffData = value;
                OnPropertyChanged(nameof(StaffData));
            }
        }
        public ObservableCollection<CharacterMetadata> CList { get { return _cList; } }
        private CollectionViewSource collectionViewSource;
        private ICollectionView collectionView;


        private CharacterMetadata _selectedCharacter;
        public CharacterMetadata SelectedCharacter
        {
            get { return _selectedCharacter; }
            set
            {
                if (lv_charlist.SelectedItems != null && value != null)
                {
                    var character = value;
                    character.StaffMetadata = _staffData;
                    character.VNID = gl.VNID;
                    character.traitDumps = _traitDumps;
                    character.setting = vndbConfig;
                    _selectedCharacter = character;
                    OnPropertyChanged(nameof(SelectedCharacter));
                }
            }
        }

        public bool ExplicitTraits
        {
            get
            {
                if (vndbConfig.ShowExplicitContent)
                    return true;
                else
                    return false;
            }
        }

        public WikiContentControl(GameList gl, Config.VndbConfig vndbConfig)
        {
            InitializeComponent();

            this.DataContext = this;
            this.gl = gl;
            this.vndbConfig = vndbConfig;
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            var window = (MainWindow)Application.Current.MainWindow;
            window.ContentArea.Content = window.GetContentControl;
            window.CommandBar.Visibility = Visibility.Visible;
            this.gl = null;
            this.collectionViewSource = null;
            this._staffData = null;
            this._traitDumps = null;
            this._cList = null;

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            BackgroundImg.Source = gl.BackgroundPath();
            var task = Task.Run(async () =>
            {
                var ch = await LoadCharData(gl.VNID);
                var cs = await LoadStaffData(gl.VNID);
                var trait = await LoadTraitDumps();
                if (ch == null || ch.Count == 0 || cs == null || cs.Count == 0 || trait == null || trait.Count == 0)
                {
                    this.Dispatcher.Invoke(() => btnHome_Click(null, null));
                    MessageBox.Show("No vndb metadata information found, please repair this game vndb metadata", "Failed to read vndb metadata information", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                _cList = new ObservableCollection<CharacterMetadata>(ch);
                _staffData = cs;
                _traitDumps = trait;
                this.Dispatcher.Invoke(() => LoadCharacterList());
            });
        }

        private Task<List<CharacterMetadata>> LoadCharData(uint vnid)
        {
            string filePath = Path.Combine(PathFinderManager.VndbDataPath, $"{vnid}\\metadata.kdm");

            List<CharacterMetadata> ca = new List<CharacterMetadata>();
            try
            {
                if (!File.Exists(filePath))
                    return Task.FromResult(ca);

                dataPath = Directory.GetParent(filePath).FullName;

                string jsonData = File.ReadAllText(filePath);
                ca = JsonConvert.DeserializeObject<List<CharacterMetadata>>(jsonData);

                return Task.FromResult(ca);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Task.FromResult(ca);
            }
        }

        private Task<List<CustomStaffMetadata>> LoadStaffData(uint vnid)
        {
            string filePathStaff = Path.Combine(PathFinderManager.VndbDataPath, $"{vnid}\\stdata.kdm");

            List<CustomStaffMetadata> cs = new List<CustomStaffMetadata>();
            try
            {
                if (!File.Exists(filePathStaff))
                    return Task.FromResult(cs);

                string jsonData = File.ReadAllText(filePathStaff);
                cs = JsonConvert.DeserializeObject<List<CustomStaffMetadata>>(jsonData);

                return Task.FromResult(cs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Task.FromResult(cs);
            }
        }

        private Task<List<CustomTraits>> LoadTraitDumps()
        {
            string traitDumpsPath = PathFinderManager.TraitDumpsDir;

            List<CustomTraits> trait = new List<CustomTraits>();
            try
            {
                if (!File.Exists(traitDumpsPath))
                    return Task.FromResult(trait);

                var contractResolver = new DefaultContractResolver();
#pragma warning disable CS0618 // Type or member is obsolete
                contractResolver.DefaultMembersSearchFlags |= BindingFlags.NonPublic;
#pragma warning restore CS0618 // Type or member is obsolete
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = contractResolver
                };

                string jsonData = File.ReadAllText(traitDumpsPath);
                trait = JsonConvert.DeserializeObject<List<CustomTraits>>(jsonData, settings);

                return Task.FromResult(trait);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Task.FromResult(trait);
            }
        }

        private void lv_charlist_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lv_charlist.SelectedItems != null)
                {
                    CharImg.Source = null;
                    CharImg.UpdateLayout();

                    var selectedChar = (CharacterMetadata)lv_charlist.SelectedItem;

                    //Console.WriteLine("Name: " + selectedChar.Name + " Role: " + selectedChar.CharacterRole);

                    foreach (var chars in CList)
                    {
                        if (chars.Id.Equals(selectedChar.Id))
                        {
                            CharImg.Source = new BitmapImage(new Uri(Path.Combine(dataPath, $"character\\{chars.Id}.jpg")));
                        }
                    }

                    /*foreach (var chars in CList)
                    {
                        if(chars.Id.Equals(selectedChar.Id))
                        {
                            var vn = chars.VoiceActorMetadata;
                            foreach (var st in vn)
                            {
                                Console.WriteLine(st.StaffId);
                                Console.WriteLine("Alias Id: " + st.AliasId);
                                Console.WriteLine("Character Id: " + chars.Id);
                            }
                        }
                    }*/
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void LoadCharacterRole()
        {
            foreach (var c in _cList)
            {
                c.VNID = gl.VNID;
            }
        }

        private bool CharacterFilter(object item)
        {
            if (item is CharacterMetadata character && string.IsNullOrEmpty(tbSearch.Text))
            {
                // Filter out characters with higher spoiler levels
                return character.CharacterSpoiler <= vndbConfig.SpoilerSetting;
            }
            else
                return ((item as CharacterMetadata).Name.IndexOf(tbSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void LoadCharacterList()
        {
            LoadCharacterRole();
            collectionViewSource = new CollectionViewSource();
            collectionViewSource.Source = CList;

            collectionViewSource.GroupDescriptions.Add(new PropertyGroupDescription("CharacterRole"));

            collectionViewSource.SortDescriptions.Add(new SortDescription("GroupOrder", ListSortDirection.Ascending));
            collectionViewSource.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            collectionView = collectionViewSource.View;
            lv_charlist.ItemsSource = collectionView;

            collectionView.Filter = CharacterFilter;
            lv_charlist.SelectedIndex = 0;
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (lv_charlist.ItemsSource != null)
                    collectionView.Refresh();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
