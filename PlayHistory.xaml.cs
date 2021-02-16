using RockSnifferGui.Common;
using RockSnifferGui.DataStore;
using RockSnifferGui.Model;
using RockSnifferLib.RSHelpers.NoteData;
using RockSnifferLib.Sniffing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace RockSnifferGui
{
    /// <summary>
    /// Interaction logic for PlayHistory.xaml
    /// </summary>
    public partial class PlayHistoryWindow : Window
    {
        private SQLiteStore songStore = new SQLiteStore();

        public PlayHistoryWindow()
        {
            this.InitializeComponent();

            this.phvm.AllSongPlays = new ObservableCollection<SongPlayInstance>(songStore.GetAll());
            this.phvm.SelectSongCommand = (ICommand)this.FindResource("selectSongCommand");
            this.playHistoryDataGrid.ScrollToBottom(); 
        }

        public void UpdateSongPlays(IEnumerable<SongPlayInstance> songPlays)
        {
            this.phvm.AllSongPlays = new ObservableCollection<SongPlayInstance>(songPlays);
        }

        public void AddSongPlay(SongPlayInstance songPlay)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                this.phvm.AddSongPlay(songPlay);
                this.playHistoryDataGrid.ScrollToBottom();
            }));
        }

        public void ScrollToBottom()
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
            this.phvm.AllSongPlays = new ObservableCollection<SongPlayInstance>(store.GetAll());
        }

        private void BackCommandBinding_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            //e.CanExecute = this.playHistoryDataGrid.Visibility == Visibility.Hidden;
            e.CanExecute = true;
        }

        private void BackCommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            this.HideSongDetails();
        }

        private void SelectSongBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            string song = string.Empty;

            if(e.Parameter != null)
            {
                song = ((SongPlayInstance)e.Parameter).SongDetails.SongName;
                this.phvm.SelectedSongInstance = (SongPlayInstance)e.Parameter;
                this.ShowSongDetails();
            }
        }

        private void ShowSongDetails()
        {
            this.playHistoryDataGrid.Visibility = Visibility.Hidden;
            this.songHistoryDetailsControl.Visibility = Visibility.Visible;
        }

        private void HideSongDetails()
        {
            this.playHistoryDataGrid.Visibility = Visibility.Visible;
            this.songHistoryDetailsControl.Visibility = Visibility.Hidden;
        }
    }
}
