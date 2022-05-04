using Battlegrounds.Locale;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BattlegroundsApp.Utilities.Converters;

public class StringToLocaleKeyConverter : DependencyObject, IValueConverter {
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture) => new LocaleKey(value?.ToString() ?? string.Empty);

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}
