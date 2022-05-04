using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match.Data.Events;

namespace Battlegrounds.Game.Match.Data;

/// <summary>
/// Simplified <see cref="ReplayMatchData"/> that's converted into a json format that's easy to upload and download remotely. Implements <see cref="IMatchData"/> and <see cref="IJsonObject"/>.
/// </summary>
public class JsonPlayback : IMatchData {

    public struct Event : IMatchEvent {

        [JsonIgnore] public char Identifier => 'J';
        [JsonIgnore] public uint Uid => 0;

        [JsonInclude] public ulong UID;
        [JsonInclude] [DefaultValue(null)] public string Type;
        [JsonInclude] [DefaultValue(ulong.MaxValue)] public ulong Player;
        [JsonInclude] [DefaultValue(ushort.MaxValue)] public ushort Id;
        [JsonInclude] [DefaultValue(null)] public string Arg1;
        [JsonInclude] [DefaultValue(null)] public string Arg2;

    }

    public struct EventTick {
        public List<Event> Events { get; set; }
    }

    public struct PlayerData {
        public string Name;
        public string Army;
        public string Profile;
        public ulong SteamID;
        public uint Id;
        public uint team;
        public Player FromData() => new Player(this.Id, this.SteamID, this.team, this.Name, Faction.FromName(this.Army), Profile);
    }

    [JsonIgnore] private readonly ReplayMatchData? m_dataSource;
    [JsonInclude] private PlayerData[] players;

    public ISession Session { get; private set; }

    public TimeSpan Length { get; private set; }

    public bool IsSessionMatch { get; private set; }

    public Dictionary<TimeSpan, EventTick> Events { get; private set; }

    [JsonIgnore]
    public ReadOnlyCollection<Player> Players => new(this.players.Select(x => x.FromData()).ToList());

    public JsonPlayback() {
        this.Session = new NullSession();
        this.Events = new();
        this.players = Array.Empty<PlayerData>();
    }

    public JsonPlayback(ReplayMatchData events) {
        this.Session = events.Session;
        this.Length = events.Length;
        this.Events = new Dictionary<TimeSpan, EventTick>();
        this.players = Array.Empty<PlayerData>();
        this.IsSessionMatch = events.IsSessionMatch;
        this.m_dataSource = events;
        this.ParseMatchData();
    }

    public string ToJson() {
        return JsonSerializer.Serialize(this.Events.Values, options: new() { IncludeFields = true, WriteIndented = true });
    }

    public bool LoadMatchData(string matchFile) => true;

    public bool ParseMatchData() {

        // Bail if invalid data source
        if (this.m_dataSource is null) {
            return false;
        }

        // Run through all elements
        foreach (var entry in this.m_dataSource) {
            if (entry is TimeEvent element) {
                if (element.UnderlyingEvent is not UIDigitEvent) {
                    if (this.Events.ContainsKey(element.Timestamp)) {
                        this.Events[element.Timestamp].Events.Add(FromData(element.UnderlyingEvent));
                    } else {
                        EventTick tick = new EventTick() {
                            Events = new List<Event>() {
                                FromData(element.UnderlyingEvent)
                            }
                        };
                        this.Events.Add(element.Timestamp, tick);
                    }
                }
            }
        }

        // Alloc player data
        this.players = new PlayerData[this.m_dataSource.Players.Count];
        for (int i = 0; i < this.players.Length; i++) {
            this.players[i] = new PlayerData() {
                Army = this.m_dataSource.Players[i].Army,
                Id = this.m_dataSource.Players[i].ID,
                Name = this.m_dataSource.Players[i].Name,
                Profile = this.m_dataSource.Players[i].Profile,
                team = this.m_dataSource.Players[i].TeamID,
                SteamID = this.m_dataSource.Players[i].SteamID
            };
        }

        return true;

    }

    private static Event FromData(IMatchEvent e) {
        return e switch {
            KillEvent k => new Event() { UID = e.Uid, Type = nameof(KillEvent), Player = k.UnitOwner.SteamID, Id = k.UnitID },
            DeployEvent d => new Event() { UID = e.Uid, Type = nameof(DeployEvent), Player = d.DeployingPlayer.SteamID, Id = d.SquadID },
            RetreatEvent r => new Event() {
                UID = e.Uid,
                Type = nameof(RetreatEvent),
                Player = r.WithdrawPlayer.SteamID,
                Id = r.WithdrawingUnitID,
                Arg1 = r.WithdrawingUnitVeterancyChange.ToString(),
                Arg2 = r.WithdrawingUnitVeterancyExperience.ToString()
            },
            VictoryEvent v => new Event() { UID = e.Uid, Type = nameof(VictoryEvent), Player = v.VictorID },
            PickupEvent i => new Event() { UID = e.Uid, Type = nameof(PickupEvent), Player = i.PickupPlayer.SteamID, Id = i.PickupSquadID, Arg1 = i.PickupItem.PBGID.ToString() },
            VerificationEvent g => new Event() {
                UID = e.Uid,
                Type = nameof(VerificationEvent),
                Player = ulong.MaxValue,
                Id = ushort.MaxValue,
                Arg1 = g.VerificationType.ToString(),
                Arg2 = g.VerificationArgument
            },
            CaptureEvent c => new Event() { UID = e.Uid, Type = nameof(CaptureEvent), Player = c.CapturingPlayer.SteamID, Id = ushort.MaxValue, Arg1 = c.CapturedBlueprint.PBGID.ToString() },
            _ => new Event() { UID = e.Uid },
        };
    }

    public IEnumerator<IMatchEvent> GetEnumerator() {
        List<IMatchEvent> events = new List<IMatchEvent>();
        foreach (var spair in this.Events) {
            foreach (var tpair in spair.Value.Events) {
                events.Add(new TimeEvent(spair.Key, tpair));
            }
        }
        return events.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

}

