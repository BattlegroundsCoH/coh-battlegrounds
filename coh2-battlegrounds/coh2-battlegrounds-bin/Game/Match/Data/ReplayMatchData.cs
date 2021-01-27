using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Linq;

using Battlegrounds.Game.DataSource.Replay;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match.Data.Events;

using RegexMatch = System.Text.RegularExpressions.Match;
using System.Diagnostics;

namespace Battlegrounds.Game.Match.Data {

    /// <summary>
    /// Object representing data read from a <see cref="ReplayFile"/>.
    /// </summary>
    public sealed class ReplayMatchData : IMatchData, IEnumerable<TimeEvent> {

        /// <summary>
        /// The path to the latest replay file.
        /// </summary>
        public static readonly string LATEST_REPLAY_FILE = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\company of heroes 2\\playback\\temp.rec";

        private ReplayFile m_replay;
        private List<TimeEvent> m_events;
        private TimeSpan m_length;
        private Player[] m_players;
        private bool m_isSessionValid;

        private readonly Regex broadcastRegex = new Regex(@"(?<cmdtype>\w)\[(?<content>(?<msg>(\w|_|-|:|\.|\d)+)|,|\s)*\]");

        public ISession Session { get; }

        public TimeSpan Length { get; }

        public bool IsSessionMatch => this.m_isSessionValid;

        /// <summary>
        /// Get the <see cref="ReplayFile"/> all match data is read from.
        /// </summary>
        public ReplayFile Replay => this.m_replay;

        /// <summary>
        /// Get all the players in this match.
        /// </summary>
        public ReadOnlyCollection<Player> Players => new ReadOnlyCollection<Player>(this.m_players);

        /// <summary>
        /// Create a <see cref="ReplayMatchData"/> instance for the specified <see cref="Match.Session"/>.
        /// </summary>
        /// <param name="session">The <see cref="Match.Session"/> to define match data for.</param>
        public ReplayMatchData(ISession session) {
            this.Session = session;
            this.m_replay = null;
            this.m_events = new List<TimeEvent>();
            this.m_isSessionValid = false;
        }

        public bool LoadMatchData(string matchFile) {

            // Create instance
            this.m_replay = new ReplayFile(matchFile);

            // Load the replay
            if (!this.m_replay.LoadReplay()) {
                Trace.WriteLine($"Failed to read replay file {matchFile}", "ReplayMatchData");
                return false;
            }

            // Return true
            return true;

        }

        public bool ParseMatchData() {

            // Get the players
            this.m_players = this.m_replay.Players;

            // Get the ticks
            var matchTicks = this.m_replay.Ticks;

            // Length of the match
            this.m_length = TimeSpan.Zero;

            // unique ID
            uint id = 0;

            // Loop through all the ticks
            for (int i = 0; i < matchTicks.Length; i++) {

                // Loop through all events
                for (int j = 0; j < matchTicks[i].Events.Count; j++) {

                    // Get event
                    var tickEvent = matchTicks[i].Events[j];

                    // Make sure it's a valid type
                    if (tickEvent.Type < (byte)GameEventType.EVENT_MAX && tickEvent.EventType == GameEventType.PCMD_BroadcastMessage) {

                        // Get the data
                        if (this.ParseBroadcastMessage(tickEvent, id++) is IMatchEvent broadcastMessage) {
                            this.m_events.Add(new TimeEvent(tickEvent.TimeStamp, broadcastMessage));
                        } else {
                            return false;
                        }

                    }

                    // Update match length
                    if (tickEvent.TimeStamp > this.m_length) {
                        this.m_length = tickEvent.TimeStamp;
                    }

                }

            }

            // Order by timestamp (And we assume all events are of TimeEvent because of how we add events in this instance).
            this.m_events = this.m_events.OrderBy(x => x.Timestamp).ToList();

            // Return true
            return true;

        }

        private IMatchEvent ParseBroadcastMessage(GameEvent gameEvent, uint id) { 
            
            // Make sure it's valid
            if (gameEvent.AttachedMessage.Length > 0) {

                // Apply match
                RegexMatch match = this.broadcastRegex.Match(gameEvent.AttachedMessage);

                // Did we match?
                if (match.Success) {

                    // Get values
                    char msgtype = char.ToUpper(match.Groups["cmdtype"].Value[0]); // Always bump it to upper (incase it's forgotten in Scar script)
                    string[] values = match.Groups["content"].Captures.ToList().Where(x => x.Value != "," && x.Value != " ").Select(x => x.Value).ToArray();

                    // Get the invoking player
                    var player = this.m_players.First(x => x.ID == gameEvent.PlayerID);

                    // TODO: Read id from message

                    // Return the proper type
                    return msgtype switch {
                        'D' => new DeployEvent(id, values, player),
                        'K' => new KillEvent(id, values, player),
                        'T' => new CaptureEvent(id, values, player),
                        'R' => new RetreatEvent(id, values, player),
                        'I' => new PickupEvent(id, values, player),
                        'V' => new VictoryEvent(id, values),
                        'G' => this.CreateVerificationEvent(id, values),
                        _ => null,
                    };

                } else {

                    // No match -> Invalid broadcast message
                    return null;

                }

            } else {
                return null;
            }

        }

        private IMatchEvent CreateVerificationEvent(uint id, string[] values) {
            VerificationEvent verification = new VerificationEvent(id, values);
            if (verification.VerificationType == VerificationType.SessionVerification) {
                this.m_isSessionValid = verification.Verify(this.Session);
            }
            return verification;
        }

        public IEnumerator<TimeEvent> GetEnumerator() => ((IEnumerable<TimeEvent>)this.m_events).GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.m_events).GetEnumerator();

    }

}
