using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media.TextFormatting;
using TMP.NET.Modules;
using VndbSharp;
using VndbSharp.Interfaces;
using VndbSharp.Models;
using VndbSharp.Models.Character;
using VndbSharp.Models.Dumps;
using VndbSharp.Models.Errors;
using VndbSharp.Models.Staff;
using VndbSharp.Models.VisualNovel;

namespace TMP.NET.WindowUI.CustomDialogWindow
{
    /// <summary>
    /// Interaction logic for DownloadMetadataDlgWindow.xaml
    /// </summary>
    public partial class DownloadMetadataDlgWindow : Window, INotifyPropertyChanged
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

        private readonly uint _vnid;
        private int _currentFileIndex = 0;
        private List<CharacterMetadata> charData = new List<CharacterMetadata>();
        private string _outDir;
        private int _stateIndex = 0;
        private int awaitableTask = 5;
        private string _CurrentStatus = "Connecting to server...";
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

        private List<string> state = new List<string>
        {
            "Downloading Character Data",
            "Downloading Staff Data",
            "Downloading Traits Dumps",
            "Saving Data",
            "Preparing"
        };

        private class CharacterMetadata
        {
            public string imgUrl { get; set; }
            public uint charID { get; set; }
        }

        public DownloadMetadataDlgWindow(uint VNID)
        {
            InitializeComponent();

            this.DataContext = this;
            this._vnid = VNID;
        }

