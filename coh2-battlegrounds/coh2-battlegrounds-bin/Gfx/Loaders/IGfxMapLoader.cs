using System.IO;

namespace Battlegrounds.Gfx.Loaders;

/// <summary>
/// Interface for a loader of <see cref="IGfxMap"/> types.
/// </summary>
public interface IGfxMapLoader {

    /// <summary>
    /// Load the <see cref="IGfxMap"/> from the given binary reader, under the assumption the version has already been consumed.
    /// </summary>
    /// <param name="reader">The binary reader to read Gfx data from.</param>
    /// <returns>The loaded <see cref="IGfxMap"/>.</returns>
    IGfxMap LoadGfxMap(BinaryReader reader);

}
