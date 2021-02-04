using RockSnifferLib.RSHelpers.NoteData;
using RockSnifferLib.Sniffing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RockSnifferGui.Controls
{
    /// <summary>
    /// Interaction logic for NotesPlayedDataControl.xaml
    /// </summary>
    public partial class NotesPlayedDataControl : UserControl
    {
        public NotesPlayedDataControl()
        {
            InitializeComponent();
            this.notesStackPanel.Loaded += notesStackPanel_Loaded;
            //this.songProgressBar.Loaded += SongProgressBar_Loaded;
        }

        //private void SongProgressBar_Loaded(object sender, RoutedEventArgs e)
        //{
        //    this.songProgressStoryboard.Begin(this);
        //}

        private void notesStackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            //this.NotesLoadedStoryboard.Begin(this);
        }

        public void UpdateNoteData(INoteData noteData, float songTimer)
        {
            this.npvm.SongTimer = songTimer;
            this.npvm.NoteData = noteData;

            this.UpdateHitMissedColumns(noteData);
        }

        private void UpdateHitMissedColumns(INoteData noteData)
        {
            Dispatcher.Invoke(() =>
            {
                if (noteData != null)
                {
                    this.hitColumn.Width = new GridLength(noteData.TotalNotesHit, GridUnitType.Star);
                    this.missedColumn.Width = new GridLength(noteData.TotalNotesMissed, GridUnitType.Star);
                }
            });
        }

        public void UpdateSong(SongDetails songDetails)
        {
            this.npvm.SongDetails = songDetails;
        }
    }
}
