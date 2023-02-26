using Battlegrounds.Gfx;

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

            // Try read
            GfxMap map = GfxMap.FromBinary(File.OpenRead(target));

            // Log details if read
            if (map is null) {
                Console.WriteLine($"Failed to read {target}");
                return;
            }

            // Do stuff
            Console.WriteLine("Successfully parsed gfx file:");
            Console.WriteLine("\tBinary Version: " + map.BinaryVersion);
            Console.WriteLine("\tResource count: " + map.Resources.Length);

        } catch (Exception ex) {
            Console.WriteLine(ex);
        }

    }

}