using RockSnifferGui.Configuration;
using RockSnifferGui.DataStore;
using RockSnifferGui.Model;
using RockSnifferGui.Services;
using RockSnifferLib.Cache;
using RockSnifferLib.Events;
using RockSnifferLib.Logging;
using RockSnifferLib.RSHelpers;
using RockSnifferLib.RSHelpers.NoteData;
using RockSnifferLib.Sniffing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace RockSnifferGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static RoutedUICommand showPlayHistoryCommand = new RoutedUICommand("Show the Play History Window", "showPlayHistory", typeof(MainWindow),
            new InputGestureCollection() {
                new KeyGesture(Key.H, ModifierKeys.Control)
        });
        public static RoutedUICommand ShowPlayHistoryCommand { get => showPlayHistoryCommand; }

        private const string version = "0.0.3";
        public static string Version { get => "v" + version; }

        internal static ICache cache;
        internal static Config config;
        internal static Sniffer sniffer;

        private static readonly bool Is64Bits = (IntPtr.Size == 8);

        private static readonly Random random = new Random();

        private SongDetails details = new SongDetails();
        private RSMemoryReadout memReadout = new RSMemoryReadout();
        private readonly System.Drawing.Image defaultAlbumCover = new Bitmap(256, 256);

        private List<SongPlayInstance> playedSongs = new List<SongPlayInstance>();
        private SongPlayInstance currentSong;

        private PlayHistoryWindow playHistoryWindow;

        SQLiteStore songPlayInstancesDb = new SQLiteStore();

        public event PropertyChangedEventHandler PropertyChanged;

        public string GameProcessServiceStatus
        {
            get
            {
                return GameProcessService.Instance.StatusForDisplay;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                GameProcessService.Instance.GameProcessChanged += GameProcessService_GameProcessChanged;
                GameProcessService.Instance.PropertyChanged += GameProcessService_PropertyChanged;
                this.Initialize();
                this.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.DataContext = this;
        }

        private void GameProcessService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName.Equals("StatusForDisplay"))
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GameProcessServiceStatus"));
            }
        }

        private void GameProcessService_GameProcessChanged(object sender, GameProcessChangedEventArgs e)
        {
            this.SetupSniffer(e.Process);
        }

        public void Initialize()
        {
            //Set title and print version
            this.Title = string.Format("Unofficial RockSniffer GUI {0}", Version);
            //Console.Title = string.Format("RockSniffer {0}", version);
            Logger.Log("RockSniffer {0} ({1}bits)", Version, Is64Bits ? "64" : "32");

            //Initialize and load configuration
            config = new Config();
            try
            {
                config.Load();
            }
            catch (Exception e)
            {
                Logger.LogError("Could not load configuration: {0}\r\n{1}", e.Message, e.StackTrace);
                throw e;
            }

            //Transfer logging options
            Logger.logStateMachine = config.debugSettings.debugStateMachine;
            Logger.logCache = config.debugSettings.debugCache;
            Logger.logFileDetailQuery = config.debugSettings.debugFileDetailQuery;
            Logger.logMemoryReadout = config.debugSettings.debugMemoryReadout;
            Logger.logSongDetails = config.debugSettings.debugSongDetails;
            Logger.logSystemHandleQuery = config.debugSettings.debugSystemHandleQuery;
            Logger.logProcessingQueue = config.debugSettings.debugProcessingQueue;

            //Initialize cache
            cache = new SQLiteCache();

            this.playedSongs = this.songPlayInstancesDb.GetAll();

            Logger.Log("Waiting for rocksmith");

            if (GameProcessService.Instance.Status == GameProcessService.ProcessStatus.RUNNING)
            {
                this.SetupSniffer(GameProcessService.Instance.GameProcess);
            }

            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MainWindow.sniffer != null)
            {
                MainWindow.sniffer.Stop();
            }

            // no Discord RPC stuff right now
            //rpcHandler?.Dispose();
            //rpcHandler = null;
        }

        public void Run()
        {
            //Add RPC event listeners
            // not doing anything with Discord right now
            //if (config.rpcSettings.enabled)
            //{
            //    rpcHandler = new DiscordRPCHandler(sniffer);
            //}

            //Inform AddonService
            // don't know what this is for yet
            //if (config.addonSettings.enableAddons && addonService != null)
            //{
            //    addonService.SetSniffer(sniffer);
            //}
        }

        private void SetupSniffer(Process process)
        {
            //Initialize file handle reader and memory reader
            MainWindow.sniffer = new Sniffer(process, cache, config.snifferSettings);

            //Listen for events
            MainWindow.sniffer.OnSongChanged += Sniffer_OnCurrentSongChanged;
            MainWindow.sniffer.OnSongStarted += Sniffer_OnSongStarted;
            MainWindow.sniffer.OnSongEnded += Sniffer_OnSongEnded;

            MainWindow.sniffer.OnMemoryReadout += Sniffer_OnMemoryReadout;
        }

        #region Sniffer Events
        private void Sniffer_OnSongStarted(object sender, OnSongStartedArgs e)
        {
            details = e.song;
            SongPlayInstance newSong = new SongPlayInstance(e.song);

            this.playedSongs.Add(newSong);
            this.currentSong = newSong;

            newSong.StartSong();

            UpdateNowPlayingValues();
        }

        private void Sniffer_OnSongEnded(object sender, OnSongEndedArgs e)
        {
            if (this.currentSong != null)
            {
                this.currentSong.FinishSong();

                songPlayInstancesDb.Add(this.currentSong);
            }

            this.currentSong = null;
        }

        private void Sniffer_OnCurrentSongChanged(object sender, OnSongChangedArgs args)
        {
            ;
            //details = args.songDetails;
        }

        private void Sniffer_OnMemoryReadout(object sender, OnMemoryReadoutArgs args)
        {
            memReadout = args.memoryReadout;
            UpdateNowPlayingValues();

            if ((this.currentSong != null) && (args.memoryReadout.noteData != null))
            {
                this.currentSong.UpdateNoteData(args.memoryReadout.noteData);
            }
        }
        #endregion

        #region UI Events
        public void TogglePlayHistoryWindow_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (this.playHistoryWindow != null)
            {
                this.playHistoryWindow.Close();
                this.playHistoryWindow = null;
            }
            else
            {
                this.playHistoryWindow = new PlayHistoryWindow(this.playedSongs, MainWindow.sniffer);
                this.playHistoryWindow.Closed += PlayHistoryWindow_Closed;
                this.playHistoryMenuItem.IsChecked = true;
                this.playHistoryWindow.Show();
            }
        }

        private void PlayHistoryWindow_Closed(object sender, EventArgs e)
        {
            this.playHistoryWindow = null;
            this.playHistoryMenuItem.IsChecked = false;
        }

        #endregion

        private void UpdateNowPlayingValues()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => this.UpdateNowPlayingValues());
                return;
            }

            this.bandNameLabel.Content = details.artistName;
            this.songNameLabel.Content = details.songName;
            this.albumNameLabel.Content = $"{details.albumName} ({details.albumYear})";

            var nd = memReadout.noteData ?? new LearnASongNoteData();
            this.songTimerimeValueLabel.Content = FormatTime(memReadout.songTimer);

            this.notesHitValueLabel.Content = nd.TotalNotesHit.ToString();
            this.notesMissedValueLabel.Content = nd.TotalNotesMissed.ToString();
            this.totalNotesValueLabel.Content = nd.TotalNotes.ToString();

            this.currentStreakValueLabel.Content = (nd.CurrentHitStreak - nd.CurrentMissStreak).ToString();
            this.highestStreakValueLabel.Content = nd.HighestHitStreak.ToString();

            this.noteHitPercentageValueLabel.Content = FormatPercentage(nd.Accuracy);

            if (details.albumArt != null)
            {
                using (var memory = new MemoryStream())
                {
                    details.albumArt.Save(memory, ImageFormat.Png);
                    memory.Position = 0;

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    this.albumArtImage.Source = bitmapImage;
                }
            }
        }

        public static string FormatTime(float lengthTime)
        {
            TimeSpan t = TimeSpan.FromSeconds(Math.Ceiling(lengthTime));
            return t.ToString(config.formatSettings.timeFormat);
        }

        public static string FormatPercentage(double frac)
        {
            return string.Format(config.formatSettings.percentageFormat, frac);
        }
    }

    public class MainWindowCommands
    {
        private static RoutedUICommand showPlayHistoryCommand = new RoutedUICommand("Show the Play History Window", "showPlayHistory", typeof(MainWindowCommands),
            new InputGestureCollection() {
                new KeyGesture(Key.H, ModifierKeys.Control)
            });
        public static RoutedUICommand ShowPlayHistoryCommand { get => showPlayHistoryCommand; }
    }
}
