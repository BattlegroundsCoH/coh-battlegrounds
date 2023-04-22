using System.IO;
using System.IO.Compression;
using System.Text;

namespace Battlegrounds.Gfx.Loaders;

/// <summary>
/// Class for loading version 2 files of a <see cref="StandardGfxMap"/>.
/// </summary>
public sealed class GfxMapLoaderV2 : IGfxMapLoader {
    
    /// <inheritdoc/>
    public IGfxMap LoadGfxMap(BinaryReader reader) {

        // Get count
        int count = reader.ReadInt32();
        StandardGfxMap map = new StandardGfxMap(count) {
            GfxVersion = GfxVersion.V2
        };

        // Get resource offset
        long resourceOffset = (sizeof(int) * 3) + reader.ReadInt32();

        // Store reader location
        long current = reader.BaseStream.Position;

        // Read resource section
        reader.BaseStream.Seek(resourceOffset, SeekOrigin.Begin);

        // Get if compressed or not
        bool isCompressed = reader.ReadByte() == 1;

        // Grab resource length
        long resourcelen = reader.ReadInt64();

        // Create resource buffer
        using MemoryStream resourceBuffer = new();

        // Decompress if decompressed and write to resource buffer, otherwise 
        if (isCompressed) {

            // Grab resource buffer
            using var buffer = new MemoryStream(reader.ReadBytes((int)resourcelen));

            // Decompress
            using var decompressor = new DeflateStream(buffer, CompressionMode.Decompress);
            decompressor.CopyTo(resourceBuffer);

        } else {

            // Simply copy to resource buffer
            resourceBuffer.Write(reader.ReadBytes((int)resourcelen));

        }

        // Create resource reader
        using BinaryReader resourceReader = new(resourceBuffer);

        // Reset
        reader.BaseStream.Position = current;

        // Read resources
        for (int i = 0; i < count; i++) {

            // Read identifier
            byte[] identifierBytes = reader.ReadBytes(128);
            string id = Encoding.ASCII.GetString(identifierBytes).Trim('\0');

            // Read with and height
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();

            // Read resource data
            int resourceDataLength = reader.ReadInt32();
            long resourceDataOffset = reader.ReadInt64();

            // Jump to resource
            resourceReader.BaseStream.Position = resourceDataOffset;
            //resourceReader.BaseStream.Seek(resourceDataOffset, SeekOrigin.Begin);

            // Read resource binary
            byte[] rawBinary = resourceReader.ReadBytes(resourceDataLength);

            // Create resource
            map.CreateResource(i, rawBinary, id, width, height, GfxResourceType.Png);

        }

        return map;

    }

}
