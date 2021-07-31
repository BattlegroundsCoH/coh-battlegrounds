using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Battlegrounds.Lua;
using Battlegrounds.Util;

namespace Battlegrounds.Gfx {

    /// <summary>
    /// Representation of a map overview of GFX resources.
    /// </summary>
    public class GfxMap {

        /// <summary>
        /// The currently used binary version for GfxMaps
        /// </summary>
        public const int GfxBinaryVersion = 100;

        private GfxResource[] m_gfxMapResources;
        private string[] m_gfxMapResourceIdentifiers;

        /// <summary>
        /// Get an array of resource identifiers.
        /// </summary>
        public string[] Resources => this.m_gfxMapResourceIdentifiers;

        public GfxMap(int elements) {
            this.m_gfxMapResources = new GfxResource[elements];
            this.m_gfxMapResourceIdentifiers = new string[elements];
        }

        public void CreateResource(int resourceIndex, byte[] rawBinary, string resourceID, double width, double height) {
            this.m_gfxMapResources[resourceIndex] = new GfxResource(resourceID, rawBinary, width, height);
            this.m_gfxMapResourceIdentifiers[resourceIndex] = resourceID;
        }

        public GfxResource GetResource(string identifier) => this.m_gfxMapResources.FirstOrDefault(x => x.IsResource(identifier));

        public byte[] AsBinary() {

            // Create memory buffer and binary writer
            MemoryStream memory = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(memory);

            // Write version and resource count
            writer.Write(GfxBinaryVersion);
            writer.Write(this.m_gfxMapResources.Length);
            writer.Write(this.m_gfxMapResources.Length * (128 + sizeof(int) * 3 + sizeof(long)));

            // Create resource buffer and binary reader
            MemoryStream resourceBuffer = new MemoryStream();
            BinaryWriter resourceBufferWriter = new BinaryWriter(resourceBuffer);

            // Keep track of resource offset (from end of resource headers)
            long resourceOffset = 0;

            // Write simple data
            foreach (GfxResource resource in this.m_gfxMapResources) {

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

            // Dump resource buffer
            writer.Write(resourceBuffer.ToArray());

            // Return memory as byte array
            return memory.ToArray();

        }

        public static GfxMap FromBinary(MemoryStream memoryStream) {

            // Create reader
            BinaryReader reader = new BinaryReader(memoryStream);

            // Read version
            if (reader.ReadInt32() != GfxBinaryVersion) {
                return null;
            }

            // Get count
            int count = reader.ReadInt32();
            GfxMap map = new GfxMap(count);

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
                map.CreateResource(i, rawBinary, id, width, height);

                // Jump back
                reader.BaseStream.Position = current;

            }

            // Return map
            return map;

        }

        public static GfxMap FromLua(LuaTable gfxTable, string gfxFolder) {

            // Create atlas
            var gfxMap = new GfxMap(gfxTable.Size);

            // Resource counter
            int resCntr = 0;

            // Run through all elements in table
            gfxTable.Pairs((k, v) => {

                // Get string
                string gfxID = k.Str();

                // If actual resource table
                if (v is LuaTable vt) {

                    // Get dimensions
                    double width = vt["width"] as LuaNumber;
                    double height = vt["height"] as LuaNumber;

                    // Get gfx source
                    string source = vt["gfx"].Str().Replace('/', '\\'); ;

                    // Get GFX source path
                    string sourcePath = Path.Combine(gfxFolder, source);

                    // Make sure file exists
                    if (File.Exists(sourcePath)) {
                        gfxMap.CreateResource(resCntr, File.ReadAllBytes(sourcePath), gfxID, width, height);
                    } else {
                        Trace.WriteLine($"Unable to locate file '{sourcePath}' - SKIPPING.", nameof(GfxMap));
                    }

                };

                // Up resource counter
                resCntr++;

            });

            // Return atlas
            return gfxMap;

        }

    }

}
