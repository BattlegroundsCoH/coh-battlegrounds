using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Battlegrounds.Converters;

public sealed class RankStarColourConverter : IValueConverter {
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        bool isEarned = (bool)value;
        return isEarned ? Application.Current.Resources["AccentBlueBrush"] : Application.Current.Resources["ForegroundGrayBrush"];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
