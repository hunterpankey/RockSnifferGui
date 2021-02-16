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
    /// Interaction logic for SongHistoryDetailsControl.xaml
    /// </summary>
    public partial class SongHistoryDetailsControl : UserControl
    {
        public static readonly DependencyProperty SongProperty = DependencyProperty.Register("Song", typeof(SongDetails), typeof(SongHistoryDetailsControl), 
            new PropertyMetadata());

        public SongDetails Song { 
            get => (SongDetails)GetValue(SongProperty);
            set => SetValue(SongProperty, value);
        }

        public SongHistoryDetailsControl()
        {
            InitializeComponent();
        }
    }
}
