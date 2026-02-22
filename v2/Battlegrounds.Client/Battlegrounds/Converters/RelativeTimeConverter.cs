using System.Globalization;
using System.Windows.Data;

namespace Battlegrounds.Converters;

public sealed class RelativeTimeConverter : IValueConverter {

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        
        if (value is not DateTime timestamp)
            return "Unknown";

        var timeSpan = DateTime.Now - timestamp;

        if (timeSpan.TotalMinutes < 1)
            return "Just now";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes}m ago";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours}h ago";
        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays}d ago";
        
        return timestamp.ToString("MMM d, yyyy");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
