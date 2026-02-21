using System.Globalization;
using System.Windows.Data;

namespace Battlegrounds.Converters;

public sealed class ReadyStateConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is bool isReady) {
            return isReady ? "Ready" : "Unready";
        }
        return "Unready";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
