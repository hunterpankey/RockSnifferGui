using RockSnifferGui.Model;
using RockSnifferGui.Services;
using System;
using System.Windows;
using System.Windows.Input;

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

            this.phvm.SelectSongCommand = (ICommand)this.FindResource("selectSongCommand");

            this.UpdateSongPlayCount();
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
            this.UpdateSongPlayCount();
        }

        private void UpdateSongPlayCount()
        {
            this.phvm.SongPlayCount = PlayHistoryService.Instance.SongPlays.Count;
        }

        private void ScrollToBottom()
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                this.playHistoryDataGrid.ScrollToBottom();
            }));
        }

        private void BackCommandBinding_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.playHistoryDataGrid?.Visibility == Visibility.Hidden;
        }

        private void BackCommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            this.HideSongDetails();
        }

        private void SelectSongBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            string song = string.Empty;

            if (e.Parameter != null)
            {
                song = ((SongPlayInstance)e.Parameter).SongDetails.SongName;
                this.phvm.SelectedSongInstance = (SongPlayInstance)e.Parameter;

                this.ShowSongDetails();
            }
        }

        private void ShowSongDetails()
        {
            this.playHistoryDataGrid.Visibility = Visibility.Hidden;
            this.playHistorySongGrid.Visibility = Visibility.Visible;
        }

        private void HideSongDetails()
        {
            this.playHistoryDataGrid.Visibility = Visibility.Visible;
            this.playHistorySongGrid.Visibility = Visibility.Hidden;
        }
    }
}
