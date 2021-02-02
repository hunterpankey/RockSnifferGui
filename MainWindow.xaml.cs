using RockSnifferGui.Common;
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
        internal static ICache cache;
        internal static Config config;
        internal static Sniffer sniffer;

        private static readonly bool Is64Bits = (IntPtr.Size == 8);

        private List<SongPlayInstance> playedSongs = new List<SongPlayInstance>();
        private SongPlayInstance currentSong;

        private PlayHistoryWindow playHistoryWindow;

        SQLiteStore songPlayInstancesDb = new SQLiteStore();

        public event PropertyChangedEventHandler PropertyChanged;

        public static string Version { get => App.Version; }
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.DataContext = this;
        }

        public void Initialize()
        {
            this.Title = string.Format("Unofficial RockSniffer GUI {0}", App.Version);
            Logger.Log("RockSniffer GUI {0} ({1}bits)", App.Version, Is64Bits ? "64" : "32");

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

        #region Game Process Events
        private void GameProcessService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("StatusForDisplay"))
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GameProcessServiceStatus"));
            }
        }

        private void GameProcessService_GameProcessChanged(object sender, GameProcessChangedEventArgs e)
        {
            this.SetupSniffer(e.Process);

            if (this.playHistoryWindow != null)
            {
                this.playHistoryWindow.AttachSniffer(MainWindow.sniffer);
            }
        }
        #endregion

        #region Sniffer Events
        private void Sniffer_OnSongStarted(object sender, OnSongStartedArgs e)
        {
            try
            {
                this.nowPlayingControl.UpdateSong(e.song);
                SongPlayInstance newSong = new SongPlayInstance(e.song);

                this.playedSongs.Add(newSong);
                this.currentSong = newSong;

                newSong.StartSong();

            }
            catch (Exception ex)
            {
                Utilities.ShowExceptionMessageBox(ex);
            }
        }

        private void Sniffer_OnSongEnded(object sender, OnSongEndedArgs e)
        {
            try
            {

                if (this.currentSong != null)
                {
                    this.currentSong.FinishSong();

                    songPlayInstancesDb.Add(this.currentSong);
                }

                this.currentSong = null;
            }
            catch (Exception ex)
            {
                Utilities.ShowExceptionMessageBox(ex);
            }
        }

        private void Sniffer_OnCurrentSongChanged(object sender, OnSongChangedArgs args)
        {
            this.nowPlayingControl.UpdateSong(args.songDetails);
        }

        private void Sniffer_OnMemoryReadout(object sender, OnMemoryReadoutArgs args)
        {
            try
            {
                this.notesPlayedControl.UpdateNoteData(args.memoryReadout.noteData, args.memoryReadout.songTimer);
                
                //this.UpdateNoteDataDisplays(args.memoryReadout);

                if ((this.currentSong != null) && (args.memoryReadout.noteData != null))
                {
                    this.currentSong.UpdateNoteData(args.memoryReadout.noteData);
                }
            }
            catch (Exception ex)
            {
                Utilities.ShowExceptionMessageBox(ex);
            }
        }
        #endregion

        #region UI Events
        public void TogglePlayHistoryCommandBinding_Executed(object sender, ExecutedRoutedEventArgs args)
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

        private void ManualTestCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GenericNoteData n = new GenericNoteData();
            n.TotalNotes = 10;

            this.notesPlayedControl.UpdateNoteData(n, 50.5f);
        }

        private void PlayHistoryWindow_Closed(object sender, EventArgs e)
        {
            this.playHistoryWindow = null;
            this.playHistoryMenuItem.IsChecked = false;
        }

        #endregion

        #region App Lifecycle Events
        private void ExitCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
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
        #endregion

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
}
