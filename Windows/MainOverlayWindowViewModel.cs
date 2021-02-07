

using System.Windows.Media;

namespace RockSnifferGui.Windows
{
    public class MainOverlayWindowViewModel
    {
        private double dropShadowDepth;
        private double dropShadowRadius;
        private Color dropShadowColor;
        private double dropShadowOpacity;

        public double DropShadowDepth { get => this.dropShadowDepth; set => this.dropShadowDepth = value; }
        public double DropShadowRadius { get => this.dropShadowRadius; set => this.dropShadowRadius = value; }
        public Color DropShadowColor { get => this.dropShadowColor; set => this.dropShadowColor = value; }
        public double DropShadowOpacity { get => this.dropShadowOpacity; set => this.dropShadowOpacity = value; }

        public MainOverlayWindowViewModel()
        {
            this.DropShadowColor = (Color)(App.Current.Resources["Clouds"]);
            this.DropShadowDepth = 0;
            this.DropShadowRadius = 15;
            this.DropShadowOpacity = .8;
        }
    }
}
