using System.Globalization;
using System.Windows.Data;

namespace Battlegrounds.Converters;

public sealed class DurationConverter : IValueConverter {

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        
        if (value is not TimeSpan duration)
            return "0m";

        if (duration.TotalMinutes < 1)
            return $"{(int)duration.TotalSeconds}s";
        if (duration.TotalHours < 1)
            return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";
        
        return $"{(int)duration.TotalHours}h {duration.Minutes}m";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
