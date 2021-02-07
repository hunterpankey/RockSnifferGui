using RockSnifferGui.Common;
using RockSnifferGui.Services;
using RockSnifferLib.RSHelpers.NoteData;
using RockSnifferLib.Sniffing;
using System.Windows.Controls;

namespace RockSnifferGui.Controls
{
    /// <summary>
    /// Interaction logic for NotesPlayedDataControl.xaml
    /// </summary>
    public partial class NotesPlayedDataControl : UserControl
    {
        public NotesPlayedDataControl()
        {
            this.InitializeComponent();
            this.SetupSniffer();

            if((SnifferService.Instance.Status == SnifferState.SONG_PLAYING)
                || (SnifferService.Instance.Status == SnifferState.SONG_STARTING))
            {
                this.ActivateSniffer();
            }
        }

        public void UpdateNoteData(INoteData noteData, float songTimer)
        {
            this.npvm.SongTimer = songTimer;
            this.npvm.NoteData = noteData;
        }

        public void ClearNoteData()
        {
            this.npvm.SongTimer = 0f;
            this.npvm.NoteData = new GenericNoteData();
        }

        public void UpdateSong(SongDetails songDetails)
        {
            this.npvm.SongDetails = songDetails;
        }

        private void SetupSniffer()
        {
            SnifferService.Instance.SongStarted += this.SnifferService_SongStarted;
            SnifferService.Instance.SongChanged += this.SnifferService_SongChanged;
            SnifferService.Instance.SongEnded += this.SnifferService_SongEnded;

            if (SnifferService.Instance.Status == SnifferState.SONG_PLAYING)
            {
                this.ActivateSniffer();
            }
        }

        private void ActivateSniffer()
        {
            SnifferService.Instance.MemoryReadout += this.SnifferService_MemoryReadout;
        }

        private void DeactivateSniffer()
        {
            SnifferService.Instance.MemoryReadout -= this.SnifferService_MemoryReadout;
        }

        #region Sniffer Events
        private void SnifferService_SongChanged(object sender, RockSnifferLib.Events.OnSongChangedArgs args)
        {
            this.UpdateNoteData(new GenericNoteData(), 0f);
        }

        private void SnifferService_SongStarted(object sender, RockSnifferLib.Events.OnSongStartedArgs args)
        {
            this.ActivateSniffer();
            this.UpdateSong(args.song);
        }

        private void SnifferService_SongEnded(object sender, RockSnifferLib.Events.OnSongEndedArgs args)
        {
            this.DeactivateSniffer();
        }

        private void SnifferService_MemoryReadout(object sender, RockSnifferLib.Events.OnMemoryReadoutArgs args)
        {
             this.UpdateNoteData(args.memoryReadout.noteData, args.memoryReadout.songTimer);
        }
        #endregion
    }
}
