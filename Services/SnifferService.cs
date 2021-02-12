using RockSnifferGui.Configuration;
using RockSnifferLib.Cache;
using RockSnifferLib.Events;
using RockSnifferLib.Logging;
using RockSnifferLib.Sniffing;
using System;
using System.Diagnostics;

namespace RockSnifferGui.Services
{
    public class SnifferService
    {
        #region Singleton
        private static SnifferService instance;

        public static SnifferService Instance
        {
            get
            {
                if (SnifferService.instance == null)
                {
                    SnifferService.instance = new SnifferService();
                }

                return SnifferService.instance;
            }
            private set => SnifferService.instance = value;
        }
        #endregion

        internal static ICache cache;
        internal static Config config;
        private static Sniffer sniffer;

        private SongDetails currentSong = new SongDetails();

        //public Sniffer Sniffer { get => SnifferService.sniffer; private set => SnifferService.sniffer = value; }
        #region Events
        public delegate void OnSnifferChanged(object sender, SnifferChangedEventArgs args);
        public event OnSnifferChanged SnifferChanged;

        #region Sniffer Event Passthrough
        public delegate void OnSongStarted(object sender, OnSongStartedArgs args);
        public event OnSongStarted SongStarted;

        public delegate void OnSongChanged(object sender, OnSongChangedArgs args);
        public event OnSongChanged SongChanged;

        public delegate void OnSongEnded(object sender, OnSongEndedArgs args);
        public event OnSongEnded SongEnded;

        public delegate void OnMemoryReadout(object sender, OnMemoryReadoutArgs args);
        public event OnMemoryReadout MemoryReadout;
        #endregion
        #endregion

        public SnifferState Status
        {
            get
            {
                SnifferState toReturn = SnifferState.NONE;

                if (SnifferService.sniffer != null)
                {
                    toReturn = SnifferService.sniffer.currentState;
                }

                return toReturn;
            }
        }

        public SongDetails CurrentSong { get => this.currentSong; set => this.currentSong = value; }

        private SnifferService()
        {
            SnifferService.config = new Config();

            try
            {
                config.Load();
            }
            catch (Exception e)
            {
                Logger.LogError("Could not load configuration: {0}\r\n{1}", e.Message, e.StackTrace);
                throw e;
            }

            SnifferService.cache = new SQLiteCache();

            GameProcessService.Instance.GameProcessChanged += this.GameProcessService_GameProcessChanged;

            if (GameProcessService.Instance.Status == GameProcessStatus.RUNNING)
            {
                this.AttachSniffer(GameProcessService.Instance.GameProcess);
            }
        }

        private void GameProcessService_GameProcessChanged(object sender, GameProcessChangedEventArgs args)
        {
            this.DetachSniffer();
            this.AttachSniffer(args.Process);
        }

        private void DetachSniffer()
        {
            if (SnifferService.sniffer != null)
            {
                SnifferService.sniffer.OnStateChanged -= this.Sniffer_OnStateChanged;
                SnifferService.sniffer.OnSongChanged -= this.Sniffer_OnSongChanged;
                SnifferService.sniffer.OnSongStarted -= this.Sniffer_OnSongStarted;
                SnifferService.sniffer.OnSongEnded -= this.Sniffer_OnSongEnded;
                SnifferService.sniffer.OnMemoryReadout -= this.Sniffer_OnMemoryReadout;
            }
        }

        private void AttachSniffer(Process process)
        {
            SnifferService.sniffer = new Sniffer(process, cache, config.snifferSettings);

            SnifferService.sniffer.OnStateChanged += this.Sniffer_OnStateChanged;
            SnifferService.sniffer.OnSongChanged += this.Sniffer_OnSongChanged;
            SnifferService.sniffer.OnSongStarted += this.Sniffer_OnSongStarted;
            SnifferService.sniffer.OnSongEnded += this.Sniffer_OnSongEnded;
            SnifferService.sniffer.OnMemoryReadout += this.Sniffer_OnMemoryReadout;

            this.SnifferChanged?.Invoke(this, new SnifferChangedEventArgs(SnifferService.sniffer));
        }

        private void Sniffer_OnStateChanged(object sender, OnStateChangedArgs e)
        {
            Logger.Log($"SnifferService: OnStateChanged: from {e.oldState.ToString()} to {e.newState.ToString()}");
        }


        #region Sniffer Event Passthrough Handlers
        private void Sniffer_OnSongChanged(object sender, OnSongChangedArgs e)
        {
            Logger.Log($"SnifferService: OnSongChanged: {e.songDetails.ToString()}");
            this.CurrentSong = e.songDetails;
            this.SongChanged?.Invoke(this, e);
        }

        private void Sniffer_OnSongStarted(object sender, OnSongStartedArgs e)
        {
            Logger.Log($"SnifferService: OnSongStarted: {e.song.ToString()}");
            this.SongStarted?.Invoke(this, e);
        }

        private void Sniffer_OnSongEnded(object sender, OnSongEndedArgs e)
        {
            Logger.Log($"SnifferService: OnSongEnded: {e.song.ToString()}");
            this.SongEnded?.Invoke(this, e);
        }

        private void Sniffer_OnMemoryReadout(object sender, OnMemoryReadoutArgs e)
        {
            this.MemoryReadout?.Invoke(this, e);
            SongDetails song = App.Cache.Get(e.memoryReadout.songID);

            if ((song != null) && (this.CurrentSong.SongID != song.SongID))
            {
                this.CurrentSong = song;
                this.SongChanged?.Invoke(this, new OnSongChangedArgs() { songDetails = this.CurrentSong });
            }
        }
        #endregion
    }

    public class SnifferChangedEventArgs
    {
        private Sniffer sniffer;

        public Sniffer Siffer { get => this.sniffer; private set => this.sniffer = value; }

        public SnifferChangedEventArgs(Sniffer sniffer)
        {
            this.sniffer = sniffer;
        }
    }
}
