using LiveCharts;
using LiveCharts.Wpf;
using RockSnifferGui.DataStore;
using RockSnifferGui.Model;
using RockSnifferLib.Cache;
using RockSnifferLib.Sniffing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace RockSnifferGui.Controls
{
    /// <summary>
    /// Interaction logic for SongGraphControl.xaml
    /// </summary>
    public partial class SongGraphControl : UserControl
    {
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }

        public Func<double, string> YFormatter { get; private set; }
        public Func<double, string> XFormatter { get; private set; }

        public float MinAccuracy { get; set; }
        public float MaxAccuracy { get; set; }

        public double GraphHeight
        {
            get => this.songChart.Height;
        }
        public SongGraphControl()
        {
            this.InitializeComponent();
            this.DoSongGraph();
            
            this.XFormatter = (rawValue) => (rawValue + 1).ToString();
            this.YFormatter = (rawValue) => rawValue.ToString("P0");
        }

        private void DoSongGraph()
        {
            SQLiteCache songCache = new SQLiteCache();
            SQLiteStore songPlayStore = new SQLiteStore();

            IEnumerable<SongDetails> allSongs = songCache.GetAll();

            IEnumerable<SongPlayInstance> allSongPlays = songPlayStore.GetAll().Where(p => p.Accuracy >= .2);
            var groupedSongPlays = allSongPlays.GroupBy(p => p.SongDetails.SongID);

            this.SeriesCollection = new SeriesCollection();

            foreach (IGrouping<string, SongPlayInstance> group in groupedSongPlays)
            {
                SongDetails song = allSongs.Where(s => s.SongID.Equals(group.Key)).First();

                int maxNotesInGroup = group.Max(p => p.TotalNotes);
                this.SeriesCollection.Add(new LineSeries
                {
                    Title = $"{song.ArtistName} - {song.SongName} ({group.Count()})",
                    Values = new ChartValues<float>(group.Select(p => p.Accuracy * p.TotalNotes / maxNotesInGroup)),
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 12,
                    PointForeground = TryFindResource("WetAsphaltBrush") as Brush,
                    LineSmoothness = 1
                });
            }

            this.MinAccuracy = 0;
            this.MaxAccuracy = 1;
            this.DataContext = this;
        }
    }
}