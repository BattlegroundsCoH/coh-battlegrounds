using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Battlegrounds.Functional;
using Battlegrounds.Modding;

namespace Battlegrounds.Compiler.Source {
    
    public class LocalSource : IWinconditionSource {

        private string m_relpath;

        private string m_intermediate => $"{this.m_relpath}coh2_battlegrounds_wincondition Intermediate Cache\\Intermediate Files\\";

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

        public WinconoditionSourceFile[] GetLocaleFiles() {
            List<WinconoditionSourceFile> files = new List<WinconoditionSourceFile>();
            string[] locFolders = Directory.GetDirectories($"{this.m_relpath}locale");
            string[] loc = locFolders.ConvertAndMerge(x => Directory.GetFiles(x, "*.ucs"));
            foreach (string file in loc) {
                files.Add(new WinconoditionSourceFile(file[this.m_relpath.Length..], File.ReadAllBytes(file)));
            }
            return files.ToArray();
        }

        public WinconoditionSourceFile[] GetScarFiles() {
            List<WinconoditionSourceFile> files = new List<WinconoditionSourceFile>();
            string[] scar = Directory
                .GetFiles($"{this.m_relpath}auxiliary_scripts\\", "*.scar")
                .Union(Directory.GetFiles($"{this.m_relpath}ui_api\\", "*.scar"))
                .Union(new string[]{ $"{this.m_relpath}coh2_battlegrounds.scar" })
                .ToArray();
            foreach (string file in scar) {
                if (!file.EndsWith("session.scar")) {
                    files.Add(new WinconoditionSourceFile(file[this.m_relpath.Length..], File.ReadAllBytes(file)));
                }
            }
            return files.ToArray();
        }

        public WinconoditionSourceFile[] GetWinFiles() {
            List<WinconoditionSourceFile> files = new List<WinconoditionSourceFile>();
            Directory.GetFiles($"{this.m_relpath}", "*.win")
                .ForEach(x => files.Add(new WinconoditionSourceFile(x[this.m_relpath.Length..], File.ReadAllBytes(x))));
            return files.ToArray();
        }

        public WinconoditionSourceFile[] GetUIFiles(IWinconditionMod mod) {
            List<WinconoditionSourceFile> files = new List<WinconoditionSourceFile> {
                new WinconoditionSourceFile($"data\\ui\\Bin\\{ModGuid.FromGuid(mod.Guid)}.gfx", File.ReadAllBytes($"{this.m_intermediate}info\\coh2_battlegrounds_wincondition_preview.dds"))
            };
            Directory.GetFiles($"{this.m_intermediate}\\data\\ui\\Assets\\Textures\\", "*.dds")
                .ForEach(x => files.Add(new WinconoditionSourceFile(x[this.m_intermediate.Length..], File.ReadAllBytes(x))));
            return files.ToArray();
        }

        public WinconoditionSourceFile GetModGraphic() 
            => new WinconoditionSourceFile($"info\\coh2_battlegrounds_wincondition_preview.dds", File.ReadAllBytes($"{this.m_intermediate}info\\coh2_battlegrounds_wincondition_preview.dds"));

    }

}
