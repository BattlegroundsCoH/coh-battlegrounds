using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using Battlegrounds.Functional;
using Battlegrounds.Modding;

namespace Battlegrounds.Compiler.Source.CoH3;

/// <summary>
/// Represents a local source implementation of the <see cref="IWinconditionSourceProvider"/> interface
/// for Company of Heroes 3.
/// </summary>
public sealed class CoH3LocalSourceProvider : IWinconditionSourceProvider {

    private readonly string relativePath;

    /// <summary>
    /// Initialise a new <see cref="CoH3LocalSourceProvider"/> instance.
    /// </summary>
    /// <param name="relativePath">The relative path to fetch source files from</param>
    public CoH3LocalSourceProvider(string relativePath) {
        this.relativePath = relativePath.EndsWith("\\") ? relativePath : relativePath + "\\";
    }

    /// <inheritdoc/>
    public WinconditionSourceFile GetInfoFile(IGamemode mod)
        => new WinconditionSourceFile("info\\mod.bin", File.ReadAllBytes(BattlegroundsContext.GetRelativePath(BattlegroundsPaths.BINARY_FOLDER, "precompiled\\3\\mod.bin")));

    /// <inheritdoc/>
    public WinconditionSourceFile[] GetLocaleFiles(string? modpath) {
        List<WinconditionSourceFile> files = new List<WinconditionSourceFile>();
        string[] loc = Directory.GetFiles($"{relativePath}..\\..\\locale\\3", "*.ucs");
        foreach (string file in loc) {
            string fn = Path.GetFileNameWithoutExtension(file);
            files.Add(new WinconditionSourceFile($"locale\\{fn}\\{fn}.ucs", File.ReadAllBytes(file)));
        }
        return files.ToArray();
    }

    /// <inheritdoc/>
    public WinconditionSourceFile GetModGraphic()
        => null;

    /// <inheritdoc/>
    public WinconditionSourceFile[] GetScarFiles() {
        List<WinconditionSourceFile> files = new();
        string[] scar = Directory
            .GetFiles($"{relativePath}bg_scripts\\", "*.scar", SearchOption.AllDirectories)
            .Union(new string[] {
                $"{relativePath}battlegrounds.scar"
            })
            .ToArray();
        foreach (string file in scar) {
            if (!file.EndsWith("session.scar", false, CultureInfo.InvariantCulture)) {
                files.Add(new WinconditionSourceFile(file[relativePath.Length..], File.ReadAllBytes(file)));
            }
        }
        return files.ToArray();
    }

    /// <inheritdoc/>
    public WinconditionSourceFile[] GetUIFiles(IGamemode mod)
        => Array.Empty<WinconditionSourceFile>();

    /// <inheritdoc/>
    public WinconditionSourceFile[] GetWinFiles()
        => new[] {
            new WinconditionSourceFile("scar\\winconditions\\battlegrounds.bin", File.ReadAllBytes(BattlegroundsContext.GetRelativePath(BattlegroundsPaths.BINARY_FOLDER, "precompiled\\3\\battlegrounds.bin")))
        };

    /// <summary>
    /// Returns a string representation of the <see cref="CoH3LocalSourceProvider"/> object.
    /// </summary>
    /// <returns>A string indicating that it's a local build from the specified path.</returns>
    public override string ToString() => $"Local build from \"{relativePath}\"";

}
