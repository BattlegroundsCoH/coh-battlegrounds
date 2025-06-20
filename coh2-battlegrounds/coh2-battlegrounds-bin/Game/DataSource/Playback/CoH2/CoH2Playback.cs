﻿using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using Battlegrounds.Game.Gameplay;
using Battlegrounds.Util;
using Battlegrounds.Game.Database;
using Battlegrounds.Functional;
using System.Diagnostics.CodeAnalysis;
using Battlegrounds.Game.Scenarios;
using Battlegrounds.Logging;

namespace Battlegrounds.Game.DataSource.Playback.CoH2;

/// <summary>
/// Represents a CoH2 replay file
/// </summary>
public sealed class CoH2Playback : IPlayback {

    private static readonly Logger logger = Logger.CreateLogger();

    /// <summary>
    /// Represents a header of a <see cref="CoH2Playback"/>
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
            version = v;
            gamename = n;
            date = d;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public readonly struct ChatMessage {
        
        /// <summary>
        /// 
        /// </summary>
        public readonly TimeSpan TimeStamp { get; init; }
        
        /// <summary>
        /// 
        /// </summary>
        public readonly string Sender { get; init; }
        
        /// <summary>
        /// 
        /// </summary>
        public readonly string Content { get; init; }

    }

    private ReplayHeader m_replayHeader;
    private ScenarioDescription? m_sdsc;

    private Player[]? m_playerlist;
    private readonly List<GameTick> m_tickList;
    private readonly string m_replayfile;

    private readonly List<ChatMessage> m_chatHistory;
    private TimeSpan m_replayParsedLength;
    private MatchType m_replayMatchType;
    private uint m_seed;

    private bool m_isParsed;
    private bool m_isPartial;

    /// <summary>
    /// Get if the <see cref="CoH2Playback"/> has been loaded and parsed.
    /// </summary>
    [MemberNotNullWhen(true, nameof(m_sdsc), nameof(m_playerlist))]
    public bool IsParsed => m_isParsed;

    /// <summary>
    /// Get if the <see cref="CoH2Playback"/> was partially read
    /// </summary>
    public bool IsPartial => m_isPartial;

    /// <summary>
    /// The header read when the file was parsed
    /// </summary>
    /// <exception cref="InvalidDataException"/>
    public ReplayHeader Header
        => m_isParsed ? m_replayHeader : throw new InvalidDataException("Replayfile has not been loaded and parsed sucessfully.");

    /// <summary>
    /// The scenario used in the replay
    /// </summary>
    /// <exception cref="InvalidDataException"/>
    public ScenarioDescription Scenario
        => IsParsed ? m_sdsc : throw new InvalidDataException("Replayfile has not been loaded and parsed sucessfully.");

    /// <summary>
    /// Array containing all players in the replay
    /// </summary>
    public Player[] Players
        => IsParsed ? m_playerlist.ToArray() : throw new InvalidDataException("Replayfile has not been loaded and parsed sucessfully.");

    /// <summary>
    /// Sorted array containing all game ticks in the replay
    /// </summary>
    public IPlaybackTick[] Ticks
        => m_isParsed ? m_tickList.OrderBy(x => x.Timestamp).ToArray() : throw new InvalidDataException("Replayfile has not been loaded and parsed sucessfully.");

    /// <summary>
    /// Get the length of the match
    /// </summary>
    public TimeSpan Length => m_replayParsedLength;

    /// <summary>
    /// New instance of a <see cref="CoH2Playback"/> from a given file path
    /// </summary>
    /// <param name="file">The file path to use when loading the file</param>
    /// <exception cref="FileNotFoundException"/>
    public CoH2Playback(string file) {
        m_isParsed = false;
        m_tickList = new List<GameTick>();
        m_chatHistory = new List<ChatMessage>();
        if (File.Exists(file)) {
            m_replayfile = file;
        } else {
            throw new FileNotFoundException(null, file);
        }
    }

    /// <summary>
    /// New instance of a <see cref="CoH2Playback"/> without a specified replay filepath.
    /// </summary>
    public CoH2Playback() {
        m_isParsed = false;
        m_tickList = new List<GameTick>();
        m_chatHistory = new List<ChatMessage>();
        m_replayfile = string.Empty;
    }

