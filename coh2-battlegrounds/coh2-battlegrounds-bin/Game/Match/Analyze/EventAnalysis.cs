using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match.Data;
using Battlegrounds.Game.Match.Data.Events;

namespace Battlegrounds.Game.Match.Analyze {

    /// <summary>
    /// Analysis of a match based on events triggered by ingame broadcast messages.
    /// </summary>
    public class EventAnalysis : IAnalyzedMatch {

        public struct RegisterEventResult {
            public bool WasAdded { get; }
            public bool WasOutsideTime { get; init; }
            public bool ConflictingTimes { get; init; }
            public RegisterEventResult(bool success) {
                this.WasAdded = success;
                this.WasOutsideTime = false;
                this.ConflictingTimes = false;
            }
            public static implicit operator bool(RegisterEventResult result) => result.WasAdded;
        }

        private int m_eventCount;
        private bool m_isFinalizable;
        private HashSet<ulong> m_winners;
        private TimeSpan m_timespan;
        private List<(TimeSpan time, IMatchEvent @event)> m_events;
        private Player[] m_players;

        private List<UnitStatus> m_units;

        public int EventCount => this.m_eventCount;

        public bool IsFinalizableMatch => this.m_isFinalizable && this.Session.AllowPersistency;

        public IReadOnlyList<UnitStatus> Units => this.m_units.AsReadOnly();

        public IReadOnlyCollection<Player> Players => this.m_players;

        public ISession Session { get; }

        public EventAnalysis(ISession session) {
            this.m_isFinalizable = false;
            this.m_timespan = TimeSpan.Zero;
            this.m_events = new List<(TimeSpan, IMatchEvent)>();
            this.m_winners = new HashSet<ulong>();
            this.Session = session;
        }

        public bool SetPlayers(Player[] players) {
            if (this.m_players == null) {
                this.m_players = players;
            } else {
                if (this.m_players.Length == players.Length) {
                    for (int i = 0; i < players.Length; i++) {
                        if (!this.m_players.Any(x => x.ID == players[i].ID && x.Name.CompareTo(players[i].Name) == 0)) {
                            return false;
                        }
                    }
                } else {
                    return false;
                }
            }
            return true;
        }

        public void SetLength(TimeSpan timeSpan) 
            => timeSpan.IfTrue(x => x > this.m_timespan).Then(x => this.m_timespan = x);

        public RegisterEventResult RegisterEvent(TimeEvent timeEvent) {
            
            // Make sure it's within the time frame
            if (timeEvent.Timestamp > this.m_timespan) {
                return new RegisterEventResult(false) { WasOutsideTime = true };
            }
            
            // Does it exist somewhere else?
            if (this.m_events.FirstOrDefault(x => x.@event.Uid == timeEvent.Uid) is (TimeSpan time, IMatchEvent value)) {
                if (!IsDifferenceGreaterThan(time, timeEvent.Timestamp, new TimeSpan(0, 0, 2))) { // More than 2 seconds of difference -> OK, probably not the same event
                    return new RegisterEventResult(false) { ConflictingTimes = true };
                }
            }

            // Add to event list
            this.m_events.Add((timeEvent.Timestamp, timeEvent.UnderlyingEvent));
            this.m_eventCount++;

            // Return result
            return new RegisterEventResult(true);

        }

        private static bool IsDifferenceGreaterThan(TimeSpan first, TimeSpan second, TimeSpan maxDifference) {
            TimeSpan max = (first > second) ? first : second;
            TimeSpan min = (max == first) ? second : first;
            return (max - min) > maxDifference;
        }

        public bool CompileResults() {

            // Success marker
            bool success = true;

            // Sort by time
            this.m_events = this.m_events.OrderBy(x => x.time).ToList();

            // Create new list
            this.m_units = new();

            // Now loop through all events.
            for (int i = 0; i < this.m_events.Count; i++) {
                var stamp = this.m_events[i].time;
                uint uid = this.m_events[i].@event.Uid;
                string dbstring = $"EventAnalysis@{stamp}#{uid}";
                switch (this.m_events[i].@event) {
                    case KillEvent killEvent:
                        int killID = this.m_units.FindIndex(x => x.UnitID == killEvent.UnitID && x.PlayerOwner.ID == killEvent.UnitOwner.ID);
                        if (killID >= 0) {
                            if (!this.m_units[killID].MakeDead(stamp)) {
                                Trace.WriteLine($"Killed unit {killID} (Owner: {killEvent.UnitOwner.Name}) but it was either not deployed or was withdrawn.", dbstring);
                                success = false;
                            }
                        } else {
                            Trace.WriteLine($"Invalid unitID was killed ({killID})", dbstring);
                            success = false;
                        }
                        break;
                    case DeployEvent deployEvent:
                        int deployID = this.m_units.FindIndex(x => x.UnitID == deployEvent.SquadID && x.PlayerOwner.ID == deployEvent.DeployingPlayer.ID);
                        if (deployID == -1) {
                            UnitStatus status = new UnitStatus(deployEvent.DeployingPlayer, deployEvent.SquadID);
                            if (status.Deploy(stamp)) {
                                this.m_units.Add(status);
                            } else {
                                Trace.WriteLine($"Deployed unit {deployID} (Owner: {deployEvent.DeployingPlayer.Name}) but was either already deployed or killed.", dbstring);
                                success = false;
                            }
                        } else {
                            if (!this.m_units[deployID].Deploy(stamp)) {
                                Trace.WriteLine($"Invalid unitID was deployed ({deployID})", dbstring);
                                success = false; // deployed a dead or already deployed unit
                            }
                        }
                        break;
                    case PickupEvent pickupEvent:
                        // TODO: Implement
                        break;
                    case CaptureEvent captureEvent:
                        // TODO: Implement
                        break;
                    case RetreatEvent retreatEvent: // retreat/withdraw
                        int retreatID = this.m_units.FindIndex(x => x.UnitID == retreatEvent.WithdrawingUnitID && x.PlayerOwner.ID == retreatEvent.WithdrawPlayer.ID);
                        if (retreatID >= 0) {
                            if (!this.m_units[retreatID].Callback(stamp, retreatEvent.WithdrawingUnitVeterancyChange, retreatEvent.WithdrawingUnitVeterancyExperience)) {
                                Trace.WriteLine($"Withdrew unit {retreatID} (Owner: {retreatEvent.WithdrawPlayer.Name}) that was either dead or not deployed", dbstring);
                                success = false;
                            }
                        } else {
                            Trace.WriteLine($"Invalid unitID was withdrawn ({retreatID})", dbstring);
                            success = false;
                        }
                        break;
                    case VictoryEvent victoryEvent:
                        if (!this.m_winners.Add(victoryEvent.VictorID)) {
                            Trace.WriteLine($"Attempted to mark player {victoryEvent.VictorID} as winner, but has already been marked as winner.", dbstring);
                            success = false;
                        }
                        break;
                    default: break;
                }
            }

            // Finally mark it finalizable
            this.m_isFinalizable = success;

            // Return true
            return success;

        }

        public bool IsWinner(Player player) => this.m_winners.Contains(player.SteamID);

    }

}
