using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

using Battlegrounds.Functional;

namespace Battlegrounds.UI.Controls;

// https://kmatyaszek.github.io/2020/04/16/wpf-image-button.html
// ^ That man saved my sanity - WPF plain sucks

public class IconButton : Button {

    static IconButton() {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(IconButton), new FrameworkPropertyMetadata(typeof(IconButton)));
    }

    public static readonly DependencyProperty ImageWidthProperty =
        DependencyProperty.Register(nameof(ImageWidth), typeof(int), typeof(IconButton),
            new FrameworkPropertyMetadata(30, FrameworkPropertyMetadataOptions.AffectsRender, (a, b) => a.Cast<IconButton>(x => x.ImageWidth = (int)b.NewValue)));

    public int ImageWidth {
        get => (int)this.GetValue(ImageWidthProperty);
        set => this.SetValue(ImageWidthProperty, value);
    }

    public static readonly DependencyProperty ImageHeightProperty =
        DependencyProperty.Register(nameof(ImageHeight), typeof(int), typeof(IconButton),
            new FrameworkPropertyMetadata(30, FrameworkPropertyMetadataOptions.AffectsRender, (a, b) => a.Cast<IconButton>(x => x.ImageHeight = (int)b.NewValue)));

    public int ImageHeight {
        get => (int)this.GetValue(ImageHeightProperty);
        set => this.SetValue(ImageHeightProperty, value);
    }

    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(nameof(ImageSource), typeof(ImageSource), typeof(IconButton), new PropertyMetadata(null));

    public ImageSource ImageSource {
        get => (ImageSource)this.GetValue(ImageSourceProperty);
        set => this.SetValue(ImageSourceProperty, value);
    }

    public static readonly DependencyProperty HoverColourProperty =
        DependencyProperty.Register(nameof(HoverColour), typeof(Brush), typeof(IconButton),
            new FrameworkPropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#536375")),
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

    [Bindable(true), Category("Appearance")]
    public Brush? HoverColour {
        get => (SolidColorBrush)this.GetValue(HoverColourProperty);
        set => this.SetValue(HoverColourProperty, value);
    }

    public static readonly DependencyProperty ImageTooltipProperty =
        DependencyProperty.Register(nameof(ImageTooltip), typeof(object), typeof(IconButton), new PropertyMetadata(null));

    public object? ImageTooltip {
        get => this.GetValue(ImageTooltipProperty);
        set => this.SetCurrentValue(ImageTooltipProperty, value);
    }

    public static readonly DependencyProperty ImageVisibilityProperty =
        DependencyProperty.Register(nameof(ImageVisibility), typeof(Visibility), typeof(IconButton), new PropertyMetadata(Visibility.Visible));

    public Visibility ImageVisibility {
        get => (Visibility)this.GetValue(ImageVisibilityProperty);
        set => this.SetValue(ImageVisibilityProperty, value);
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

}
