﻿using System;
using System.Collections.Generic;
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

        private bool m_isFinalizable;
        private TimeSpan m_timespan;
        private List<(TimeSpan time, IMatchEvent @event)> m_events;
        private Player[] m_players;

        private List<UnitStatus> m_units;

        public bool IsFinalizableMatch => this.m_isFinalizable && this.Session.AllowPersistency;

        public IReadOnlyList<UnitStatus> Units => this.m_units.AsReadOnly();

        public IReadOnlyCollection<Player> Players => this.m_players;

        public ISession Session { get; }

        public EventAnalysis(ISession session) {
            this.m_isFinalizable = false;
            this.m_timespan = TimeSpan.Zero;
            this.m_events = new List<(TimeSpan, IMatchEvent)>();
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

            // Return result
            return new RegisterEventResult(true);

        }

        private static bool IsDifferenceGreaterThan(TimeSpan first, TimeSpan second, TimeSpan maxDifference) {
            TimeSpan max = (first > second) ? first : second;
            TimeSpan min = (max == first) ? second : first;
            return (max - min) > maxDifference;
        }

        public bool CompileResults() {

            // Sort by time
            this.m_events = this.m_events.OrderBy(x => x.time).ToList();

            // Create new list
            this.m_units = new();

            // Now loop through all events.
            for (int i = 0; i < this.m_events.Count; i++) {
                var stamp = this.m_events[i].time;
                switch (this.m_events[i].@event) {
                    case KillEvent killEvent:
                        int killID = this.m_units.FindIndex(x => x.UnitID == killEvent.UnitID && x.PlayerOwner.ID == killEvent.UnitOwner.ID);
                        if (killID >= 0) {
                            if (!this.m_units[killID].MakeDead(stamp)) {
                                return false; // Killed a unit that was either not deployed or was withdrawn
                            }
                        } else {
                            return false;
                        }
                        break;
                    case DeployEvent deployEvent:
                        int deployID = this.m_units.FindIndex(x => x.UnitID == deployEvent.SquadID && x.PlayerOwner.ID == deployEvent.DeployingPlayer.ID);
                        if (deployID == -1) {
                            UnitStatus status = new UnitStatus(deployEvent.DeployingPlayer, deployEvent.SquadID);
                            if (status.Deploy(stamp)) {
                                this.m_units.Add(status);
                            } else {
                                return false;
                            }
                        } else {
                            if (!this.m_units[deployID].Deploy(stamp)) {
                                return false; // deployed a dead or already deployed unit
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
                                return false; // Withdrew a unit that was either dead or not deployed
                            }
                        } else {
                            return false;
                        }
                        break;
                    default: break;
                }
            }

            // Finally mark it finalizable
            this.m_isFinalizable = true;

            // Return true
            return true;

        }

    }

}