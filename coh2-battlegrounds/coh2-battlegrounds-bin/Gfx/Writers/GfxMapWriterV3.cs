using System;
using System.IO;
using System.IO.Compression;
using System.Text;

using Battlegrounds.Functional;
using Battlegrounds.Logging;
using Battlegrounds.Util;

namespace Battlegrounds.Gfx.Writers;

/// <summary>
/// 
/// </summary>
public sealed class GfxMapWriterV3 : IGfxMapWriter {

    private static readonly Logger logger = Logger.CreateLogger();

    private readonly IGfxMapWriterOptions options;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    public GfxMapWriterV3(IGfxMapWriterOptions options) {
        this.options = options;
    }

    /// <inheritdoc/>
    public bool Save(IGfxMap map, Stream outputStream) {

        using BinaryWriter writer = new BinaryWriter(outputStream);

        writer.Write((int)GfxVersion.V3);
        writer.Write(map.Count);

        // Notify reader what type of structure to create
        switch (map) {
            case StandardGfxMap:
                writer.Write(40);
                break;
            case PathGfxMap path:
                writer.Write(42);
                writer.Write((ushort)path.Delimiter);
                break;
            default:
                throw new InvalidOperationException("Cannot save instances of " + map.GetType().FullName);
        }

        using MemoryStream dataBuffer = new MemoryStream();
        using BinaryWriter dataWriter = new BinaryWriter(dataBuffer);

        using MemoryStream manifestBuffer = new MemoryStream();
        using BinaryWriter manifestWriter = new BinaryWriter(manifestBuffer);

        foreach (GfxResource r in map) {

            // Open for copying
            using var rs = r.Open();

            // Write identifier
            var identifierEncoded = r.Identifier.Encode(Encoding.ASCII);
            if (identifierEncoded.Any(x => x == 0)) {
                throw new IllegalFormatException("Identifier name containing terminating byte is not allowed.");
            }

            // Error if path too long
            if (identifierEncoded.Length > 255) {
                throw new PathTooLongException($"Identifier path {r.Identifier} has more than the allowed 255 characters.");
            }

            writer.Write(identifierEncoded);
            writer.Write((byte)0);

            // Write manifest
            manifestWriter.Write((ushort)r.Width);
            manifestWriter.Write((ushort)r.Height);
            manifestWriter.Write(r.GfxType switch {
                GfxResourceType.Png => (byte)'P',
                GfxResourceType.Tga => (byte)'T',
                GfxResourceType.Bmp => (byte)'B',
                GfxResourceType.Xaml => (byte)'X',
                GfxResourceType.Html => (byte)'H',
                _ => throw new InvalidDataException()
            });

            // Read resource to buffer
            var tmp = rs.ToArray();
            uint chcksum = tmp.Fold(0u, (h, b) => (uint)(h + (b << 16) * 2 + 1));

            // Write checksum
            manifestWriter.Write(chcksum);
            manifestWriter.Write(tmp.Length);

            // Write to data buffer
            dataWriter.Write(tmp);

        }

        // Write manifest
        writer.Write(manifestBuffer.ToArray());

        // Try compress
        using MemoryStream compressed = new();
        using var compressor = options.GetCompressionStream(compressed, CompressionMode.Compress);
        dataBuffer.Position = 0;
        dataBuffer.CopyTo(compressor);

        // Log compression rate
        double compressionRate = (1.0 - (compressed.Length / (double)dataBuffer.Length));
        string msg = $"""
            Gfx binary map:
                Uncompressed:   {dataBuffer.Length}
                Compressed:     {compressed.Length}
                Rate:           {(compressionRate * 100):0.00}%
                Mode:           {options.CompressionMethod}
            """;
        logger.Info(msg);

        // Write compression method
        ushort dataMode = (ushort)(compressionRate >= options.CompressionThreshold ? options.CompressionMethod : 0);
        writer.Write(dataMode);
        writer.Write(dataMode == 0 ? dataBuffer.Length : compressed.Length);
        writer.Write(dataMode == 0 ? dataBuffer.ToArray() : compressed.ToArray());

        return true;

    }

}
