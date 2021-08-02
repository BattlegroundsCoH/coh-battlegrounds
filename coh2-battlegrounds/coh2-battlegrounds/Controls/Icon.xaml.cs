using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BattlegroundsApp.Controls {

    /// <summary>
    /// Enum representing the various supported dimensions for an <see cref="Icon"/>
    /// </summary>
    public enum IconDimension {

        /// <summary>
        /// A 24x24 icon.
        /// </summary>
        Size24x24,

        /// <summary>
        /// A 32x32 icon.
        /// </summary>
        Size32x32,

        /// <summary>
        /// A 36x36 icon.
        /// </summary>
        Size36x36,

        /// <summary>
        /// A 48x48 icon.
        /// </summary>
        Size48x48,

        /// <summary>
        /// A 64x64 icon.
        /// </summary>
        Size64x64

    }

    /// <summary>
    /// Enum representing the state of an <see cref="Icon"/>.
    /// </summary>
    public enum IconState {

        /// <summary>
        /// Icon will represent something available but not selected.
        /// </summary>
        Available,

        /// <summary>
        /// Icon will represent something not available.
        /// </summary>
        Disabled,

        /// <summary>
        /// Icon will represent the currently active state.
        /// </summary>
        Active,

    }

    /// <summary>
    /// Interaction logic for Icon.xaml
    /// </summary>
    public partial class Icon : UserControl, INotifyPropertyChanged {

        /// <summary>
        /// Identifies the <see cref="IconSource"/> property.
        /// </summary>
        public static readonly DependencyProperty IconSourceProperty
            = DependencyProperty.Register(nameof(IconSource), typeof(string), typeof(Icon), new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (a, b) => (a as Icon).IconSource = b.NewValue as string));

        /// <summary>
        /// Identifies the <see cref="IconName"/> property.
        /// </summary>
        public static readonly DependencyProperty IconNameProperty
            = DependencyProperty.Register(nameof(IconName), typeof(string), typeof(Icon), new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (a, b) => (a as Icon).IconName = b.NewValue as string));

        /// <summary>
        /// Identifies the <see cref="Source"/> property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(ImageSource), typeof(Icon));

        /// <summary>
        /// Identifies the <see cref="IconState"/> property.
        /// </summary>
        public static readonly DependencyProperty IconStateProperty = DependencyProperty.Register(nameof(IconState), typeof(IconState), typeof(Icon), new(IconState.Available));

        /// <summary>
        /// Identifies the <see cref="Dimension"/> property.
        /// </summary>
        public static readonly DependencyProperty DimensionProperty
            = DependencyProperty.Register(nameof(Dimension), typeof(IconDimension), typeof(Icon), new FrameworkPropertyMetadata(
                IconDimension.Size24x24,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (a, b) => (a as Icon).Dimension = (IconDimension)b.NewValue));

        /// <summary>
        /// Identifies the <see cref="DimensionWidth"/> property.
        /// </summary>
        public static readonly DependencyProperty DimensionWidthProperty = DependencyProperty.Register(nameof(DimensionWidth), typeof(double), typeof(Icon), new(24.0));

        /// <summary>
        /// Identifies the <see cref="DimensionCanvasWidth"/> property.
        /// </summary>
        public static readonly DependencyProperty DimensionCanvasWidthProperty = DependencyProperty.Register(nameof(DimensionCanvasWidth), typeof(double), typeof(Icon), new(26.0));

        /// <summary>
        /// Identifies the <see cref="DimensionMargin"/> property.
        /// </summary>
        public static readonly DependencyProperty DimensionMarginProperty = DependencyProperty.Register(nameof(DimensionMargin), typeof(double), typeof(Icon), new(1.0));

        /// <summary>
        /// Identifies the <see cref="MaskOpacity"/> property.
        /// </summary>
        public static readonly DependencyProperty MaskOpacityProperty = DependencyProperty.Register(nameof(MaskOpacity), typeof(double), typeof(Icon), new(1.0));

        /// <summary>
        /// Identifies the <see cref="MaskColour"/> property.
        /// </summary>
        public static readonly DependencyProperty MaskColourProperty = DependencyProperty.Register(nameof(MaskColour), typeof(Brush), typeof(Icon));

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        public string IconSource {
            get => this.GetValue(IconSourceProperty) as string;
            set {
                this.SetValue(IconSourceProperty, value);
                this.NotifyPropertyChanged();
                this.TrySetImage();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string IconName {
            get => this.GetValue(IconNameProperty) as string;
            set {
                this.SetValue(IconNameProperty, value);
                this.NotifyPropertyChanged();
                this.TrySetImage();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IconState IconState {
            get => (IconState)this.GetValue(IconStateProperty);
            set {
                this.SetValue(IconStateProperty, value);
                this.SetState();
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
                this.SetDimensions();
                this.NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ImageSource Source => this.GetValue(SourceProperty) as ImageSource;

        /// <summary>
        /// 
        /// </summary>
        public double DimensionWidth => (double)this.GetValue(DimensionWidthProperty);

        /// <summary>
        /// 
        /// </summary>
        public double DimensionCanvasWidth => (double)this.GetValue(DimensionCanvasWidthProperty);

        /// <summary>
        /// 
        /// </summary>
        public double DimensionMargin => (double)this.GetValue(DimensionMarginProperty);

        /// <summary>
        /// 
        /// </summary>
        protected double MaskOpacity => (double)this.GetValue(MaskOpacityProperty);

        /// <summary>
        /// 
        /// </summary>
        protected Brush MaskColour => this.GetValue(MaskColourProperty) as Brush;

        public Icon() {
            this.DataContext = this;
            this.InitializeComponent();
        }

        private void TrySetImage() {

            // Do nothing if name is not valid
            if (string.IsNullOrEmpty(this.IconName)) {
                return;
            }

            // Do nothing if source is not valid
            if (string.IsNullOrEmpty(this.IconSource)) {
                return;
            }

            // If resource handler is missing for some reason
            if (App.ResourceHandler is null) {
                try {
                    this.SetValue(SourceProperty, new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/no_icon.png")));
                    this.NotifyPropertyChanged(nameof(this.Source));
                } catch { }
                return;
            }

            // do nothing if icon is invalid
            if (!App.ResourceHandler.HasIcon(this.IconSource, this.IconName)) {
                return;
            }

            // Set source
            this.SetValue(SourceProperty, App.ResourceHandler.GetIcon(this.IconSource, this.IconName));
            this.NotifyPropertyChanged(nameof(this.Source));

        }

        private void SetDimensions() {
            switch (this.Dimension) {
                case IconDimension.Size24x24:
                    this.SetValue(DimensionWidthProperty, 24.0);
                    this.SetValue(DimensionCanvasWidthProperty, 26.0);
                    this.SetValue(DimensionMarginProperty, 1.0);
                    break;
                case IconDimension.Size32x32:
                    this.SetValue(DimensionWidthProperty, 32.0);
                    this.SetValue(DimensionCanvasWidthProperty, 34.0);
                    this.SetValue(DimensionMarginProperty, 1.0);
                    break;
                case IconDimension.Size36x36:
                    this.SetValue(DimensionWidthProperty, 36.0);
                    this.SetValue(DimensionCanvasWidthProperty, 40.0);
                    this.SetValue(DimensionMarginProperty, 2.0);
                    break;
                case IconDimension.Size48x48:
                    this.SetValue(DimensionWidthProperty, 48.0);
                    this.SetValue(DimensionCanvasWidthProperty, 52.0);
                    this.SetValue(DimensionMarginProperty, 2.0);
                    break;
                case IconDimension.Size64x64:
                    this.SetValue(DimensionWidthProperty, 64.0);
                    this.SetValue(DimensionCanvasWidthProperty, 68.0);
                    this.SetValue(DimensionMarginProperty, 2.0);
                    break;
                default:
                    break;
            }
            this.NotifyPropertyChanged(nameof(this.DimensionWidth));
            this.NotifyPropertyChanged(nameof(this.DimensionMargin));
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void SetState() {
            switch (this.IconState) {
                case IconState.Active:
                    this.SetValue(MaskOpacityProperty, 0.0);
                    this.SetValue(MaskColourProperty, Brushes.White);
                    break;
                case IconState.Available:
                    this.SetValue(MaskColourProperty, Brushes.DarkGray);
                    this.SetValue(MaskOpacityProperty, 0.6);
                    break;
                case IconState.Disabled:
                    this.SetValue(MaskColourProperty, Brushes.Black);
                    this.SetValue(MaskOpacityProperty, 0.75);
                    break;
                default:
                    break;
            }
            this.NotifyPropertyChanged(nameof(this.MaskColour));
            this.NotifyPropertyChanged(nameof(this.MaskOpacity));
        }

        protected override void OnMouseEnter(MouseEventArgs e) {
            base.OnMouseEnter(e);
            if (this.IconState is IconState.Available) {
                this.SetValue(MaskOpacityProperty, 0.075);
                this.NotifyPropertyChanged(nameof(this.MaskColour));
                this.NotifyPropertyChanged(nameof(this.MaskOpacity));
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e) {
            base.OnMouseLeave(e);
            if (this.IconState is IconState.Available) {
                this.SetValue(MaskOpacityProperty, 0.6);
                this.NotifyPropertyChanged(nameof(this.MaskColour));
                this.NotifyPropertyChanged(nameof(this.MaskOpacity));
            }
        }

    }

}
