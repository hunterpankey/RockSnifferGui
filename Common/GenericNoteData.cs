using RockSnifferLib.RSHelpers.NoteData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockSnifferGui.Common
{
    public class GenericNoteData : INoteData
    {
        private int totalNotes;
        public int TotalNotes
        {
            get => this.totalNotes;
            set
            {
                this.totalNotes = value;
            }
        }

        private int totalNotesHit;
        public int TotalNotesHit
        {
            get => this.totalNotesHit;
            set
            {
                this.totalNotesHit = value;
            }
        }

        private int totalNotesMissed;
        public int TotalNotesMissed
        {
            get => this.totalNotesMissed;
            set
            {
                this.totalNotesMissed = value;
            }
        }

        private int currentHitStreak;
        public int CurrentHitStreak
        {
            get => this.currentHitStreak;
            set
            {
                this.currentHitStreak = value;
            }
        }

        private int currentMissStreak;
        public int CurrentMissStreak
        {
            get => this.currentMissStreak;
            set
            {
                this.currentMissStreak = value;
            }
        }

        private int highestHitStreak;
        public int HighestHitStreak
        {
            get => this.highestHitStreak;
            set
            {
                this.highestHitStreak = value;
            }
        }

        private float accuracy;
        public float Accuracy
        {
            get => this.accuracy;
            set
            {
                this.accuracy = value;
            }
        }
    }
}
