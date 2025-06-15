using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Blueprints.Extensions;
using Battlegrounds.Models.Playing;

using Serilog;

namespace Battlegrounds.Converters;

public sealed class BlueprintIconConverter : IValueConverter {

    private static readonly ILogger logger = Log.ForContext<BlueprintIconConverter>();
    private readonly ImageSourceConverter imageSourceConverter = new();

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is Blueprint blueprint && blueprint.TryGetExtension<UIExtension>(out var uiExtension)) {
            string path = $"Assets/Factions/{parameter}/icons/{uiExtension.IconName}.png";
            if (!File.Exists(path)) {
                logger.Warning("Icon file not found for blueprint {Blueprint}: {Path}", blueprint.Id, path);
                return DependencyProperty.UnsetValue; // TODO: Use a fallback icon
            }
            return imageSourceConverter.ConvertFromString($"pack://siteoforigin:,,,/{path}");
        }
        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}

public sealed class BlueprintIconMultiBindingConverter : IMultiValueConverter {
    private readonly BlueprintIconConverter _converter = new();
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
        if (values.Length != 2) {
            throw new ArgumentException("Expected two values: blueprint and gameID.", nameof(values));
        }
        string gameId = values[1] is string s ? s : string.Empty;
        if (string.IsNullOrEmpty(gameId)) {
            if (values[1] is Game g)
                gameId = g.Id;
            else
                return DependencyProperty.UnsetValue; // Handle case where second value is not a string or Game
        }
        if (values[0] is not Blueprint blueprint) {
            return DependencyProperty.UnsetValue; // Handle case where first value is not a Blueprint
        }
        return _converter.Convert(blueprint, targetType, gameId, culture);
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
