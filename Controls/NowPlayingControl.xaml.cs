using RockSnifferGui;
using RockSnifferLib.Sniffing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RockSnifferGui.Controls
{
    /// <summary>
    /// Interaction logic for NowPlayingControl.xaml
    /// </summary>
    public partial class NowPlayingControl : UserControl
    {        
        public NowPlayingControl()
        {
            InitializeComponent();

            //Binding imageBinding = new Binding("SongDetails.albumArt");
            //imageBinding.Source = this.npvm;

            //this.albumArtImage.SetBinding(Image.SourceProperty, imageBinding);
        }

        public void UpdateSong(SongDetails songDetails)
        {
            this.npvm.SongDetails = songDetails;
        }
    }
}
