using System.Windows;
using System.Windows.Controls;

namespace Battlegrounds.Controls;

/// <summary>
/// A large gold figure above a small tracked caption — "248 / TOTAL MATCHES".
/// </summary>
/// <remarks>
/// Used in the home dashboard's stat row. <see cref="Value"/> is a string rather than a
/// number because the tiles show formatted values ("68%", "F. Marshal") as often as counts.
/// </remarks>
public class StatTile : Control {

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value), typeof(string), typeof(StatTile),
            new FrameworkPropertyMetadata(string.Empty));

    /// <summary>Caption below the figure. Authored uppercase.</summary>
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(StatTile),
            new FrameworkPropertyMetadata(string.Empty));

    public string Value {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Label {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    static StatTile() {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(StatTile), new FrameworkPropertyMetadata(typeof(StatTile)));
    }

}
