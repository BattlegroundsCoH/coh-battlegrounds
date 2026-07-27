using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Battlegrounds.Converters;

public sealed class NullVisibilityConverter : IValueConverter {
    
    public bool IsInverted { get; set; } = false;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch {
        // A blank string is "nothing to show" just as much as a null is — without this a missing
        // description or email still reserves a line box.
        null => IsInverted ? Visibility.Visible : Visibility.Collapsed,
        string s when string.IsNullOrWhiteSpace(s) => IsInverted ? Visibility.Visible : Visibility.Collapsed,
        _ => IsInverted ? Visibility.Collapsed : Visibility.Visible
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
