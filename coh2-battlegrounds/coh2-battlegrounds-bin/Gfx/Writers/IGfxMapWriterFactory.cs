namespace Battlegrounds.Gfx.Writers;

/// <summary>
/// Defines a factory for creating instances of <see cref="IGfxMapWriter"/>.
/// </summary>
public interface IGfxMapWriterFactory {

    /// <summary>
    /// Creates an instance of IGfxMapWriter with default options.
    /// </summary>
    /// <returns>An instance of IGfxMapWriter.</returns>
    IGfxMapWriter GetWriter();

    /// <summary>
    /// Creates an instance of IGfxMapWriter for the specified IGfxMap with default options.
    /// </summary>
    /// <param name="gfxMap">The IGfxMap to create the writer for.</param>
    /// <returns>An instance of IGfxMapWriter.</returns>
    IGfxMapWriter GetWriter(IGfxMap gfxMap);

    /// <summary>
    /// Creates an instance of IGfxMapWriter for the specified IGfxMap with the specified options.
    /// </summary>
    /// <param name="gfxMap">The IGfxMap to create the writer for.</param>
    /// <param name="options">The options to use when creating the writer.</param>
    /// <returns>An instance of IGfxMapWriter.</returns>
    IGfxMapWriter GetWriter(IGfxMap gfxMap, IGfxMapWriterOptions options);

    /// <summary>
    /// Creates an instance of IGfxMapWriter with the specified options.
    /// </summary>
    /// <param name="options">The options to use when creating the writer.</param>
    /// <returns>An instance of IGfxMapWriter.</returns>
    IGfxMapWriter GetWriter(IGfxMapWriterOptions options);

}
