using RockSnifferGui.Common;
using RockSnifferGui.Model;
using RockSnifferLib.Sniffing;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RockSnifferGui.Controls
{
    public class PlayHistoryGridViewModel : GenericViewModel
    {
        private ObservableCollection<SongPlayInstance> songPlays = new ObservableCollection<SongPlayInstance>();
        private SongPlayInstance selectedSongInstance;
        private SongDetails selectedSong;
        private ICommand selectSongCommand;

        public ObservableCollection<SongPlayInstance> SongPlays
        {
            get => this.songPlays;
            set
            {
                this.SetProperty(ref this.songPlays, value, "SongPlays");
            }
        }

        public SongPlayInstance SelectedSongInstance
        {
            get => this.selectedSongInstance;
            set
            {
                this.SetProperty(ref this.selectedSongInstance, value, "SelectedSongInstance");
            }
        }

        public SongDetails SelectedSong
        {
            get => this.selectedSong;
            set
            {
                this.SetProperty(ref this.selectedSong, value, "SelectedSong");
            }
        }

        public ICommand SelectSongCommand
        {
            get => this.selectSongCommand;
            set
            {
                this.SetProperty(ref this.selectSongCommand, value, "SelectSongCommand");
            }
        }

    }
}