    /// <summary>
    /// Load the replay file from the given file path at instantiation.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader"/> to use when reading the binary replay data</param>
    /// <returns>True if no errors occured</returns>
    /// <exception cref="IOException"/>
    public bool LoadReplay(BinaryReader reader) {
        bool res = ParseReplayBinary(reader);
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
    public bool LoadPlayback() {

        // Verify we can do this
        if (string.IsNullOrEmpty(m_replayfile)) {
            throw new ArgumentNullException(nameof(m_replayfile), "Cannot load replay without being given a valid replay file path");
        }
        
        if (!File.Exists(m_replayfile)) {
            throw new FileNotFoundException(null, m_replayfile);
        }

        // Create streams
        using FileStream fs = File.OpenRead(m_replayfile);
        using BinaryReader br = new BinaryReader(fs);

        // Parse the binary data
        if (!ParseReplayBinary(br)) {
            return false;
        }

        // Return OK -> read file
        return true;

    }

    private bool ParseReplayBinary(BinaryReader binaryReader) {

        // Parse header
        if (!ParseHeader(binaryReader.ReadBytes(76))) {
            logger.Warning("Failed to parse replay header data");
            return false;
        }

        // Read intro information
        var introchunk = new ChunkyFile();
        if (!introchunk.LoadFile(binaryReader)) {
            logger.Warning("Failed to read replay intro data");
            return false;
        }

        // Read match outro data
        var datachunk = new ChunkyFile();
        if (!datachunk.LoadFile(binaryReader)) {
            logger.Warning("Failed to read replay outro data");
            return false;
        }

        // Read scenario data
        if (!ParseScenarioDescription(datachunk)) {
            logger.Warning("Failed to read scenario data");
            return false;
        }

        // Read replay data
        if (!ParseReplayContent(datachunk, binaryReader.ReadToEnd())) {
            logger.Warning("Failed to read raw replay data");
            return false;
        }

        // Return true (and set parsed flag to true)
        return m_isParsed = true;

    }

    private bool ParseHeader(byte[] header) {

        // Get psan
        var headerSpan = header.AsSpan();

        uint version = BitConverter.ToUInt32(headerSpan[0..4]); // read version (unsigned 32-bit integer ==> 4 bytes)
        string name = Encoding.ASCII.GetString(headerSpan[4..12]); // read game version (ASCII, 1 char = 1 byte, length is fixed and equal to 8)

        StringBuilder dateBuilder = new StringBuilder();

        int i = 12; // start position
        while (i < headerSpan.Length) { // UTF-8 encoding (1 char = 2 byte)
            ushort u = BitConverter.ToUInt16(headerSpan[i..(i + 2)]);
            if (u == 0) {
                break;
            } else {
                dateBuilder.Append((char)u);
            }
            i += 2;
        }

        // Create header
        m_replayHeader = new ReplayHeader(version, name, dateBuilder.ToString());

        // Return true if i was not out of bounds and the game name is "COH2_REC"
        return i < headerSpan.Length;

    }

    private bool ParseScenarioDescription(ChunkyFile chunky) {

        // Get the scenario target chunk
        if (chunky["SDSC"] is not Chunk scenarioChunk) {
            logger.Warning("Failed to get scenario description.");
            return false;
        }

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
                m_sdsc = new ScenarioDescription(scenariopath, scenarioname, (int)scenariodimensions[0], (int)scenariodimensions[1]) {
                    Description = scenariodesc,
                };

            }
        }

