using System;
using System.IO;
using System.Text;

using Battlegrounds.Util;

namespace Battlegrounds.Gfx.Writers;

/// <summary>
/// 
/// </summary>
public sealed class GfxMapWriterV1 : IGfxMapWriter {

    /// <inheritdoc/>
    public bool Save(IGfxMap map, Stream outputStream) {

        // Create writer
        using BinaryWriter writer = new BinaryWriter(outputStream);

        // Write version and resource count
        writer.Write((int)GfxVersion.V1);
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

        writer.Write(resourceBuffer.ToArray());

        return true;

    }

}
