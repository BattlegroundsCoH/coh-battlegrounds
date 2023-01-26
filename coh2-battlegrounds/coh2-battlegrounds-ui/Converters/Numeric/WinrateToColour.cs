using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Battlegrounds.UI.Converters.Numeric;

/// <summary>
/// Class to converter from a winrate to a colour.
/// </summary>
public sealed class WinrateToColour : IValueConverter { // These colours should probably be settable (as input)

    private readonly SolidColorBrush _below50ColourBrush = new SolidColorBrush(Color.FromRgb(173, 74, 74));
    private readonly SolidColorBrush _at50ColourBrush = new SolidColorBrush(Colors.Orange);
    private readonly SolidColorBrush _above50ColourBrush = new SolidColorBrush(Color.FromRgb(72, 150, 53));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value switch {
            double d when d < 50.0 => _below50ColourBrush,
            double d when d > 50.0 => _above50ColourBrush,
            _ => _at50ColourBrush
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
