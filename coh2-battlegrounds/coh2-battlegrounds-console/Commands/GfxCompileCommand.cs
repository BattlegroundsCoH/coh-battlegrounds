using Battlegrounds.Developer.Gfx;
using Battlegrounds.Gfx;

namespace Battlegrounds.Developer.Commands;

class GfxCompileCommand : Command {

    public static readonly Argument<string> DIR = new Argument<string>("-d", "Specifies the directory to compile.", string.Empty);
    public static readonly Argument<string> OUT = new Argument<string>("-o", "Specifies the name of the file to output gfx map to.", "gfx.dat");
    public static readonly Argument<int> VER = new Argument<int>("-v", "Specifies the gfx version to target.", GfxMap.GfxBinaryVersion);
    public static readonly Argument<string> REG = new Argument<string>("-r", "Specifies regex pattern to select specific files in folder to compile.", string.Empty);

    public GfxCompileCommand() : base("gfxdir", "Compiles directory gfx content into a gfx data file.", DIR, OUT, VER, REG) { }

    public override void Execute(CommandArgumentList argumentList)
        => GfxFolderCompiler.Compile(argumentList.GetValue(DIR), argumentList.GetValue(OUT), version: argumentList.GetValue(VER));

}
