using System;
using System.Collections.Generic;
using System.Text;

namespace Battlegrounds.Online.Lobby {
    
    public enum ManagedLobbyTeamType {
        Spectator = 0,
        Allies = 1,
        Axus = 2,
    }

    public sealed class ManagedLobbyTeam {

        private List<ManagedLobbyMember> m_members;
        public List<ManagedLobbyMember> Members => this.m_members;

        public int Size => this.Members.Count;

        public ManagedLobbyTeamType Team { get; }

        public ManagedLobbyTeam(ManagedLobbyTeamType teamtype) {
            this.Team = teamtype;
            this.m_members = new List<ManagedLobbyMember>();
        }

        public void ForEachMember(Action<ManagedLobbyMember> action) => this.m_members.ForEach(action);

        public bool AllMembers(Predicate<ManagedLobbyMember> predicate) => this.m_members.TrueForAll(predicate);

    }

}
