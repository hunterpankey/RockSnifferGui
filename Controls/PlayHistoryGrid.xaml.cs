using RockSnifferGui.Model;
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
    /// Interaction logic for PlayHistoryGrid.xaml
    /// </summary>
    public partial class PlayHistoryGrid : UserControl
    {
        public PlayHistoryGrid()
        {
            InitializeComponent();
        }

        public void RefreshDisplay()
        {
            //this.playHistoryDataGrid.ItemsSource = this.SongPlays;
            this.playHistoryDataGrid.Items.Refresh();

            if (this.playHistoryDataGrid.Items.Count > 0)
            {
                var border = VisualTreeHelper.GetChild(this.playHistoryDataGrid, 0) as Decorator;
                if (border != null)
                {
                    var scroll = border.Child as ScrollViewer;
                    if (scroll != null) scroll.ScrollToEnd();
                }
            }
        }

        public void SetItems(IEnumerable<SongPlayInstance> items)
        {
            this.playHistoryDataGrid.ItemsSource = items;
        }

        public int ItemCount
        {
            get => this.playHistoryDataGrid.Items.Count;
        }
    }
}
