using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

using Battlegrounds.Locale;

namespace Battlegrounds.UI.Converters.Locale;

public class StringLocaleKey : DependencyObject, IValueConverter {
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture) => new LocaleKey(value?.ToString() ?? string.Empty);

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}
