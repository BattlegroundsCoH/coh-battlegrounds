using System.Globalization;
using System.Windows.Data;

using Battlegrounds.Models.Blueprints;

using Battlegrounds.Models.Blueprints.Extensions;

namespace Battlegrounds.Converters;

public sealed class BlueprintHelpTextConverter : IValueConverter {

    public const string EmptyIfNotFound = "EmptyStringIfNotFound";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is Blueprint blueprint && blueprint.TryGetExtension(out UIExtension? uiExtension)) {
            if (parameter is EmptyIfNotFound) {
                return uiExtension.HelpText.AsStringOrEmpty();
            }
            return uiExtension.HelpText.AsString();
        }
        return "Unknown Blueprint";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
