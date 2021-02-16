using RockSnifferGui.Common;
using RockSnifferGui.Configuration;
using RockSnifferGui.DataStore;
using RockSnifferGui.Model;
using RockSnifferGui.Services;
using RockSnifferGui.Windows;
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
        internal static Config config;

        private static readonly bool Is64Bits = (IntPtr.Size == 8);

        private PlayHistoryWindow playHistoryWindow;
        private MainOverlayWindow mainOverlayWindow;
        private GraphWindow graphWindow;

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
            this.ContentRendered += this.MainWindow_ContentRendered;

            GameProcessService.Instance.PropertyChanged += this.GameProcessService_PropertyChanged;
            this.Initialize();

            this.DataContext = this;
        }

        public void Initialize()
        {
            this.Title = string.Format("Unofficial RockSniffer GUI {0}", App.Version);
            this.Closing += this.MainWindow_Closing;

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

            Logger.Log("Waiting for rocksmith");

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

        public void RestoreLocation()
        {
            Properties.Settings settings = Properties.Settings.Default;

            this.Left = settings.MainWindowLeft;
            this.Top = settings.MainWindowTop;
            this.Width = settings.MainWindowWidth;
            this.Height = settings.MainWindowHeight;
        }

        #region Game Process Events
        private void GameProcessService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("StatusForDisplay"))
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GameProcessServiceStatus"));
            }
        }
        #endregion

        #region UI Events
        private void TogglePlayHistoryCommandBinding_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (this.playHistoryWindow != null)
            {
                this.playHistoryWindow.Close();
                this.playHistoryWindow = null;
            }
            else
            {
                this.playHistoryWindow = new PlayHistoryWindow();
                this.playHistoryWindow.Closed += this.PlayHistoryWindow_Closed;
                this.playHistoryMenuItem.IsChecked = true;
                this.playHistoryWindow.Show();
            }
        }

        private void ToggleOverlayCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.mainOverlayWindow != null)
            {
                this.mainOverlayWindow.Close();
                this.mainOverlayWindow = null;
            }
            else
            {
                this.mainOverlayWindow = new MainOverlayWindow();
                this.mainOverlayWindow.WindowState = WindowState.Normal;

                this.mainOverlayWindow.Closed += this.MainOverlayWindow_Closed;
                this.mainOverlayMenuItem.IsChecked = true;
                this.mainOverlayWindow.Show();
            }
        }

        private void ToggleGraphsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.graphWindow != null)
            {
                this.graphWindow.Close();
                this.graphWindow = null;
            }
            else
            {
                this.graphWindow = new GraphWindow();
                this.graphWindow.Closed += this.GraphWindow_Closed;
                this.graphMenuItem.IsChecked = true;
                this.graphWindow.Show();
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

        private void MainOverlayWindow_Closed(object sender, EventArgs e)
        {
            this.mainOverlayWindow = null;
            this.mainOverlayMenuItem.IsChecked = false;
        }

        private void GraphWindow_Closed(object sender, EventArgs e)
        {
            this.graphWindow = null;
            this.graphMenuItem.IsChecked = false;
        }
        #endregion

        #region App Lifecycle Events
        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            this.RestoreLocation();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;

            settings.MainWindowLeft = this.Left;
            settings.MainWindowTop = this.Top;
            settings.MainWindowWidth = this.Width;
            settings.MainWindowHeight = this.Height;
        }

        private void ExitCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }
        #endregion
    }
}
