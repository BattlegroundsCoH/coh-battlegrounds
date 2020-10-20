using System;

namespace Battlegrounds.Online.Lobby {

    public sealed class HumanLobbyMember : ManagedLobbyMember {

        private ManagedLobby m_lobby;
        private ulong m_uniqueID;
        private string m_faction;
        private string m_name;
        private string m_company;
        private double m_strength;
        private int m_index;

        public override ulong ID => this.m_uniqueID;

        public override string Name => this.m_name;

        public override string Faction => this.m_faction;

        public override string CompanyName => this.m_company;

        public override double CompanyStrength => this.m_strength;

        public override int LobbyIndex => this.m_index;

        private HumanLobbyMember(ManagedLobby lobby, int index, ulong id, string name, string company, double strnegth) {
            this.m_lobby = lobby;
            this.m_index = index;
            this.m_uniqueID = id;
            this.m_name = name;
            this.m_company = company;
            this.m_strength = strnegth;
        }

        public override bool IsSamePlayer(ManagedLobbyMember other) {
            if (other is HumanLobbyMember otherHuman) {
                return otherHuman.ID == this.ID;
            } else {
                return false;
            }
        }

    }

}
