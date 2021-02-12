using RockSnifferGui.DataStore;
using RockSnifferGui.Model;
using RockSnifferGui.Services;
using System;
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
        public PlayHistoryWindow()
        {
            this.InitializeComponent();

            this.phvm.SongPlays = new ObservableCollection<SongPlayInstance>(PlayHistoryService.Instance.SongPlays);
            this.playHistoryDataGrid.ScrollToBottom();

            PlayHistoryService.Instance.NewSongHistorySong += this.PlayHistoryService_NewSongHistorySong;
            this.playHistoryDataGrid.Loaded += this.PlayHistoryDataGrid_Loaded;
        }

        private void PlayHistoryDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            this.ScrollToBottom();
        }

        private void PlayHistoryService_NewSongHistorySong(object sender, PlayHistorySongEndedArgs args)
        {
            this.AddSongPlay(args.song);
        }

        public void AddSongPlay(SongPlayInstance songPlay)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                this.phvm.AddSongPlay(songPlay);
                this.playHistoryDataGrid.ScrollToBottom();
            }));
        }

        private void ScrollToBottom()
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                this.playHistoryDataGrid.ScrollToBottom();
            }));
        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            SQLiteStore store = new SQLiteStore();
            store.Test();
            this.phvm.SongPlays = new ObservableCollection<SongPlayInstance>(store.GetAll());
        }
    }
}
