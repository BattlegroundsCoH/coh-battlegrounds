using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace BattlegroundsApp.Controls {

    /// <summary>
    /// 
    /// </summary>
    public class IconGridElement {

        /// <summary>
        /// 
        /// </summary>
        public object Tooltip { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public object Tag { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public string Icon { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public IconState State { get; set; } = IconState.Active;

    }

    /// <summary>
    /// 
    /// </summary>
    public class IconGridIconClickedEventArgs : EventArgs {

        /// <summary>
        /// 
        /// </summary>
        public int Row { get; }

        /// <summary>
        /// 
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// 
        /// </summary>
        public object ClickTag { get; }

        /// <summary>
        /// 
        /// </summary>
        public Icon Element { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="c"></param>
        /// <param name="t"></param>
        /// <param name="e"></param>
        public IconGridIconClickedEventArgs(int r, int c, object t, Icon e) {
            this.Row = r;
            this.Column = c;
            this.ClickTag = t;
            this.Element = e;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void IconGridIconClickedHandler(object sender, IconGridIconClickedEventArgs args);

    /// <summary>
    /// Interaction logic for IconGrid.xaml
    /// </summary>
    [ContentProperty(nameof(Icons))]
    public partial class IconGrid : UserControl, INotifyPropertyChanged {

        /// <summary>
        /// Identifies the <see cref="Icons"/> property.
        /// </summary>
        public static readonly DependencyProperty IconsProperty
            = DependencyProperty.Register(nameof(Icons), typeof(ObservableCollection<object>), typeof(IconGrid), new(new ObservableCollection<object>()));

        /// <summary>
        /// Identifies the <see cref="IconSource"/> property.
        /// </summary>
        public static readonly DependencyProperty IconSourceProperty
            = DependencyProperty.Register(nameof(IconSource), typeof(string), typeof(IconGrid), new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (a, b) => (a as IconGrid).IconSource = b.NewValue as string));

        /// <summary>
        /// Identifies the <see cref="IconState"/> property.
        /// </summary>
        public static readonly DependencyProperty IconStateProperty = DependencyProperty.Register(nameof(IconState), typeof(IconState), typeof(IconGrid), new(IconState.Available));

        /// <summary>
        /// Identifies the <see cref="Dimension"/> property.
        /// </summary>
        public static readonly DependencyProperty DimensionProperty
            = DependencyProperty.Register(nameof(Dimension), typeof(IconDimension), typeof(IconGrid), new FrameworkPropertyMetadata(
                IconDimension.Size24x24,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (a, b) => (a as IconGrid).Dimension = (IconDimension)b.NewValue));

        /// <summary>
        /// Identifies the <see cref="Rows"/> property.
        /// </summary>
        public static readonly DependencyProperty RowsProperty
            = DependencyProperty.Register(nameof(Rows), typeof(int), typeof(IconGrid), new FrameworkPropertyMetadata(
                0,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (a, b) => (a as IconGrid).Rows = (int)b.NewValue));

        /// <summary>
        /// Identifies the <see cref="Columns"/> property.
        /// </summary>
        public static readonly DependencyProperty ColumnsProperty
            = DependencyProperty.Register(nameof(Columns), typeof(int), typeof(IconGrid), new FrameworkPropertyMetadata(
                0,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (a, b) => (a as IconGrid).Columns = (int)b.NewValue));

        public event PropertyChangedEventHandler PropertyChanged;

        private Icon[,] m_grid;

        /// <summary>
        /// 
        /// </summary>
        public int Rows {
            get => (int)this.GetValue(RowsProperty);
            set {
                this.SetValue(RowsProperty, value);
                this.RefreshGrid();
                this.NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Columns {
            get => (int)this.GetValue(ColumnsProperty);
            set {
                this.SetValue(ColumnsProperty, value);
                this.RefreshGrid();
                this.NotifyPropertyChanged();
            }
        }

        public ObservableCollection<object> Icons {
            get => this.GetValue(IconsProperty) as ObservableCollection<object>;
            set {
                this.SetValue(IconsProperty, value);
                value.CollectionChanged += this.IconsCollectionChanged;
                this.RefreshDisplay();
                this.NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string IconSource {
            get => this.GetValue(IconSourceProperty) as string;
            set {
                this.SetValue(IconSourceProperty, value);
                this.RefreshGrid();
                this.NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IconState IconState {
            get => (IconState)this.GetValue(IconStateProperty);
            set {
                this.SetValue(IconStateProperty, value);
                this.RefreshGrid();
                this.NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IconDimension Dimension {
            get => (IconDimension)this.GetValue(DimensionProperty);
            set {
                this.SetValue(DimensionProperty, value);
                this.RefreshGrid();
                this.NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public event IconGridIconClickedHandler Clicked;

        public IconGrid() {
            this.InitializeComponent();
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void RefreshGrid() {

            // If either has none, do no display
            if (this.Rows is <= 0 || this.Columns is <= 0) {
                return;
            }

            // Clear canvas
            this.GridCanvas.Children.Clear();

            // Create container
            this.m_grid = new Icon[this.Rows, this.Columns];

            // Add all
            for (int i = 0; i < this.Rows; i++) {
                for (int j = 0; j < this.Columns; j++) {

                    // Create icon
                    this.m_grid[i, j] = new() {
                        Dimension = this.Dimension,
                        IconSource = this.IconSource
                    };
                    (int u, int v) = (i, j); // Closure Capture
                    this.m_grid[i, j].MouseDown += (sender, args) => {
                        this.Clicked?.Invoke(this, new(u, v, (sender as Control)?.Tag, this.m_grid[u, v]));
                    };

                    // Set dimensions
                    this.m_grid[i, j].SetValue(Canvas.LeftProperty, j * this.m_grid[i, j].DimensionCanvasWidth);
                    this.m_grid[i, j].SetValue(Canvas.TopProperty, i * this.m_grid[i, j].DimensionCanvasWidth);

                    // Add to canvas
                    _ = this.GridCanvas.Children.Add(this.m_grid[i, j]);

                }
            }

            // Update dimension
            this.Width = this.m_grid[0, 0].DimensionCanvasWidth * this.Columns;
            this.Height = this.m_grid[0, 0].DimensionCanvasWidth * this.Rows;

        }

        private void IconsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            => this.RefreshDisplay();

        private void RefreshDisplay() {

            // Row and Column counter
            int i = 0;
            int j = 0;

            // Loop over each item
            foreach (object icon in this.Icons) {

                // Assign based on element
                if (icon is IconGridElement gridElement) {
                    this.m_grid[i, j].IconName = gridElement.Icon;
                    this.m_grid[i, j].Tag = gridElement.Tag;
                    this.m_grid[i, j].ToolTip = gridElement.Tooltip;
                    this.m_grid[i, j].IconState = gridElement.State;
                } else {
                    this.m_grid[i, j].IconName = icon.ToString();
                }

                // Goto next icon
                j++;
                if (j >= this.Columns) {
                    j = 0;
                    i++;
                }

                // Bail if eof has been reached
                if (i >= this.Rows && j == 0) {
                    int missing = this.Icons.Count - i * j;
                    if (missing > 0) {
                        Trace.WriteLine($"Warning: Failed to display {missing} (of {this.Icons.Count}) icons in icon grid of dimensions {this.Rows}x{this.Columns}", nameof(IconGrid));
                        break;
                    }
                }

            }

        }

    }

}
