namespace Battlegrounds.Online.Lobby {

    public sealed class HumanLobbyMember : ManagedLobbyMember {

        private ManagedLobby m_lobby;
        private ulong m_uniqueID;
        private string m_faction;
        private string m_name;
        private string m_company;
        private double m_strength;

        public override ulong ID => this.m_uniqueID;

        public override string Name => this.m_name;

        public override string Faction => this.m_faction;

        public override string CompanyName => this.m_company;

        public override double CompanyStrength => this.m_strength;

        public HumanLobbyMember(ManagedLobby lobby, ulong id, string name, string company, double strnegth) {
            this.m_lobby = lobby;
            this.m_uniqueID = id;
            this.m_name = name;
            this.m_company = company;
            this.m_strength = strnegth;
            this.m_faction = "soviet";
        }

        public override void UpdateFaction(string faction) => this.m_faction = faction;

        public override void UpdateCompany(string name, double strength) {
            this.m_company = name;
            this.m_strength = strength;
        }

        public override bool IsSamePlayer(ManagedLobbyMember other) {
            if (other is HumanLobbyMember otherHuman) {
                return otherHuman.ID == this.ID;
            } else {
                return false;
            }
        }

        public override string ToString() => $"{this.Name}#{this.ID} [{this.Faction}][{this.CompanyName}${this.CompanyStrength:0000.00}]";

    }

}
