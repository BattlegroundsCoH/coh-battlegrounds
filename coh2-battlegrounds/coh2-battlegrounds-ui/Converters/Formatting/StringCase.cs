using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Battlegrounds.UI.Converters.Formatting;

public class StringCase : IValueConverter {

    public CharacterCasing Case { get; set; }

    public StringCase() {
        Case = CharacterCasing.Upper;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch {
        string str => Case switch {
            CharacterCasing.Lower => str.ToLower(),
            CharacterCasing.Normal => str,
            CharacterCasing.Upper => str.ToUpper(),
            _ => str,
        },
        _ => string.Empty
    };

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        return null;
    }

}
