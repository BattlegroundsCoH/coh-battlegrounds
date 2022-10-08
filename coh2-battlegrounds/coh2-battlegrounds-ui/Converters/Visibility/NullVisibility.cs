using System;
using System.Globalization;
using System.Windows.Data;

namespace Battlegrounds.UI.Converters.Visibility;

/// <summary>
/// Class to convert a null value into an appropriate visibility value.
/// </summary>
public sealed class NullVisibility : IValueConverter {

    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) {
        if (value is null)
            return System.Windows.Visibility.Collapsed;
        else
            return System.Windows.Visibility.Visible;
    }

    public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) {
        return null;
    }

}
