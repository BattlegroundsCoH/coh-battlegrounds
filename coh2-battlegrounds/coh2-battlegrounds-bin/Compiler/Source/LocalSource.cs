using System;
using System.Collections.Generic;
using System.Text;

namespace Battlegrounds.Compiler.Source {
    
    public class LocalSource : IWinconditionSource {

        private string m_relpath;

        public LocalSource(string path) {
            this.m_relpath = path;
        }

        public WinconoditionSourceFile GetInfoFile() => throw new NotImplementedException();
        
        public WinconoditionSourceFile[] GetLocaleFiles() => throw new NotImplementedException();
        
        public WinconoditionSourceFile[] GetScarFiles() => throw new NotImplementedException();
        
        public WinconoditionSourceFile[] GetWinFiles() => throw new NotImplementedException();


    }

}
