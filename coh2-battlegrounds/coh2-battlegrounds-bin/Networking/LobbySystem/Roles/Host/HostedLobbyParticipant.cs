namespace Battlegrounds.Networking.LobbySystem.Roles.Host {

    public class HostedLobbyParticipant : ILobbyParticipant {

        private ILobbyCompany m_selectedCompany;

        public ulong Id { get; }

        public virtual bool IsSelf => this.Id == BattlegroundsInstance.Steam.User.ID;

        public virtual string Name { get; }

        public ILobbyCompany Company => this.m_selectedCompany;

        public HostedLobbyParticipant(ulong id, string name) {
            this.Id = id;
            this.Name = name;

        }

        public bool SetCompany(ILobbyCompany company) => this.m_selectedCompany == company;

    }

}
