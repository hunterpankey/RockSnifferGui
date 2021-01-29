using RockSnifferGui.Model;
using RockSnifferLib.RSHelpers.NoteData;
using RockSnifferLib.Sniffing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RockSnifferGui
{
    /// <summary>
    /// Interaction logic for PlayHistory.xaml
    /// </summary>
    public partial class PlayHistoryWindow : Window
    {
        private IEnumerable<SongPlayInstance> songPlays;
        private Sniffer sniffer;

        public IEnumerable<SongPlayInstance> SongPlays { get => songPlays; set => songPlays = value; }
        public int SongPlayCount { get => SongPlays.Count(); }

        public PlayHistoryWindow(List<SongPlayInstance> songPlays, Sniffer sniffer)
        {
            InitializeComponent();

            this.songPlays = songPlays;
            this.playHistoryDataGrid.ItemsSource = this.SongPlays;

            this.sniffer = sniffer;
            this.AttachSniffer();

            this.DataContext = this;
        }

        private void AttachSniffer()
        {
            if (this.sniffer != null)
            {
                this.sniffer.OnSongEnded += Sniffer_OnSongEnded;
            }
        }

        private void Sniffer_OnSongEnded(object sender, RockSnifferLib.Events.OnSongEndedArgs e)
        {
            Thread.Sleep(1000);
            this.RefreshDisplay();
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            this.RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            //this.playHistoryDataGrid.ItemsSource = this.songPlays;
            this.playHistoryDataGrid.Items.Refresh();
        }
    }
}
