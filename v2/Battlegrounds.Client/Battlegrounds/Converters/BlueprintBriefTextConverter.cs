using System.Globalization;
using System.Windows.Data;

using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Blueprints.Extensions;

namespace Battlegrounds.Converters;

public sealed class BlueprintBriefTextConverter : IValueConverter {

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is Blueprint blueprint && blueprint.TryGetExtension(out UIExtension? uiExtension)) {
            return uiExtension.BriefText.AsString();
        }
        return "Unknown Blueprint";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
