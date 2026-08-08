using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Battlegrounds.Converters;

/// <summary>
/// Derives a height from a width and a <c>W:H</c> ratio, so a box holds its aspect ratio however
/// wide its container makes it.
/// </summary>
/// <remarks>
/// WPF has no aspect-ratio panel and no way to express one in markup. The codebase's existing
/// idiom for the square case is <c>Height="{Binding ActualWidth, RelativeSource={RelativeSource
/// Self}}"</c> (the lobby minimaps); this is that same trick for a ratio other than 1:1.
/// <para>
/// It exists because news cover art is uploaded at a fixed ratio while the tile holding it is
/// fluid. A fixed pixel height only matched at one window width; maximised, the box grew wide but
/// not tall and <c>UniformToFill</c> cropped the artwork to a letterbox strip.
/// </para>
/// <para>
/// Binding a height to the element's own <c>ActualWidth</c> does not loop: the width comes from the
/// parent, and nothing derives the width back from the height.
/// </para>
/// </remarks>
/// <example><code>
/// Height="{Binding ActualWidth, RelativeSource={RelativeSource Self},
///          Converter={StaticResource AspectRatio}, ConverterParameter=16:9}"
/// </code></example>
public sealed class AspectRatioConverter : IValueConverter {

    /// <summary>
    /// Scales a width by the ratio given as the converter parameter.
    /// </summary>
    /// <param name="value">The width, normally the element's own <c>ActualWidth</c>.</param>
    /// <param name="parameter">The aspect ratio as <c>W:H</c>, for example <c>16:9</c>.</param>
    /// <returns>The height, or <see cref="DependencyProperty.UnsetValue"/> before layout has given
    /// the element a usable width — returning 0 there would collapse the box on the first pass.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {

        if (value is not double width || double.IsNaN(width) || double.IsInfinity(width) || width <= 0) {
            return DependencyProperty.UnsetValue;
        }

        if (!TryParseRatio(parameter, out double ratio)) {
            return DependencyProperty.UnsetValue;
        }

        return width * ratio;

    }

    /// <summary>
    /// Parses a <c>W:H</c> parameter into a height-over-width multiplier.
    /// </summary>
    /// <remarks>Invariant culture, because the parameter is authored in XAML rather than entered by
    /// a user — a decimal comma on a German machine would otherwise reinterpret it.</remarks>
    private static bool TryParseRatio(object? parameter, out double ratio) {

        ratio = 0;

        if (parameter?.ToString()?.Split(':') is not [string widthPart, string heightPart]) {
            return false;
        }

        if (!double.TryParse(widthPart, NumberStyles.Float, CultureInfo.InvariantCulture, out double w)
            || !double.TryParse(heightPart, NumberStyles.Float, CultureInfo.InvariantCulture, out double h)
            || w <= 0 || h <= 0) {
            return false;
        }

        ratio = h / w;
        return true;

    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(AspectRatioConverter)} is one-way; a height does not imply a width.");

}
