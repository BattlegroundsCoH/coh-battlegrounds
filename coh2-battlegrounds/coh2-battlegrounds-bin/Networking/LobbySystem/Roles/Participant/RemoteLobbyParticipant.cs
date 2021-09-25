using System;

using Battlegrounds.Networking.Memory;
using Battlegrounds.Networking.Remoting.Objects;

namespace Battlegrounds.Networking.LobbySystem.Roles.Participant {

    public class RemoteLobbyParticipant : ILobbyParticipant, IRemoteReference {

        protected readonly IRemoteHandle m_handle;
        private readonly ObjectCache<ILobbyCompany> m_company;
        private readonly AuthObject m_authObj;

        public ulong Id { get; }

        public virtual string Name { get; }

        public virtual bool IsSelf => AuthService.AuthInstance(this.m_authObj, x => x.IsUser(this.Id));

        public ILobbyCompany Company
            => this.m_company.GetCachedValue(() => this.m_handle.GetRemoteProperty<ILobbyCompany, ILobbyParticipant>(this.ObjectID));

        public IObjectID ObjectID { get; }

        public event ObservableValueChangedHandler<ILobbyParticipant> ValueChanged;
        public event ObservableMethodInvokedHandler<ILobbyParticipant> MethodInvoked;

        public RemoteLobbyParticipant(IObjectID objectID, IRemoteHandle remoteHandle, ulong id, string name) {

            this.ObjectID = objectID;
            this.m_handle = remoteHandle;

            this.Id = id;
            this.Name = name;

        }

        public bool SetCompany(ILobbyCompany company) => throw new NotImplementedException();

    }

}
