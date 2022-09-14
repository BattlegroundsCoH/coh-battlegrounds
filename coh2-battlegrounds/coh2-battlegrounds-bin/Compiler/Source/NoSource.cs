using System;

using Battlegrounds.Modding;

namespace Battlegrounds.Compiler.Source;

public class NoSource : IWinconditionSource {

    public WinconditionSourceFile GetInfoFile(IGamemode mod) => null;

    public WinconditionSourceFile[] GetLocaleFiles(string? path) => Array.Empty<WinconditionSourceFile>();

    public WinconditionSourceFile GetModGraphic() => null;

    public WinconditionSourceFile[] GetScarFiles() => Array.Empty<WinconditionSourceFile>();

    public WinconditionSourceFile[] GetUIFiles(IGamemode mod) => Array.Empty<WinconditionSourceFile>();

    public WinconditionSourceFile[] GetWinFiles() => Array.Empty<WinconditionSourceFile>();

}

