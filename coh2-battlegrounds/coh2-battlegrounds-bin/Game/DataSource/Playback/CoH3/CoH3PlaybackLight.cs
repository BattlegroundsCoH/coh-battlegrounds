using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;

using Battlegrounds.Functional;
using Battlegrounds.Game.DataSource.Gamedata;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Logging;
using Battlegrounds.Util;

namespace Battlegrounds.Game.DataSource.Playback.CoH3;

/// <summary>
/// 
/// </summary>
public class CoH3PlaybackLight : CoH3Playback {

    private static readonly Logger logger = Logger.CreateLogger();

    private static readonly byte[] BG_MESSAGE = "BG_MESSAGE".Encode(Encoding.ASCII);

    private enum BgMessageType : byte {
        Undef = 0,
        INFO_EVENT = 42,
        STARTUP_EVENT = 43,
        UI_EVENT = 48,
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filepath"></param>
    public CoH3PlaybackLight(string filepath) : base(filepath) {}

    /// <inheritdoc/>
    protected override bool ParsePlaybackInfo(IChunk chunk) {

        using MemoryStream stream = new MemoryStream(chunk.Body);
        using BinaryReader reader = new BinaryReader(stream);

        uint type = reader.ReadUInt32();
        matchType = type == 1 ? MatchType.Skirmish : MatchType.Multiplayer;

        reader.Skip(6);

        players = new Player[reader.ReadUInt32()];
        /*for (int i = 0; i < players.Length; i++) {

            while (true) {
                if (ReadPlayerInfo(reader) is Player p) {
                    players[i] = p;
                    break;
                } else {
                    reader.BaseStream.Position++; // TODO: Find out how to read past a player section...
                }
            }

        }*/

        return true;
    }

    private Player? ReadPlayerInfo(BinaryReader reader) {

        byte playerType = reader.ReadByte();
        if (playerType > 1) {
            return null;
        }

        uint nameLen = reader.ReadUInt32();
        if (reader.BaseStream.Position + nameLen > reader.BaseStream.Length || nameLen == 0) {
            return null;
        }

        string name = reader.ReadUTF8String(nameLen);
        uint tid = reader.ReadUInt32();
        if (tid > 1) {
            return null;
        }

        uint pid = reader.ReadUInt32();
        if (pid > 7) {
            return null;
        }

        if (reader.ReadByte() != 1) {
            return null;
        }

        int factionLen = reader.ReadInt32();
        if (reader.BaseStream.Position + factionLen > reader.BaseStream.Length || factionLen <= 0) {
            return null;
        }

        string faction = reader.ReadASCIIString(factionLen);
        Faction gameFaction = Faction.FromName(faction, GameCase.CompanyOfHeroes3);

        reader.Skip(8);

        int personalityLen = reader.ReadInt32();
        if (reader.BaseStream.Position + personalityLen > reader.BaseStream.Length || personalityLen == 0) {
            return null;
        }

        string personality = reader.ReadASCIIString(personalityLen);

        if (playerType == 1) {
            reader.Skip(-personality.Length + 71);
            string steamId = reader.ReadUTF8String(reader.ReadUInt32());
            return new Player(pid, ulong.Parse(steamId), tid, name, gameFaction, personality);
        } else {
            return new Player(pid, 0, tid, name, gameFaction, personality);
        }

    }

    /// <inheritdoc/>
    protected override bool ParsePlaybackData(byte[] data) {
        
        // Get indices of all BG_MESSAGEs
        var messages = ScanForMessages(data);

        // Log
        logger.Info("Detected {0} BG message(s) in CoH3 playback file", messages.Count);

        // Map the messages into events
        var events = messages.Map(x =>
           (BgMessageType)x.Type switch {
               BgMessageType.STARTUP_EVENT => HandleStartup(x),
               _ => null
           }
        ).NotNull();

        // Determine length of the match (last found event)
        length = events.MaxValue(x => x.Timestamp);

        return false;

    }

    private IList<BgMessage> ScanForMessages(byte[] data) {
        var messageStarts = new List<BgMessage>();
        int pos = 8; // Skip first 8 bytes, wont contain stuff
        while (pos < data.Length - 10) { // Skip last 10 bytes, not enough content
            if (data[pos] == (byte)'B') {
                if (ByteUtil.Match(data, pos, BG_MESSAGE)) {
                    
                    int type = BitConverter.ToInt32(data.AsSpan()[(pos - 8) .. (pos - 4)]);
                    int len = BitConverter.ToInt32(data.AsSpan()[(pos - 4)..pos]);
                    
                    byte[] content = new byte[len];
                    Array.Copy(data, pos, content, 0, len);

                    var msgData = BgMessage.FromBytes(type, content);

                    messageStarts.Add(msgData);
                    
                    pos += len;
                    continue;

                }
            }
            pos++;
        }
        return messageStarts;
    }

    private record struct BgMessage(int Type, string SenderId, TimeSpan Timestamp, string ClassId, char EncodedType, string EncodedMessage) {
        public static BgMessage FromBytes(int type, byte[] bytes) {
            StringContentReader contentStr = Encoding.ASCII.GetString(bytes);

            string encodedMessage = contentStr.Seek(10)
                .ReadUntil(',', out var senderId)
                .ReadUntil(',', out var time)
                .ReadUntil(',', out var classId)
                .ReadUntil(',', out var encodedType)
                .Seek(SeekOrigin.Current, 1)
                .ReadToEnd();

            // Get timestamp
            TimeSpan timestamp = TimeSpan.FromSeconds(double.Parse(time, CultureInfo.InvariantCulture));

            // Return message
            return new BgMessage(type, senderId, timestamp, classId, encodedType[0], encodedMessage[..^1]);

        }
    }

    private record StartupPlayer(string faction, string displayName, bool isAIPlayer, string personality);
    private record StartupStructure(string scenario, Dictionary<string, Dictionary<string, StartupPlayer>> teams);

    private CoH3Event? HandleStartup(BgMessage message) {
        var startup = JsonSerializer.Deserialize<StartupStructure>(message.EncodedMessage);
        if (startup is null) {
            return null;
        }
        logger.Info("Detected scenario {0}", startup.scenario);
        foreach (var team in startup.teams) {
            foreach (var player in team.Value) {
                int pid = int.Parse(player.Key) % 1000;
                Faction faction = Faction.FromName(player.Value.faction, GameCase.CompanyOfHeroes3);
                players[pid] = new Player((uint)pid, 0, uint.Parse(team.Key), player.Value.displayName, faction, player.Value.personality);
            }
        }
        return new CoH3Event(message.Timestamp, 0, 0, GameEventType.CMD_DefaultAction, string.Empty);
    }

}
