using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace coh2_battlegrounds_bin {
    
    /// <summary>
    /// Represents a chunk of data in a <see cref="ChunkyFile"/>.
    /// </summary>
    public class Chunk {

        /// <summary>
        /// The <see cref="Chunk"/> type
        /// </summary>
        public enum ChunkyType {

            /// <summary>
            /// Unknown (Invalid)
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// Folder -> Has subtypes
            /// </summary>
            FOLD,

            /// <summary>
            /// Data chunk -> Contains raw data
            /// </summary>
            DATA,

        }

        private string m_name;
        private string m_descriptor;
        private int m_version;
        private int m_chunkyVersion;
        private byte[] m_raw;
        private ChunkyType m_type;

        private List<Chunk> m_childChunks;

        /// <summary>
        /// Raw data contained within the chunk
        /// </summary>
        public byte[] Data => m_raw;

        /// <summary>
        /// The name of the chunk
        /// </summary>
        public string Name => m_name;

        /// <summary>
        /// The descriptive text of the chunk
        /// </summary>
        public string Descriptor => m_descriptor;

        /// <summary>
        /// Create a new <see cref="Chunk"/> with parent file version supplied
        /// </summary>
        /// <param name="chunkyVersion"><see cref="ChunkyFile"/> parent version</param>
        public Chunk(int chunkyVersion) {
            this.m_name = "DATA";
            this.m_chunkyVersion = chunkyVersion;
        }

        /// <summary>
        /// Read a chunk from a <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="stream">Stream to read chunk data from</param>
        public bool ReadChunk(BinaryReader stream) {

            // Read chunk type
            m_type = this.ReadChunkyType(stream);

            // Failed to find a valid type
            if (m_type == ChunkyType.Unknown) {
                stream.BaseStream.Position -= 4; // Backtrack by 4
                return false;
            }

            // Read name
            m_name = Encoding.ASCII.GetString(stream.ReadBytes(4));

            // Read version
            m_version = stream.ReadInt32();

            // Read lengths
            uint datalength = stream.ReadUInt32();
            uint descriptorlength = stream.ReadUInt32();

            // Read some extra stuff
            if (this.m_chunkyVersion >= 3) {
                stream.ReadBytes(sizeof(uint) * 2);
            }

            if (descriptorlength > 0) {
                // Read descriptor (may be 0)
                m_descriptor = Encoding.ASCII.GetString(stream.ReadBytes((int)descriptorlength));
            }

            if (m_type == ChunkyType.FOLD) {

                // Create child chunk list
                m_childChunks = new List<Chunk>();

                // Calculate stop position
                uint stop = (uint)stream.BaseStream.Position + datalength;

                // While there are child elements
                while (stream.BaseStream.Position < stop) {

                    Chunk chunk = new Chunk(m_chunkyVersion);
                    if (chunk.ReadChunk(stream)) {
                        m_childChunks.Add(chunk);
                    } else {
                        return false;
                    }

                }

            } else if (m_type == ChunkyType.DATA) {
                m_raw = stream.ReadBytes((int)datalength);
            }

            return true;

        }

        private ChunkyType ReadChunkyType(BinaryReader reader) {

            byte[] chunkType = reader.ReadBytes(4);

            if (ByteUtil.Match(chunkType, "FOLD")) {
                return ChunkyType.FOLD;
            } else if (ByteUtil.Match(chunkType, "DATA")) {
                return ChunkyType.DATA;
            } else {
                return ChunkyType.Unknown;
            }

        }

    }

}
