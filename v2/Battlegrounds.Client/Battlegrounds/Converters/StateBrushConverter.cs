using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Battlegrounds.Converters;

/// <summary>
/// Picks the brush for a control's current interaction state.
/// </summary>
/// <remarks>
/// <para>
/// Bound as a <see cref="MultiBinding"/> from inside a control template, in this order:
/// </para>
/// <list type="number">
///   <item><description><c>IsMouseOver</c> (bool)</description></item>
///   <item><description><c>IsPressed</c> (bool)</description></item>
///   <item><description>the resting brush</description></item>
///   <item><description>the hover brush, or null to reuse the resting one</description></item>
///   <item><description>the pressed brush, or null to reuse the hover one</description></item>
/// </list>
/// <para>
/// This exists because <c>Setter.Value</c> cannot hold a binding in WPF, which rules out
/// the obvious "trigger sets Background to an attached property" approach and is what
/// normally forces a separate <c>ControlTemplate</c> per visual variant. See
/// <see cref="Controls.ControlAssist"/>.
/// </para>
/// <para>
/// Pressed beats hover, since a control being pressed is always also hovered.
/// </para>
/// </remarks>
public sealed class StateBrushConverter : IMultiValueConverter {

    public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture) {

        if (values.Length < 5) {
            return DependencyProperty.UnsetValue;
        }

        var isMouseOver = values[0] as bool? ?? false;
        var isPressed = values[1] as bool? ?? false;

        var resting = values[2] as Brush;
        var hover = values[3] as Brush ?? resting;
        var pressed = values[4] as Brush ?? hover;

        if (isPressed) {
            return pressed;
        }

        return isMouseOver ? hover : resting;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(StateBrushConverter)} is one-way.");

}
