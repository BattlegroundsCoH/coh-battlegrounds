using System.Globalization;
using System.Windows.Data;

namespace Battlegrounds.Converters;

public sealed class BoolToItemType : IValueConverter {

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch {
        true => "Team Weapon",
        _ => "Weapon Pickup"
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
