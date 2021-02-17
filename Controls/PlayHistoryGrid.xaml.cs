using RockSnifferGui.Model;
using RockSnifferGui.Services;
using RockSnifferLib.Sniffing;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RockSnifferGui.Controls
{
    /// <summary>
    /// Interaction logic for PlayHistoryGrid.xaml
    /// </summary>
    public partial class PlayHistoryGrid : UserControl
    {
        private PlayHistoryGridViewModel vm;

        #region Dependency Properties
        public static readonly DependencyProperty SelectSongCommandProperty = DependencyProperty.Register("SelectSongCommand", typeof(ICommand), typeof(PlayHistoryGrid),
            new UIPropertyMetadata(null, OnSelectSongCommandPropertyUpdated));

        private static void OnSelectSongCommandPropertyUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayHistoryGrid grid = (PlayHistoryGrid)d;
            grid.OnSelectSongCommandPropertyUpdated(e.NewValue);
        }

        private void OnSelectSongCommandPropertyUpdated(object newValue)
        {
            this.SelectSongCommand = (ICommand)newValue;
        }

        public static readonly DependencyProperty SelectedSongProperty = DependencyProperty.Register("SelectedSong", typeof(SongDetails), typeof(PlayHistoryGrid),
    new UIPropertyMetadata(null, OnSelectedSongPropertyUpdated));

        private static void OnSelectedSongPropertyUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayHistoryGrid grid = (PlayHistoryGrid)d;
            grid.OnSelectedSongPropertyUpdated(e.NewValue);
        }

        private void OnSelectedSongPropertyUpdated(object newValue)
        {
            this.SelectedSong = (SongDetails)newValue;
        }
        #endregion

        public ICommand SelectSongCommand
        {
            get => (ICommand)this.GetValue(SelectSongCommandProperty);
            set
            {
                this.SetValue(SelectSongCommandProperty, value);
                this.vm.SelectSongCommand = value;
            }
        }

        public SongDetails SelectedSong
        {
            get => (SongDetails)this.GetValue(SelectedSongProperty);
            set
            {
                this.SetValue(SelectedSongProperty, value);
                this.vm.SelectedSong = value;
                this.UpdateSongPlays();
            }
        }

        public PlayHistoryGrid()
        {
            this.InitializeComponent();
            this.vm = new PlayHistoryGridViewModel();
            this.LayoutRoot.DataContext = this.vm;

            PlayHistoryService.Instance.NewSongHistorySong += this.PlayHistoryService_NewSongHistorySong;

            this.UpdateSongPlays();
        }

        private void PlayHistoryService_NewSongHistorySong(object sender, PlayHistorySongEndedArgs args)
        {
            this.UpdateSongPlays();
        }

        private void UpdateSongPlays()
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                IEnumerable<SongPlayInstance> songs = PlayHistoryService.Instance.SongPlays;

                if (this.SelectedSong != null)
                {
                    songs = PlayHistoryService.Instance.GetSongPlaysBySongId(this.SelectedSong.songID);
                }
                this.vm.SongPlays.Clear();

                foreach (SongPlayInstance songPlayInstance in songs)
                {
                    this.vm.SongPlays.Add(songPlayInstance);
                }
            }));
        }

        public void ScrollToBottom()
        {
            if (this.playHistoryDataGrid.Items.Count > 0)
            {
                var lastItem = this.playHistoryDataGrid.Items[this.playHistoryDataGrid.Items.Count - 1];
                this.playHistoryDataGrid.ScrollIntoView(lastItem);
            }
        }

        public int ItemCount
        {
            get => this.playHistoryDataGrid.Items.Count;
        }
    }
}
