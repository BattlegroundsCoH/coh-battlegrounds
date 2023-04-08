using System;
using System.Drawing;
using System.Drawing.Imaging;

using Battlegrounds.Game.Scenarios;
using Battlegrounds.Gfx;

namespace Battlegrounds.Developer;

public static class Minimapper {

    public static unsafe void Map(Scenario scenario) {

        if (!OperatingSystem.IsWindows()) {
            return;
        }

        // Find scenario minimap
        var mapFile = BattlegroundsContext.GetRelativePath(BattlegroundsPaths.MOD_ART_FOLDER, $"map_icons\\{scenario.RelativeFilename}_map.tga");
        var mapData = TgaPixelReader.ReadTarga(mapFile);

        // Greate overlay
        using var bmp = new Bitmap(mapData.Width, mapData.Height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Transparent);

        var xs = (bmp.Width / scenario.TerrainSize.Width) * 2;
        var ys = (bmp.Height / scenario.TerrainSize.Length) * 2;

        var ox = (bmp.Width * 0.5);
        var oy = (bmp.Height * 0.5);

        Console.WriteLine($"S = ({xs}, {ys})");

        for (int i = 0; i < scenario.Points.Length; i++) { 
            var point = scenario.Points[i];
            var x = (point.Position.X * xs + ox);
            var y = (-point.Position.Y * ys + oy);

            if (point.EntityBlueprint is "starting_position_shared_territory") {

                g.DrawEllipse(new Pen(new SolidBrush(Color.Red), 5), (float)x, (float)y, 10, 10);

            } else if (point.EntityBlueprint is "victory_point") {

                g.DrawRectangle(new Pen(new SolidBrush(Color.Yellow), 2.5f), (float)x, (float)y, 5, 5);

            } else {

                g.DrawRectangle(new Pen(new SolidBrush(Color.Blue), 2.5f), (float)x, (float)y, 5, 5);

            }

            Console.WriteLine($"{point.EntityBlueprint}: ({x},{y})");

        }

        // Save
        g.Save();
        bmp.Save("mini.bmp");


    }

}
