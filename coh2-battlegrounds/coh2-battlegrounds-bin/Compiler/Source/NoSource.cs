using System;

using Battlegrounds.Modding;

namespace Battlegrounds.Compiler.Source {

    public class NoSource : IWinconditionSource {
        
        public WinconditionSourceFile GetInfoFile(IWinconditionMod mod) => null;
        
        public WinconditionSourceFile[] GetLocaleFiles() => Array.Empty<WinconditionSourceFile>();
        
        public WinconditionSourceFile GetModGraphic() => null;
        
        public WinconditionSourceFile[] GetScarFiles() => Array.Empty<WinconditionSourceFile>();
        
        public WinconditionSourceFile[] GetUIFiles(IWinconditionMod mod) => Array.Empty<WinconditionSourceFile>();
        
        public WinconditionSourceFile[] GetWinFiles() => Array.Empty<WinconditionSourceFile>();

    }

}
