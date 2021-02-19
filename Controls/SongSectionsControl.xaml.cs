using RockSnifferGui.Services;
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
    /// Interaction logic for SongSectionsControl.xaml
    /// </summary>
    public partial class SongSectionsControl : UserControl
    {
        private SongDetails songDetails;
        public SongSectionsControl()
        {
            InitializeComponent();

            SnifferService.Instance.SongStarted += this.SnifferService_SongStarted;
            SnifferService.Instance.SongEnded += this.SnifferService_SongEnded;
            SnifferService.Instance.MemoryReadout += this.SnifferService_MemoryReadout;
            CurrentSongService.Instance.SongSectionChanged += this.CurrentSongService_SongSectionChanged;
        }

        private void SnifferService_MemoryReadout(object sender, RockSnifferLib.Events.OnMemoryReadoutArgs args)
        {
            // only process this if there's an active song and we don't already know about its sections
            if ((this.songDetails != null) && (this.ssvm.Sections.Count == 0))
            {
                // the first readouts of a song, and all the readouts from menus have blank arrangementIDs
                if ((args.memoryReadout != null) && (!string.IsNullOrEmpty(args.memoryReadout.arrangementID)))
                {
                    ArrangementDetails arrangement = this.songDetails.arrangements.Find(a => a.arrangementID.Equals(args.memoryReadout.arrangementID));

                    if (arrangement != null)
                    {
                        this.ssvm.Sections = arrangement.sections;
                    }
                }
            }
        }

        private void SnifferService_SongStarted(object sender, RockSnifferLib.Events.OnSongStartedArgs args)
        {
            this.songDetails = args.song;
            this.ssvm.Sections = new List<ArrangementDetails.SectionDetails>();
        }

        private void SnifferService_SongEnded(object sender, RockSnifferLib.Events.OnSongEndedArgs args)
        {
            this.songDetails = new SongDetails();
            this.ssvm.Reset();
        }

        private void CurrentSongService_SongSectionChanged(object sender, OnSongSectionChangedArgs args)
        {
            if (this.ssvm.Sections.Contains(args.NewSection))
            {
                this.ssvm.ActiveSection = args.NewSection;
            }
        }
    }
}
