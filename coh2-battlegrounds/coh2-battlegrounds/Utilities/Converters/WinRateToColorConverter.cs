using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BattlegroundsApp.Utilities.Converters;

public class WinRateToColorConverter : IValueConverter {

    private readonly Color _sub50Color = Color.FromRgb(173, 74, 74);
    private readonly Color _50Color = Colors.Orange;
    private readonly Color _above50Color = Color.FromRgb(72, 150, 53);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        
        if (value is double) {

            if ((double)value < 50.0) {
                return new SolidColorBrush(_sub50Color);
            } else if ((double)value == 50.0) {
                return new SolidColorBrush(_50Color);
            } else {
                return new SolidColorBrush(_above50Color);
            }

        }

        return Binding.DoNothing;

    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
