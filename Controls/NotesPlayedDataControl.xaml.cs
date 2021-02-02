﻿using RockSnifferLib.RSHelpers.NoteData;
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
    /// Interaction logic for NotesPlayedDataControl.xaml
    /// </summary>
    public partial class NotesPlayedDataControl : UserControl
    {
        public NotesPlayedDataControl()
        {
            InitializeComponent();
        }

        public void UpdateNoteData(INoteData noteData, float songTimer)
        {
            this.npvm.SongTimer = songTimer;
            this.npvm.NoteData = noteData;
        }
    }
}