using System.IO;

namespace Battlegrounds.Gfx.Writers;

/// <summary>
/// Interface for a writer that saves a <see cref="IGfxMap"/> to a <see cref="Stream"/>.
/// </summary>
public interface IGfxMapWriter {

    /// <summary>
    /// Save the <see cref="IGfxMap"/> map to a <see cref="Stream"/>.
    /// </summary>
    /// <param name="map">The map to save.</param>
    /// <param name="outputStream">The stream to write gfx map data to.</param>
    /// <returns>When saved <see langword="true"/> is returned; Otherwise <see langword="false"/>.</returns>
    bool Save(IGfxMap map, Stream outputStream);

}
