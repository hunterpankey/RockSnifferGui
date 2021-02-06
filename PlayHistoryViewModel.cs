using RockSnifferGui.Common;
using RockSnifferGui.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace RockSnifferGui
{
    public class PlayHistoryViewModel : GenericViewModel, INotifyPropertyChanged
    {
        private ObservableCollection<SongPlayInstance> songPlays = new ObservableCollection<SongPlayInstance>();

        public int SongPlayCount { get => this.SongPlays.Count(); }
        public ObservableCollection<SongPlayInstance> SongPlays
        {
            get => this.songPlays;
            set
            {
                this.SetProperty(ref this.songPlays, value);
                this.OnPropertyChanged(new PropertyChangedEventArgs("SongPlayCount"));
            }
        }

        public void AddSongPlay(SongPlayInstance songPlay)
        {
            this.songPlays.Add(songPlay);
            this.OnPropertyChanged(new PropertyChangedEventArgs("SongPlays"));
            this.OnPropertyChanged(new PropertyChangedEventArgs("SongPlayCount"));
        }
    }
}
