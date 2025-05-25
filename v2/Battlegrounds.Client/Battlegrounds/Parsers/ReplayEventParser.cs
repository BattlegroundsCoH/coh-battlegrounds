using System.Text;

using Battlegrounds.Models.Replays;

namespace Battlegrounds.Parsers;

public static class ReplayEventParser {

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

        ReplayPlayer? player = eventTable.TryGetValue("player", out object? playerIdStr) 
            && playerIdStr is string playerIdStrValue 
            && int.TryParse(playerIdStrValue, out int playerId) 
            ? players.FirstOrDefault(p => p.PlayerId == playerId)
            : null;

        ReplayEvent? parsedEvent = eventTable["type"] switch {
            "squad_killed" => new SquadKilledEvent(timestamp, player ?? throw new Exception(), ushort.Parse((string)eventTable["companyId"])),
            "squad_deployed" => new SquadDeployedEvent(timestamp, player ?? throw new Exception(), ushort.Parse((string)eventTable["companyId"])),
            "item_pickup" => new SquadWeaponPickupEvent(
                timestamp,
                player ?? throw new Exception(),
                ushort.Parse((string)eventTable["companyId"]),
                (eventTable.TryGetValue("ebp", out object? value) ? value.ToString() : eventTable["upg"].ToString()) ?? throw new ArgumentException("Weapon name not found in event table.", nameof(luaEncodedTable)),
                eventTable.ContainsKey("ebp")),
            "match_data" => MapToMatchStartReplayEvent(timestamp, eventTable),
            _ => null
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
        return new MatchStartReplayEvent(timestamp, scenario, players);
    }

}
