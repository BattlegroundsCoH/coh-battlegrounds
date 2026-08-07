using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Battlegrounds.Converters;

public sealed class NullVisibilityConverter : IValueConverter {
    
    public bool IsInverted { get; set; } = false;

    // A blank string is "nothing to show" just as much as a null is — without this a missing
    // description or email still reserves a line box. For non-string values (e.g. view-model
    // references), only a genuine null counts as "nothing to show".
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        bool isNothingToShow = value switch {
            null => true,
            string s => string.IsNullOrWhiteSpace(s),
            _ => false,
        };
        return isNothingToShow
            ? (IsInverted ? Visibility.Visible : Visibility.Collapsed)
            : (IsInverted ? Visibility.Collapsed : Visibility.Visible);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
