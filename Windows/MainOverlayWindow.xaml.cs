using System.Windows;
using System.Windows.Input;

namespace RockSnifferGui.Windows
{
    /// <summary>
    /// Interaction logic for MainOverlayWindow.xaml
    /// </summary>
    public partial class MainOverlayWindow : Window
    {
        public MainOverlayWindow()
        {
            this.InitializeComponent();
            this.MouseLeftButtonDown += this.MainOverlayWindow_MouseLeftButtonDown;

            this.nowPlayingControl.Tag = "MainOverlayWindow NowPlayingControl";
        }

        private void MainOverlayWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
