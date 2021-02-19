using RockSnifferGui.Model;
using RockSnifferLib.Logging;
using RockSnifferLib.RSHelpers;
using RockSnifferLib.RSHelpers.NoteData;
using RockSnifferLib.Sniffing;
using System.Collections.Generic;
using System.Linq;

namespace RockSnifferGui.Services
{
    public class CurrentSongService
    {
        #region Singleton
        private static CurrentSongService instance;

        public static CurrentSongService Instance
        {
            get
            {
                if (CurrentSongService.instance == null)
                {
                    CurrentSongService.instance = new CurrentSongService();
                }

                return CurrentSongService.instance;
            }

            private set => CurrentSongService.instance = value;
        }
        #endregion

        #region Events
        public delegate void OnSongSectionChanged(object sender, OnSongSectionChangedArgs args);
        public event OnSongSectionChanged SongSectionChanged;

        public delegate void OnNotesAdded(object sender, OnNotesAddedEventArgs args);
        public event OnNotesAdded NotesAdded;
        #endregion

        #region State variables
        private RSMemoryReadout currentReadount;
        private int totalNotes;
        private int totalNotesHit;
        private int totalNotesMissed;

        private bool noActiveSectionAlreadyNotified = false;
        #endregion

        #region Variables and Properties
        private SongDetails currentSong;
        private List<NoteStatus> noteList;
        private ArrangementDetails currentArrangement;
        private ArrangementDetails.SectionDetails currentSongSection;

        public SongDetails CurrentSong { get => this.currentSong; private set => this.currentSong = value; }
        public List<NoteStatus> NoteList { get => this.noteList; private set => this.noteList = value; }
        public ArrangementDetails CurrentArrangement { get => this.currentArrangement; private set => this.currentArrangement = value; }
        public ArrangementDetails.SectionDetails CurrentSongSection { get => this.currentSongSection; private set => this.currentSongSection = value; }
        #endregion

        private CurrentSongService()
        {
            this.NoteList = new List<NoteStatus>();

            SnifferService.Instance.MemoryReadout += this.SnifferService_MemoryReadout;
            SnifferService.Instance.SongStarted += this.SnifferService_SongStarted;
            SnifferService.Instance.SongEnded += this.SnifferService_SongEnded;
        }

        #region Sniffer event handlers
        private void SnifferService_SongStarted(object sender, RockSnifferLib.Events.OnSongStartedArgs args)
        {
            this.currentReadount = null;
            this.totalNotes = 0;
            this.totalNotesHit = 0;
            this.totalNotesMissed = 0;

            this.CurrentSong = args.song;
            this.CurrentArrangement = null;
            this.noActiveSectionAlreadyNotified = false;

        }

        private void SnifferService_SongEnded(object sender, RockSnifferLib.Events.OnSongEndedArgs args)
        {
            Logger.Log($"CurrentSongService: Song ended: hit {this.NoteList.Count(n => n.IsHit)}, missed {this.NoteList.Count(n => !n.IsHit)}");
        }

        private void SnifferService_MemoryReadout(object sender, RockSnifferLib.Events.OnMemoryReadoutArgs args)
        {
            if (SnifferService.Instance.CurrentState == SnifferState.SONG_PLAYING)
            {
                this.ProcessNoteData(args.memoryReadout);
                this.ProcessArrangementSection(args.memoryReadout);
            }
        }
        #endregion

