using System;
using System.IO;

using Battlegrounds.Gfx;
using Battlegrounds.Gfx.Loaders;
using Battlegrounds.Logging;

namespace Battlegrounds.Developer.Commands;

public class GfxUnpackCommand : Command {

    private static readonly Logger logger = Logger.CreateLogger();

    public static readonly Argument<string> FILE = new Argument<string>("-gfx", "Specifies the gfx file to unpack", "gfx.dat");
    public static readonly Argument<string> OUT = new Argument<string>("-o", "Specifies the name of the directory to output gfx map to.", "gfx_out");

    public GfxUnpackCommand() : base("gfxun", "Unpacks the specified GFX file to the specified directory", FILE, OUT) { }

    public override void Execute(CommandArgumentList argumentList) {

        // Clear
        if (Directory.Exists(argumentList.GetValue(OUT))) {
            logger.Warning("Clearing directory: " + argumentList.GetValue(OUT));
            Directory.Delete(argumentList.GetValue(OUT), true);
        }

        // Get file
        var filename = argumentList.GetValue(FILE);
        if (!File.Exists(filename)) {
            logger.Error("File not found: " + Path.GetFullPath(filename));
            return;
        }

        using var fs = File.OpenRead(filename);
        using var br = new BinaryReader(fs);

        GfxVersion v = (GfxVersion)br.ReadInt32();

        var factory = new GfxMapLoaderFactory();
        var loader = factory.GetGfxMapLoader(v);

        var gfxmap = loader.LoadGfxMap(br);

        // Dump
        foreach (var res in gfxmap) {
            string path = Path.Combine(argumentList.GetValue(OUT), res.Identifier + GetExtFromType(res.GfxType));
            string dirPath = Path.GetDirectoryName(path)!;
            if (!Directory.Exists(dirPath)) {
                Directory.CreateDirectory(dirPath);
            }
            using var o = File.OpenWrite(path);
            using var r = res.Open();
            r.CopyTo(o);
            logger.Info("Extracted " + res.Identifier + " to " + path);
        }

    }

    private static string GetExtFromType(GfxResourceType rt) => rt switch {
        GfxResourceType.Xaml => ".xaml",
        GfxResourceType.Tga => ".tga",
        GfxResourceType.Png => ".png",
        GfxResourceType.Html => ".html",
        GfxResourceType.Bmp => ".bmp",
        _ => throw new NotImplementedException()
    };

}
