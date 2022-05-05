using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using Battlegrounds.Functional;
using Battlegrounds.Modding;

namespace Battlegrounds.Compiler.Source {

    public class PublicSource : IWinconditionSource {

        private readonly string m_relpath;

        private string Intermediate => $"{this.m_relpath}coh2_battlegrounds_wincondition Intermediate Cache\\Intermediate Files\\";

        private string GfxPath = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.BINARY_FOLDER, "gfx");

        public PublicSource(string path) {
            this.m_relpath = path;
            if (!this.m_relpath.EndsWith("\\", false, CultureInfo.InvariantCulture)) {
                this.m_relpath += "\\";
            }
        }

        public WinconditionSourceFile GetInfoFile(IGamemode mod) {
            string[] info = {
                "hidden=false",
                $"name=\"{mod.Name}\"",
                "description=\"DO NOT DISTRIBUTE!\"",
                $"dependencies = {{ {{ type = \"PropertyBagGroupPack\", id=\"142b113740474c82a60b0a428bd553d5\", published_file_id=\"2793333867\" }} }}"
            };
            return new WinconditionSourceFile($"info\\{mod.Guid}.info", Encoding.UTF8.GetBytes(string.Join('\n', info)));
        }

        public WinconditionSourceFile[] GetLocaleFiles(string? modpath) {
            
            // Null check
            if (string.IsNullOrEmpty(modpath)) {
                //return Array.Empty<WinconditionSourceFile>();
            }

            // Collect all locale files
            List<WinconditionSourceFile> files = new List<WinconditionSourceFile>();
            string binpath = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.BINARY_FOLDER);
            string locpath = Path.Combine(binpath, "locale");
            string[] locFolders = Directory.GetDirectories(Path.GetFullPath(locpath));
            string[] loc = locFolders.MapAndFlatten(x => Directory.GetFiles(x, "*.gamemode.ucs"));
            foreach (string file in loc) {
                var fp = Path.GetFullPath(file);
                files.Add(new WinconditionSourceFile(file[Path.GetFullPath(binpath).Length..], File.ReadAllBytes(file)));
            }

            return files.ToArray();

        }

        public WinconditionSourceFile[] GetScarFiles() {
            List<WinconditionSourceFile> files = new();
            string[] scar = Directory
                .GetFiles($"{this.m_relpath}auxiliary_scripts\\", "*.scar")
                .Union(Directory.GetFiles($"{this.m_relpath}ui_api\\", "*.scar"))
                .Union(new string[] {
                    $"{this.m_relpath}coh2_battlegrounds.scar",
                    $"{this.m_relpath}coh2_battlegrounds_supply.scar",
                    $"{this.m_relpath}coh2_battlegrounds_supply_ui.scar",
                    $"{this.m_relpath}coh2_battlegrounds_weather.scar"
                })
                .ToArray();
            foreach (string file in scar) {
                if (!file.EndsWith("session.scar", false, CultureInfo.InvariantCulture)) {
                    files.Add(new WinconditionSourceFile(file[this.m_relpath.Length..], File.ReadAllBytes(file)));
                }
            }
            return files.ToArray();
        }

        public WinconditionSourceFile[] GetWinFiles() {
            List<WinconditionSourceFile> files = new();
            _ = Directory.GetFiles($"{this.m_relpath}", "*.win")
                .ForEach(x => files.Add(new WinconditionSourceFile(x[this.m_relpath.Length..], File.ReadAllBytes(x))));
            return files.ToArray();
        }

        public WinconditionSourceFile[] GetUIFiles(IGamemode mod) {
            List<WinconditionSourceFile> files = new List<WinconditionSourceFile> {
                new WinconditionSourceFile($"data\\ui\\Bin\\{mod.Guid}.gfx", File.ReadAllBytes($"{this.GfxPath}\\{mod.Guid}.gfx"))
            };
            Directory.GetFiles(this.GfxPath, "*.dds")
                .ForEach(x => files.Add(new WinconditionSourceFile(x[this.GfxPath.Length..], File.ReadAllBytes(x))));
            return files.ToArray();
        }

        public WinconditionSourceFile GetModGraphic()
            => new WinconditionSourceFile(
                "info\\coh2_battlegrounds_wincondition_preview.dds", 
                File.ReadAllBytes(BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.BINARY_FOLDER, "coh2_battlegrounds_wincondition_preview.dds")));

        public override string ToString() => $"Local build from \"{this.m_relpath}\"";

    }

}
