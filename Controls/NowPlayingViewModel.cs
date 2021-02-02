using RockSnifferLib.Sniffing;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace RockSnifferGui.Controls
{
    public class NowPlayingViewModel : INotifyPropertyChanged
    {
        private SongDetails songDetails;

        private string artistName;
        private string songName;
        private string albumName;
        private int albumYear;
        private System.Windows.Controls.Image albumArtImage;

        public SongDetails SongDetails
        {
            get => this.songDetails;
            set
            {
                this.SetProperty<SongDetails>(ref songDetails, value);
                this.SetProperty(ref this.artistName, value.artistName, "ArtistName");
                this.SetProperty(ref this.songName, value.songName, "SongName");
                this.SetProperty(ref this.albumName, value.albumName, "AlbumName");
                this.SetProperty(ref this.albumYear, value.albumYear, "AlbumYear");
                //this.SetProperty(ref this.albumArtImage, value.albumArt, "AlbumArtImage");

                if (value.albumArt != null)
                {
                    using (var memory = new MemoryStream())
                    {
                        value.albumArt.Save(memory, ImageFormat.Png);
                        memory.Position = 0;

                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = memory;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();

                        this.albumArtImage = new System.Windows.Controls.Image();
                        this.albumArtImage.Source = bitmapImage;
                        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AlbumArtImage"));
                    }
                }

            }
        }

        public string ArtistName { get => artistName; set => artistName = value; }
        public string SongName { get => songName; set => songName = value; }
        public string AlbumName { get => albumName; set => albumName = value; }
        public int AlbumYear { get => albumYear; set => albumYear = value; }
        public System.Windows.Controls.Image AlbumArtImage { get => albumArtImage; set => albumArtImage = value; }

        public NowPlayingViewModel()
        {
            this.SongDetails = new SongDetails();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!(object.Equals(field, newValue)))
            {
                field = (newValue);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }
    }
}