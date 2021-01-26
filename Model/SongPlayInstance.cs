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

        public SongPlayInstance(SongDetails details)
        {
            this.songDetails = details;
            this.noteData = new LearnASongNoteData();
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
