using System;

using Battlegrounds.Modding;

namespace Battlegrounds.Compiler.Source.CoH3;

/// <summary>
/// Represents a local source implementation of the <see cref="IWinconditionSourceProvider"/> interface
/// for Company of Heroes 3.
/// </summary>
public class CoH3LocalSourceProvider : IWinconditionSourceProvider {

    private readonly string relativePath;

    /// <summary>
    /// Initialise a new <see cref="CoH3LocalSourceProvider"/> instance.
    /// </summary>
    /// <param name="relativePath">The relative path to fetch source files from</param>
    public CoH3LocalSourceProvider(string relativePath) {
        this.relativePath = relativePath;
    }

    /// <inheritdoc/>
    public WinconditionSourceFile GetInfoFile(IGamemode mod) {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public WinconditionSourceFile[] GetLocaleFiles(string? modpath) {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public WinconditionSourceFile GetModGraphic() {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public WinconditionSourceFile[] GetScarFiles() {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public WinconditionSourceFile[] GetUIFiles(IGamemode mod) {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public WinconditionSourceFile[] GetWinFiles() {
        throw new NotImplementedException();
    }

}
