using Battlegrounds.Modding;

namespace Battlegrounds.Compiler.Source {
    
    /// <summary>
    /// Interface for retrieving source files for use by the <see cref="WinconditionCompiler"/>.
    /// </summary>
    public interface IWinconditionSource {

        /// <summary>
        /// Get all scar code files
        /// </summary>
        /// <returns>Array of all scar files</returns>
        WinconoditionSourceFile[] GetScarFiles();

        /// <summary>
        /// Get all wincondition files
        /// </summary>
        /// <returns>Array of all win files</returns>
        WinconoditionSourceFile[] GetWinFiles();

        /// <summary>
        /// Get all locale files
        /// </summary>
        /// <returns>Array of all locale files</returns>
        WinconoditionSourceFile[] GetLocaleFiles();

        /// <summary>
        /// Get all ingame UI files (in .dds format).
        /// </summary>
        /// <param name="mod">The <see cref="IWinconditionMod"/> mod associated with these UI files</param>
        /// <returns>Array of all ui files for ingame use.</returns>
        WinconoditionSourceFile[] GetUIFiles(IWinconditionMod mod);

        /// <summary>
        /// Get the preview graphic for the mod.
        /// </summary>
        /// <returns>The preview graphic <see cref="WinconoditionSourceFile"/>.</returns>
        WinconoditionSourceFile GetModGraphic();

        /// <summary>
        /// Get the info file.
        /// </summary>
        /// <param name="mod">The <see cref="IWinconditionMod"/> mod associated with the info file</param>
        /// <returns>The info file</returns>
        WinconoditionSourceFile GetInfoFile(IWinconditionMod mod);

    }

}
