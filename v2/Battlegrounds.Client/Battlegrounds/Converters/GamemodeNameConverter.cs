using System.Globalization;
using System.Windows.Data;

using Battlegrounds.Models.Lobbies;

namespace Battlegrounds.Converters;

/// <summary>
/// Represents a converter that converts a gamemode identifier to its corresponding display name.
/// </summary>
public sealed class GamemodeNameConverter : IValueConverter {
    
    private static readonly Dictionary<string, string> GamemodeNames = new() { // TODO: Localize these names
        { LobbySetting.SETTING_GAMEMODE, "Gamemode" },
        { LobbySetting.SETTING_VICTORY_POINTS, "Victory Points" },
        { LobbySetting.SETTING_SUPPLY_SYSTEM, "Supply System" },
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is string gamemode && GamemodeNames.TryGetValue(gamemode, out var name)) {
            return name;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
