using System;
using System.Globalization;
using System.Windows.Data;

namespace Battlegrounds.UI.Converters.Numeric;

public sealed class DoubleToRating : IValueConverter {

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value switch {
            double d when d < BattlegroundsDefine.COMPANY_RATING_D => "D",
            double d when d < BattlegroundsDefine.COMPANY_RATING_C => "C",
            double d when d < BattlegroundsDefine.COMPANY_RATING_B => "B",
            _ => "A"
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
