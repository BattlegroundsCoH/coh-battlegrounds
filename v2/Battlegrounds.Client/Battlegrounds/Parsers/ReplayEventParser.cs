﻿using System.Globalization;
using System.IO;
using System.Text;

using Battlegrounds.Models.Replays;

using Serilog;

namespace Battlegrounds.Parsers;

public static class ReplayEventParser {

    private static readonly ILogger _logger = Log.ForContext<ReplayEvent>();

    private static readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    public static readonly string BGMATCH_EVENT_PREFIX = "bg_match_event";

    public static ReplayEvent? ParseEvent(string luaEncodedTable, ReplayPlayer[] players, TimeSpan timestamp) {

        if (!luaEncodedTable.StartsWith(BGMATCH_EVENT_PREFIX)) {
            return null;
        }

        luaEncodedTable = luaEncodedTable[BGMATCH_EVENT_PREFIX.Length..].Trim('(', ')', ' ');
        var (table, _) = ParseValue(luaEncodedTable, 0);

        if (table is not Dictionary<string, object> eventTable) {
            throw new ArgumentException("Invalid Lua encoded table format.", nameof(luaEncodedTable));
        }

        ReplayPlayer? player = (eventTable.TryGetValue("player", out object? playerIdStr) 
            && playerIdStr is string playerIdStrValue 
            && int.TryParse(playerIdStrValue, out int playerId) 
            ? players.FirstOrDefault(p => p.PlayerId == playerId)
            : null);

        ReplayEvent? parsedEvent = eventTable["type"] switch {
            "squad_killed" => new SquadKilledEvent(timestamp, player ?? throw new InvalidDataException("Expected player but found none"), ushort.Parse((string)eventTable["companyId"])),
            "squad_deployed" => new SquadDeployedEvent(timestamp, player ?? throw new InvalidDataException("Expected player but found none"), ushort.Parse((string)eventTable["companyId"])),
            "squad_recalled" => new SquadRecalledEvent(timestamp, player ?? throw new InvalidDataException("Expected player but found none"), ushort.Parse((string)eventTable["companyId"]),
                ParseFloat(eventTable.GetValueOrDefault("experience", "0").ToString() ?? "0"),
                int.Parse(eventTable.GetValueOrDefault("infantryKills", "0").ToString() ?? "0"),
                int.Parse(eventTable.GetValueOrDefault("vehicleKills", "0").ToString() ?? "0")),
            "item_pickup" => new SquadWeaponPickupEvent(
                timestamp,
                player ?? throw new InvalidDataException("Expected player but found none"),
                ushort.Parse((string)eventTable["companyId"]),
                (eventTable.TryGetValue("ebp", out object? value) ? value.ToString() : eventTable["upg"].ToString()) ?? throw new ArgumentException("Weapon name not found in event table.", nameof(luaEncodedTable)),
                eventTable.ContainsKey("ebp")),
            "match_data" => MapToMatchStartReplayEvent(timestamp, eventTable),
            "match_over_results" => MapToMatchOverReplayEvent(timestamp, eventTable),
            _ => MapToUnknownEvent(timestamp, eventTable["type"].ToString() ?? string.Empty, eventTable)
        };

        return parsedEvent;

    }

    private static (object, int) ParseValue(string luaEncodedTable, int pos) {
        var table = new Dictionary<string, object>();
        if (luaEncodedTable[pos] == '{') {
            pos++;
            while (pos < luaEncodedTable.Length && luaEncodedTable[pos] != '}') {
                var (keyValuePair, newPos) = ParseValue(luaEncodedTable, pos);
                pos = newPos;
                if (keyValuePair is KeyValuePair<string, object> kvp) {
                    table.Add(kvp.Key, kvp.Value);
                } 
            }
            if (pos >= luaEncodedTable.Length || luaEncodedTable[pos] != '}') {
                throw new ArgumentException("Invalid Lua encoded table format.", nameof(luaEncodedTable));
            }
            pos++; // Skip the closing '}'
        } else if (luaEncodedTable[pos] == '<') {
            int eq = luaEncodedTable.IndexOf('=', pos);
            if (eq < 0) {
                throw new ArgumentException("Invalid Lua encoded table format.", nameof(luaEncodedTable));
            }
            string key = luaEncodedTable.Substring(pos + 1, eq - pos - 1).Trim();
            pos = eq + 1;
            var (value, newPos) = ParseValue(luaEncodedTable, pos);
            return (new KeyValuePair<string, object>(key, value), newPos+1);
        } else {
            StringBuilder sb = new StringBuilder();
            while (pos < luaEncodedTable.Length && luaEncodedTable[pos] != '>' && luaEncodedTable[pos] != ',' && luaEncodedTable[pos] != '}') {
                sb.Append(luaEncodedTable[pos]);
                pos++;
            }
            if (pos >= luaEncodedTable.Length || luaEncodedTable[pos] != '>') {
                throw new ArgumentException("Invalid Lua encoded table format.", nameof(luaEncodedTable));
            }
            return (sb.ToString().Trim(), pos);
        }
        return (table, pos);
    }

