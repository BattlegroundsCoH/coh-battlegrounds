using System.Globalization;

using Battlegrounds.Errors.Common;
using Battlegrounds.Game;
using Battlegrounds.Game.Scenarios;
using Battlegrounds.Modding;

namespace Battlegrounds.Lobby.Lookups;

public static class SettingsLookup {

    public static string GetScenarioName(IScenario? scen, string defaultName)
        => scen is null ?
        defaultName :
        scen.Name.StartsWith("$", false, CultureInfo.InvariantCulture) && uint.TryParse(scen.Name[1..], out uint key) 
            ? BattlegroundsContext.DataSource.GetLocale(scen.Game).GetString(key) : scen.Name;

    public static string GetGamemodeName(string gamemode, IModPackage? package, GameCase game) {

        if (package is null) {
            return gamemode;
        }

        var gamemodesSource = BattlegroundsContext.DataSource.GetGamemodeList(package, game) ?? throw new ObjectNotFoundException("Failed finding gamemode list");
        if (gamemodesSource.GetGamemodeByName(package.GamemodeGUID, gamemode) is not IGamemode wincondition) {
            return gamemode;
        }

        return wincondition.ToString() ?? wincondition.Name;

    }

}
