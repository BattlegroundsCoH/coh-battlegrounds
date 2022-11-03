using System;
using System.Globalization;
using System.Windows.Data;

namespace Battlegrounds.UI.Converters.Visibility;

public sealed class BooleanToVisibility : IValueConverter {

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value switch {
            true => System.Windows.Visibility.Visible,
            false => System.Windows.Visibility.Hidden,
            _ => System.Windows.Visibility.Collapsed
        };

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return null;
    }

}
