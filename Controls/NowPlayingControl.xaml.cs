using RockSnifferGui.Services;
using RockSnifferLib.Events;
using RockSnifferLib.Logging;
using RockSnifferLib.Sniffing;
using System;
using System.Windows.Controls;

namespace RockSnifferGui.Controls
{
    /// <summary>
    /// Interaction logic for NowPlayingControl.xaml
    /// </summary>
    public partial class NowPlayingControl : UserControl
    {
        public NowPlayingControl()
        {
            this.InitializeComponent();
            this.SetupSniffer();

            this.UpdateSong(SnifferService.Instance.CurrentSong);
        }

        private void SetupSniffer()
        {
            SnifferService.Instance.SongStarted += this.Sniffer_OnSongStarted;
            SnifferService.Instance.SongChanged += this.Sniffer_OnSongChanged;

            this.npvm.SongDetails = SnifferService.Instance.CurrentSong;
        }

        public void UpdateSong(SongDetails songDetails)
        {
            this.npvm.SongDetails = songDetails;
        }

        #region Sniffer Events

        private void Sniffer_OnSongChanged(object sender, OnSongChangedArgs e)
        {
            //this.UpdateSong(e.songDetails);
        }

        private void Sniffer_OnSongStarted(object sender, OnSongStartedArgs e)
        {
            try
            {
                this.UpdateSong(e.song);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
        #endregion
    }
}
