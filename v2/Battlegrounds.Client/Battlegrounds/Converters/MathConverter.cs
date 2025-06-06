using System.Globalization;
using System.Windows.Data;
using System.Data;

namespace Battlegrounds.Converters;

public sealed class MathConverter : IValueConverter {

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is null || parameter is null) {
            return value;
        }

        // Replace 'x' with the value in the expression
        string expression = parameter.ToString()?.Replace("x", value.ToString()) ?? string.Empty;
        if (string.IsNullOrEmpty(expression)) {
            return value;
        }

        try {
            DataTable dt = new DataTable();
            var result = dt.Compute(expression, "");
            return System.Convert.ChangeType(result, targetType);
        }
        catch {
            return value;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
