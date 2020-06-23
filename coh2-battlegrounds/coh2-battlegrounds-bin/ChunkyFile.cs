using System;
using System.Collections.Generic;
using System.IO;

namespace coh2_battlegrounds_bin {
    
    /// <summary>
    /// Represents a chunky file containing <see cref="Chunk"/> data.
    /// </summary>
    public class ChunkyFile {

        protected byte[] m_header;
        protected byte[] m_unknowns;
        protected int m_version;
        protected List<Chunk> m_chunks;

        /// <summary>
        /// Found chunk elements in <see cref="ChunkyFile"/>.
        /// </summary>
        public Chunk[] Chunks => m_chunks.ToArray();

        /// <summary>
        /// 
        /// </summary>
        public ChunkyFile() {
            m_chunks = new List<Chunk>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public bool LoadFile(string filepath) {
            using (FileStream fs = File.OpenRead(filepath)) {
                using (BinaryReader bin = new BinaryReader(fs)) {
                    return this.LoadFile(bin);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public bool LoadFile(BinaryReader stream) {

            // Make sure the header is valid
            if (!VerifyHeader(stream)) {
                return false;
            }

            // Read the version
            m_version = stream.ReadInt32();

            // Verify version
            if (m_version != 3) {
                // TODO: Set error saying incorrect version
                return false;
            }

            // Read dummy bytes (could use a 'skip' method)
            stream.ReadBytes(4);

            // Read dummy bytes
            stream.ReadBytes(stream.ReadInt32() - 28);

            // While there's content to read
            while(stream.BaseStream.Position < stream.BaseStream.Length) {

                // Read chunk
                Chunk chunk = new Chunk(this.m_version);
                if (chunk.ReadChunk(stream)) {
                    m_chunks.Add(chunk);
                } else {
                    break; // done reading or reading into some other file content (should check the status of the chunk)
                }

            }

            // Return true
            return true;

        }

        private bool VerifyHeader(BinaryReader stream) {
            m_header = stream.ReadBytes(16);
            return ByteUtil.Match(m_header, "Relic Chunky\x0D\x0A\x1A\x00");
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dump() => this.Dump(Console.OpenStandardOutput());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public void Dump(Stream stream) {



        }

    }

}
