using System.Globalization;
using System.Windows.Data;

namespace Battlegrounds.Converters;

public sealed class LockImageConverter : IValueConverter {

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return (bool)value ? "/Resources/Images/unlock_icon.png" : "/Resources/Images/lock_icon.png";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
