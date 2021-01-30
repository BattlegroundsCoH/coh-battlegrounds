using System.Collections.Generic;
using System.IO;
using System.Linq;
using Battlegrounds.Util;

namespace Battlegrounds.Game.DataSource {
    
    /// <summary>
    /// Represents a chunky file containing <see cref="Chunk"/> data.
    /// </summary>
    public class ChunkyFile {

        protected byte[] m_header;
        protected byte[] m_unknowns;
        protected int m_version;
        protected List<Chunk> m_chunks;

        /// <summary>
        /// 
        /// </summary>
        public ChunkyFile() {
            this.m_chunks = new List<Chunk>();
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
            if (!this.VerifyHeader(stream)) {
                return false;
            }

            // Read the version
            this.m_version = stream.ReadInt32();

            // Verify version
            if (this.m_version != 3) {
                return false;
            }

            // Skip 4 bytes
            stream.Skip(4);

            // Read dummy bytes
            stream.ReadBytes(stream.ReadInt32() - 28);

            // While there's content to read
            while(stream.BaseStream.Position < stream.BaseStream.Length) {

                // Read chunk
                Chunk chunk = new Chunk(this.m_version);
                if (chunk.ReadChunk(stream)) {
                    this.m_chunks.Add(chunk);
                } else {
                    break; // done reading or reading into some other file content (should check the status of the chunk)
                }

            }

            // Return true
            return true;

        }

        private bool VerifyHeader(BinaryReader stream) {
            this.m_header = stream.ReadBytes(16);
            return ByteUtil.Match(this.m_header, "Relic Chunky\x0D\x0A\x1A\x00");
        }

        /// <summary>
        /// Lookup a root chunk by name in the chunk file
        /// </summary>
        /// <param name="name">The name of the chunk to find</param>
        /// <returns>The first chunk with name or null</returns>
        public Chunk this[string name] => this.m_chunks.FirstOrDefault(x => x.Name.CompareTo(name) == 0);

    }

}
