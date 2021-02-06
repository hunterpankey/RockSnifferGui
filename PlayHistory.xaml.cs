using RockSnifferGui.DataStore;
using RockSnifferGui.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace RockSnifferGui
{
    /// <summary>
    /// Interaction logic for PlayHistory.xaml
    /// </summary>
    public partial class PlayHistoryWindow : Window
    {
        public PlayHistoryWindow(List<SongPlayInstance> songPlays)
        {
            this.InitializeComponent();

            this.phvm.SongPlays = new ObservableCollection<SongPlayInstance>(songPlays);
            this.playHistoryDataGrid.ScrollToBottom();
        }

        public void UpdateSongPlays(IEnumerable<SongPlayInstance> songPlays)
        {
            this.phvm.SongPlays = new ObservableCollection<SongPlayInstance>(songPlays);
        }

        public void AddSongPlay(SongPlayInstance songPlay)
        {
            this.phvm.AddSongPlay(songPlay);
            this.playHistoryDataGrid.ScrollToBottom();
        }

        public void ScrollToBottom()
        {
            this.playHistoryDataGrid.ScrollToBottom();
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This button doesn't do anything anymore. Thanks, ObservableCollection class!", "No Operation", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            SQLiteStore store = new SQLiteStore();
            store.Test();
            this.phvm.SongPlays = new ObservableCollection<SongPlayInstance>(store.GetAll());
        }
    }
}
