using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using Battlegrounds.Functional;
using Battlegrounds.Modding;

namespace Battlegrounds.Compiler.Source.CoH2;

/// <summary>
/// Represents a local source implementation of the <see cref="IWinconditionSourceProvider"/> interface
/// for Company of Heroes 2.
/// </summary>
public sealed class CoH2LocalSourceProvider : IWinconditionSourceProvider {

    private readonly string m_relpath;

    private string Intermediate => $"{m_relpath}coh2_battlegrounds_wincondition Intermediate Cache\\Intermediate Files\\";

    /// <summary>
    /// Initializes a new instance of the <see cref="CoH2LocalSourceProvider"/> class with the specified relative path.
    /// </summary>
    /// <param name="path">The relative path to the CoH2 game files.</param>
    public CoH2LocalSourceProvider(string path) {
        m_relpath = path;
        if (!m_relpath.EndsWith("\\", false, CultureInfo.InvariantCulture)) {
            m_relpath += "\\";
        }
    }

    /// <inheritdoc/>
    public WinconditionSourceFile GetInfoFile(IGamemode mod) {
        string[] info = {
            "hidden=false",
            $"name=\"{mod.Name}\"",
            "description=\"\"",
            "dependencies = {}"
        };
        return new WinconditionSourceFile($"info\\{mod.Guid}.info", Encoding.UTF8.GetBytes(string.Join('\n', info)));
    }

    /// <inheritdoc/>
    public WinconditionSourceFile[] GetLocaleFiles(string? modpath) {
        List<WinconditionSourceFile> files = new List<WinconditionSourceFile>();
        string[] locFolders = Directory.GetDirectories($"{m_relpath}..\\..\\locale");
        string[] loc = locFolders.Filter(x => Path.GetDirectoryName(x) is not "3").MapAndFlatten(x => Directory.GetFiles(x, "*.ucs"));
        foreach (string file in loc) {
            files.Add(new WinconditionSourceFile(file[m_relpath.Length..], File.ReadAllBytes(file)));
        }
        return files.ToArray();
    }

    /// <inheritdoc/>
    public WinconditionSourceFile[] GetScarFiles() {
        List<WinconditionSourceFile> files = new();
        string[] scar = Directory
            .GetFiles($"{m_relpath}auxiliary_scripts\\", "*.scar")
            .Union(Directory.GetFiles($"{m_relpath}ui_api\\", "*.scar"))
            .Union(new string[] {
                $"{m_relpath}coh2_battlegrounds.scar",
                $"{m_relpath}coh2_battlegrounds_supply.scar",
                $"{m_relpath}coh2_battlegrounds_supply_ui.scar",
                $"{m_relpath}coh2_battlegrounds_weather.scar"
            })
            .ToArray();
        foreach (string file in scar) {
            if (!file.EndsWith("session.scar", false, CultureInfo.InvariantCulture)) {
                files.Add(new WinconditionSourceFile(file[m_relpath.Length..], File.ReadAllBytes(file)));
            }
        }
        return files.ToArray();
    }

    /// <inheritdoc/>
    public WinconditionSourceFile[] GetWinFiles() {
        List<WinconditionSourceFile> files = new();
        _ = Directory.GetFiles($"{m_relpath}", "*.win")
            .ForEach(x => files.Add(new WinconditionSourceFile(x[m_relpath.Length..], File.ReadAllBytes(x))));
        return files.ToArray();
    }

    /// <inheritdoc/>
    public WinconditionSourceFile[] GetUIFiles(IGamemode mod) {
        List<WinconditionSourceFile> files = new List<WinconditionSourceFile> {
            new WinconditionSourceFile($"data\\ui\\Bin\\{mod.Guid}.gfx", File.ReadAllBytes($"{Intermediate}data\\ui\\Bin\\{mod.Guid}.gfx"))
        };
        Directory.GetFiles($"{Intermediate}data\\ui\\Assets\\Textures\\", "*.dds")
            .ForEach(x => files.Add(new WinconditionSourceFile(x[Intermediate.Length..], File.ReadAllBytes(x))));
        return files.ToArray();
    }

    /// <inheritdoc/>
    public WinconditionSourceFile GetModGraphic()
        => new WinconditionSourceFile($"info\\coh2_battlegrounds_wincondition_preview.dds", File.ReadAllBytes($"{Intermediate}info\\coh2_battlegrounds_wincondition_preview.dds"));

    /// <summary>
    /// Returns a string representation of the <see cref="CoH2LocalSourceProvider"/> object.
    /// </summary>
    /// <returns>A string indicating that it's a local build from the specified path.</returns>
    public override string ToString() => $"Local build from \"{m_relpath}\"";

}
