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

        private DateTime startTime;
        private DateTime endTime;

        public SongDetails SongDetails { get => songDetails; set => songDetails = value; }
        public INoteData NoteData { get => noteData; set => noteData = value; }
        public DateTime StartTime { get => startTime;}
        public DateTime EndTime { get => endTime;}

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
            this.noteData = noteData;
        }

        public void UpdateNoteData(INoteData noteData)
        {
            this.noteData = noteData;
        }
    }
}
