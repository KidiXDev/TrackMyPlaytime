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
using System.Windows.Media.Imaging;
using TMP.NET.Modules;
using VndbSharp.Models.Dumps;

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

        private CharacterMetadata _selectedCharacter;
        public CharacterMetadata SelectedCharacter
        {
            get { return _selectedCharacter; }
            set
            {
                var character = value;
                character.StaffMetadata = _staffData;
                character.VNID = gl.VNID;
                character.traitDumps = _traitDumps;
                _selectedCharacter = character;
                OnPropertyChanged(nameof(SelectedCharacter));
            }
        }

        public WikiContentControl(GameList gl)
        {
            InitializeComponent();

            this.DataContext = this;
            this.gl = gl;
        }

        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            var window = (MainWindow)Application.Current.MainWindow;
            window.ContentArea.Content = window.GetContentControl;
            window.CommandBar.Visibility = Visibility.Visible;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
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
                this.Dispatcher.Invoke(() => lv_charlist.ItemsSource = CList);
            });
        }

        private Task<List<CharacterMetadata>> LoadCharData(uint vnid)
        {
#if DEBUG
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"KidiXDev\\TrackMyPlaytime\\Debug\\data\\vndb\\{vnid}\\metadata.kdm");
            //string filePathStaff = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"KidiXDev\\TrackMyPlaytime\\Debug\\data\\vndb\\{vnid}\\stdata.kdm");
#else
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KidiXDev\\TrackMyPlaytime\\Debug\\config.cfg");
#endif
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
#if DEBUG
            string filePathStaff = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"KidiXDev\\TrackMyPlaytime\\Debug\\data\\vndb\\{vnid}\\stdata.kdm");
#endif
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
#if DEBUG
            string traitDumpsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"KidiXDev\\TrackMyPlaytime\\Debug\\data\\traitdumps.kdm");
#endif
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
                CharImg.Source = null;
                CharImg.UpdateLayout();

                var selectedChar = (CharacterMetadata)lv_charlist.SelectedItem;

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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
