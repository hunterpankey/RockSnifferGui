using RockSnifferLib.RSHelpers.NoteData;
using RockSnifferLib.Sniffing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockSnifferGui.Model
{
    public class SongPlayInstance
    {
        private SongDetails songDetails;
        private INoteData noteData;

        private int notesHit;
        private int notesMissed;
        private int totalNotes;
        private int highestHitStreak;

        private DateTime startTime;
        private DateTime endTime;

        public SongDetails SongDetails { get => songDetails; set => songDetails = value; }
        public INoteData NoteData { get => noteData; set => noteData = value; }
        public DateTime StartTime { get => startTime; }
        public DateTime EndTime { get => endTime; }
        public int NotesHit { get => notesHit; set => notesHit = value; }
        public int NotesMissed { get => notesMissed; set => notesMissed = value; }
        public int TotalNotes { get => totalNotes; set => totalNotes = value; }
        public int HighestHitStreak { get => highestHitStreak; set => highestHitStreak = value; }
        public float Accuracy
        {
            get
            {
                if (totalNotes > 0)
                {
                    return 1.0f * NotesHit / TotalNotes;
                }
                else
                {
                    return 0f;
                }
            }
        }

        public string AccuracyAsPercentage
        {
            get
            {
                return this.Accuracy.ToString("P");
            }
        }

        public object imageLock = new object();

        public SongPlayInstance(SongDetails details, INoteData noteData = null, DateTime startTime = default(DateTime), DateTime endTime = default(DateTime))
        {
            this.SongDetails = details;
            this.noteData = noteData ?? new LearnASongNoteData();
            this.startTime = startTime;
            this.endTime = endTime;
        }

        public void StartSong()
        {
            this.startTime = DateTime.UtcNow;
        }

        public void FinishSong()
        {
            this.endTime = DateTime.UtcNow;
        }

        public void FinishSong(INoteData noteData)
        {
            this.endTime = DateTime.UtcNow;
            this.UpdateNoteData(noteData);
        }

        public void UpdateNoteData(INoteData noteData)
        {
            this.noteData = noteData;

            this.NotesHit = noteData.TotalNotesHit;
            this.NotesMissed = noteData.TotalNotesMissed;
            this.TotalNotes = noteData.TotalNotes;
            this.HighestHitStreak = noteData.HighestHitStreak;
        }
    }
}
