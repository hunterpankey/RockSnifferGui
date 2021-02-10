using LiveCharts.Definitions.Series;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RockSnifferGui.Controls
{
    /// <summary>
    /// Interaction logic for SongGraphLegend.xaml
    /// </summary>

    public partial class SongGraphLegend : UserControl, IChartLegend
    {
        /// <summary>
        /// Orientation of the legend entries
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)this.GetValue(OrientationProperty); }
            set { this.SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(SongGraphLegend), new PropertyMetadata(Orientation.Horizontal));

        //public static readonly DependencyProperty CustomHeightProperty =
        //    DependencyProperty.Register("Height", typeof(double), typeof(SongGraphLegend), new PropertyMetadata(Orientation.Horizontal));

        public SongGraphLegend()
        {
            this.InitializeComponent();

            this.itemsControl.DataContext = this;
        }

        public ObservableCollection<CustomSeriesViewModel> LegendEntries { get; } = new ObservableCollection<CustomSeriesViewModel>();

        public List<SeriesViewModel> Series
        {
            get => this.LegendEntries.Select(x => x.SeriesViewModel).ToList();
            set
            {
                Chart ownerChart = this.GetOwnerChart();

                // note: value only contains the visible series                
                // remove all entries which also have been removed from the chart
                var removedSeries = this.LegendEntries.Where(x => !ownerChart.Series.Any(s => s == x.View)).ToList();
                foreach (var rs in removedSeries)
                    this.LegendEntries.Remove(rs);

                foreach (var svm in value.OrderBy(x => x.Title))
                {
                    // add entries which are new                                        
                    // The SeriesViewModel instances are always new, so we have to compare using the title
                    if (!this.LegendEntries.Any(x => x.Title == svm.Title))
                    {
                        // find the series' UIElement by title
                        var seriesView = ownerChart.Series.FirstOrDefault(x => x.Title == svm.Title);
                        this.LegendEntries.Add(new CustomSeriesViewModel(svm, seriesView));
                    }

                }

                this.OnPropertyChanged();
            }
        }

        private Chart GetOwnerChart()
        {
            return FindParent<Chart>(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        private void selectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var legendEntry in this.LegendEntries)
            {
                legendEntry.IsVisible = true;
            }
        }

        private void selectNoneButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var legendEntry in this.LegendEntries)
            {
                legendEntry.IsVisible = false;
            }
        }
    }

    public class CustomSeriesViewModel : INotifyPropertyChanged
    {
        public string Title { get => this.SeriesViewModel.Title; }

        public Brush Fill { get => this.SeriesViewModel.Fill ?? this.SeriesViewModel.Stroke; }

        public Brush Stroke { get => this.SeriesViewModel.Stroke ?? this.SeriesViewModel.Fill; }

        public SeriesViewModel SeriesViewModel { get; }

        public ISeriesView View { get; }

        public bool IsVisible
        {
            get => ((UIElement)this.View).Visibility == Visibility.Visible;
            set
            {
                if (this.IsVisible != value)
                {
                    ((UIElement)this.View).Visibility = value ? Visibility.Visible : Visibility.Hidden;

                    this.OnPropertyChanged();
                }
            }
        }

        public CustomSeriesViewModel(SeriesViewModel svm, ISeriesView view)
        {
            this.SeriesViewModel = svm;
            this.View = view;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}