using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

using Battlegrounds.ErrorHandling.IO;
using Battlegrounds.Lua;
using Battlegrounds.Util;

namespace Battlegrounds.Gfx {

    /// <summary>
    /// Representation of a map overview of GFX resources.
    /// </summary>
    public class GfxMap {

        /// <summary>
        /// The version 1 variant of the GFX binary format
        /// </summary>
        public const int GfxBinaryVersion1 = 100;

        /// <summary>
        /// The version 2 variant of the GFX binary format
        /// </summary>
        public const int GfxBinaryVersion2 = 200;

        /// <summary>
        /// The currently used binary version for GfxMaps
        /// </summary>
        public const int GfxBinaryVersion = GfxBinaryVersion2;

        private readonly GfxResource[] m_gfxMapResources;
        private readonly string[] m_gfxMapResourceIdentifiers;

        /// <summary>
        /// Get an array of resource identifiers.
        /// </summary>
        public string[] Resources => this.m_gfxMapResourceIdentifiers;

        /// <summary>
        /// Get or initialsie the binary version of the GfxMap.
        /// </summary>
        /// <remarks>
        /// This value is ignored when invoking <see cref="AsBinary(int)"/> where the version argument will take precedence.
        /// </remarks>
        public int BinaryVersion { get; init; }

        /// <summary>
        /// Initialise a new <see cref="GfxMap"/> instance capable of holding <paramref name="n"/> resource elements.
        /// </summary>
        /// <param name="n">The amount of elements in the map.</param>
        public GfxMap(int n) {
            this.BinaryVersion = GfxBinaryVersion;
            this.m_gfxMapResources = new GfxResource[n];
            this.m_gfxMapResourceIdentifiers = new string[n];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceIndex"></param>
        /// <param name="rawBinary"></param>
        /// <param name="resourceID"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void CreateResource(int resourceIndex, byte[] rawBinary, string resourceID, double width, double height) {
            if (resourceIndex > this.m_gfxMapResources.Length) {
                throw new ArgumentOutOfRangeException(nameof(resourceIndex), resourceIndex, $"Resource index out of range (Max resource count = {this.m_gfxMapResources.Length})");
            }
            this.m_gfxMapResources[resourceIndex] = new GfxResource(resourceID, rawBinary, width, height);
            this.m_gfxMapResourceIdentifiers[resourceIndex] = resourceID;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public GfxResource GetResource(string identifier) => this.m_gfxMapResources.FirstOrDefault(x => x.IsResource(identifier));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceIndex"></param>
        /// <returns></returns>
        public GfxResource GetResource(int resourceIndex) => this.m_gfxMapResources[resourceIndex];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public byte[] AsBinary(int version = GfxBinaryVersion, float threshold = 0.0f) {

            // Check GFX version
            if (version is not (GfxBinaryVersion1 or GfxBinaryVersion2)) {
                throw new ArgumentOutOfRangeException(nameof(version), version, $"Invalid GFX version {version}. Currently only supports 100 or 200");
            }

            // Create memory buffer and binary writer
            MemoryStream memory = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(memory);

            // Write version and resource count
            writer.Write(version);
            writer.Write(this.m_gfxMapResources.Length); // file count
            writer.Write(this.m_gfxMapResources.Length * (128 + sizeof(int) * 3 + sizeof(long))); // header size

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
            if (version == GfxBinaryVersion2) {

                // Try compress
                using MemoryStream compressed = new();
                using var compressor = new DeflateStream(compressed, CompressionMode.Compress);
                resourceBuffer.Position = 0;
                resourceBuffer.CopyTo(compressor);

                // Log compression rate
                float compressionRate = (1.0f - (compressed.Length / (float)resourceBuffer.Length));
                string msg = $"Gfx binary map:\n\tUncompressed: {resourceBuffer.Length}\n\tCompressed: {compressed.Length}\n\tRate: {(compressionRate*100):0.00}%";
                Console.WriteLine(msg);

                // Check length (is compression worth it?)
                if (compressionRate >= threshold) {
                    writer.Write((byte)1); // Write Compressed = TRUE
                    writer.Write(BitConverter.GetBytes(compressed.Length));
                    writer.Write(compressed.ToArray());
                } else {
                    writer.Write((byte)0); // Write Compressed = FALSE
                    writer.Write(BitConverter.GetBytes(compressed.Length));
                    writer.Write(resourceBuffer.ToArray()); // Dump uncompressed
                }

            } else { // is GfxBinaryVersion1
                writer.Write(resourceBuffer.ToArray());
            }

            // Return memory as byte array
            return memory.ToArray();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="streamSource"></param>
        /// <returns></returns>
        /// <exception cref="InvalidFileVersionException"></exception>
        public static GfxMap FromBinary(Stream streamSource) {

            // Create reader
            BinaryReader reader = new BinaryReader(streamSource);

            // Read version
            int v = reader.ReadInt32();
            if (v is not GfxBinaryVersion1 and not GfxBinaryVersion2) {
                throw new InvalidFileVersionException(GfxBinaryVersion1, GfxBinaryVersion2);
            }

            // Get count
            int count = reader.ReadInt32();
            GfxMap map = new GfxMap(count) {
                BinaryVersion = v
            };

            // Get resource offset
            long resourceOffset = (sizeof(int) * 3) + reader.ReadInt32();

            // If version one, do that
            if (v == GfxBinaryVersion1) {
                ReadVersion1(reader, map, count, resourceOffset);
            } else if (v == GfxBinaryVersion2) { 
                ReadVersion2(reader, map, count, resourceOffset);
            }

            // Return map
            return map;

        }

        private static void ReadVersion1(BinaryReader reader, GfxMap map, int count, long offset) {

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
                reader.BaseStream.Seek(resourceDataOffset + offset, SeekOrigin.Begin);

                // Read resource binary
                byte[] rawBinary = reader.ReadBytes(resourceDataLength);

                // Create resource
                map.CreateResource(i, rawBinary, id, width, height);

                // Jump back
                reader.BaseStream.Position = current;

            }

        }

        private static void ReadVersion2(BinaryReader reader, GfxMap map, int count, long offset) {

            // Store reader location
            long current = reader.BaseStream.Position;

            // Read resource section
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);

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
                resourceReader.BaseStream.Seek(resourceDataOffset, SeekOrigin.Begin);

                // Read resource binary
                byte[] rawBinary = resourceReader.ReadBytes(resourceDataLength);

                // Create resource
                map.CreateResource(i, rawBinary, id, width, height);

            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gfxTable"></param>
        /// <param name="gfxFolder"></param>
        /// <returns></returns>
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
