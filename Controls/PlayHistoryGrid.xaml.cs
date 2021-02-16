using RockSnifferGui.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RockSnifferGui.Controls
{
    /// <summary>
    /// Interaction logic for PlayHistoryGrid.xaml
    /// </summary>
    public partial class PlayHistoryGrid : UserControl
    {
        private PlayHistoryGridViewModel vm;
        public static readonly DependencyProperty SelectSongCommandProperty = DependencyProperty.Register("SelectSongCommand", typeof(ICommand), typeof(PlayHistoryGrid),
            new UIPropertyMetadata(null));

        public ICommand SelectSongCommand
        {
            get => (ICommand)this.GetValue(SelectSongCommandProperty);
            set
            {
                this.SetValue(SelectSongCommandProperty, value);
                this.vm.SelectSongCommand = value;
            }
        }

        public static readonly DependencyProperty SongPlaysProperty = DependencyProperty.Register("SongPlays", typeof(ObservableCollection<SongPlayInstance>),
            typeof(PlayHistoryGrid), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnSongPlaysPropertyChanged), null, false, System.Windows.Data.UpdateSourceTrigger.PropertyChanged));

        private static void OnSongPlaysPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PlayHistoryGrid)d).OnSongPlaysPropertyChanged((ObservableCollection<SongPlayInstance>)(e.NewValue));
        }

        public ObservableCollection<SongPlayInstance> SongPlays
        {
            get => (ObservableCollection<SongPlayInstance>)this.GetValue(SongPlaysProperty);
            set
            {
                this.SetValue(SongPlaysProperty, value);
                this.vm.SongPlays = value;
            }
        }

        public void OnSongPlaysPropertyChanged(ObservableCollection<SongPlayInstance> songPlays)
        {
            this.SongPlays = this.SongPlays;
        }

        public static readonly DependencyProperty TestProperty = DependencyProperty.Register("Test", typeof(string), typeof(PlayHistoryGrid), new PropertyMetadata());
        public string Test
        {
            get => (string)this.GetValue(TestProperty);
            set => this.SetValue(TestProperty, value);
        }


        public PlayHistoryGrid()
        {
            this.InitializeComponent();
            this.vm = new PlayHistoryGridViewModel();
            this.LayoutRoot.DataContext = vm;
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
