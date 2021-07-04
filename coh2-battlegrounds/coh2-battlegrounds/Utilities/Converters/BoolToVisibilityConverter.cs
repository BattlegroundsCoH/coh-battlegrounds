using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BattlegroundsApp.Utilities.Converters {
    public class BoolToVisibilityConverter : IValueConverter {

        public string Visibility { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

            if (value is bool trueFalseValue) {

                if (trueFalseValue == true) {

                    Visibility = "Visible";
                    return Visibility;

                } else {

                    Visibility = "Hidden";
                    return Visibility;

                }

            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }

    }
}
