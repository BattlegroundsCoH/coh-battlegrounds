namespace Battlegrounds.Networking.LobbySystem.Roles.Host {

    public class HostedLobbyParticipant : ILobbyParticipant {

        private ILobbyCompany m_selectedCompany;
        private readonly AuthObject m_authObj;

        public ulong Id { get; }

        public virtual bool IsSelf => AuthService.AuthInstance(this.m_authObj, x => x.IsUser(this.Id));

        public virtual string Name { get; }

        public ILobbyCompany Company => this.m_selectedCompany;

        public HostedLobbyParticipant(ulong id, string name) {
            this.Id = id;
            this.Name = name;
            this.m_authObj = new AuthObject(id, name);
        }

        public bool SetCompany(ILobbyCompany company) => this.m_selectedCompany == company;

    }

}
