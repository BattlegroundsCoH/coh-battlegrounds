using System.Diagnostics;
using System.IO;

using Battlegrounds.Modding;

namespace Battlegrounds.Compiler.Source; 
public static class WinconditionSourceFinder {

    private static IWinconditionSource? GetLocalSource() {
        string path;
#if DEBUG
        if (!BattlegroundsInstance.Debug.UseLocalWincondition) {
            int top = 0; // try find debug folder before using local distribution
            path = "coh2-battlegrounds-mod\\wincondition_mod\\";
            try {
                do {
                    if (Directory.Exists(path)) {
                        return new LocalSource(path);
                    } else {
                        top++;
                        path = $"..\\{path}";
                    }
                } while (top < 11);
            } catch { }
        }
#endif
        path = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.BINARY_FOLDER, "bg_wc\\");
        if (Directory.Exists(path)) {
            return new PublicSource(path);
        } else {
            return null; 
        }
    }

    public static IWinconditionSource GetSource(IGamemode wincondition) {

        if (GetLocalSource() is IWinconditionSource src) {
            return src;
        } else {
            Trace.WriteLine("Failed to source location of wincondition code.", nameof(WinconditionSourceFinder));
            return new NoSource();
        }

    }

}
