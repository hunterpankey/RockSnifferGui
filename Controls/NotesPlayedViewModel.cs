using RockSnifferGui.Common;
using RockSnifferLib.RSHelpers.NoteData;
using RockSnifferLib.Sniffing;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace RockSnifferGui.Controls
{
    public class NotesPlayedViewModel : GenericViewModel, INotifyPropertyChanged
    {
        private INoteData noteData;
        private SongDetails songDetails;

        private float songTimer;
        private int notesHit;
        private int notesMissed;
        private int totalNotes;
        private int currentStreak;
        private int maxStreak;
        private float accuracy;

        public INoteData NoteData
        {
            get => noteData;
            set
            {
                this.SetProperty(ref this.noteData, value, "NoteData");

                if (value != null)
                {
                    this.SetProperty(ref this.notesHit, value.TotalNotesHit, "NotesHit");
                    this.SetProperty(ref this.notesMissed, value.TotalNotesMissed, "NotesMissed");
                    this.SetProperty(ref this.totalNotes, value.TotalNotes, "TotalNotes");

                    this.SetProperty(ref this.currentStreak, value.CurrentHitStreak - value.CurrentMissStreak, "CurrentStreak");
                    this.SetProperty(ref this.maxStreak, value.HighestHitStreak, "MaxStreak");

                    this.SetProperty(ref this.accuracy, value.Accuracy, "Accuracy");
                    this.OnPropertyChanged(new PropertyChangedEventArgs("AccuracyDisplay"));
                    this.OnPropertyChanged(new PropertyChangedEventArgs("NotesHitBarWidth"));
                    this.OnPropertyChanged(new PropertyChangedEventArgs("NotesMissedBarWidth"));
                }
            }
        }

        public SongDetails SongDetails
        {
            get => songDetails; set
            {
                this.SetProperty(ref this.songDetails, value, "SongDetails");
            }
        }

        public float SongTimer
        {
            get => songTimer;
            set
            {
                this.SetProperty(ref this.songTimer, value, "SongTimer");
                this.OnPropertyChanged(new PropertyChangedEventArgs("SongTimerDisplay"));
                this.OnPropertyChanged(new PropertyChangedEventArgs("SongPercentage"));
            }
        }

        public string SongTimerDisplay { get => TimeSpan.FromSeconds(this.SongTimer).ToString(@"m\:ss"); }
        public double SongPercentage { get => (this.SongDetails != null) ? (this.SongTimer / this.SongDetails.songLength) : 0; }
        public int NotesHit { get => notesHit; set => notesHit = value; }
        public int NotesMissed { get => notesMissed; set => notesMissed = value; }
        public int TotalNotes { get => totalNotes; set => totalNotes = value; }
        public int CurrentStreak { get => currentStreak; set => currentStreak = value; }
        public int MaxStreak { get => maxStreak; set => maxStreak = value; }
        public float Accuracy { get => accuracy; set => accuracy = value; }
        public string AccuracyDisplay { get => string.Format(@"{0:f2}%", this.Accuracy); }
    }
}