        private void DownloadMetadata()
        {
            if (_vnid == 0)
            {
                MessageBox.Show("Failed to retreive metadata", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            Timer timer = new Timer(200);
            int dotCount = 0;
            const int maxDotCount = 3;
            Task.Run(async () =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    timer.Elapsed += (sender, e) =>
                    {
                        dotCount = (dotCount + 1) % (maxDotCount + 1);
                        string statusText = state[_stateIndex];
                        string dots = new string('.', dotCount);
                        string fullText = $"{statusText}{dots}";
                        Dispatcher.Invoke(() => CurrentStatus = fullText);
                    };

                    pbProgress.Value = 0;
                    pbProgress.IsIndeterminate = true;
                    tblockTaskProgress.Text = $"{_stateIndex + 1}/{awaitableTask}";
                });
                timer.Start();

                var chData = await GetCharacterDataAsync();

                this.Dispatcher.Invoke(() =>
                {
                    pbProgress.Value = 0;
                    pbProgress.IsIndeterminate = true;
                    _stateIndex++;
                    tblockTaskProgress.Text = $"{_stateIndex + 1}/{awaitableTask}";
                });
                var stData = await GetStaffData(chData);

                this.Dispatcher.Invoke(() =>
                {
                    pbProgress.Value = 0;
                    pbProgress.IsIndeterminate = true;
                    _stateIndex++;
                    tblockTaskProgress.Text = $"{_stateIndex + 1}/{awaitableTask}";
                });
                var traitDump = await GetTraitDumps();

                this.Dispatcher.Invoke(() =>
                {
                    pbProgress.Value = 0;
                    pbProgress.IsIndeterminate = true;
                    _stateIndex++;
                    tblockTaskProgress.Text = $"{_stateIndex + 1}/{awaitableTask}";
                });
                await SerializeWikiData(chData, stData, traitDump);

                timer.Stop();
                this.Dispatcher.Invoke(() =>
                {
                    pbProgress.Value = 0;
                    pbProgress.IsIndeterminate = false;
                    _stateIndex++;
                    tblockTaskProgress.Text = $"{_stateIndex + 1}/{awaitableTask}";
                });
                await DownloadCharacterImageAsync(chData);
            });
        }

        private async Task<List<Character>> GetCharacterDataAsync()
        {
            using (var client = new Vndb())
            {
                int page = 1;
                bool morePage = true;
                var characterList = new List<Character>();
                var reqOptions = new RequestOptions();

                Console.WriteLine("Retreiving Character Data...");

                while (morePage)
                {
                    reqOptions.Page = page;
                    var ch = await client.GetCharacterAsync(VndbFilters.VisualNovel.Equals(_vnid), VndbFlags.FullCharacter, reqOptions);

                    switch (ch)
                    {
                        case null when client.GetLastError().Type == ErrorType.Throttled:
                            await ThrottledWaitAsync((ThrottledError)client.GetLastError(), 0);
                            break;
                        case null:
                            return null;
                        default:
                            {
                                morePage = ch.HasMore;
                                characterList.AddRange(ch.Items);
                                page++;
                                Console.WriteLine("Loading Page: " + page);
                                break;
                            }
                    }
                }

                return characterList;
            }
        }

        private async Task DownloadCharacterImageAsync(List<Character> ch)
        {
#if DEBUG
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"KidiXDev\\TrackMyPlaytime\\Debug\\data\\vndb\\{_vnid}\\character");
#else
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KidiXDev\\TrackMyPlaytime\\Debug\\config.cfg");
#endif

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            _outDir = dir;

            Console.WriteLine("Character Count: " + ch.Count);

            foreach (var character in ch)
            {
                Console.WriteLine("\nDEBUG: Adding Character...\nName: " + character.Name + " ID: " + character.Id + " URL: " + character.Image + "\n");
                charData.Add(new CharacterMetadata() { imgUrl = character.Image, charID = character.Id });
            }

            this.Dispatcher.Invoke(() =>
            {
                pbProgress.Value = 0;
                pbProgress.IsIndeterminate = true;
                CurrentStatus = "Preparing...";
            });

            List<string> imgUrl = new List<string>();
            foreach (var url in charData)
            {
                imgUrl.Add(url.imgUrl);
            }

            if (_currentFileIndex < imgUrl.Count)
                await DownloadHandler(_outDir);
        }


        private async Task DownloadHandler(string outputDir)
        {
            if (_currentFileIndex < charData.Count)
            {
                Console.WriteLine("Downloading Character Images...");
                foreach (var character in charData)
                {
                    if (!string.IsNullOrEmpty(character.imgUrl))
                        await DownloadFileAsync(character.imgUrl, Path.Combine(outputDir, character.charID.ToString() + ".jpg"));

                    _currentFileIndex++;
                    this.Dispatcher.Invoke(() =>
                    {
                        CurrentStatus = $"Downloading Character Images... ({_currentFileIndex}/{charData.Count})";
                        pbProgress.IsIndeterminate = false;
                        pbProgress.Maximum = charData.Count;
                        pbProgress.Value++;
                    });
                    if (_currentFileIndex == charData.Count)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            this.Close();
                            ResetDownload();
                        });
                    }
                }
            }
        }

        private async Task DownloadFileAsync(string url, string fileName)
        {
            try
            {
                Uri uri = new Uri(url);

                Console.WriteLine("URL: " + uri.ToString());

                using (WebClient webClient = new WebClient())
                {
                    await webClient.DownloadFileTaskAsync(uri, fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error downloading current file: {ex.Message}");
            }
        }

        private void ResetDownload()
        {
            this.Dispatcher.Invoke(() => pbProgress.Value = 0);
        }

        private async Task<List<Staff>> GetStaffData(List<Character> ch)
        {
            using(Vndb client = new Vndb())
            {
                int page = 1;
                bool morePage = true;
                var staffList = new List<Staff>();
                var reqOptions = new RequestOptions();

                var staffID = new List<uint>();
                foreach (var character in ch)
                {
                    var vaData = character.VoiceActorMetadata;
                    foreach (var va in vaData)
                    {
                        staffID.Add(Convert.ToUInt32(va.StaffId));
                    }
                }

                uint[] staffFilter = staffID.ToArray();

                Console.WriteLine("Retreiving Staff Data...");

                while (morePage)
                {
                    reqOptions.Page = page;
                    var staff = await client.GetStaffAsync(VndbFilters.Id.Equals(staffFilter), VndbFlags.FullStaff, reqOptions);

                    if (staff == null && client.GetLastError().Type == ErrorType.Throttled)
                    {
                        await ThrottledWaitAsync((ThrottledError)client.GetLastError(), 0);
                    }

                    switch (staff)
                    {
                        case null when client.GetLastError().Type == ErrorType.Throttled:
                            await ThrottledWaitAsync((ThrottledError)client.GetLastError(), 0);
                            break;
                        case null:
                            return null;
                        default:
                            {
                                morePage = staff.HasMore;
                                staffList.AddRange(staff.Items);
                                page++;
                                Console.WriteLine("Loading Page: " + page);
                                break;
                            }
                    }
                }

                return staffList;
            }
        }

        private async Task ThrottledWaitAsync(ThrottledError throttled, int counter)
        {
            try
            {
                if (throttled == null)
                    return;

                const int bufferWait = 10;
                var minWait = TimeSpan.FromSeconds((throttled.MinimumWait - DateTime.Now).TotalSeconds);
                var maxWait = TimeSpan.FromSeconds((throttled.FullWait - DateTime.Now).TotalSeconds);
                Console.WriteLine($"\nWarning: A Throttled Error occured! You need to wait {minWait.Seconds} seconds minimum or {maxWait.Seconds} seconds maximum before issuing new commands\nErrorCounter:{counter}\n");

                double waitTime = counter == 0 ? minWait.TotalSeconds : TimeSpan.FromSeconds(5).TotalSeconds;
                if (counter >= 1)
                    waitTime = waitTime > maxWait.TotalSeconds ? maxWait.TotalSeconds : minWait.TotalSeconds + bufferWait;

                waitTime = Math.Abs(waitTime);
                var timeSpan = TimeSpan.FromSeconds(waitTime);
                await Task.Delay(timeSpan);
            }   
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        private async Task<List<Trait>> GetTraitDumps()
        {
            Console.WriteLine("Updating Trait Dumps...");
            return (await VndbUtils.GetTraitsDumpAsync()).ToList();
        }

        private async Task SerializeWikiData(List<Character> chData, List<Staff> stData, List<Trait> traits)
        {
#if DEBUG
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"KidiXDev\\TrackMyPlaytime\\Debug\\data\\vndb\\{_vnid}\\metadata.kdm");
            string filePathStaff = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"KidiXDev\\TrackMyPlaytime\\Debug\\data\\vndb\\{_vnid}\\stdata.kdm");
            string fileTraitDumps = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"KidiXDev\\TrackMyPlaytime\\Debug\\data\\traitdumps.kdm");
#else
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KidiXDev\\TrackMyPlaytime\\Debug\\config.cfg");
#endif
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                if (!Directory.Exists(Path.GetDirectoryName(filePathStaff)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePathStaff));
                }

                string jsonData = JsonConvert.SerializeObject(chData, Formatting.Indented);

                using (StreamWriter file = new StreamWriter(filePath))
                {
                    await file.WriteAsync(jsonData);
                }

                string jsonStaffData = JsonConvert.SerializeObject(stData, Formatting.Indented);

                using (StreamWriter file = new StreamWriter(filePathStaff))
                {
                    await file.WriteAsync(jsonStaffData);
                }

                string jsonTraitsData = JsonConvert.SerializeObject(traits, Formatting.Indented);

                using (StreamWriter file = new StreamWriter(fileTraitDumps))
                {
                    await file.WriteAsync(jsonTraitsData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DownloadMetadata();
        }

        private void HandleError(IVndbError error)
        {
            if (error is MissingError missing)
            {
                Console.WriteLine($"A Missing Error occured, the field \"{missing.Field}\" was missing.");
            }
            else if (error is BadArgumentError badArg)
            {
                Console.WriteLine($"A BadArgument Error occured, the field \"{badArg.Field}\" is invalid.");
            }
            else if (error is ThrottledError throttled)
            {
                var minSeconds = (throttled.MinimumWait - DateTime.Now).TotalSeconds;
                var fullSeconds = (throttled.FullWait - DateTime.Now).TotalSeconds;
                Console.WriteLine(
                    $"A Throttled Error occured, you need to wait at minimum \"{minSeconds}\" seconds, " +
                    $"and preferably \"{fullSeconds}\" before issuing commands.");
            }
            else if (error is GetInfoError getInfo)
            {
                Console.WriteLine($"A GetInfo Error occured, the flag \"{getInfo.Flag}\" is not valid on the issued command.");
            }
            else if (error is InvalidFilterError invalidFilter)
            {
                Console.WriteLine(
                    $"A InvalidFilter Error occured, the filter combination of \"{invalidFilter.Field}\", " +
                    $"\"{invalidFilter.Operator}\", \"{invalidFilter.Value}\" is not a valid combination.");
            }
            else
            {
                Console.WriteLine($"A {error.Type} Error occured.");
            }
            Console.WriteLine($"Message: {error.Message}");
        }
    }
}