        return true;

    }

    private bool ParseReplayContent(ChunkyFile chunky, byte[] replayData) {

        // Get the important chunk data
        if (chunky.Walk("INFO", "DATA") is not Chunk gameinfoChunk) {
            logger.Warning("Failed to get replay match info chunk data");
            return false;
        }

        // Parse the player data
        if (!ParseMatchdata(gameinfoChunk)) {
            return false;
        }

        // Parse the recorded data
        if (!ParseRecordedData(replayData)) {
            return false;
        }

        return true;

    }

    private bool ParseMatchdata(Chunk infoChunk) {

        using MemoryStream stream = new MemoryStream(infoChunk.Data);
        using BinaryReader reader = new BinaryReader(stream);

        if (infoChunk.Version.InRange(27, 28)) {

            // Read match type
            uint type = reader.ReadUInt32();
            if (type.InRange(1, 2)) {
                m_replayMatchType = type == 1 ? MatchType.Multiplayer : MatchType.Skirmish;
            }

            // Skip 10 bytes
            reader.Skip(10);

            // Read in the seed (random seed)
            m_seed = reader.ReadUInt32();

            // Read player count
            uint pcount = reader.ReadUInt32();
            m_playerlist = new Player[pcount];

            // Read player data
            for (int i = 0; i < m_playerlist.Length; i++) {
                if (ParsePlayerInfo(reader) is Player p) {
                    m_playerlist[i] = p;
                } else {
                    logger.Warning($"Failed to read player {i} from replay data!");
                    m_playerlist[i] = new(uint.MaxValue, ulong.MaxValue, uint.MaxValue, "Error", Faction.America, string.Empty);
                }
            }

        }

        // Return OK
        return true;

    }

    private static Player? ParsePlayerInfo(BinaryReader reader) {

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
            logger.Warning("Failed to read skin(s)");
            return null;
        }

        // Skip an additional ten bytes
        reader.Skip(10);

        // Read Steam ID
        ulong steamID = reader.ReadUInt64();

        // Read faceplate (Why is this even saved in the replay?)
        if (!ReadItem(reader, ServerItemType.Faceplate, out _)) {
            logger.Warning("Failed to read faceplate");
            return null;
        }

        // Read victory strike
        if (!ReadItem(reader, ServerItemType.VictoryStrike, out _)) {
            logger.Warning("Failed to read victory strike");
            return null;
        }

        // Read decal
        if (!ReadItem(reader, ServerItemType.Decal, out _)) {
            logger.Warning("Failed to read decal");
            return null;
        }

        // Skip the rest of the inventory stuff
        reader.SkipUntil(new byte[] { 255, 255, 255, 255 });
        reader.Skip(4);

        // Create player object
        Player player = new Player(playerID, steamID, teamID, name, Faction.FromName(faction, GameCase.CompanyOfHeroes2), aiprofile) {
            IsAIPlayer = playertype != 1
        };
        player.Skins[0] = skin1;
        player.Skins[1] = skin2;
        player.Skins[2] = skin3;

        // Return the player data
        return player;

    }

    private static bool ReadItem(BinaryReader reader, ServerItemType itemType, out ServerItem item) {

        // Read type
        var itemty = reader.ReadUInt16();

        // Determine what to read
        ServerItem? tmp = itemty switch {
            1 => ServerItem.None,
            265 => Read265(itemType, reader),
            518 => Read518(itemType, reader),
            534 => Read534(itemType, reader),
            _ => null,
        };

        // Bail
        if (tmp is null) {
            item = ServerItem.None;
            logger.Warning("Unknown server item type {itemty}!");
            return false;
        }

        // Return result
        item = tmp.Value;
        return true;

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

    private bool ParseRecordedData(byte[] replayContent) {

        using (MemoryStream stream = new MemoryStream(replayContent)) {
            using (BinaryReader reader = new BinaryReader(stream)) {

                while (!reader.HasReachedEOS()) {

                    uint dataType = reader.ReadUInt32();
                    uint dataSize = reader.ReadUInt32();

                    if (dataSize == 0)
                        continue;

                    if (dataType == 0) {

                        GameTick tick = new GameTick();
                        tick.Parse(reader);

                        m_tickList.Add(tick);
                        m_isPartial = true;

                        if (tick.Timestamp > m_replayParsedLength) {
                            m_replayParsedLength = tick.Timestamp;
                        }

                    } else if (dataType == 1) { // Chat

                        uint mode = reader.ReadUInt32();
                        if (mode == 1) {
                            reader.Skip(8);
                            string sender = reader.ReadASCIIString();
                            string content = reader.ReadASCIIString();
                            logger.Info($"{sender}: {content}");
                            m_chatHistory.Add(new ChatMessage() {
                                Sender = sender,
                                Content = content,
                                TimeStamp = m_replayParsedLength
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
