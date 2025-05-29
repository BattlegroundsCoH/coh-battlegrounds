using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Battlegrounds.Converters;

public sealed class FactionIconConverter : IValueConverter {

    private readonly ImageSourceConverter imageSourceConverter = new();

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) 
        => imageSourceConverter.ConvertFromString($"pack://siteoforigin:,,,/Assets/Factions/{parameter}_{value}.png");

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}

public sealed class FactionIconMultiBindingConverter : IMultiValueConverter {

    private readonly FactionIconConverter _converter = new();

    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
        if (values.Length != 2) {             
            throw new ArgumentException("Expected two values: faction and gameID.", nameof(values));
        }
        if (values[0] is not string faction || values[1] is not string gameId) {
            return Binding.DoNothing; // Handle invalid input gracefully
        }
        return _converter.Convert(faction, targetType, gameId, culture);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
