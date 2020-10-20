using System;

namespace Battlegrounds.Online.Lobby {
    
    public enum ManagedLobbyTeamType {
        Spectator = 0,
        Allies = 1,
        Axus = 2,
    }

    public sealed class ManagedLobbyTeam {

        private ManagedLobby m_lobby;
        private ManagedLobbyMember[] m_members;
        private int m_nextIndex;

        public ManagedLobbyMember[] Members => this.m_members;

        public int Capacity => this.Members.Length;

        public int Size => this.m_nextIndex;

        public ManagedLobbyTeamType Team { get; }

        public ManagedLobbyTeam(ManagedLobby lobby, byte teamsize, ManagedLobbyTeamType teamtype) {
            this.Team = teamtype;
            this.m_nextIndex = 0;
            this.m_members = new ManagedLobbyMember[teamsize];
            this.m_lobby = lobby;
        }

        public void ForEachMember(Action<ManagedLobbyMember> action) { 
            for (int i = 0; i < this.Capacity; i++) {
                if (this.m_members[i] is ManagedLobbyMember member) {
                    action(member);
                } else {
                    break;
                }
            }
        }

        public bool AllMembers(Predicate<ManagedLobbyMember> predicate) {
            for (int i = 0; i < this.Capacity; i++) {
                if (this.m_members[i] is ManagedLobbyMember member) {
                    if (!predicate(member)) {
                        return false;
                    }
                } else {
                    break;
                }
            }
            return true;
        }

    }

}
