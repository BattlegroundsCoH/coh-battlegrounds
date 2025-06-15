using System;
using System.IO;
using System.Text;

using Battlegrounds.Game.DataSource.Gamedata;
using Battlegrounds.Game.DataSource.Gamedata.CoH3;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Logging;
using Battlegrounds.Util;

namespace Battlegrounds.Game.DataSource.Playback.CoH3;

/// <summary>
/// 
/// </summary>
public class CoH3Playback : IPlayback {

    private static readonly Logger logger = Logger.CreateLogger();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Version"></param>
    /// <param name="Game"></param>
    /// <param name="Date"></param>
    public record struct Header(ushort Version, string Game, string Date);

    /// <summary>
    /// file location of playback file
    /// </summary>
    protected string playbackFile;

    /// <summary>
    /// Header struct
    /// </summary>
    protected Header header;

    /// <summary>
    /// Internal flag setting if partially read
    /// </summary>
    protected bool isPartial;

    /// <summary>
    /// 
    /// </summary>
    protected IPlaybackTick[] ticks;

    /// <summary>
    /// 
    /// </summary>
    protected TimeSpan length;

    /// <summary>
    /// 
    /// </summary>
    protected Player[] players;

    /// <summary>
    /// 
    /// </summary>
    protected MatchType matchType;

    /// <summary>
    /// 
    /// </summary>
    public CoH3Playback() { 
        this.playbackFile = string.Empty;
        this.players = Array.Empty<Player>();
        this.ticks = Array.Empty<IPlaybackTick>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="playbackFile"></param>
    public CoH3Playback(string playbackFile) : this() { 
        this.playbackFile = playbackFile;
    }

    ///<inheritdoc/>
    public bool IsPartial => isPartial;

    ///<inheritdoc/>
    public Player[] Players => players;

    ///<inheritdoc/>
    public IPlaybackTick[] Ticks => ticks;

    ///<inheritdoc/>
    public TimeSpan Length => length;

    /// <inheritdoc/>
    public bool LoadPlayback() {

        // Ensure we have something to read
        if (string.IsNullOrEmpty(playbackFile))
            return false;

        // Ensure specified file actually exists
        if (!File.Exists(playbackFile)) {
            throw new FileNotFoundException(null, playbackFile);
        }

        // Create streams
        using FileStream fs = File.OpenRead(playbackFile);
        using BinaryReader br = new BinaryReader(fs);

        // Parse the binary data
        if (!ParsePlaybackBinary(br)) {
            return false;
        }

        // Return true => playback data in memory
        return true;

    }

    private bool ParsePlaybackBinary(BinaryReader br) {

        // Read header
        if (!ParseHeader(br.ReadBytes(76))) {
            return false;
        }

        // Read info chunky
        IChunky infoChunky = new Chunky();
        if (!infoChunky.Load(br)) {
            return false;
        }

        infoChunky.DumpJson("playback_info.json");

        // Read replay chunky
        IChunky playbackChunky = new Chunky();
        if (!playbackChunky.Load(br)) {
            return false;
        }

        playbackChunky.DumpJson("playback_playback.json");

        logger.Debug("Playback offset: " + br.BaseStream.Position + " | 0x" + br.BaseStream.Position.ToString("X0"));

        // Get playback data
        byte[] replaydata = br.ReadToEnd();

        // Dump playback data
        File.WriteAllBytes("playback_raw.bin", replaydata);

        // Load info
        IChunk? infodata = playbackChunky.Walk("INFO", "DATA");
        if (infodata is null) {
            logger.Info("Failed finding playback info data section: " + this.playbackFile);
            return false;
        }

        if (!ParsePlaybackInfo(infodata)) {
            logger.Info("Failed reading playback info data section: " + this.playbackFile);
            return false;
        }

        // TODO: Parse chunky raw binary
        if (!ParsePlaybackData(replaydata)) {
            logger.Info("Failed reading playback data: " + this.playbackFile);
            return false;
        }

        return true;

    }

    private bool ParseHeader(Span<byte> headerBytes) {
        ushort version = 0;
        string gameVersion = string.Empty; 
        StringBuilder dateBuilder = new();

        try {

            version = BitConverter.ToUInt16(headerBytes[2..4]); // read version (unsigned 32-bit integer ==> 4 bytes)
            gameVersion = Encoding.ASCII.GetString(headerBytes[4..12]); // read game version (ASCII, 1 char = 1 byte, length is fixed and equal to 8)

            int i = 12; // start position
            while (i < headerBytes.Length) {
                ushort u = BitConverter.ToUInt16(headerBytes[i..(i + 2)]);
                if (u == 0) {
                    break;
                } else {
                    dateBuilder.Append((char)u);
                }
                i += 2;
            }

        } catch (Exception e) {
            logger.Exception(e);
        }

        // Store
        this.header = new(version, gameVersion, dateBuilder.ToString());

        // Return OK
        return true;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="chunk"></param>
    /// <returns></returns>
    protected virtual bool ParsePlaybackInfo(IChunk chunk) {
        return false;
    }

    private const int PlaybackDataType_Tick = 0;
    private const int PlaybackDataType_Chat = 1;

    /// <summary>
    /// Protected method that handles the loading of playback data.
    /// </summary>
    /// <param name="data">byte data to read.</param>
    /// <returns>if playback data is successfully read, <see langword="true"/>; Otherise <see langword="false"/>.</returns>
    protected virtual bool ParsePlaybackData(byte[] data) {
       
        // Open parsers
        using MemoryStream ms = new MemoryStream(data);
        using BinaryReader reader = new BinaryReader(ms);

        uint initData = reader.ReadUInt32();

        reader.BaseStream.Position += initData;

        // Read until EOF
        while (ms.Position < ms.Length) {

            uint u1 = reader.ReadUInt32();
            if (u1 < 4) {
                return false;
            }

            uint u2 = reader.ReadUInt32();
            if (u2 < 4) {
                // Do something
            }

        }

        return true;

    }

}
