using Battlegrounds.Gfx;
using Battlegrounds.Gfx.Loaders;

using System;
using System.IO;

namespace Battlegrounds.Developer.Commands;

/// <summary>
/// Represents the GfxVerify flag command.
/// </summary>
public class GfxVerifyCommand : Command {

    public static readonly Argument<string> PATH = new Argument<string>("-f", "Specifies the file to verify integrity of.", string.Empty);

    public GfxVerifyCommand() : base("gfxverify", "Verifies the integrity of a gfx file.", PATH) { }

    public override void Execute(CommandArgumentList argumentList) {

        // Grab target path
        var target = argumentList.GetValue(PATH);

        try {

            // Create reader factory
            var factory = new GfxMapLoaderFactory();

            // Read version
            using var fs = File.OpenRead(target);
            using var fsr = new BinaryReader(fs);
            GfxVersion ver = (GfxVersion)fsr.ReadInt32();

            // Try read
            IGfxMap map = factory.GetGfxMapLoader(ver).LoadGfxMap(fsr);

            // Log details if read
            if (map is null) {
                Console.WriteLine($"Failed to read {target}");
                return;
            }

            // Do stuff
            Console.WriteLine("Successfully parsed gfx file:");
            Console.WriteLine("\tBinary Version: " + map.GfxVersion);
            Console.WriteLine("\tResource capacity: " + map.Capacity);
            Console.WriteLine("\tResource count: " + map.Count);

        } catch (Exception ex) {
            Console.WriteLine(ex);
        }

    }

}
