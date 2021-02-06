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
        }

        public void UpdateNoteData(INoteData noteData, float songTimer)
        {
            this.npvm.SongTimer = songTimer;
            this.npvm.NoteData = noteData;
        }

        public void UpdateSong(SongDetails songDetails)
        {
            this.npvm.SongDetails = songDetails;
        }
    }
}
