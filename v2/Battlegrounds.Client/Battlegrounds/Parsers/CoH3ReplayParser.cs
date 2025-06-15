using System.Buffers.Binary;
using System.IO;
using System.Text;

using Battlegrounds.Helpers;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;

namespace Battlegrounds.Parsers;

public sealed class CoH3ReplayParser : IReplayParser {

    public const float COH3_TICK_RATE = 1.0f / 8.0f; // CoH3 uses 8 ticks per second

    public Replay ParseReplayFile(string replayLocation) {

        if (!File.Exists(replayLocation))
            throw new FileNotFoundException($"Replay file not found: {replayLocation}");

        using var stream = File.OpenRead(replayLocation);
        using var chunkyReader = new ChunkyReader(stream);

        // Skip the first 76 bytes (Playback header)
        // Then skip the first chunky file which appears to consistently be 68 bytes
        stream.Seek(76 + 68, SeekOrigin.Begin);

        // Parse the chunky data
        try {
            chunkyReader.Parse();
        } catch (InvalidDataException ex) {
            throw new InvalidDataException($"Failed to parse replay file: {replayLocation}", ex);
        }

        long endOfChunky = chunkyReader.Position;

        // Grab data chunk
        if (chunkyReader.Chunks.Count == 0)
            throw new InvalidDataException($"No chunks found in replay file: {replayLocation}");

        var infoFolder = chunkyReader.Chunks["INFO"];
        if (!infoFolder.IsFolder)
            throw new InvalidDataException($"Expected 'INFO' chunk to be a folder in replay file: {replayLocation}");

        var dataFolder = infoFolder.Chunks["DATA"];
        chunkyReader.GoToChunk(dataFolder);

        var dataVersion = chunkyReader.ReadUInt32();
        if (dataVersion != 1)
            throw new InvalidDataException($"Unsupported version {dataVersion} in replay file: {replayLocation}");

        chunkyReader.Advance(6); // Skip 6 bytes (unknown data)
        uint playerCount = chunkyReader.ReadUInt32();

        ReplayPlayer[] players = new ReplayPlayer[playerCount];
        for (int i = 0; i < playerCount; i++) {

            long startPos = chunkyReader.Position;
            bool isHuman = chunkyReader.ReadByte() == 1;

            string playerName = chunkyReader.ReadUTF16String(-1);
            uint team = chunkyReader.ReadUInt32();
            uint playerId = 1000 + chunkyReader.ReadUInt32(); // + 1000 to match CoH3's player ID scheme
            chunkyReader.Advance(1); // Skip 1 byte (unknown data)

            string faction = chunkyReader.ReadASCIIString(-1);
            chunkyReader.Advance(8); // Skip 8 bytes (unknown data)

            string aiProfile = chunkyReader.ReadASCIIString(-1);
            chunkyReader.Advance(40); // Skip 40 bytes (unknown data)

            ulong profileId = chunkyReader.ReadUInt64();
            chunkyReader.Advance(1); // Skip 1 byte (unknown data)

            string steamIdString = chunkyReader.ReadUTF16String(-1);
            if (string.IsNullOrEmpty(steamIdString)) {
                steamIdString = "0"; // Default to 0 if empty
            }
            if (!ulong.TryParse(steamIdString, out ulong steamId)) {
                throw new InvalidDataException($"Invalid Steam ID '{steamIdString}' for player {i + 1} in replay file: {replayLocation}");
            }
            chunkyReader.Advance(18); // Skip 18 bytes (unknown data)

            players[i] = new ReplayPlayer((int)playerId, (int)team, playerName, profileId, steamId, faction, aiProfile);

            // Skip over items
            SkipCoH3PlayerItems(chunkyReader, isHuman);
            chunkyReader.Advance(4); // Skip 4 bytes (unknown data)
            SkipCoH3PlayerItems(chunkyReader, isHuman);

        }

        // TODO: Verify mod ID from data chunk

        stream.Seek(endOfChunky, SeekOrigin.Begin); // Go to the end of the chunky data and begin reading events
        using BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, false); // Use BinaryReader for reading events

