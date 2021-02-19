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
            this.Closing += this.MainOverlayWindow_Closing;
            this.ContentRendered += this.MainOverlayWindow_ContentRendered;
            this.MouseLeftButtonDown += this.MainOverlayWindow_MouseLeftButtonDown;
        }

        private void MainOverlayWindow_ContentRendered(object sender, System.EventArgs e)
        {
            this.SetLocation();
        }

        private void SetLocation()
        {
            Properties.Settings settings = Properties.Settings.Default;
            this.Left = settings.OverlayLeft;
            this.Top = settings.OverlayTop;
        }

        private void MainOverlayWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;

            settings.OverlayLeft = this.Left;
            settings.OverlayTop = this.Top;
        }

        private void MainOverlayWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
