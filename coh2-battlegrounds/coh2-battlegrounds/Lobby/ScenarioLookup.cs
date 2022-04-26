using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Battlegrounds.Game.Database;

using BattlegroundsApp.Resources;

namespace BattlegroundsApp.Lobby;

public static class ScenarioLookup {

    private static readonly ImageSource __mapNotFound = new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/unknown_map.png"));

    public static BitmapSource? TryGetMapSource(Scenario? scenario) {

        // Check scenario
        if (scenario is null) {
            Trace.WriteLine($"Failed to set **null** scenario.", nameof(ScenarioLookup));
            return (BitmapSource?)__mapNotFound;
        }

        // Get Path
        string fullpath = Path.GetFullPath($"bg_common\\gfx\\map_icons\\{scenario.RelativeFilename}_map.tga");

        // Check if file exists
        if (File.Exists(fullpath)) {
            try {
                return TgaImageSource.TargaBitmapSourceFromFile(fullpath);
            } catch (BadImageFormatException bife) {
                Trace.WriteLine(bife, nameof(TryGetMapSource));
            }
        } else {
            fullpath = Path.GetFullPath($"usr\\mods\\map_icons\\{scenario.RelativeFilename}_map.tga");
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
        return (BitmapSource?)__mapNotFound;

    }

}
