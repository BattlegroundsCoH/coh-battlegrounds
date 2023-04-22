using System.IO;
using System.Text;

namespace Battlegrounds.Gfx.Loaders;

/// <summary>
/// Class for loading version 1 files of a <see cref="StandardGfxMap"/>.
/// </summary>
public sealed class GfxMapLoaderV1 : IGfxMapLoader {

    /// <inheritdoc/>
    public IGfxMap LoadGfxMap(BinaryReader reader) {

        // Get count
        int count = reader.ReadInt32();
        StandardGfxMap map = new StandardGfxMap(count) {
            GfxVersion = GfxVersion.V1
        };

        // Get resource offset
        long resourceOffset = (sizeof(int) * 3) + reader.ReadInt32();

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
            long current = reader.BaseStream.Position;
            reader.BaseStream.Seek(resourceDataOffset + resourceOffset, SeekOrigin.Begin);

            // Read resource binary
            byte[] rawBinary = reader.ReadBytes(resourceDataLength);

            // Create resource
            map.CreateResource(i, rawBinary, id, width, height, GfxResourceType.Png);

            // Jump back
            reader.BaseStream.Position = current;

        }

        // Return loaded map
        return map;

    }

}
