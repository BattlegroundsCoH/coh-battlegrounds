using Battlegrounds.Errors.IO;

namespace Battlegrounds.Gfx.Loaders;

/// <summary>
/// Factory for creating loaders that can load a <see cref="StandardGfxMap"/> instance.
/// </summary>
public sealed class GfxMapLoaderFactory : IGfxMapLoaderFactory {

    private readonly IGfxMapLoader v1Loader = new GfxMapLoaderV1();
    private readonly IGfxMapLoader v2Loader = new GfxMapLoaderV2();
    private readonly IGfxMapLoader v3Loader = new GfxMapLoaderV3();

    /// <inheritdoc/>
    public bool IsSupportedVersion(int version) => 
        version is ((int)GfxVersion.V1) or ((int)GfxVersion.V2) or ((int)GfxVersion.V2_1) or ((int)GfxVersion.V3);

    /// <inheritdoc/>
    public IGfxMapLoader GetGfxMapLoader(GfxVersion version) => version switch {
        GfxVersion.V1 => v1Loader,
        GfxVersion.V2 => v2Loader,
        GfxVersion.V3 => v3Loader,
        _ => throw new InvalidFileVersionException($"Invalid file version: {version}")
    };

}
