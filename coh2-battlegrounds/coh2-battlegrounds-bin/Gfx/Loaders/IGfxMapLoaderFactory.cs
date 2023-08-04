namespace Battlegrounds.Gfx.Loaders;

/// <summary>
/// Interface representing a <see cref="IGfxMapLoader"/> factory.
/// </summary>
public interface IGfxMapLoaderFactory {

    /// <summary>
    /// Determine if the given version number is a support <see cref="GfxVersion"/>.
    /// </summary>
    /// <param name="version">The version to check.</param>
    /// <returns>If the given version is supported <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    bool IsSupportedVersion(int version);

    /// <summary>
    /// Get the <see cref="IGfxMapLoader"/> that loads the specified version.
    /// </summary>
    /// <param name="version">The version to get <see cref="IGfxMapLoader"/> for.</param>
    /// <returns>A <see cref="IGfxMapLoader"/> instance that can load the given version.</returns>
    /// <exception cref="Errors.IO.InvalidFileVersionException"/>
    IGfxMapLoader GetGfxMapLoader(GfxVersion version);

}
