using System.Globalization;
using System.Windows;
using System.Windows.Data;

using Battlegrounds.Models.Playing;
using Battlegrounds.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Battlegrounds.Converters;

public sealed class FactionIdToNameConverter : AbstractAppDependable, IMultiValueConverter {

    private IGameService GameService => ServiceProvider.GetRequiredService<IGameService>();

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
        if (values is [string factionId, string gameId]) {
            Game game = GameService.GetGame(gameId);
            return game.GetFactionName(factionId) ?? DependencyProperty.UnsetValue;
        } else if (values is [string factionId2, Game game2]) {
            return game2.GetFactionName(factionId2) ?? DependencyProperty.UnsetValue;
        }
        return DependencyProperty.UnsetValue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
