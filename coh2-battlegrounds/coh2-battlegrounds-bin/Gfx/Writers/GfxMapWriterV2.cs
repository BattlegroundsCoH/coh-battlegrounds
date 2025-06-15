using System;
using System.IO;
using System.IO.Compression;
using System.Text;

using Battlegrounds.Logging;
using Battlegrounds.Util;

namespace Battlegrounds.Gfx.Writers;

/// <summary>
/// 
/// </summary>
public sealed class GfxMapWriterV2 : IGfxMapWriter {

    private static readonly Logger logger = Logger.CreateLogger();

    private readonly bool useFix;
    private readonly IGfxMapWriterOptions options;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="useFix"></param>
    /// <param name="options"></param>
    public GfxMapWriterV2(bool useFix, IGfxMapWriterOptions options) {
        this.useFix = useFix;
        this.options = options;
    }

    /// <inheritdoc/>
    public bool Save(IGfxMap map, Stream outputStream) {

        // Create writer
        using BinaryWriter writer = new BinaryWriter(outputStream);

        // Write version and resource count
        writer.Write((int)(useFix ? GfxVersion.V2_1 : GfxVersion.V2));
        writer.Write(map.Capacity); // file count
        writer.Write(map.Capacity * (128 + sizeof(int) * 3 + sizeof(long))); // header size

        // Create resource buffer and binary reader
        MemoryStream resourceBuffer = new MemoryStream();
        BinaryWriter resourceBufferWriter = new BinaryWriter(resourceBuffer);

        // Keep track of resource offset (from end of resource headers)
        long resourceOffset = 0;

        // Write simple data
        foreach (GfxResource resource in map) {

            // Make sure there's a resource available
            if (resource is not null) {

                // Open Resource
                var resStream = resource.Open();

                // Read resource
                byte[] buffer = resStream.ToArray();

                // Write resource data
                byte[] resourceID = resStream.ResourceIdentifier.Encode();
                byte[] idBuffer = new byte[128];
                Array.Copy(resourceID, idBuffer, resourceID.Length);

                // Write resource header
                writer.Write(idBuffer);
                writer.Write(resStream.ResourceImageWidth);
                writer.Write(resStream.ResourceImageHeight);
                writer.Write(buffer.Length);
                writer.Write(resourceOffset);

                // Read buffer
                resourceBufferWriter.Write(buffer);

                // Increment offset
                resourceOffset += buffer.Length;

            } else {

                // Write som null stuff
                writer.Write(new byte[128]);
                writer.Write(0);
                writer.Write(0);
                writer.Write(4);
                writer.Write(resourceOffset);
                resourceBufferWriter.Write("NULL".Encode(Encoding.ASCII));
                resourceOffset += 4;

            }

        }

        if (useFix) {

            // TOOD:

        }

        // Try compress
        using MemoryStream compressed = new();
        using var compressor = new DeflateStream(compressed, CompressionMode.Compress);
        resourceBuffer.Position = 0;
        resourceBuffer.CopyTo(compressor);

        // Log compression rate
        float compressionRate = (1.0f - (compressed.Length / (float)resourceBuffer.Length));
        string msg = $"Gfx binary map:\n\tUncompressed: {resourceBuffer.Length}\n\tCompressed: {compressed.Length}\n\tRate: {(compressionRate * 100):0.00}%";
        logger.Info(msg);

        // Check length (is compression worth it?)
        if (compressionRate >= options.CompressionThreshold) {
            writer.Write((byte)1); // Write Compressed = TRUE
            writer.Write(BitConverter.GetBytes(compressed.Length));
            writer.Write(compressed.ToArray());
        } else {
            writer.Write((byte)0); // Write Compressed = FALSE
            writer.Write(BitConverter.GetBytes(resourceBuffer.Length));
            writer.Write(resourceBuffer.ToArray()); // Dump uncompressed
        }

        return true;

    }

}
