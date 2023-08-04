using System;
using System.IO;
using System.IO.Compression;
using System.Text;

using Battlegrounds.Functional;
using Battlegrounds.Gfx.Writers;
using Battlegrounds.Util;

namespace Battlegrounds.Gfx.Loaders;

/// <summary>
/// Class for loading version 3 files of a <see cref="StandardGfxMap"/>.
/// </summary>
public sealed class GfxMapLoaderV3 : IGfxMapLoader {

    private record FileManifest(string Identifier, ushort Width, ushort Height, GfxResourceType GfxType, int Length, uint Checksum);

    /// <inheritdoc/>
    public IGfxMap LoadGfxMap(BinaryReader reader) {

        // Read count and type
        int count = reader.ReadInt32();
        int mapType = reader.ReadInt32();

        // Read delimiter
        char delim = ',';
        if (mapType == 42) {
            delim = (char)reader.ReadUInt16();
        }

        // Create map
        IGfxMap map = mapType switch {
            40 => new StandardGfxMap(count),
            42 => new PathGfxMap() { Delimiter = delim },
            _ => throw new InvalidDataException("Invalid map type: " + mapType)
        };

        // Alloc manifest data
        FileManifest[] manifests = new FileManifest[count];

        // Load in resource identifiers
        byte[] identifierContainer = new byte[256];
        for (int i = 0; i < manifests.Length; i++) {
            for (int j = 0; j < count; j++) {
                identifierContainer[j] = reader.ReadByte();
                if (identifierContainer[j] == 0) {
                    manifests[i] = new FileManifest(Encoding.ASCII.GetString(identifierContainer[..j]), 0, 0, GfxResourceType.Png, 0, 0);
                    break;
                }
            }
        }

        // Fill manifest data
        for (int i = 0; i < manifests.Length; i++) {
            ushort width = reader.ReadUInt16();
            ushort height = reader.ReadUInt16();
            GfxResourceType gfxType = reader.ReadByte() switch {
                (byte)'P' => GfxResourceType.Png,
                (byte)'T' => GfxResourceType.Tga,
                (byte)'B' => GfxResourceType.Bmp,
                (byte)'X' => GfxResourceType.Xaml,
                (byte)'H' => GfxResourceType.Html,
                byte gfxTypeByte => throw new InvalidDataException($"Invalid gfx resource type: 0x{gfxTypeByte:X2}")
            };
            uint checksum = reader.ReadUInt32();
            int len = reader.ReadInt32();
            manifests[i] = manifests[i] with {
                GfxType = gfxType,
                Width = width,
                Height = height,
                Length = len,
                Checksum = checksum
            };
        }

        // Read mode and buffer length
        GfxMapCompressionType mode = (GfxMapCompressionType)reader.ReadUInt16();
        long imageBufferLength = reader.ReadInt64();

        // Create resource buffer
        using MemoryStream resourceBuffer = new();

        // Grab
        switch (mode) {
            case GfxMapCompressionType.None:
                reader.ReadInto(resourceBuffer, imageBufferLength);
                resourceBuffer.Position = 0;
                break;
            default: {

                // Grab resource buffer
                using var buffer = new MemoryStream();
                reader.ReadInto(buffer, imageBufferLength);

                // Decompress
                using var decompressor = GetDecompressionStream(buffer, mode);
                decompressor.CopyTo(resourceBuffer);
                resourceBuffer.Position = 0;

                break;
            }
        }

        // Attach reader
        using BinaryReader resourceReader = new BinaryReader(resourceBuffer);

        // Iterate and add
        for (int i = 0; i < manifests.Length; i++) {

            // Read data
            byte[] data = resourceReader.ReadBytes(manifests[i].Length);
            uint chcksum = data.Fold(0u, (h, b) => (uint)(h + (b << 16) * 2 + 1));
            if (chcksum != manifests[i].Checksum) {
                throw new InvalidDataException($"The checksum value for resource {manifests[i].Identifier} did not match expected checksum {manifests[i].Checksum}; got {chcksum}");
            }

            // Create
            map.CreateResource(i, data, manifests[i].Identifier, manifests[i].Width, manifests[i].Height, manifests[i].GfxType);

        }

        // Return map
        return map;

    }

    private static Stream GetDecompressionStream(Stream source, GfxMapCompressionType mode) => mode switch {
        GfxMapCompressionType.Deflate => new DeflateStream(source, CompressionMode.Decompress),
        GfxMapCompressionType.Brotli => new BrotliStream(source, CompressionMode.Decompress),
        GfxMapCompressionType.Gzip => new GZipStream(source, CompressionMode.Decompress),
        _ => throw new NotSupportedException($"Compression mode {mode:X2} not supported.")
    };

}
