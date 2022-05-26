using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds;
using Battlegrounds.Game.Database;
using Battlegrounds.Gfx;

namespace coh2_battlegrounds_console;
public static class Minimapper {

    public static void Map(Scenario scenario) {

        var mapFile = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.MOD_ART_FOLDER, $"map_icons\\{scenario.RelativeFilename}_map.tga");
        var mapData = TgaPixelReader.ReadTarga(mapFile);

    }

}
