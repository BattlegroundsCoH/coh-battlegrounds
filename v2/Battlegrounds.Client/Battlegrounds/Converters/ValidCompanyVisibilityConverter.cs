using System.Globalization;
using System.Windows.Data;

using Battlegrounds.ViewModels.LobbyHelpers;

namespace Battlegrounds.Converters;

/// <summary>
/// A converter that converts a <see cref="PickableCompany"/> to a <see cref="System.Windows.Visibility"/> value.
/// </summary>
public sealed class ValidCompanyVisibilityConverter : IValueConverter {

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch {
        null or PickableCompany { IsNone: true } or PickableCompany { GenerateRandom: true } => System.Windows.Visibility.Collapsed,
        _ => System.Windows.Visibility.Visible,
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
