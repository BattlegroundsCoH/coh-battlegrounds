using System.Globalization;
using System.Windows;
using System.Windows.Data;

using Battlegrounds.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Battlegrounds.Converters;

public sealed class GameIdToGameNameConverter : AbstractAppDependable, IValueConverter {

    private IGameService GameService => ServiceProvider.GetRequiredService<IGameService>();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is string gameName) {
            return GameService.GetGame(gameName).GameName;
        }
        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
