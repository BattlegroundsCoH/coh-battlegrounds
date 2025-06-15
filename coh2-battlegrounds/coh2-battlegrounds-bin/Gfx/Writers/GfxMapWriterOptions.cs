using System.IO;
using System.IO.Compression;

using Battlegrounds.Errors.Common;

namespace Battlegrounds.Gfx.Writers;

/// <summary>
/// Default <see cref="IGfxMapWriter"/> options.
/// </summary>
public sealed class GfxMapWriterOptions : IGfxMapWriterOptions {

    /// <inheritdoc/>
    public GfxVersion Version { get; set; }
    
    /// <inheritdoc/>
    public float CompressionThreshold { get; set; }

    /// <inheritdoc/>
    public GfxMapCompressionType CompressionMethod { get; set; }

    /// <summary>
    /// Initialise a new basic <see cref="GfxMapWriterOptions"/> instance.
    /// </summary>
    public GfxMapWriterOptions() {
        this.Version = GfxVersion.Latest;
        this.CompressionThreshold = 0.5f;
        this.CompressionMethod = GfxMapCompressionType.Gzip;
    }

    /// <summary>
    /// Initialise a new basic <see cref="GfxMapWriterOptions"/> instance based on another <see cref="IGfxMapWriterOptions"/> instance.
    /// </summary>
    /// <param name="options">The options to copy</param>
    public GfxMapWriterOptions(IGfxMapWriterOptions options) {
        this.Version = options.Version;
        this.CompressionThreshold = options.CompressionThreshold;
        this.CompressionMethod = options.CompressionMethod;
    }

    /// <inheritdoc/>
    public Stream GetCompressionStream(Stream sourceOrDestination, CompressionMode mode) => this.CompressionMethod switch {
        GfxMapCompressionType.Deflate => new DeflateStream(sourceOrDestination, mode),
        GfxMapCompressionType.Brotli => new BrotliStream(sourceOrDestination, CompressionLevel.Optimal),
        GfxMapCompressionType.Gzip => new GZipStream(sourceOrDestination, mode),
        _ => mode switch {
            CompressionMode.Compress => CopyOf(sourceOrDestination),
            CompressionMode.Decompress => sourceOrDestination,
            _ => throw new EnumValueNotSupportedException<CompressionMode>(mode)
        }
    };

    private static Stream CopyOf(Stream source) {
        var ms = new MemoryStream();
        source.CopyTo(ms);
        return ms;
    }

}
