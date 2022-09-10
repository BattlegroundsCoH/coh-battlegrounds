using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using Battlegrounds.Functional;
using Battlegrounds.Modding;

namespace Battlegrounds.Compiler.Source;

public class LocalSource : IWinconditionSource {

    private readonly string m_relpath;

    private string Intermediate => $"{this.m_relpath}coh2_battlegrounds_wincondition Intermediate Cache\\Intermediate Files\\";

    public LocalSource(string path) {
        this.m_relpath = path;
        if (!this.m_relpath.EndsWith("\\", false, CultureInfo.InvariantCulture)) {
            this.m_relpath += "\\";
        }
    }

    public WinconditionSourceFile GetInfoFile(IGamemode mod) {
        string[] info = {
            "hidden=false",
            $"name=\"{mod.Name}\"",
            "description=\"\"",
            "dependencies = {}"
        };
        return new WinconditionSourceFile($"info\\{mod.Guid}.info", Encoding.UTF8.GetBytes(string.Join('\n', info)));
    }

    public WinconditionSourceFile[] GetLocaleFiles(string? modpath) {
        List<WinconditionSourceFile> files = new List<WinconditionSourceFile>();
        string[] locFolders = Directory.GetDirectories($"{this.m_relpath}locale");
        string[] loc = locFolders.MapAndFlatten(x => Directory.GetFiles(x, "*.ucs"));
        foreach (string file in loc) {
            files.Add(new WinconditionSourceFile(file[this.m_relpath.Length..], File.ReadAllBytes(file)));
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
            new WinconditionSourceFile($"data\\ui\\Bin\\{mod.Guid}.gfx", File.ReadAllBytes($"{this.Intermediate}data\\ui\\Bin\\{mod.Guid}.gfx"))
        };
        Directory.GetFiles($"{this.Intermediate}data\\ui\\Assets\\Textures\\", "*.dds")
            .ForEach(x => files.Add(new WinconditionSourceFile(x[this.Intermediate.Length..], File.ReadAllBytes(x))));
        return files.ToArray();
    }

    public WinconditionSourceFile GetModGraphic()
        => new WinconditionSourceFile($"info\\coh2_battlegrounds_wincondition_preview.dds", File.ReadAllBytes($"{this.Intermediate}info\\coh2_battlegrounds_wincondition_preview.dds"));

    public override string ToString() => $"Local build from \"{this.m_relpath}\"";

}

