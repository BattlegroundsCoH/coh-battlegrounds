using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Battlegrounds.Converters;

public sealed class ZeroToVisibilityConverter : IValueConverter {

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch {
        0 => Visibility.Collapsed,
        0.0f => Visibility.Collapsed,
        null => Visibility.Collapsed,
        _ => Visibility.Visible
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
