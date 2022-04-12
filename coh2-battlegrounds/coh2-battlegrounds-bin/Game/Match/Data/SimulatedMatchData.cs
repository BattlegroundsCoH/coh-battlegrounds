using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match.Data.Events;

namespace Battlegrounds.Game.Match.Data {

    public class SimulatedMatchData : IMatchData {

        private List<Player> m_players;
        private List<TimeEvent> m_events;
        private TimeSpan m_length;

        public ISession Session { get; }

        public TimeSpan Length => this.m_length;

        public bool IsSessionMatch => true;

        public ReadOnlyCollection<Player> Players => new ReadOnlyCollection<Player>(this.m_players);

        public SimulatedMatchData(ISession session) {
            this.Session = session;
            this.m_length = TimeSpan.Zero;
            this.m_events = new List<TimeEvent>();
            this.m_players = new List<Player>();
        }

        public bool LoadMatchData(string matchFile) => true;

        public bool ParseMatchData() => true;

        public void CreatePlayer(Player player) => this.m_players.Add(player);

        public void AddEvent(TimeSpan timeStamp, IMatchEvent matchEvent) {

            // Update length
            if (timeStamp > this.m_length) {
                this.m_length = timeStamp;
            }

            // Add event
            TimeEvent e = new TimeEvent(timeStamp, matchEvent);
            this.m_events.Add(e);

        }

        public IEnumerator<IMatchEvent> GetEnumerator() => this.m_events.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    }

}
