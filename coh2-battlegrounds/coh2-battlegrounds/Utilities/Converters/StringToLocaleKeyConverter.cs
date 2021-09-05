using Battlegrounds.Locale;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace BattlegroundsApp.Utilities.Converters {
    public class StringToLocaleKeyConverter : DependencyObject, IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

            return new LocaleKey(value.ToString());

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
}
