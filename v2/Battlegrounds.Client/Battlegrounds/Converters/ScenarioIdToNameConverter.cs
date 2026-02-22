using System.Globalization;
using System.Windows;
using System.Windows.Data;

using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Battlegrounds.Converters;

public sealed class ScenarioIdToNameConverter : AbstractAppDependable, IMultiValueConverter {

    private IGameService GameService => ServiceProvider.GetRequiredService<IGameService>();

    private IGameMapService GameMapService => ServiceProvider.GetRequiredService<IGameMapService>();

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
        if (values is [string scenarioId, string gameId]) {
            Game game = GameService.GetGame(gameId);
            if (GameMapService.TryGetMapByScenarioName(game, scenarioId, out Map? map)) {
                return map.Name;
            }
            return scenarioId;
        } else if (values is [string scenarioId2, Game game2]) {
            if (GameMapService.TryGetMapByScenarioName(game2, scenarioId2, out Map? map)) {
                return map.Name;
            }
            return scenarioId2;
        }
        return DependencyProperty.UnsetValue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
