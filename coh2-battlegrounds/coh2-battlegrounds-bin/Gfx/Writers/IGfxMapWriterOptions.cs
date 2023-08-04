using System.IO;
using System.IO.Compression;

namespace Battlegrounds.Gfx.Writers;

/// <summary>
/// Enum for describing the compression type.
/// </summary>
public enum GfxMapCompressionType : ushort {

    /// <summary>
    /// None, raw data stream in use.
    /// </summary>
    None = 0,

    /// <summary>
    /// Use the <c>Deflate</c> Algorithm (Used in <see cref="GfxVersion.V2"/>).
    /// </summary>
    Deflate = 1,

    /// <summary>
    /// Use the <c>Brotli</c> algorithm.
    /// </summary>
    Brotli = 2,

    /// <summary>
    /// Use the <c>GZip</c> Algorithm (Default in <see cref="GfxVersion.V3"/>).
    /// </summary>
    Gzip = 4,

    /// <summary>
    /// Algorithm is unspecified (support for custom algorithm).
    /// </summary>
    Unspecified = 8,

}

/// <summary>
/// Interface for <see cref="IGfxMapWriter"/> options.
/// </summary>
public interface IGfxMapWriterOptions {

    /// <summary>
    /// Get or set the targetted <see cref="GfxVersion"/> to write.
    /// </summary>
    GfxVersion Version { get; set; }

    /// <summary>
    /// Get or set the threshold a compression must use in order to be used.
    /// </summary>
    float CompressionThreshold { get; set; }

    /// <summary>
    /// Get or set the compression method.
    /// </summary>
    GfxMapCompressionType CompressionMethod { get; set; }

    /// <summary>
    /// Get the Stream to hold compressed or decompressed data.
    /// </summary>
    /// <param name="sourceOrDestination">The source to compress or destination of decompression</param>
    /// <param name="mode">The decompress or compress mode of the Stream.</param>
    /// <returns>A concrete <see cref="Stream"/> instance set up for compressing or decompressing data.</returns>
    Stream GetCompressionStream(Stream sourceOrDestination, CompressionMode mode);

}