        private void ProcessArrangementSection(RSMemoryReadout memoryReadout)
        {
            if (this.CurrentSong != null)
            {
                this.CurrentArrangement = this.CurrentSong.arrangements.Find(a => a.arrangementID.Equals(memoryReadout.arrangementID));

                if (this.CurrentArrangement != null)
                {
                    ArrangementDetails.SectionDetails currentSection = this.CurrentArrangement.sections.Find(
                        s => (s.startTime < memoryReadout.songTimer) && (s.endTime > memoryReadout.songTimer));

                    if (currentSection != null)
                    {
                        if ((this.CurrentSongSection == null) || (currentSection != this.CurrentSongSection))
                        {
                            // new song part
                            Logger.Log($"CurrentSongService: ProcessArrangementPart: at {memoryReadout.songTimer}, entered a new song part: {currentSection.name}");
                            ArrangementDetails.SectionDetails previousSection = this.currentSongSection;
                            this.CurrentSongSection = currentSection;

                            this.SongSectionChanged?.Invoke(this, new OnSongSectionChangedArgs() { PreviousSection = previousSection, NewSection = currentSection });
                        }
                    }
                    else
                    {
                        if (this.noActiveSectionAlreadyNotified)
                        {
                            // Only log this once. This is normal-ish for an intro before any music.
                            Logger.Log($"CurrentSongService: ProcessArrangementPart: couldn't find active song part at {memoryReadout.songTimer} seconds");
                            this.noActiveSectionAlreadyNotified = true;
                        }
                    }
                }
            }
        }

        private void ProcessNoteData(RSMemoryReadout memoryReadout)
        {
            INoteData noteData = memoryReadout.noteData;

            if (noteData != null)
            {
                if ((this.currentReadount == null))
                {
                    this.AddNotes(0, noteData.TotalNotesHit, noteData.TotalNotesMissed);

                    this.totalNotes = noteData.TotalNotes;
                    this.totalNotesHit = noteData.TotalNotesHit;
                    this.totalNotesMissed = noteData.TotalNotesMissed;

                    this.currentReadount = memoryReadout;

                    this.NotesAdded?.Invoke(this, new OnNotesAddedEventArgs()
                    {
                        SongTimer = memoryReadout.songTimer,
                        NotesHit = noteData.TotalNotesHit,
                        NotesMissed = noteData.TotalNotesMissed
                    });
                }
                else
                {
                    if (noteData.TotalNotes > this.totalNotes)
                    {

                        if (SnifferService.Instance.CurrentState != SnifferState.SONG_PLAYING)
                        {
                            Logger.Log($"CurrentSongService: Logging {noteData.TotalNotes - this.totalNotes} notes outside of SONG_PLAYING state.");
                        }

                        // calculate newly hit and missed notes, in case we somehow get an update with more than one note added
                        int newHit = noteData.TotalNotesHit - this.totalNotesHit;
                        int newMiss = noteData.TotalNotesMissed - this.totalNotesMissed;


                        this.AddNotes(this.totalNotes, newHit, newMiss);

                        this.totalNotes = noteData.TotalNotes;
                        this.totalNotesHit = noteData.TotalNotesHit;
                        this.totalNotesMissed = noteData.TotalNotesMissed;

                        // replace currentReadout with args.memReadout
                        this.currentReadount = memoryReadout;

                        this.NotesAdded?.Invoke(this, new OnNotesAddedEventArgs()
                        {
                            SongTimer = memoryReadout.songTimer,
                            NotesHit = newHit,
                            NotesMissed = newMiss
                        });
                    }
                }
            }
        }

        private void AddNotes(int startFrom, int hit, int missed)
        {
            int noteIndex = startFrom;

            if (SnifferService.Instance.CurrentState != SnifferState.SONG_PLAYING)
            {
                Logger.Log($"CurrentSongService: logNotes: Logging outide of SONG_PLAYING state. startFrom = {startFrom}, hit = {hit}, missed = {missed}");
            }

            for (int i = 0; i < hit; i++)
            {
                this.NoteList.Add(new NoteStatus() { NoteIndex = noteIndex, IsHit = true });
                noteIndex++;
            }

            for (int i = 0; i < missed; i++)
            {
                this.NoteList.Add(new NoteStatus() { NoteIndex = noteIndex, IsHit = false });
                noteIndex++;
            }
        }
    }

    public class OnNotesAddedEventArgs
    {
        public float SongTimer { get; set; }
        public int NotesHit { get; set; }
        public int NotesMissed { get; set; }

        public OnNotesAddedEventArgs()
        {
        }
    }

    public class OnSongSectionChangedArgs
    {
        public ArrangementDetails.SectionDetails PreviousSection { get; set; }
        public ArrangementDetails.SectionDetails NewSection { get; set; }

        public OnSongSectionChangedArgs()
        {
        }
    }
}
