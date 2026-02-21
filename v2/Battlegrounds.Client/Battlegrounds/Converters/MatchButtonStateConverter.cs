using System.Globalization;
using System.Windows.Data;

namespace Battlegrounds.Converters;

public sealed class MatchButtonStateConverter : IMultiValueConverter {

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
        if (values.Length < 4) {
            return "CanStart"; // Default state
        }

        bool isMatchStarting = false;
        bool isWaitingForMatchOver = false;
        bool canStartMatch = false;
        bool isHost = false;

        if (values[0] is bool starting) 
            isMatchStarting = starting;

        if (values[1] is bool waiting) 
            isWaitingForMatchOver = waiting;

        if (values[2] is bool canStart) 
            canStartMatch = canStart;

        if (values[3] is bool host)
            isHost = host;

        // Non-host players see different states
        if (!isHost) {
            if (isMatchStarting)
                return "Starting";
            else if (isWaitingForMatchOver)
                return "Waiting";
            // For non-host, we'll handle Ready/Unready in a separate binding
            return "Ready";
        }

        // Host logic
        if (isMatchStarting)
            return "Starting";
        else if (isWaitingForMatchOver)
            return "Waiting";
        else if (canStartMatch)
            return "CanStart";
        else
            return "CannotStart";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}

public sealed class ButtonStateToContentConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is string state) {
            return state switch {
                "Starting" => "STARTING MATCH...",
                "Waiting" => "MATCH IN PROGRESS",
                "CanStart" => "START MATCH",
                "CannotStart" => "CANNOT START",
                "Ready" => "READY",
                "Unready" => "NOT READY",
                _ => "START MATCH"
            };
        }

        return "START MATCH";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
