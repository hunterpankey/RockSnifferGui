using System.Windows.Controls;

namespace RockSnifferGui.Controls
{
    /// <summary>
    /// Interaction logic for PlayHistoryGrid.xaml
    /// </summary>
    public partial class PlayHistoryGrid : UserControl
    {
        public PlayHistoryGrid()
        {
            this.InitializeComponent();
        }

        public void ScrollToBottom()
        {
            if (this.playHistoryDataGrid.Items.Count > 0)
            {
                var lastItem = this.playHistoryDataGrid.Items[this.playHistoryDataGrid.Items.Count - 1];
                this.playHistoryDataGrid.ScrollIntoView(lastItem);
            }
        }

        public int ItemCount
        {
            get => this.playHistoryDataGrid.Items.Count;
        }
    }
}
