using RockSnifferGui.Common;
using RockSnifferGui.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace RockSnifferGui
{
    public class PlayHistoryViewModel : GenericViewModel, INotifyPropertyChanged
    {
        private ObservableCollection<SongPlayInstance> allSongPlays = new ObservableCollection<SongPlayInstance>();
        private SongPlayInstance selectedSongInstance = new SongPlayInstance();

        public int SongPlayCount { get => this.AllSongPlays.Count(); }
        public ObservableCollection<SongPlayInstance> AllSongPlays
        {
            get => this.allSongPlays;
            set
            {
                this.SetProperty(ref this.allSongPlays, value, "AllSongPlays");
                this.OnPropertyChanged(new PropertyChangedEventArgs("SongPlayCount"));
            }
        }

        public void AddSongPlay(SongPlayInstance songPlay)
        {
            this.allSongPlays.Add(songPlay);
            this.OnPropertyChanged(new PropertyChangedEventArgs("AllSongPlays"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("SongPlayCount"));
        }

        public ICommand SelectSongCommand
        {
            get;
            set;
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
