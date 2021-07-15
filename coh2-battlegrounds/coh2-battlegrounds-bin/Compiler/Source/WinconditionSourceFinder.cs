using System.Diagnostics;
using System.IO;
using Battlegrounds.Modding;
using Battlegrounds.Networking.Server;

namespace Battlegrounds.Compiler.Source {
    
    public static class WinconditionSourceFinder {

        private static bool HasLocalCopy(out string path) {
            int top = 0;
            path = "coh2-battlegrounds-mod\\wincondition_mod\\";
            try {
                do {
                    if (Directory.Exists(path)) {
                        return true;
                    } else {
                        top++;
                        path = $"..\\{path}";
                    }
                } while (top < 11);
            } catch {}
            return false;
        }

        private static bool HasManifest() => File.Exists(BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.BINARY_FOLDER, "scripts.manifest"));

        public static IWinconditionSource GetSource(IWinconditionMod wincondition, ServerAPI serverAPI) {

            // TODO: Checkup on the wincondition

            if (HasLocalCopy(out string path)) {
                return new LocalSource(path);
            } else {
                if (HasManifest()) {
                    return new ManifestSource(serverAPI);
                } else {
                    Trace.WriteLine("Failed to source location of wincondition code.", nameof(WinconditionSourceFinder));
                    return new NoSource();
                }
            }

        }

    }

}
