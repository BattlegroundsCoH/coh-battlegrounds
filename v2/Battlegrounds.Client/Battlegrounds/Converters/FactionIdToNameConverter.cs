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
        if (values is [_, ""]) {
            return DependencyProperty.UnsetValue;
        } else if (values is [string factionId, string gameId]) {
            Game game = GameService.GetGame(gameId);
            if (game.TryGetFactionName(factionId, out string? factionName)) {
                return factionName;
            }
            return factionId;
        } else if (values is [string factionId2, Game game2]) {
            if (game2.TryGetFactionName(factionId2, out string? factionName2)) {
                return factionName2;
            }
            return factionId2;
        }
        return DependencyProperty.UnsetValue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
