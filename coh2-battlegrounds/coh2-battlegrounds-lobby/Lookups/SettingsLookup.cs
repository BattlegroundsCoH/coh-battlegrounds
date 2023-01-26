using System;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Media.Imaging;
using System.Windows.Media;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.Scenarios;
using Battlegrounds.Locale;
using Battlegrounds.Modding;
using Battlegrounds.Resources.Imaging;

namespace Battlegrounds.Lobby.Lookups;

public static class SettingsLookup {

    public static string GetScenarioName(Scenario? scen, string defaultName)
        => scen is null ?
        defaultName :
        scen.Name.StartsWith("$", false, CultureInfo.InvariantCulture) && uint.TryParse(scen.Name[1..], out uint key) ? GameLocale.GetString(key) : scen.Name;

    public static string GetGamemodeName(string gamemode, ModPackage? package) {

        if (package is null) {
            return gamemode;
        }

        if (WinconditionList.GetGamemodeByName(package.GamemodeGUID, gamemode) is not IGamemode wincondition) {
            return gamemode;
        }

        return wincondition.ToString() ?? wincondition.Name;

    }

}
