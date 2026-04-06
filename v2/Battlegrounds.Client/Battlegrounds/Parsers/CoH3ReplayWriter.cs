using System.Globalization;
using System.IO;
using System.Text;

using Battlegrounds.Models.Replays;

namespace Battlegrounds.Parsers;

/// <summary>
/// Produces a well-formed CoH3 replay binary that can be round-tripped through
/// <see cref="CoH3ReplayParser"/>. Intended for testing and simulation purposes.
/// </summary>
public sealed class CoH3ReplayWriter {

    /// <summary>
    /// Builds a dummy replay byte array from the given players and events.
    /// </summary>
    public byte[] WriteDummyReplay(ReplayPlayer[] players, ReplayEvent[] events) {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);

        // Playback header (76 bytes, unused by the parser)
        writer.Write(new byte[76]);

        // First chunky file (68 bytes, skipped by the parser)
        writer.Write(new byte[68]);

        // Second chunky: INFO folder containing the DATA chunk with player data
        WriteChunky(writer, players);

        // Tick stream
        WriteTicks(writer, events);

        writer.Flush();
        return ms.ToArray();
    }

    // -------------------------------------------------------------------------
    // Chunky (INFO / DATA)
    // -------------------------------------------------------------------------

    private static void WriteChunky(BinaryWriter writer, ReplayPlayer[] players) {
        // "Relic Chunky\r\n\x1A\0" – exactly what ChunkyReader.Parse() validates
        writer.Write(Encoding.ASCII.GetBytes("Relic Chunky\r\n\x1A\0"));
        writer.Write((uint)4); // version
        writer.Write((uint)1); // platform

        byte[] dataContent = BuildDataChunkContent(players);

        // DATA DATA chunk  = chunk header + content
        byte[] dataChunkHeader = BuildChunkHeader(isFolder: false, id: "DATA", version: 1, size: (uint)dataContent.Length);
        uint dataChunkTotalSize = (uint)(dataChunkHeader.Length + dataContent.Length);

        // FOLD INFO chunk header (its `size` field covers everything it contains)
        byte[] infoFoldHeader = BuildChunkHeader(isFolder: true, id: "INFO", version: 1, size: dataChunkTotalSize);

        writer.Write(infoFoldHeader);
        writer.Write(dataChunkHeader);
        writer.Write(dataContent);
    }

    /// <summary>
    /// Produces the 20-byte fixed chunk header (type[4] + id[4] + version[4] + size[4] + nameSize[4]).
    /// </summary>
    private static byte[] BuildChunkHeader(bool isFolder, string id, uint version, uint size) {
        using var ms = new MemoryStream(20);
        using var w = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: false);
        w.Write(Encoding.ASCII.GetBytes(isFolder ? "FOLD" : "DATA"));
        w.Write(Encoding.ASCII.GetBytes(id.PadRight(4)[..4]));
        w.Write(version);
        w.Write(size);
        w.Write((uint)0); // nameSize – no name
        return ms.ToArray();
    }

    private static byte[] BuildDataChunkContent(ReplayPlayer[] players) {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: false);

        w.Write((uint)1);               // dataVersion (must be 1)
        w.Write(new byte[6]);           // 6 unknown bytes
        w.Write((uint)players.Length);  // playerCount

        foreach (var player in players)
            WritePlayer(w, player);

        return ms.ToArray();
    }

    private static void WritePlayer(BinaryWriter w, ReplayPlayer player) {
        bool isHuman = string.IsNullOrEmpty(player.AIProfile);
        w.Write((byte)(isHuman ? 1 : 0));

        WriteUTF16String(w, player.PlayerName);

        w.Write((uint)player.TeamId);
        w.Write((uint)(player.PlayerId - 1000)); // strip the +1000 offset added by the parser
        w.Write((byte)0);                         // unknown byte

        WriteASCIIString(w, player.Faction);
        w.Write(new byte[8]);  // 8 unknown bytes

        WriteASCIIString(w, player.AIProfile);
        w.Write(new byte[40]); // 40 unknown bytes

        w.Write(player.ProfileId);
        w.Write((byte)0);       // unknown byte

        WriteUTF16String(w, player.SteamId.ToString());
        w.Write(new byte[18]); // 18 unknown bytes

        // Two empty item sets separated by the 4 unknown bytes the parser skips
        w.Write((uint)0); // first SkipCoH3PlayerItems → itemCount = 0
        w.Write((uint)0); // the Advance(4) between the two calls
        w.Write((uint)0); // second SkipCoH3PlayerItems → itemCount = 0
    }

    private static void WriteUTF16String(BinaryWriter w, string value) {
        byte[] bytes = Encoding.Unicode.GetBytes(value);
        w.Write((uint)(bytes.Length / 2)); // length in UTF-16 code units
        w.Write(bytes);
    }

    private static void WriteASCIIString(BinaryWriter w, string value) {
        byte[] bytes = Encoding.ASCII.GetBytes(value);
        w.Write((uint)bytes.Length);
        w.Write(bytes);
    }

    // -------------------------------------------------------------------------
    // Tick stream
    // -------------------------------------------------------------------------

    private static void WriteTicks(BinaryWriter writer, ReplayEvent[] events) {
        var groups = events
            .GroupBy(e => (uint)(e.Timestamp.TotalSeconds / CoH3ReplayParser.COH3_TICK_RATE))
            .OrderBy(g => g.Key);

        foreach (var group in groups)
            WriteActionTick(writer, group.Key, [.. group]);
    }

    private static void WriteActionTick(BinaryWriter writer, uint tickId, ReplayEvent[] events) {
        using var bodyMs = new MemoryStream();
        using var body = new BinaryWriter(bodyMs, Encoding.UTF8, leaveOpen: false);

        body.Write((byte)1);               // version byte (unused by parser)
        body.Write(tickId);
        body.Write((uint)0);               // unknown uint
        body.Write((uint)events.Length);   // tickEvents count

        for (int i = 0; i < events.Length; i++) {
            ReplayEvent ev = events[i];
            byte rawPlayerId = ev.Player is not null ? (byte)(ev.Player.PlayerId - 1000) : (byte)0;

            byte[] broadcastData = BuildBroadcastData(EncodeLuaEvent(ev));
            ushort commandSize = (ushort)(8 + broadcastData.Length); // 8 = fixed header bytes

            using var cmdMs = new MemoryStream();
            using var cmd = new BinaryWriter(cmdMs, Encoding.UTF8, leaveOpen: false);
            cmd.Write(commandSize);
            cmd.Write((byte)148);   // eventType – broadcast message
            cmd.Write(rawPlayerId);
            cmd.Write((uint)i);     // idx
            cmd.Write(broadcastData);

            byte[] cmdBytes = cmdMs.ToArray();

            body.Write((uint)(i + 1));         // actionId
            body.Write((uint)0);               // 4 unknown bytes
            body.Write((uint)cmdBytes.Length); // eventDataSize
            body.Write(cmdBytes);
        }

        byte[] bodyBytes = bodyMs.ToArray();
        writer.Write((uint)0);                  // tickType = 0 (action tick)
        writer.Write((uint)bodyBytes.Length);   // len
        writer.Write(bodyBytes);
    }

    /// <summary>
    /// Builds the raw data bytes for an event-type-148 broadcast command.
    /// Layout: BroadcastMessageHeader[5] + int32(499) + uint32(msgLen) + UTF-8 message.
    /// </summary>
    private static byte[] BuildBroadcastData(string message) {
        byte[] msgBytes = Encoding.UTF8.GetBytes(message);
        using var ms = new MemoryStream(5 + 4 + 4 + msgBytes.Length);
        using var w = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: false);
        w.Write(new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff }); // BroadcastMessageHeader
        w.Write((int)499);                                     // broadcast type marker searched by IndexOfInt32LE
        w.Write((uint)msgBytes.Length);                        // string byte length
        w.Write(msgBytes);
        return ms.ToArray();
    }

    // -------------------------------------------------------------------------
    // Lua-table event encoding (inverse of ReplayEventParser)
    // -------------------------------------------------------------------------

    private static string EncodeLuaEvent(ReplayEvent @event) {
        string inner = @event switch {
            SquadDeployedEvent e     => $"{{<type=squad_deployed><player={e.Player.PlayerId}><companyId={e.SquadCompanyId}>}}",
            SquadKilledEvent e       => $"{{<type=squad_killed><player={e.Player.PlayerId}><companyId={e.SquadCompanyId}>}}",
            SquadRecalledEvent e     => EncodeSquadRecalled(e),
            SquadWeaponPickupEvent e => EncodeWeaponPickup(e),
            MatchStartReplayEvent e  => EncodeMatchStart(e),
            MatchOverReplayEvent e   => EncodeMatchOver(e),
            _                        => "{<type=unknown>}"
        };
        return $"{ReplayEventParser.BGMATCH_EVENT_PREFIX}({inner})";
    }

    private static string EncodeSquadRecalled(SquadRecalledEvent e) =>
        $"{{<type=squad_recalled><player={e.Player.PlayerId}><companyId={e.SquadCompanyId}>" +
        $"<experience={e.Experience.ToString(CultureInfo.InvariantCulture)}>" +
        $"<infantryKills={e.InfantryKills}><vehicleKills={e.VehicleKills}><losses={e.EntityLosses}>}}";

    private static string EncodeWeaponPickup(SquadWeaponPickupEvent e) {
        string weaponKey = e.IsEntityBlueprint ? "ebp" : "upg";
        return $"{{<type=item_pickup><player={e.Player.PlayerId}><companyId={e.SquadCompanyId}><{weaponKey}={e.WeaponName}>}}";
    }

    private static string EncodeMatchStart(MatchStartReplayEvent e) {
        string playerData = string.Concat(e.Players.Select(p =>
            $"<{p.PlayerId}={{<name={p.Name}><company={p.CompanyId}><mod_id={p.ModId}>}}>"));
        return $"{{<type=match_data><match_id={e.MatchId}><mod_version={e.ModVersion}><scenario={e.Scenario}>" +
               $"<playerdata={{{playerData}}}>}}";
    }

    private static string EncodeMatchOver(MatchOverReplayEvent e) {
        string winners = string.Concat(e.Winners.Select((id, i) => $"<{i}={id}>"));
        string losers  = string.Concat(e.Losers.Select((id, i)  => $"<{i}={id}>"));
        string stats   = string.Concat(e.PlayerStats.Select(s =>
            $"<{s.PlayerId}={{<team_id={s.TeamId}><name={s.Name}><mod_id={s.ModId}><kills={s.Kills}><losses={s.Losses}>}}>"));
        return $"{{<type=match_over_results><winners={{{winners}}}><losers={{{losers}}}><player_stats={{{stats}}}>}}";
    }

}
