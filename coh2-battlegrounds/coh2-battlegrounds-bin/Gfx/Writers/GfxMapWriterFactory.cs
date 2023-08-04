using System;

namespace Battlegrounds.Gfx.Writers;

/// <summary>
/// Default <see cref="IGfxMapWriter"/> factory.
/// </summary>
public sealed class GfxMapWriterFactory : IGfxMapWriterFactory {

    /// <inheritdoc/>
    public IGfxMapWriter GetWriter() => GetWriter(new GfxMapWriterOptions());

    /// <inheritdoc/>
    public IGfxMapWriter GetWriter(IGfxMap gfxMap) => GetWriter(new GfxMapWriterOptions() { Version = gfxMap.GfxVersion });

    /// <inheritdoc/>
    public IGfxMapWriter GetWriter(IGfxMap gfxMap, IGfxMapWriterOptions options) => GetWriter(new GfxMapWriterOptions(options) { Version = gfxMap.GfxVersion });

    /// <inheritdoc/>
    public IGfxMapWriter GetWriter(IGfxMapWriterOptions options) => options.Version switch {
        GfxVersion.V1 => new GfxMapWriterV1(),
        GfxVersion.V2 or GfxVersion.V2_1 => new GfxMapWriterV2(options.Version is GfxVersion.V2_1, options),
        GfxVersion.V3 => new GfxMapWriterV3(options),
        _ => throw new Exception()
    };

}
