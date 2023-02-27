using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

using Battlegrounds.Logging;
using Battlegrounds.Util;

namespace Battlegrounds.Game.DataSource.Gamedata.CoH3;

/// <summary>
/// 
/// </summary>
public class Chunky : IChunky {

    private static readonly Logger logger = Logger.CreateLogger();

    private readonly IList<IChunk> chunks;

    /// <summary>
    /// 
    /// </summary>
    public Chunky() {
        this.chunks = new List<IChunk>();
    }

    /// <inheritdoc/>
    public bool Load(BinaryReader reader) {
        
        // Verify header magic (Relic Chunky  ...)
        if (!VerifyMagic(reader)) {
            logger.Info("Failed reading chunky file: invalid header magic");
            return false;
        }

        // Read version
        uint version = reader.ReadUInt32();
        if (version != 4) {
            logger.Warning($"Invalid chunky file version 0x{version:x00} (Expected 0x{4:X00})");
            return false;
        }

        // Skip next four bytes
        reader.Skip(4);

        // Read while content and in chunky file
        while (reader.BaseStream.Position < reader.BaseStream.Length) {
            if (ReadChunk(reader) is not IChunk chunk) {
                break;
            }
            this.chunks.Add(chunk);
        }

        // Return OK => Parsed
        return true;

    }

    private static bool VerifyMagic(BinaryReader reader) => ByteUtil.Match(reader.ReadBytes(16), "Relic Chunky\x0D\x0A\x1A\x00");

    private IChunk? ReadChunk(BinaryReader reader) {

        // Read 4 bytes (the type)
        var chunkType = reader.ReadBytes(4) switch {
            [70, 79, 76, 68] => ChunkyType.FOLD,
            [68, 65, 84, 65] => ChunkyType.DATA,
            _ => ChunkyType.Unknown
        };
        if (chunkType is ChunkyType.Unknown) {
            reader.BaseStream.Position -= 4;
            return null;
        }

        // Read name
        string chunkName = Encoding.ASCII.GetString(reader.ReadBytes(4));

        // Read version
        uint version = reader.ReadUInt32();

        // Read len
        uint len = reader.ReadUInt32();
        if (reader.BaseStream.Position + len > reader.BaseStream.Length) {
            logger.Info($"Invalid chunk length. Chunk has length {len} but goes out of bounds.");
            return null;
        }

        // Read descriptor length
        uint descLen = reader.ReadUInt32();
        string desc = Encoding.ASCII.GetString(reader.ReadBytes((int)descLen));

        // Read body
        var chunkBody = reader.ReadBytes((int)len);

        // Parse subnodes
        var subchunks = new List<IChunk>();
        if (chunkType is ChunkyType.FOLD) {
            using var ms = new MemoryStream(chunkBody);
            using var br = new BinaryReader(ms);
            while (ms.Position < ms.Length) {
                if (ReadChunk(br) is IChunk chunk) {
                    subchunks.Add(chunk);
                }
            }
        }

        // Return chunk
        return new Chunk(chunkType, chunkName, desc, (int)version, subchunks, subchunks.Count > 0 ? Array.Empty<byte>() : chunkBody);

    }

    /// <inheritdoc/>
    public void DumpJson(string jsonOutputFile)
        => File.WriteAllText(jsonOutputFile, JsonSerializer.Serialize(this.chunks));

}
