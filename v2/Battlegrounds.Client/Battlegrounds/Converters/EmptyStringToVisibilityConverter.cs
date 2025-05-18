using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Battlegrounds.Converters;

public sealed class EmptyStringToVisibilityConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return string.IsNullOrEmpty((string)value) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
