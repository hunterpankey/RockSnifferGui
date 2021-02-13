using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockSnifferGui.Model
{
    public class NoteStatus
    {
        private int noteIndex;
        private bool isHit;

        public bool IsHit { get => this.isHit; set => this.isHit = value; }
        public int NoteIndex { get => this.noteIndex; set => this.noteIndex = value; }
    }
}
