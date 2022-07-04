using System;
using System.Globalization;
using System.Windows.Data;

namespace BattlegroundsApp.Utilities.Converters;
public class IsGreaterThanConverer : IValueConverter {

    public static readonly IValueConverter Instance = new IsGreaterThanConverer();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

        double doubleValue = System.Convert.ToDouble(value);
        double compareToValue = System.Convert.ToDouble(parameter);

        return doubleValue > compareToValue;

    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {

        throw new NotImplementedException();
    
    }
}

