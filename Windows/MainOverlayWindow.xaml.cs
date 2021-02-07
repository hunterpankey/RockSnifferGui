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
using System.Windows.Shapes;

namespace RockSnifferGui.Windows
{
    /// <summary>
    /// Interaction logic for MainOverlayWindow.xaml
    /// </summary>
    public partial class MainOverlayWindow : Window
    {
        public MainOverlayWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += this.MainOverlayWindow_MouseLeftButtonDown;

            this.nowPlayingControl.Tag = "MainOverlayWindow NowPlayingControl";
        }

        private void MainOverlayWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
