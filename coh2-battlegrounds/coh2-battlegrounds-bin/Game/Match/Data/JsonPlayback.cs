using System;
using System.Collections.Generic;
using Battlegrounds.Game.Match.Analyze;
using Battlegrounds.Game.Match.Data.Events;
using Battlegrounds.Json;

namespace Battlegrounds.Game.Match.Data {

    /// <summary>
    /// Simplified <see cref="ReplayMatchData"/> that's converted into a json format that's easy to upload and download remotely. Implements <see cref="IMatchData"/> and <see cref="IJsonObject"/>.
    /// </summary>
    public class JsonPlayback : IMatchData, IJsonObject {

        public struct Event : IJsonObject {
            public ulong UID;
            [JsonIgnoreIfValue("")] public string Type;
            [JsonIgnoreIfValue(ulong.MaxValue)] public ulong Player;
            [JsonIgnoreIfValue(ushort.MaxValue)] public ushort Id;
            [JsonIgnoreIfValue("")] public string Arg1;
            [JsonIgnoreIfValue("")] public string Arg2;
            public string ToJsonReference() => throw new NotImplementedException();
        }

        public struct EventTick : IJsonObject {
            public List<Event> Events { get; set; }
            public string ToJsonReference() => throw new NotImplementedException();
        }

        [JsonIgnore] private ReplayMatchData m_dataSource;

        public ISession Session { get; private set; }

        public TimeSpan Length { get; private set; }

        public bool IsSessionMatch { get; private set; }

        public Dictionary<TimeSpan, EventTick> Events { get; private set; }

        public JsonPlayback() {
            this.Session = new NullSession();
        }

        public JsonPlayback(ReplayMatchData events) {
            this.Session = events.Session;
            this.Length = events.Length;
            this.IsSessionMatch = events.IsSessionMatch;
            this.m_dataSource = events;
            this.ParseMatchData();
        }

        public static JsonPlayback FromJsonFile(string filepath) => JsonParser.ParseFile<JsonPlayback>(filepath);

        public bool LoadMatchData(string matchFile) => true;

        public bool ParseMatchData() {

            // Run through all elements
            foreach (var element in this.m_dataSource) {

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

            return true;

        }

        private static Event FromData(IMatchEvent e) {
            return e switch {
                KillEvent k => new Event() { UID = e.Uid, Type = k.GetType().Name, Player = k.UnitOwner.ID, Id = k.UnitID },
                DeployEvent d => new Event() { UID = e.Uid, Type = d.GetType().Name, Player = d.DeployingPlayer.ID, Id = d.SquadID },
                RetreatEvent r => new Event() { UID = e.Uid, Type = r.GetType().Name, Player = r.WithdrawPlayer.ID, 
                    Id = r.WithdrawingUnitID, 
                    Arg1 = r.WithdrawingUnitVeterancyChange.ToString(), 
                    Arg2 = r.WithdrawingUnitVeterancyExperience.ToString() 
                },
                VictoryEvent v => new Event() { UID = e.Uid, Type = v.GetType().Name, Player = v.VictorID },
                PickupEvent i => new Event() { UID = e.Uid, Type = i.GetType().Name, Player = i.PickupPlayer.ID, Id = i.PickupSquadID, Arg1 = i.PickupItem.ToJsonReference() },
                VerificationEvent g => new Event() { UID = e.Uid, Type = g.GetType().Name, Player = ulong.MaxValue, Id = ushort.MaxValue, 
                    Arg1 = g.VerificationType.ToString(), 
                    Arg2 = g.VerificationArgument 
                },
                CaptureEvent c => new Event() { UID = e.Uid, Type = c.GetType().Name, Player = c.CapturingPlayer.ID, Id = ushort.MaxValue, Arg1 = c.CapturedBlueprint.ToJsonReference() },
                _ => new Event() { UID = e.Uid },
            };
        }

        public bool CompareAgainst(EventAnalysis m_analysisResult) {
            // TODO: Implement
            return true;
        }

        public string ToJsonReference() => throw new NotImplementedException();

    }

}
