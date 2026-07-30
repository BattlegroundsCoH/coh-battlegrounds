using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Battlegrounds.Controls;

namespace Battlegrounds.Views.Dev;

/// <summary>
/// A colour chip with its token name and hex, for the style gallery.
/// </summary>
/// <remarks>
/// Built in code rather than XAML so the gallery can list a token per line instead of
/// repeating a dozen lines of border-and-label markup for each one. The hex is read back
/// off the brush rather than typed alongside it, so a swatch cannot drift out of step with
/// the value it is showing.
/// </remarks>
public sealed class Swatch : Control {

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(Swatch),
            new FrameworkPropertyMetadata(string.Empty));

    public static readonly DependencyProperty FillProperty =
        DependencyProperty.Register(
            nameof(Fill), typeof(Brush), typeof(Swatch),
            new FrameworkPropertyMetadata(Brushes.Transparent));

    public string Label {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public Brush Fill {
        get => (Brush)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    static Swatch() {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Swatch), new FrameworkPropertyMetadata(typeof(Swatch)));
    }

    /// <summary>The brush's colour as #AARRGGBB, or a dash if it is not a solid colour.</summary>
    public string Hex => Fill is SolidColorBrush solid ? solid.Color.ToString() : "—";

}
