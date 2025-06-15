using System.Globalization;
using System.Windows.Data;

namespace Battlegrounds.Converters;

public sealed class InverseBooleanConverter : IValueConverter {

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is bool boolValue ? !boolValue : value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is bool boolValue ? !boolValue : value;
    }

}
