using System.Globalization;
using System.Windows.Data;

namespace Battlegrounds.Converters;

public sealed class JoinableStatusConverter : IValueConverter {

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return value is bool canJoin ? (canJoin ? "Open" : "Full") : "Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
