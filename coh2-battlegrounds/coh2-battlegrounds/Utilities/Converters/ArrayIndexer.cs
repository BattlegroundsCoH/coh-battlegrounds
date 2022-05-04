using System;
using System.Globalization;
using System.Windows.Data;

namespace BattlegroundsApp.Utilities.Converters;

public class ArrayIndexer : IValueConverter {

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {

        // Get index
        if (!int.TryParse(parameter.ToString(), out int index)) {
            throw new ArgumentException("Expected parseable index but received unparsable value.", nameof(parameter));
        }

        // Get array
        Array? array = value as Array;

        // Validate range
        if (array is not null && (index < 0 || index >= array.Length)) {

            // Throw exception
            throw new ArgumentOutOfRangeException(nameof(parameter), "Index was out of range.");

        }

        // Return value at index
        return array?.GetValue(index);


    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();

}
