using Battlegrounds.Modding;

namespace Battlegrounds.Compiler.Source;

/// <summary>
/// Interface for retrieving source files for use by the <see cref="WinconditionCompiler"/>.
/// </summary>
public interface IWinconditionSource {

    /// <summary>
    /// Get all scar code files
    /// </summary>
    /// <returns>Array of all scar files</returns>
    WinconditionSourceFile[] GetScarFiles();

    /// <summary>
    /// Get all wincondition files
    /// </summary>
    /// <returns>Array of all win files</returns>
    WinconditionSourceFile[] GetWinFiles();

    /// <summary>
    /// Get all locale files
    /// </summary>
    /// <returns>Array of all locale files</returns>
    WinconditionSourceFile[] GetLocaleFiles(string? modpath);

    /// <summary>
    /// Get all ingame UI files (in .dds format).
    /// </summary>
    /// <param name="mod">The <see cref="IGamemode"/> mod associated with these UI files</param>
    /// <returns>Array of all ui files for ingame use.</returns>
    WinconditionSourceFile[] GetUIFiles(IGamemode mod);

    /// <summary>
    /// Get the preview graphic for the mod.
    /// </summary>
    /// <returns>The preview graphic <see cref="WinconditionSourceFile"/>.</returns>
    WinconditionSourceFile GetModGraphic();

    /// <summary>
    /// Get the info file.
    /// </summary>
    /// <param name="mod">The <see cref="IGamemode"/> mod associated with the info file</param>
    /// <returns>The info file</returns>
    WinconditionSourceFile GetInfoFile(IGamemode mod);

}

