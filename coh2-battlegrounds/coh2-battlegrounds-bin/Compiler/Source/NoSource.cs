using System;

using Battlegrounds.Modding;

namespace Battlegrounds.Compiler.Source;

/// <summary>
/// Represents a default implementation of the <see cref="IWinconditionSource"/> interface
/// that provides no actual data for win conditions, locales, graphics, UI, or win files.
/// </summary>
public sealed class NoSource : IWinconditionSource {

    /// <summary>
    /// Gets the information file related to the specified game mode.
    /// This method always returns null.
    /// </summary>
    /// <param name="mod">The game mode for which to retrieve the information file.</param>
    /// <returns>Always returns null.</returns>
    public WinconditionSourceFile GetInfoFile(IGamemode mod) => null;

    /// <summary>
    /// Gets an array of locale files related to the specified path.
    /// This method always returns an empty array.
    /// </summary>
    /// <param name="path">The path for which to retrieve the locale files.</param>
    /// <returns>An empty array of <see cref="WinconditionSourceFile"/>.</returns>
    public WinconditionSourceFile[] GetLocaleFiles(string? path) => Array.Empty<WinconditionSourceFile>();

    /// <summary>
    /// Gets the graphic file related to the game mod.
    /// This method always returns null.
    /// </summary>
    /// <returns>Always returns null.</returns>
    public WinconditionSourceFile GetModGraphic() => null;

    /// <summary>
    /// Gets an array of scar files.
    /// This method always returns an empty array.
    /// </summary>
    /// <returns>An empty array of <see cref="WinconditionSourceFile"/>.</returns>
    public WinconditionSourceFile[] GetScarFiles() => Array.Empty<WinconditionSourceFile>();

    /// <summary>
    /// Gets an array of UI files related to the specified game mode.
    /// This method always returns an empty array.
    /// </summary>
    /// <param name="mod">The game mode for which to retrieve the UI files.</param>
    /// <returns>An empty array of <see cref="WinconditionSourceFile"/>.</returns>
    public WinconditionSourceFile[] GetUIFiles(IGamemode mod) => Array.Empty<WinconditionSourceFile>();

    /// <summary>
    /// Gets an array of win files.
    /// This method always returns an empty array.
    /// </summary>
    /// <returns>An empty array of <see cref="WinconditionSourceFile"/>.</returns>
    public WinconditionSourceFile[] GetWinFiles() => Array.Empty<WinconditionSourceFile>();

}