    private static MatchStartReplayEvent MapToMatchStartReplayEvent(TimeSpan timestamp, Dictionary<string, object> eventTable) {
        string mathId = eventTable["match_id"] as string ?? throw new ArgumentException("Match Id not found in event table.", nameof(eventTable));
        string modVersion = eventTable["mod_version"] as string ?? throw new ArgumentException("Mod version not found in event table.", nameof(eventTable));
        string scenario = eventTable["scenario"] as string ?? throw new ArgumentException("Scenario not found in event table.", nameof(eventTable));
        List<MatchStartReplayEvent.PlayerData> players = [];
        foreach (var playerEntry in eventTable["playerdata"] as Dictionary<string, object> ?? throw new ArgumentException("Player data not found in event table.", nameof(eventTable))) {
            if (playerEntry.Value is not Dictionary<string, object> playerData) {
                throw new ArgumentException("Invalid player data format.", nameof(eventTable));
            }
            int playerId = int.Parse(playerEntry.Key);
            string name = playerData["name"] as string ?? throw new ArgumentException("Player name not found in player data.", nameof(eventTable));
            string companyId = playerData["company"] as string ?? throw new ArgumentException("Company ID not found in player data.", nameof(eventTable));
            int modId = playerData.TryGetValue("mod_id", out object? value) ? int.Parse(value.ToString() ?? "0") : 0;
            players.Add(new MatchStartReplayEvent.PlayerData(playerId, name, companyId, modId));
        }
        return new MatchStartReplayEvent(timestamp, mathId, modVersion, scenario, players);
    }

    private static MatchOverReplayEvent MapToMatchOverReplayEvent(TimeSpan timestamp, Dictionary<string, object> eventTable) {
        /*List<int> winners = []; // eventTable["winners"] as List<int> ?? throw new ArgumentException("Winners not found in event table.", nameof(eventTable));
        List<int> losers = []; // eventTable["losers"] as List<int> ?? throw new ArgumentException("Losers not found in event table.", nameof(eventTable));
        List<MatchOverReplayEvent.PlayerStatistics> playerStats = [];
        foreach (var playerEntry in eventTable["player_stats"] as Dictionary<string, object> ?? throw new ArgumentException("Player stats not found in event table.", nameof(eventTable))) {
            if (playerEntry.Value is not Dictionary<string, object> playerData) {
                throw new ArgumentException("Invalid player stats format.", nameof(eventTable));
            }
            int playerId = int.Parse(playerEntry.Key);
            int teamId = int.Parse(playerData["team_id"].ToString() ?? "0");
            string name = playerData["name"] as string ?? throw new ArgumentException("Player name not found in player data.", nameof(eventTable));
            int modId = int.Parse(playerData["mod_id"].ToString() ?? "0");
            int kills = int.Parse(playerData["kills"].ToString() ?? "0");
            playerStats.Add(new MatchOverReplayEvent.PlayerStatistics(playerId, teamId, name, modId, kills));
        }*/
        // TODO: Wait for a real example of this event to implement it properly
        return new MatchOverReplayEvent(timestamp, [], [], []);
    }

    private static UnknownReplayEvent MapToUnknownEvent(TimeSpan timestamp, string eventType, Dictionary<string, object> details) {
        _logger.Warning("Unknown event type '{EventType}' at {Timestamp} with details: {Details}", eventType, timestamp, details);
        return new UnknownReplayEvent(timestamp, eventType, details);
    }

    private static float ParseFloat(object value) {
        if (value is string strValue) {
            if (float.TryParse(strValue, NumberStyles.Float, _culture, out float result)) {
                return MathF.Round(result, 2);
            }
        } else if (value is float floatValue) {
            return floatValue;
        }
        throw new ArgumentException($"Cannot parse float from value: {value}", nameof(value));
    }

}
