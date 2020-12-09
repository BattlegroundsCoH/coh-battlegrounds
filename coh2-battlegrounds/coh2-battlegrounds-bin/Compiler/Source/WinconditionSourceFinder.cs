using System.IO;
using Battlegrounds.Modding;

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

        public static IWinconditionSource GetSource(IWinconditionMod wincondition) {

            // TODO: Checkup on the wincondition

            if (HasLocalCopy(out string path)) {
                return new LocalSource(path);
            } else {
                return new GithubSource(GetBranch());
            }

        }

        private static string GetBranch() =>
#if DEBUG
            "scar-dev-branch";
#else
            "scar-release-branch";
#endif


    }

}
