using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BattlegroundsApp.Utilities.Converters;
public class DoubleToRatingConverter : IValueConverter {
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is double d) { // :)
            return d switch {
                < 0.3 => "D",
                < 0.5 => "C",
                < 0.8 => "B",
                _ => "A"
            };
        }
        return "E";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
