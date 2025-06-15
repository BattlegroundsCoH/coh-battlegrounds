using System.Globalization;
using System.Windows.Data;

namespace Battlegrounds.Converters;

public sealed class LockStateConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return (bool)value ? "Unlock Slot" : "Lock Slot";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
