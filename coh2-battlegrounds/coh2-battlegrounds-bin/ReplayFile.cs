using System;
using System.IO;
using System.Text;

namespace coh2_battlegrounds_bin {

    /// <summary>
    /// Represents a CoH2 replay file
    /// </summary>
    public class ReplayFile {

        /// <summary>
        /// Represents a header of a <see cref="ReplayFile"/>
        /// </summary>
        public struct ReplayerHeader {

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
            public ReplayerHeader(uint v, string n, string d) {
                this.version = v;
                this.gamename = n;
                this.date = d;
            }
        }

        private ReplayerHeader m_replayHeader;

        private byte[] m_header;
        private string m_replayfile;

        private bool m_isParsed;

        private ChunkyFile m_scenarioChunkyFile;
        private ChunkyFile m_replayEventsChunkyFile;

        /// <summary>
        /// Check if the <see cref="ReplayFile"/> has been loaded and parsed.
        /// </summary>
        public bool IsParsed => m_isParsed;

        /// <summary>
        /// 
        /// </summary>
        public ReplayerHeader Header => m_replayHeader;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        public ReplayFile(string file) {
            this.m_isParsed = false;
            if (File.Exists(file)) {
                this.m_replayfile = file;
            } else {
                throw new FileNotFoundException(null, file);
            }
        }

        /// <summary>
        /// Load the replay file from the given file path
        /// </summary>
        /// <returns>True if no errors occured</returns>
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

            uint version = BitConverter.ToUInt32(m_header[0 .. 4]); // read version (unsigned 32-bit integer ==> 4 bytes)
            string name = Encoding.ASCII.GetString(m_header[4..12]); // read game version (ASCII, 1 char = 1 byte, length is fixed and equal to 8)
            
            StringBuilder dateBuilder = new StringBuilder();

            int i = 12; // start position
            while (i < m_header.Length) { // UTF-8 encoding (1 char = 2 byte)
                ushort u = BitConverter.ToUInt16(m_header[i..(i + 2)]);
                if (u == 0) {
                    break;
                } else {
                    dateBuilder.Append((char)u);
                }
                i += 2;
            }

            m_replayHeader = new ReplayerHeader(version, name, dateBuilder.ToString());

            return true;

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
