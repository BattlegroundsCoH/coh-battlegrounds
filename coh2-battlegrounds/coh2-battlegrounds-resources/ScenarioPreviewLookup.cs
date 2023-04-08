using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Windows.Media;

using Battlegrounds.Game.Scenarios;
using Battlegrounds.Resources.Imaging;

namespace Battlegrounds.Resources;

/// <summary>
/// 
/// </summary>
public static class ScenarioPreviewLookup {

    /// <summary>
    /// 
    /// </summary>
    public static readonly ImageSource MapNotFound = new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/unknown_map.png"));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="scenario"></param>
    /// <returns></returns>
    public static BitmapSource? TryGetMapSource(Scenario? scenario) {

        // Check scenario
        if (scenario is null) {
            return (BitmapSource?)MapNotFound;
        }

        // Get Path
        string fullpath = BattlegroundsContext.GetRelativePath(BattlegroundsPaths.MOD_ART_FOLDER, $"map_icons\\{scenario.RelativeFilename}_map.tga");

        // Check if file exists
        if (File.Exists(fullpath)) {
            try {
                return TgaImageSource.TargaBitmapSourceFromFile(fullpath);
            } catch (BadImageFormatException bife) {
                Trace.WriteLine(bife, nameof(TryGetMapSource));
            }
        } else {
            fullpath = BattlegroundsContext.GetRelativePath(BattlegroundsPaths.MOD_USER_ICONS_FODLER, $"{scenario.RelativeFilename}_map.tga");
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

}