        // Read to EOF
        List<CoH3Tick> ticks = [];
        while (stream.Position < stream.Length) {
            var tick = ParseCoH3Tick(reader);
            if (tick != null) {
                ticks.Add(tick);
            } else {
                break; // Exit if no more valid ticks can be parsed
            }
        }

        // Calculate length of replay
        var duration = TimeSpan.FromSeconds(ticks.Count * COH3_TICK_RATE); // Total duration in seconds

        // Extract events with actions
        ReplayEvent[] events = [.. ticks.Where(x => x.Size > 0)
            .OrderBy(x => x.TickId)
            .SelectMany(x => MapCoH3TickToCoH3Event(x, players))
            .OrderBy(x => x.Timestamp)];

        var playback = new Replay {
            GameId = CoH3.GameId,
            Duration = duration,
            Players = players,
            Events = events
        };

        return playback;

    }

    private static void SkipCoH3PlayerItems(ChunkyReader chunkyReader, bool isHuman) {
        uint itemCount = chunkyReader.ReadUInt32();
        for (int j = 0; j < itemCount; j++) {
            if (isHuman) {
                chunkyReader.Advance(24);
                uint data = chunkyReader.ReadUInt32();
                chunkyReader.Advance(data);
                chunkyReader.Advance(4); // Skip 4 bytes (unknown data)
            } else {
                chunkyReader.Advance(12); // Skip 12 bytes (unknown data)
            }
        }
    }

    private record CoH3Tick(uint TickId, uint TickType, List<CoH3TickActions>? Actions, List<CoH3TickMessage>? Messages) {
        public int Size => Actions?.Count ?? 0 + (Messages?.Count ?? 0);
    }
    private record CoH3TickMessage(string SenderId, string Message);
    private record CoH3TickActions(uint EventId, CoH3TickCommand[] Commands);
    private record CoH3TickCommand(byte EventType, uint PlayerId, uint Index, ushort Size, byte[] Data);

    private static CoH3Tick ParseCoH3Tick(BinaryReader reader) {
        uint tickType = reader.ReadUInt32();
        if (tickType == 0) {

            uint len = reader.ReadUInt32();
            long endOfTick = reader.BaseStream.Position + len;

            _ = reader.ReadByte(); // Version byte (not used)?
            uint tickId = reader.ReadUInt32();
            _ = reader.ReadUInt32(); // Unknown data (not used)?
            uint tickEvents = reader.ReadUInt32();

            List<CoH3TickActions> actions = [];
            for (int i = 0; i < tickEvents; i++) {
                uint actionId = reader.ReadUInt32();
                reader.BaseStream.Seek(4, SeekOrigin.Current); // Skip 4 bytes (unknown data)
                uint eventDataSize = reader.ReadUInt32();
                long endOfEvent = reader.BaseStream.Position + eventDataSize;

                List<CoH3TickCommand> events = [];
                while (reader.BaseStream.Position < endOfEvent) {
                    ushort eventSize = reader.ReadUInt16();
                    byte eventType = reader.ReadByte();
                    uint playerId = 1000u + reader.ReadByte(); // + 1000 to match CoH3's player ID scheme
                    uint idx = reader.ReadUInt32(); // Index of the event
                    byte[] eventData = reader.ReadBytes(eventSize - 8); // Read the event data
                    events.Add(new CoH3TickCommand(eventType, playerId, idx, eventSize, eventData));
                }
                actions.Add(new CoH3TickActions(actionId, [.. events]));

            }

            // Ensure we read the expected number of bytes for this tick
            if (reader.BaseStream.Position != endOfTick) {
                throw new InvalidDataException($"Expected to read {endOfTick - reader.BaseStream.Position} bytes, but only read {reader.BaseStream.Position - (endOfTick - len)} bytes for tick type {tickType}.");
            }

            return new CoH3Tick(tickId, tickType, actions, null);

        } else {

            uint len = reader.ReadUInt32();
            long endOfTick = reader.BaseStream.Position + len;

            List<CoH3TickMessage> messages = [];
            uint count = reader.ReadUInt32(); // Number of messages
            if (count == 0) {
                reader.BaseStream.Seek(endOfTick - reader.BaseStream.Position, SeekOrigin.Current); // Skip to end of tick
            } else {
                _ = reader.ReadUInt32(); // Unknown
                _ = reader.ReadUInt32(); // Unknown
                _ = reader.ReadUInt32(); // Unknown
                _ = reader.ReadUInt32(); // Unknown
                _ = reader.ReadUInt32(); // Unknown
                for (uint i = 0; i < count; i++) {
                    int senderIdLen = (int)reader.ReadUInt32();
                    string senderId = Encoding.Unicode.GetString(reader.ReadBytes(senderIdLen * 2));
                    int messageLen = (int)reader.ReadUInt32();
                    string message = Encoding.Unicode.GetString(reader.ReadBytes(messageLen * 2));
                    messages.Add(new CoH3TickMessage(senderId, message));
                }
            }

            return new CoH3Tick(0, tickType, null, messages);

        }

    }

    private static readonly byte[] BroadcastMessageHeader = [0xff, 0xff, 0xff, 0xff, 0xff];

    private static HashSet<ReplayEvent> MapCoH3TickToCoH3Event(CoH3Tick tick, ReplayPlayer[] players) {
        HashSet<ReplayEvent> events = [];
        if (tick.Actions is not null) {
            foreach (var action in tick.Actions) {
                foreach (var command in action.Commands) {
                    AddReplayActionEvent(tick.TickId, command, players, events);
                }
            }
        }
        if (tick.Messages is not null) {
            // TODO: Handle messages if needed
        }
        return events;
    }

    private static void AddReplayActionEvent(uint tickId, CoH3TickCommand command, ReplayPlayer[] players, HashSet<ReplayEvent> events) {
        if (command.EventType == 148) {
            var commandData = command.Data.AsSpan();

            if (!commandData[..5].SequenceEqual(BroadcastMessageHeader)) {
                throw new InvalidDataException($"Invalid broadcast message header in tick {tickId} for player {command.PlayerId}.");
            }

            int broadCastTypeIdx = IndexOfInt32LE(commandData, 499);
            if (broadCastTypeIdx == -1) {
                return; // Skip broadcast message (Not a 499 broadcast message)
            }

            int strLenStartIdx = broadCastTypeIdx + 4;
            int strLenEndIdx = broadCastTypeIdx + 8;
            uint strLen = BitConverter.ToUInt32(commandData[strLenStartIdx..strLenEndIdx]);
            if (strLenEndIdx + strLen > commandData.Length) {
                throw new InvalidDataException($"Invalid broadcast message length in tick {tickId} for player {command.PlayerId}.");
            }
            string message = Encoding.UTF8.GetString(commandData[strLenEndIdx..(strLenEndIdx + (int)strLen)].ToArray());

            ReplayEvent replayEvent = ReplayEventParser.ParseEvent(message, players, TimeSpan.FromSeconds(tickId * COH3_TICK_RATE))
                ?? throw new InvalidDataException($"Failed to parse broadcast message '{message}' in tick {tickId} for player {command.PlayerId}.");

            // Filter to only track events published by the player
            if (replayEvent is MatchStartReplayEvent || (replayEvent.Player is not null && replayEvent.Player.PlayerId == command.PlayerId)) {
                events.Add(replayEvent);
            }

        }
    }

    private static int IndexOfInt32LE(ReadOnlySpan<byte> span, int target) {
        for (int i = 0; i <= span.Length - sizeof(int); i++) {
            int value = BinaryPrimitives.ReadInt32LittleEndian(span.Slice(i, sizeof(int)));
            if (value == target)
                return i;
        }
        return -1;
    }

}
