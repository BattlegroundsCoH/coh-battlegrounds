using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Battlegrounds.Modding;

namespace Battlegrounds.Compiler.Source {
    
    public class LocalSource : IWinconditionSource {

        private string m_relpath;

        public LocalSource(string path) {
            this.m_relpath = path;
            if (!this.m_relpath.EndsWith("\\")) {
                this.m_relpath += "\\";
            }
        }

        public WinconoditionSourceFile GetInfoFile(IWinconditionMod mod) {
            string[] info = {
                "hidden=false",
                $"name=\"{mod.Name}\"",
                "description=\"\"",
                "dependencies = {}"
            };
            return new WinconoditionSourceFile($"info\\{ModGuid.FromGuid(mod.Guid)}.info", Encoding.UTF8.GetBytes(string.Join('\n', info)));
        }
        
        public WinconoditionSourceFile[] GetLocaleFiles() => throw new NotImplementedException();

        public WinconoditionSourceFile[] GetScarFiles() {
            List<WinconoditionSourceFile> files = new List<WinconoditionSourceFile>();
            string[] scar = Directory
                .GetFiles($"{this.m_relpath}auxiliary_scripts\\", "*.scar")
                .Union(new string[]{ $"{this.m_relpath}coh2_battlegrounds.scar" })
                .ToArray();
            foreach (string file in scar) {
                if (!file.EndsWith("session.scar")) {
                    files.Add(new WinconoditionSourceFile(file[this.m_relpath.Length..], File.ReadAllBytes(file)));
                }
            }
            return files.ToArray();
        }
        
        public WinconoditionSourceFile[] GetWinFiles() => throw new NotImplementedException();


    }

}
