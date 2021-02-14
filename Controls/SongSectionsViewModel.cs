using RockSnifferGui.Common;
using RockSnifferLib.Sniffing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockSnifferGui.Controls
{
    public class SongSectionsViewModel : GenericViewModel, INotifyPropertyChanged
    {
        private List<ArrangementDetails.SectionDetails> sections;
        private ArrangementDetails.SectionDetails activeSection;
        private string allSectionsCombined;

        private string before;
        private string current;
        private string after;

        public List<ArrangementDetails.SectionDetails> Sections
        {
            get => this.sections;
            set
            {
                this.SetProperty(ref this.sections, value, "Sections");

                this.AllSectionsCombined = string.Join(", ", this.sections.Select(s => s.name));
            }
        }
        public ArrangementDetails.SectionDetails ActiveSection
        {
            get => this.activeSection; 
            set
            {
                this.SetProperty(ref this.activeSection, value, "ActiveSection");
                this.Current = this.ActiveSection.name;

                int thisIndex = this.sections.IndexOf(value);

                if (thisIndex != -1)
                {
                    this.Before = string.Join("   ", this.sections.GetRange(0, thisIndex).OrderBy(s => s.startTime).Select(s => s.name));

                    // if current is valid and isn't the last one
                    if (thisIndex != this.sections.Count - 1)
                    {
                        this.After = string.Join("   ", this.sections.GetRange(thisIndex + 1, this.sections.Count - thisIndex - 1).Select(s => s.name));
                    }
                    else
                    {
                        this.After = string.Empty;
                    }
                }
                else
                {
                    this.Before = string.Empty;
                    this.After = string.Empty;
                }
            }
        }
        public string AllSectionsCombined { get => this.allSectionsCombined; set => this.SetProperty(ref this.allSectionsCombined, value, "AllSectionsCombined"); }
        public string Before { get => this.before; set => this.SetProperty(ref this.before, value, "Before"); }
        public string Current { get => this.current; set => this.SetProperty(ref this.current, value, "Current"); }
        public string After { get => this.after; set => this.SetProperty(ref this.after, value, "After"); }


        public SongSectionsViewModel()
        {
            this.Reset();
        }

        internal void Reset()
        {
            this.Sections = new List<ArrangementDetails.SectionDetails>();
            this.ActiveSection = new ArrangementDetails.SectionDetails();
        }
    }
}
