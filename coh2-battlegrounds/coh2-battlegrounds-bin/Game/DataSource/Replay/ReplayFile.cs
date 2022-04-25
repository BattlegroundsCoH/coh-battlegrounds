using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

using Battlegrounds.Game.Gameplay;
using Battlegrounds.Util;
using Battlegrounds.Game.Database;
using Battlegrounds.Functional;

namespace Battlegrounds.Game.DataSource.Replay;

/// <summary>
/// Enum flag marking match type
/// </summary>
public enum MatchType {

    /// <summary>
    /// PVP
    /// </summary>
    Multiplayer = 1,
    
    /// <summary>
    /// Skirmish (vs AI)
    /// </summary>
    Skirmish = 2,

}

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

    public readonly struct ChatMessage {
        public readonly TimeSpan TimeStamp { get; init; }
        public readonly string Sender { get; init; }
        public readonly string Content { get; init; }
    }

    private ReplayHeader m_replayHeader;
    private ScenarioDescription m_sdsc;

    private Player[] m_playerlist;
    private List<GameTick> m_tickList;

    private byte[] m_header;
    private byte[] m_replaycontent;
    private string m_replayfile;

    private List<ChatMessage> m_chatHistory;
    private TimeSpan m_replayParsedLength;
    private MatchType m_replayMatchType;
    private uint m_seed;

    private bool m_isParsed;
    private bool m_isPartial;

    private ChunkyFile m_scenarioChunkyFile;
    private ChunkyFile m_replayEventsChunkyFile;

    /// <summary>
    /// Get if the <see cref="ReplayFile"/> has been loaded and parsed.
    /// </summary>
    public bool IsParsed => this.m_isParsed;

    /// <summary>
    /// Get if the <see cref="ReplayFile"/> was partially read
    /// </summary>
    public bool IsPartial => this.m_isPartial;

    /// <summary>
    /// The header read when the file was parsed
    /// </summary>
    /// <exception cref="InvalidDataException"/>
    public ReplayHeader Header
        => (this.m_isParsed) ? this.m_replayHeader : throw new InvalidDataException("Replayfile has not been loaded and parsed sucessfully.");

    /// <summary>
    /// The scenario used in the replay
    /// </summary>
    /// <exception cref="InvalidDataException"/>
    public ScenarioDescription Scenario
        => (this.m_isParsed) ? this.m_sdsc : throw new InvalidDataException("Replayfile has not been loaded and parsed sucessfully.");

    /// <summary>
    /// Array containing all players in the replay
    /// </summary>
    public Player[] Players
        => (this.m_isParsed) ? this.m_playerlist.ToArray() : throw new InvalidDataException("Replayfile has not been loaded and parsed sucessfully.");

    /// <summary>
    /// Sorted array containing all game ticks in the replay
    /// </summary>
    public GameTick[] Ticks
        => (this.m_isParsed) ? this.m_tickList.OrderBy(x => x.TimeStamp).ToArray() : throw new InvalidDataException("Replayfile has not been loaded and parsed sucessfully.");

    /// <summary>
    /// New instance of a <see cref="ReplayFile"/> from a given file path
    /// </summary>
    /// <param name="file">The file path to use when loading the file</param>
    /// <exception cref="FileNotFoundException"/>
    public ReplayFile(string file) {
        this.m_isParsed = false;
        this.m_tickList = new List<GameTick>();
        this.m_chatHistory = new List<ChatMessage>();
        if (File.Exists(file)) {
            this.m_replayfile = file;
        } else {
            throw new FileNotFoundException(null, file);
        }
    }

    /// <summary>
    /// New instance of a <see cref="ReplayFile"/> without a specified replay filepath.
    /// </summary>
    public ReplayFile() {
        this.m_isParsed = false;
        this.m_tickList = new List<GameTick>();
        this.m_chatHistory = new List<ChatMessage>();
        this.m_replayfile = null;
    }

    /// <summary>
    /// Load the replay file from the given file path at instantiation.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader"/> to use when reading the binary replay data</param>
    /// <returns>True if no errors occured</returns>
    /// <exception cref="IOException"/>
    public bool LoadReplay(BinaryReader reader) {
        bool res = this.ParseReplayBinary(reader);
        reader.Close();
        return res;
    }

    /// <summary>
    /// Load the replay file from the given file path at instantiation.
    /// </summary>
    /// <returns>True if no errors occured</returns>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="FileNotFoundException"/>
    /// <exception cref="IOException"/>
    public bool LoadReplay() {

        // Verify we can do this
        if (string.IsNullOrEmpty(this.m_replayfile)) {
            throw new ArgumentNullException(nameof(m_replayfile), "Cannot load replay without being given ");
        } else if (!File.Exists(this.m_replayfile)) {
            throw new FileNotFoundException(null, this.m_replayfile);
        }

        // Create streams
        using FileStream fs = File.OpenRead(this.m_replayfile);
        using BinaryReader br = new BinaryReader(fs);

        // Parse the binary data
        if (!this.ParseReplayBinary(br)) {
            return false;
        }

        return true;

    }

    private bool ParseReplayBinary(BinaryReader binaryReader) {

        // Constant data beforehand
        this.m_header = binaryReader.ReadBytes(76);

        // Parse header
        if (!this.ParseHeader()) {
            Trace.WriteLine("Failed to parse replay header data", "ReplayFile");
            return false;
        }

        this.m_scenarioChunkyFile = new ChunkyFile();
        if (!this.m_scenarioChunkyFile.LoadFile(binaryReader)) {
            Trace.WriteLine("Failed to read replay intro data", "ReplayFile");
            return false;
        }

        this.m_replayEventsChunkyFile = new ChunkyFile();
        if (!this.m_replayEventsChunkyFile.LoadFile(binaryReader)) {
            Trace.WriteLine("Failed to read replay outro data", "ReplayFile");
            return false;
        }

        this.m_replaycontent = binaryReader.ReadToEnd();

        if (!this.ParseScenarioDescription()) {
            Trace.WriteLine("Failed to read scenario data", "ReplayFile");
            return false;
        }

        if (!this.ParseReplayContent()) {
            Trace.WriteLine("Failed to read raw replay data", "ReplayFile");
            return false;
        }

        return this.m_isParsed = true;

    }

    private bool ParseHeader() {

        uint version = BitConverter.ToUInt32(this.m_header.AsSpan()[0..4]); // read version (unsigned 32-bit integer ==> 4 bytes)
        string name = Encoding.ASCII.GetString(this.m_header[4..12]); // read game version (ASCII, 1 char = 1 byte, length is fixed and equal to 8)

        StringBuilder dateBuilder = new StringBuilder();

        int i = 12; // start position
        while (i < this.m_header.Length) { // UTF-8 encoding (1 char = 2 byte)
            ushort u = BitConverter.ToUInt16(this.m_header.AsSpan()[i..(i + 2)]);
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

    private bool ParseScenarioDescription() {

        // Get the scenario target chunk
        Chunk scenarioChunk = this.m_replayEventsChunkyFile["SDSC"];

        // Read data
        using (MemoryStream stream = new MemoryStream(scenarioChunk.Data)) {
            using (BinaryReader reader = new BinaryReader(stream)) {

                // Skip first 28 bytes (no idea what that is yet)
                reader.Skip(28);

                // Scenario path
                string scenariopath = reader.ReadASCIIString();

                // Skip another 28 bytes
                reader.Skip(28);

                // Read the scenario name
                string scenarioname = reader.ReadUTF8String();

                reader.Skip(4);

                // Read the scenario description
                string scenariodesc = reader.ReadUTF8String();

                reader.Skip(4);

                // Read the scenario dimensions
                uint[] scenariodimensions = new uint[2]; // Player count could potentially be just before this
                scenariodimensions[0] = reader.ReadUInt32();
                scenariodimensions[1] = reader.ReadUInt32();

                reader.Skip(4);

                // If we continue to read, we can get some mod data (asset packs)

                // Create scenario description
                this.m_sdsc = new ScenarioDescription(scenariopath, scenarioname, (int)scenariodimensions[0], (int)scenariodimensions[1]) {
                    Description = scenariodesc,
                };

            }
        }

        return true;

    }

    private bool ParseReplayContent() {

        // Get the important chunk data
        Chunk gameinfoChunk = this.m_replayEventsChunkyFile["INFO"]["DATA"];

        // Parse the player data
        if (!this.ParseMatchdata(gameinfoChunk)) {
            return false;
        }

        // Parse the recorded data
        if (!this.ParseRecordedData()) {
            return false;
        }

        return true;

    }

    private bool ParseMatchdata(Chunk infoChunk) {

        using (MemoryStream stream = new MemoryStream(infoChunk.Data)) {
            using (BinaryReader reader = new BinaryReader(stream)) {

                if (infoChunk.Version.InRange(27, 28)) {

                    // Read match type
                    uint type = reader.ReadUInt32();
                    if (type.InRange(1, 2)) {
                        this.m_replayMatchType = type == 1 ? MatchType.Multiplayer : MatchType.Skirmish;
                    }

                    // Skip 10 bytes
                    reader.Skip(10);

                    // Read in the seed
                    this.m_seed = reader.ReadUInt32();

                    // Read player count
                    uint pcount = reader.ReadUInt32();
                    this.m_playerlist = new Player[pcount];

                    // Read player data
                    for (int i = 0; i < this.m_playerlist.Length; i++) {
                        this.m_playerlist[i] = ParsePlayerInfo(reader);
                    }

                }

            }
        }

        return true;

    }

    private static Player ParsePlayerInfo(BinaryReader reader) {

        // Read playertype
        byte playertype = reader.ReadByte();

        // Read player name, team, and faction
        string name = reader.ReadUTF8String(reader.ReadUInt32());
        uint teamID = reader.ReadUInt32();
        string faction = reader.ReadASCIIString();

        // Skip the next eight bytes (unknown)
        reader.Skip(8);

        // Profile and player ID
        string aiprofile = reader.ReadASCIIString();
        uint playerID = reader.ReadUInt32();

        // Skip next 14 bytes
        reader.Skip(14);

        // Read skins
        const ServerItemType skin = ServerItemType.Skin;
        if (!ReadItem(reader, skin, out ServerItem skin1) || !ReadItem(reader, skin, out ServerItem skin2) || !ReadItem(reader, skin, out ServerItem skin3)) {
            return null;
        }

        // Skip an additional two bytes
        reader.Skip(10);

        // Read Steam ID
        ulong steamID = reader.ReadUInt64();

        // Read faceplate (Why is this even saved in the replay?)
        if (!ReadItem(reader, ServerItemType.Faceplate, out _)) {
            return null;
        }

        // Read victory strike
        if (!ReadItem(reader, ServerItemType.Faceplate, out _)) {
            return null;
        }

        // Read decal
        if (!ReadItem(reader, ServerItemType.Faceplate, out _)) {
            return null;
        }

        // Skip the rest of the inventory stuff
        reader.SkipUntil(new byte[] { 255, 255, 255, 255 });
        reader.Skip(4);

        // Create player object
        Player player = new Player(playerID, steamID, teamID, name, Faction.FromName(faction), aiprofile) {
            IsAIPlayer = playertype != 1
        };
        player.Skins[0] = skin1;
        player.Skins[1] = skin2;
        player.Skins[2] = skin3;

        // Return the player data
        return player;

    }

    private static bool ReadItem(BinaryReader reader, ServerItemType itemType, out ServerItem item) {
        ServerItem? tmp = reader.ReadUInt16() switch {
            1 => ServerItem.None,
            265 => Read265(itemType, reader),
            518 => Read518(itemType, reader),
            534 => Read534(itemType, reader),
            _ => null,
        };
        if (tmp is null) {
            item = ServerItem.None;
            return false;
        } else {
            item = tmp.Value;
            return true;
        }
    }

    private static ServerItem? Read265(ServerItemType itemType, BinaryReader reader) {
        reader.Skip(8);
        var item = new ServerItem(itemType, reader.ReadUInt32());
        reader.Skip(4);
        reader.Skip(reader.ReadUInt16());
        return item;
    }

    private static ServerItem? Read518(ServerItemType itemType, BinaryReader reader) {
        reader.Skip(5); // TODO: Read PBGID
        return new ServerItem(itemType, uint.MaxValue);
    }

    private static ServerItem? Read534(ServerItemType itemType, BinaryReader reader) {
        reader.Skip(21);
        return new ServerItem(itemType, uint.MaxValue);
    }

    private bool ParseRecordedData() {

        using (MemoryStream stream = new MemoryStream(this.m_replaycontent)) {
            using (BinaryReader reader = new BinaryReader(stream)) {

                while (!reader.HasReachedEOS()) {

                    uint dataType = reader.ReadUInt32();
                    uint dataSize = reader.ReadUInt32();

                    if (dataSize == 0)
                        continue;

                    if (dataType == 0) {

                        GameTick tick = new GameTick();
                        tick.Parse(reader);

                        this.m_tickList.Add(tick);
                        this.m_isPartial = true;

                        if (tick.TimeStamp > this.m_replayParsedLength) {
                            this.m_replayParsedLength = tick.TimeStamp;
                        }

                    } else if (dataType == 1) { // Chat

                        uint mode = reader.ReadUInt32();
                        if (mode == 1) {
                            reader.Skip(8);
                            string sender = reader.ReadASCIIString();
                            string content = reader.ReadASCIIString();
                            Trace.WriteLine("{sender}: {content}", nameof(ReplayFile));
                            this.m_chatHistory.Add(new ChatMessage() {
                                Sender = sender,
                                Content = content,
                                TimeStamp = this.m_replayParsedLength
                            });
                            reader.Skip(2 * reader.ReadUInt32());
                        } else {
                            reader.Skip(12);
                        }

                    } else {

                        return false;

                    }

                }

            }
        }

        return true;

    }

}
