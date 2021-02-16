using RockSnifferGui.Common;
using RockSnifferGui.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RockSnifferGui.Controls
{
    public class PlayHistoryGridViewModel : GenericViewModel
    {
        private ObservableCollection<SongPlayInstance> songPlays;
        private SongPlayInstance selectedSongInstance;
        private ICommand selectSongCommand;

        public ObservableCollection<SongPlayInstance> SongPlays
        {
            get => this.songPlays;
            set
            {
                this.SetProperty(ref this.songPlays, value, "SongPlays");
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

        public SongPlayInstance SelectedSongInstance
        {
            get => this.selectedSongInstance;
            set
            {
                this.SetProperty(ref this.selectedSongInstance, value, "SelectedSongInstance");
            }
        }
    }
}
