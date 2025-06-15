using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Battlegrounds.Converters;

public sealed class NullVisibilityConverter : IValueConverter {
    
    public bool IsInverted { get; set; } = false;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch {
        null => IsInverted ? Visibility.Visible : Visibility.Collapsed,
        _ => IsInverted ? Visibility.Collapsed : Visibility.Visible
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
