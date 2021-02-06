using RockSnifferGui.Common;
using RockSnifferGui.Configuration;
using RockSnifferGui.DataStore;
using RockSnifferGui.Model;
using RockSnifferGui.Services;
using RockSnifferLib.Cache;
using RockSnifferLib.Events;
using RockSnifferLib.Logging;
using RockSnifferLib.Sniffing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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
            this.InitializeComponent();

            try
            {
                GameProcessService.Instance.GameProcessChanged += this.GameProcessService_GameProcessChanged;
                GameProcessService.Instance.PropertyChanged += this.GameProcessService_PropertyChanged;
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

            this.Closing += this.MainWindow_Closing;

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
            // Initialize file handle reader and memory reader
            MainWindow.sniffer = new Sniffer(process, cache, config.snifferSettings);

            // Listen for events
            MainWindow.sniffer.OnSongStarted += this.Sniffer_OnSongStarted;
            MainWindow.sniffer.OnSongEnded += this.Sniffer_OnSongEnded;

            MainWindow.sniffer.OnMemoryReadout += this.Sniffer_OnMemoryReadout;
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
        }
        #endregion

        #region Sniffer Events
        private void Sniffer_OnSongStarted(object sender, OnSongStartedArgs e)
        {
            try
            {
                this.nowPlayingControl.UpdateSong(e.song);
                this.notesPlayedControl.UpdateSong(e.song);

                this.currentSong = new SongPlayInstance(e.song);
                this.currentSong.StartSong();
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
                    this.playedSongs.Add(this.currentSong);

                    this.songPlayInstancesDb.Add(this.currentSong);

                    if (this.playHistoryWindow != null)
                    {
                        this.playHistoryWindow.AddSongPlay(this.currentSong);
                    }
                }

                this.currentSong = null;
            }
            catch (Exception ex)
            {
                Utilities.ShowExceptionMessageBox(ex);
            }
        }

        private void Sniffer_OnMemoryReadout(object sender, OnMemoryReadoutArgs args)
        {
            try
            {
                this.notesPlayedControl.UpdateNoteData(args.memoryReadout.noteData, args.memoryReadout.songTimer);

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
                this.playHistoryWindow = new PlayHistoryWindow(this.playedSongs);
                this.playHistoryWindow.Closed += this.PlayHistoryWindow_Closed;
                this.playHistoryMenuItem.IsChecked = true;
                this.playHistoryWindow.Show();
                this.playHistoryWindow.ScrollToBottom();
            }
        }

        private void ManualTestCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            new Task(() =>
            {
                SongDetails songDetails = new SongDetails() { SongLength = 60f };
                this.notesPlayedControl.UpdateSong(songDetails);

                GenericNoteData n = new GenericNoteData();
                n.TotalNotesHit = 150;
                n.TotalNotesMissed = 5;
                n.TotalNotes = 155;
                this.notesPlayedControl.UpdateNoteData(n, 140);

                //for (int i = 0; i < 30; i++)
                //{
                //    GenericNoteData n = new GenericNoteData();
                //    n.TotalNotes = i * 3;

                //    if (i % 2 == 0)
                //    {
                //        n.TotalNotesHit = i;
                //        n.TotalNotesMissed = i * 2;
                //        n.CurrentHitStreak = i / 2;
                //    }
                //    else
                //    {
                //        n.CurrentMissStreak = i - 1 / 2;
                //        n.TotalNotesHit = i + 1;
                //        n.TotalNotesMissed = i * 2 - 1;
                //    }

                //    n.HighestHitStreak = i + 1 / 2;

                //    n.Accuracy = (float)(1.0 * n.TotalNotesHit) / n.TotalNotes;
                //    this.notesPlayedControl.UpdateNoteData(n, i);

                //    Thread.Sleep(1000);
                //}
            }).Start();
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

            Application.Current.Shutdown(0);
        }
        #endregion
    }
}
