using System.Globalization;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.Scenarios;
using Battlegrounds.Modding;

namespace Battlegrounds.Lobby.Lookups;

public static class SettingsLookup {

    public static string GetScenarioName(IScenario? scen, string defaultName)
        => scen is null ?
        defaultName :
        scen.Name.StartsWith("$", false, CultureInfo.InvariantCulture) && uint.TryParse(scen.Name[1..], out uint key) 
            ? BattlegroundsContext.DataSource.GetLocale(scen.Game).GetString(key) : scen.Name;

    public static string GetGamemodeName(string gamemode, IModPackage? package) {

        if (package is null) {
            return gamemode;
        }

        if (Winconditions.GetGamemodeByName(package.GamemodeGUID, gamemode) is not IGamemode wincondition) {
            return gamemode;
        }

        return wincondition.ToString() ?? wincondition.Name;

    }

}
