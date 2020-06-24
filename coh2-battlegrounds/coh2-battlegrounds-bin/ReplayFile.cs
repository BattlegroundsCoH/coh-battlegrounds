using System;
using System.IO;
using System.Text;

namespace coh2_battlegrounds_bin {

    /// <summary>
    /// Represents a CoH2 replay file
    /// </summary>
    public sealed class ReplayFile {

        /// <summary>
        /// Represents a header of a <see cref="ReplayFile"/>
        /// </summary>
        public struct ReplayHeader {

            /// <summary>
            /// The replay version
            /// </summary>
            public uint version;

            /// <summary>
            /// The game version
            /// </summary>
            public string gamename;

            /// <summary>
            /// The date of replay save
            /// </summary>
            public string date;

            /// <summary>
            /// Create instance of replay header with specified values
            /// </summary>
            /// <param name="v"></param>
            /// <param name="n"></param>
            /// <param name="d"></param>
            public ReplayHeader(uint v, string n, string d) {
                this.version = v;
                this.gamename = n;
                this.date = d;
            }
        }

        private ReplayHeader m_replayHeader;

        private byte[] m_header;
        private string m_replayfile;

        private bool m_isParsed;

        private ChunkyFile m_scenarioChunkyFile;
        private ChunkyFile m_replayEventsChunkyFile;

        /// <summary>
        /// Check if the <see cref="ReplayFile"/> has been loaded and parsed.
        /// </summary>
        public bool IsParsed => this.m_isParsed;

        /// <summary>
        /// The header read when the file was parsed
        /// </summary>
        public ReplayHeader Header => (this.m_isParsed) ? this.m_replayHeader : throw new Exception("Replayfile has not been loaded and parsed sucessfully");

        /// <summary>
        /// New instance of a <see cref="ReplayFile"/> from a given file path
        /// </summary>
        /// <param name="file">The file path to use when loading the file</param>
        /// <exception cref="FileNotFoundException"/>
        public ReplayFile(string file) {
            this.m_isParsed = false;
            if (File.Exists(file)) {
                this.m_replayfile = file;
            } else {
                throw new FileNotFoundException(null, file);
            }
        }

        /// <summary>
        /// Load the replay file from the given file path at instantiation.
        /// </summary>
        /// <returns>True if no errors occured</returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="IOException"/>
        public bool LoadReplay() {

            using (FileStream fs = File.OpenRead(this.m_replayfile)) {
                using (BinaryReader bin = new BinaryReader(fs)) {

                    // Constant data beforehand
                    this.m_header = bin.ReadBytes(76);

                    // Parse header
                    if (!this.ParseHeader()) {
                        return false;
                    }

                    this.m_scenarioChunkyFile = new ChunkyFile();
                    if (!this.m_scenarioChunkyFile.LoadFile(bin)) {
                        Console.WriteLine("Failed to read intro");
                        return false;
                    }

                    this.m_replayEventsChunkyFile = new ChunkyFile();
                    if (!this.m_replayEventsChunkyFile.LoadFile(bin)) {
                        Console.WriteLine("Failed to read outro");
                        return false;
                    }

                    // TODO: Parse chunk data



                    this.m_isParsed = true;

                }
            }

            return true;

        }

        private bool ParseHeader() {

            uint version = BitConverter.ToUInt32(this.m_header[0 .. 4]); // read version (unsigned 32-bit integer ==> 4 bytes)
            string name = Encoding.ASCII.GetString(this.m_header[4..12]); // read game version (ASCII, 1 char = 1 byte, length is fixed and equal to 8)
            
            StringBuilder dateBuilder = new StringBuilder();

            int i = 12; // start position
            while (i < this.m_header.Length) { // UTF-8 encoding (1 char = 2 byte)
                ushort u = BitConverter.ToUInt16(this.m_header[i..(i + 2)]);
                if (u == 0) {
                    break;
                } else {
                    dateBuilder.Append((char)u);
                }
                i += 2;
            }

            // Create header
            this.m_replayHeader = new ReplayHeader(version, name, dateBuilder.ToString());

            // Return true if i was not out of bounds and the game name is "COH2_REC"
            return i < this.m_header.Length && name.Equals("COH2_REC");

        }

        /// <summary>
        /// 
        /// </summary>
        public void Dump() {
            this.m_scenarioChunkyFile.Dump();
            this.m_replayEventsChunkyFile.Dump();
        }

    }

}
