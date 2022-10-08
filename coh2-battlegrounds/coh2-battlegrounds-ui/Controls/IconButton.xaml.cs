using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Battlegrounds.Functional;

namespace Battlegrounds.UI.Controls;

/// <summary>
/// Interaction logic for IconButton.xaml
/// </summary>
public partial class IconButton : UserControl {

    private Brush? m_background;

    public static readonly DependencyProperty ImageWidthProperty =
        DependencyProperty.Register(nameof(ImageWidth), typeof(int), typeof(IconButton),
            new FrameworkPropertyMetadata(30, FrameworkPropertyMetadataOptions.AffectsRender, (a, b) => a.Cast<IconButton>(x => x.ImageWidth = (int)b.NewValue)));

    public int ImageWidth {
        get => (int)this.GetValue(ImageWidthProperty);
        set {
            this.SetCurrentValue(ImageWidthProperty, value);
            this.ImgColumn.Width = new GridLength(value);
            this.SelfImage.Width = value;
        }
    }

    public static readonly DependencyProperty ImageHeightProperty =
        DependencyProperty.Register(nameof(ImageHeight), typeof(int), typeof(IconButton),
            new FrameworkPropertyMetadata(30, FrameworkPropertyMetadataOptions.AffectsRender, (a, b) => a.Cast<IconButton>(x => x.ImageHeight = (int)b.NewValue)));

    public int ImageHeight {
        get => (int)this.GetValue(ImageHeightProperty);
        set {
            this.SetCurrentValue(ImageHeightProperty, value);
            this.SelfImage.Height = value;
        }
    }

    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(nameof(ImageSource), typeof(ImageSource), typeof(IconButton), new PropertyMetadata(null));

    public ImageSource ImageSource {
        get => (ImageSource)this.GetValue(ImageSourceProperty);
        set {
            this.SetCurrentValue(ImageSourceProperty, value);
            this.SelfImage.Source = value;
        }
    }

    public static readonly DependencyProperty HoverColourProperty =
        DependencyProperty.Register(nameof(HoverColour), typeof(Brush), typeof(IconButton),
            new FrameworkPropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#536375")),
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

    [Bindable(true), Category("Appearance")]
    public Brush? HoverColour {
        get => (SolidColorBrush)this.GetValue(HoverColourProperty);
        set => this.SetCurrentValue(HoverColourProperty, value);
    }

    public static readonly DependencyProperty ImageTooltipProperty =
        DependencyProperty.Register(nameof(ImageTooltip), typeof(object), typeof(IconButton), new PropertyMetadata(null));

    public object? ImageTooltip {
        get => this.GetValue(ImageTooltipProperty);
        set {
            this.SetCurrentValue(ImageTooltipProperty, value);
            this.SelfImage.ToolTip = value;
        }
    }

    public static readonly DependencyProperty ImageVisibilityProperty =
        DependencyProperty.Register(nameof(ImageVisibility), typeof(Visibility), typeof(IconButton), new PropertyMetadata(Visibility.Visible));

    public Visibility ImageVisibility {
        get => (Visibility)this.GetValue(ImageVisibilityProperty);
        set {
            this.SetCurrentValue(ImageVisibilityProperty, value);
            this.SelfImage.Visibility = value;
        }
    }

    public static readonly DependencyProperty DisabledColourProperty =
        DependencyProperty.Register(nameof(DisabledColour), typeof(SolidColorBrush), typeof(IconButton),
            new PropertyMetadata(DefaultDisabledBrush(Application.Current.FindResource("BackgroundLightBlueBrush") is SolidColorBrush s ? s : Brushes.Pink)));

    private static SolidColorBrush DefaultDisabledBrush(SolidColorBrush brush) {
        return new SolidColorBrush(brush.Color * 0.9f);
    }

    public SolidColorBrush DisabledColour {
        get {
            if (this.GetValue(BackgroundProperty) is not SolidColorBrush brush) {
                return Brushes.Pink;
            }
            return DefaultDisabledBrush(brush);
        }
    }

    public static readonly DependencyProperty TextVerticalAlignmentProperty =
        DependencyProperty.Register(nameof(TextVerticalAlignment), typeof(VerticalAlignment), typeof(IconButton), new PropertyMetadata(VerticalAlignment.Center));

    public VerticalAlignment TextVerticalAlignment {
        get => (VerticalAlignment)this.GetValue(TextVerticalAlignmentProperty);
        set {
            this.SetCurrentValue(TextVerticalAlignmentProperty, value);
            this.SelfContent.VerticalAlignment = value;
        }
    }

    public static readonly DependencyProperty TextHorizontalAlignmentProperty =
        DependencyProperty.Register(nameof(TextHorizontalAlignment), typeof(HorizontalAlignment), typeof(IconButton), new PropertyMetadata(HorizontalAlignment.Center));

    public HorizontalAlignment TextHorizontalAlignment {
        get => (HorizontalAlignment)this.GetValue(TextHorizontalAlignmentProperty);
        set {
            this.SetCurrentValue(TextHorizontalAlignmentProperty, value);
            this.SelfContent.HorizontalAlignment = value;
        }
    }

    public IconButton() {
        InitializeComponent();
    }

    protected override void OnContentChanged(object oldContent, object newContent) {
        base.OnContentChanged(oldContent, newContent);
        this.SelfContent.Content = newContent;
    }

    private void GridContainer_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
        this.m_background ??= this.Background;
        if (this.IsEnabled)
            this.Background = this.HoverColour;

    }

    private void GridContainer_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
        this.m_background ??= this.Background;
        if (this.IsEnabled)
            this.Background = this.m_background;
    }

    private void UserControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
        this.m_background ??= this.Background;
        this.Background = e.NewValue is true ? this.m_background : this.DisabledColour;
    }

}
