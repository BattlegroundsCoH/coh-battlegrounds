using System;
using System.IO;
using System.Text;

using Battlegrounds.Functional;
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
    public record struct Header(uint Version, string Game, string Date);

    private string playbackFile;
    private Header header;

    /// <summary>
    /// 
    /// </summary>
    public CoH3Playback() { 
        this.playbackFile = string.Empty;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="playbackFile"></param>
    public CoH3Playback(string playbackFile) { 
        this.playbackFile = playbackFile;
    }

    public bool IsPartial => throw new NotImplementedException();

    public Player[] Players => throw new NotImplementedException();

    public IPlaybackTick[] Ticks => throw new NotImplementedException();

    public TimeSpan Length => throw new NotImplementedException();

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

        // Get playback data
        byte[] replaydata = br.ReadToEnd();

        // Dump playback data
        File.WriteAllBytes("playback_raw.bin", replaydata);

        // TODO: Parse chunky raw binary
        if (!ParsePlaybackData(replaydata)) {
            logger.Info("Failed reading playback data: " + this.playbackFile);
            return false;
        }

        return true;

    }

    private bool ParseHeader(Span<byte> headerBytes) {
        uint version = 0;
        string gameVersion = string.Empty; 
        StringBuilder dateBuilder = new();

        try {

            version = BitConverter.ToUInt32(headerBytes[0..4]); // read version (unsigned 32-bit integer ==> 4 bytes)
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

    private const int PlaybackDataType_Tick = 0;
    private const int PlaybackDataType_Chat = 1;

    private bool ParsePlaybackData(byte[] data) {
        
        // Open parsers
        using MemoryStream ms = new MemoryStream(data);
        using BinaryReader reader = new BinaryReader(ms);

        // Read until EOF
        while (ms.Position < ms.Length) {

            uint dataType = reader.ReadUInt32();
            uint dataSize = reader.ReadUInt32();

            if (dataSize == 0)
                continue;

            if (dataType == PlaybackDataType_Tick) {

                // Skip one byte
                logger.Debug("First byte of tick: " + reader.ReadByte());

                // Get tick index
                uint index = reader.ReadUInt32();
                
                // Calculate the timestamp
                TimeSpan timestamp = new TimeSpan((long)(10000000.0 * index / 8.0));

                // Skip next four bytes
                logger.Debug("Skipping next four bytes: [" + string.Join("; ", reader.ReadBytes(4).Map(x => x.ToString("X2"))) + "]");

                // Read tick events
                uint events = reader.ReadUInt32();

                // Read event bundle
                for (uint i = 0; i < events; i++) {

                    // Skip first 8 bytes
                    logger.Debug("Skipping next four bytes: [" + string.Join("; ", reader.ReadBytes(8).Map(x => x.ToString("X2"))) + "]");

                    uint count = reader.ReadUInt32();

                    //logger.Debug("Skipping next byte: [" + string.Join("; ", reader.ReadBytes(1).Map(x => x.ToString("X2"))) + "]");

                    // Read all
                    for (int j = 0; j < count; j++) {

                        ushort len = reader.ReadUInt16();
                        if (len != 0) {

                            reader.ReadBytes(len);

                        }

                    }

                }

            } else if (dataType == PlaybackDataType_Chat) {

            } else {
                logger.Error("Failed reading replay file with invalid dataType: " + dataType);
            }

        }

        return true;

    }

}
