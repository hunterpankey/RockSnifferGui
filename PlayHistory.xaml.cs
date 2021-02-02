using RockSnifferGui.DataStore;
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

            this.SongPlays = songPlays;
            this.playHistoryDataGrid.Loaded += PlayHistoryDataGrid_Loaded;
            this.SizeChanged += PlayHistoryWindow_SizeChanged;

            this.sniffer = sniffer;
            this.AttachSniffer();

            this.DataContext = this;
        }

        private void PlayHistoryWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.playHistoryDataGrid.Width = e.NewSize.Width - SystemParameters.VerticalScrollBarWidth;
        }

        private void PlayHistoryDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            this.RefreshDisplay();
        }

        private void AttachSniffer()
        {
            if (this.sniffer != null)
            {
                this.sniffer.OnSongEnded += Sniffer_OnSongEnded;
            }
        }

        public void AttachSniffer(Sniffer sniffer)
        {
            this.sniffer = sniffer;
            this.AttachSniffer();
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
            this.playHistoryDataGrid.ItemsSource = this.SongPlays;
            this.playHistoryDataGrid.Items.Refresh();

            if (this.playHistoryDataGrid.Items.Count > 0)
            {
                var border = VisualTreeHelper.GetChild(this.playHistoryDataGrid, 0) as Decorator;
                if (border != null)
                {
                    var scroll = border.Child as ScrollViewer;
                    if (scroll != null) scroll.ScrollToEnd();
                }
            }
        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            SQLiteStore store = new SQLiteStore();
            store.Test();
            this.songPlays = store.GetAll();
            this.RefreshDisplay();
        }
    }
}
