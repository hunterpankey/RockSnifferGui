using RockSnifferGui.DataStore;
using RockSnifferGui.Model;
using RockSnifferLib.Events;
using RockSnifferLib.Logging;
using RockSnifferLib.Sniffing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockSnifferGui.Services
{
    public class PlayHistoryService
    {
        private static PlayHistoryService instance;
        private SongPlayInstance currentSong;
        private SongDetails highlightedSong;

        private SQLiteStore songPlayInstancesDb = new SQLiteStore();

        public delegate void OnNewSongHistorySong(object sender, PlayHistorySongEndedArgs args);
        public event OnNewSongHistorySong NewSongHistorySong;

        public static PlayHistoryService Instance
        {
            get
            {
                if (PlayHistoryService.instance == null)
                {
                    PlayHistoryService.instance = new PlayHistoryService();
                }

                return PlayHistoryService.instance;
            }
            private set => PlayHistoryService.instance = value;
        }

        public SongPlayInstance CurrentSong { get => this.currentSong; private set => this.currentSong = value; }
        public List<SongPlayInstance> SongPlays { get => this.songPlayInstancesDb.GetAll(); }
        public SongDetails HighlightedSong { get => this.highlightedSong; set => this.highlightedSong = value; }

        private PlayHistoryService()
        {
            SnifferService.Instance.SongStarted += this.SnifferService_SongStarted;
            SnifferService.Instance.SongEnded += this.SnifferService_SongEnded;
            SnifferService.Instance.SongChanged += this.SnifferService_SongChanged;
        }

        #region Sniffer Event Handlers
        private void SnifferService_SongStarted(object sender, OnSongStartedArgs args)
        {
            try
            {
                SnifferService.Instance.MemoryReadout += this.SnifferService_MemoryReadout;
                this.currentSong = new SongPlayInstance(args.song);
                this.currentSong.StartSong();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void SnifferService_SongEnded(object sender, OnSongEndedArgs args)
        {
            try
            {
                SnifferService.Instance.MemoryReadout -= this.SnifferService_MemoryReadout;

                if (this.currentSong != null)
                {
                    this.CurrentSong.FinishSong();
                    this.SongPlays.Add(this.currentSong);

                    this.songPlayInstancesDb.Add(this.currentSong);

                    this.NewSongHistorySong?.Invoke(this, new PlayHistorySongEndedArgs() { song = this.currentSong });
                }

                this.currentSong = null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void SnifferService_SongChanged(object sender, OnSongChangedArgs args)
        {
            this.HighlightedSong = args.songDetails;
        }

        private void SnifferService_MemoryReadout(object sender, OnMemoryReadoutArgs args)
        {
            try
            {
                if ((this.CurrentSong != null) && (args.memoryReadout.noteData != null))
                {
                    this.CurrentSong.UpdateNoteData(args.memoryReadout.noteData);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
        #endregion

    }

    public class PlayHistorySongEndedArgs
    {
        public SongPlayInstance song;
    }
}
