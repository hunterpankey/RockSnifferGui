using RockSnifferGui.Common;
using RockSnifferLib.Sniffing;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace RockSnifferGui.Controls
{
    public class NowPlayingViewModel : GenericViewModel, INotifyPropertyChanged
    {
        private SongDetails songDetails;

        private string artistName;
        private string songName;
        private string albumName;
        private int albumYear;
        private System.Drawing.Image albumArtImage;

        public SongDetails SongDetails
        {
            get => this.songDetails;
            set
            {
                this.SetProperty(ref songDetails, value);

                this.SetProperty(ref this.artistName, value.artistName, "ArtistName");
                this.SetProperty(ref this.songName, value.songName, "SongName");
                this.SetProperty(ref this.albumName, value.albumName, "AlbumName");
                this.SetProperty(ref this.albumYear, value.albumYear, "AlbumYear");
                this.SetProperty(ref this.albumArtImage, value.albumArt, "AlbumArtImage");

                this.OnPropertyChanged(new PropertyChangedEventArgs("AlbumDisplay"));
            }
        }

        public string ArtistName { get => artistName; set => artistName = value; }
        public string SongName { get => songName; set => songName = value; }
        public string AlbumName { get => albumName; set => albumName = value; }
        public int AlbumYear { get => albumYear; set => albumYear = value; }
        public string AlbumDisplay
        {
            get
            {
                if(string.IsNullOrEmpty(this.AlbumName) && (this.AlbumYear == 0))
                {
                    return string.Empty;
                }
                else
                {
                    return $"{this.AlbumName} ({this.AlbumYear})";
                }
            }
            set
            {

            }
        }
        public System.Drawing.Image AlbumArtImage { get => albumArtImage; set => albumArtImage = value; }

        public NowPlayingViewModel()
        {
            this.SongDetails = new SongDetails();
        }

    }

    [ValueConversion(typeof(System.Drawing.Image), typeof(System.Windows.Controls.Image))]
    public class AlbumArtImageConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapImage toReturn = null;
            var input = (System.Drawing.Image)value;

            if (value != null)
            {
                using (var memory = new MemoryStream())
                {
                    input.Save(memory, ImageFormat.Png);
                    memory.Position = 0;

                    toReturn = new BitmapImage();
                    toReturn.BeginInit();
                    toReturn.StreamSource = memory;
                    toReturn.CacheOption = BitmapCacheOption.OnLoad;
                    toReturn.EndInit();
                }
            }

            return toReturn;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Bitmap toReturn = null;

            if (value != null)
            {
                System.Windows.Controls.Image image = (System.Windows.Controls.Image)value;

                using (MemoryStream ms = new MemoryStream())
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)image.Source));
                    encoder.Save(ms);

                    using (Bitmap bmp = new Bitmap(ms))
                    {
                        toReturn = new Bitmap(bmp);
                    }
                }
            }

            return toReturn;
        }
    }
}