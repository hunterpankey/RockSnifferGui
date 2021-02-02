﻿using RockSnifferGui.Common;
using RockSnifferLib.RSHelpers.NoteData;
using RockSnifferLib.Sniffing;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace RockSnifferGui.Controls
{
    public class NotesPlayedViewModel : GenericViewModel, INotifyPropertyChanged
    {
        private INoteData noteData;

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
                }
            }
        }

        public float SongTimer
        {
            get => songTimer; 
            set
            {
                this.SetProperty(ref this.songTimer, value, "SongTimer");
            }
        }
        public int NotesHit { get => notesHit; set => notesHit = value; }
        public int NotesMissed { get => notesMissed; set => notesMissed = value; }
        public int TotalNotes { get => totalNotes; set => totalNotes = value; }
        public int CurrentStreak { get => currentStreak; set => currentStreak = value; }
        public int MaxStreak { get => maxStreak; set => maxStreak = value; }
        public float Accuracy { get => accuracy; set => accuracy = value; }

    }
}