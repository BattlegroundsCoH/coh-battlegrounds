using System.Globalization;
using System.Windows.Data;

namespace Battlegrounds.Converters;

public sealed class LockImageConverter : IValueConverter {

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return (bool)value ? "pack://siteoforigin:,,,/Assets/Misc/unlocked.png" : "pack://siteoforigin:,,,/Assets/Misc/locked.png";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
