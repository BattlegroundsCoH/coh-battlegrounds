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

    /// <summary>
    /// Converts a gamemode identifier to its display name, upper-cased when the converter
    /// parameter is <c>upper</c> — settings panels label their controls in caps.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is not string gamemode) {
            return value;
        }
        var name = GamemodeNames.TryGetValue(gamemode, out var display) ? display : gamemode;
        return parameter is "upper" ? name.ToUpperInvariant() : name;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

}
