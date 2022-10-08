using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Battlegrounds;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Scenarios;
using Battlegrounds.Locale;
using Battlegrounds.Modding;
using Battlegrounds.Resources.Imaging;

namespace BattlegroundsApp.Lobby;

public static class LobbySettingsLookup {

    public static readonly ImageSource MapNotFound = new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/unknown_map.png"));

    public static BitmapSource? TryGetMapSource(Scenario? scenario) {

        // Check scenario
        if (scenario is null) {
            return (BitmapSource?)MapNotFound;
        }

        // Get Path
        string fullpath = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.MOD_ART_FOLDER, $"map_icons\\{scenario.RelativeFilename}_map.tga");

        // Check if file exists
        if (File.Exists(fullpath)) {
            try {
                return TgaImageSource.TargaBitmapSourceFromFile(fullpath);
            } catch (BadImageFormatException bife) {
                Trace.WriteLine(bife, nameof(TryGetMapSource));
            }
        } else {
            fullpath = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.MOD_USER_ICONS_FODLER, $"{scenario.RelativeFilename}_map.tga");
            if (File.Exists(fullpath)) {
                try {
                    return TgaImageSource.TargaBitmapSourceFromFile(fullpath);
                } catch (BadImageFormatException bife) {
                    Trace.WriteLine(bife, nameof(TryGetMapSource));
                }
            } else {
                Trace.WriteLine($"Failed to locate file: {fullpath}", nameof(TryGetMapSource));
            }
        }

        // Nothing found
        return (BitmapSource?)MapNotFound;

    }

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
