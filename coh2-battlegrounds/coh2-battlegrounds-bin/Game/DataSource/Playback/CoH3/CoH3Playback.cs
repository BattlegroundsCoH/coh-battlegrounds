using System;
using System.IO;
using System.Text;

using Battlegrounds.Game.Gameplay;
using Battlegrounds.Logging;

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

}
