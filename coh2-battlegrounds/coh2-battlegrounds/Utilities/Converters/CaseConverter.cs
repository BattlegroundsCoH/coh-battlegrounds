using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace BattlegroundsApp.Utilities.Converters;

public class CaseConverter : IValueConverter {

    public CharacterCasing Case { get; set; }
    public CaseConverter() {
        Case = CharacterCasing.Upper;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

        if (value is string str) {
            return Case switch {
                CharacterCasing.Lower => str.ToLower(),
                CharacterCasing.Normal => str,
                CharacterCasing.Upper => str.ToUpper(),
                _ => str,
            };
        }

        return string.Empty;

    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return null;
    }

}
