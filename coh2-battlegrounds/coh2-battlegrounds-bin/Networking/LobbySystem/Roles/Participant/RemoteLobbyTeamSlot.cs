using System;

using Battlegrounds.Networking.Memory;
using Battlegrounds.Networking.Remoting.Objects;
using Battlegrounds.Networking.Remoting.Reflection;

namespace Battlegrounds.Networking.LobbySystem.Roles.Participant {

    public class RemoteLobbyTeamSlot : ILobbyTeamSlot, IRemoteReference {

        private readonly IRemoteHandle m_handle;
        private readonly ValueCache<TeamSlotState> m_slotState;
        private readonly ObjectCache<ILobbyParticipant> m_slotOccupant;

        public IObjectID ObjectID { get; }

        public ILobbyTeam SlotTeam { get; }

        public TeamSlotState SlotState
            => this.m_slotState.GetCachedValue(() => this.m_handle.GetRemoteProperty<TeamSlotState, ILobbyTeamSlot>(this.ObjectID));

        public ILobbyParticipant SlotOccupant
            => this.m_slotOccupant.GetCachedValue(() => this.m_handle.GetRemoteProperty<ILobbyParticipant, ILobbyTeamSlot>(this.ObjectID));

        public event ObservableValueChangedHandler<ILobbyTeamSlot> ValueChanged;
        public event ObservableMethodInvokedHandler<ILobbyTeamSlot> MethodInvoked;

        public RemoteLobbyTeamSlot(IObjectID id, IRemoteHandle handle, ILobbyTeam team, TeamSlotState initialState, ILobbyParticipant initialOccupant) {

            // Set networking properties
            this.ObjectID = id;
            this.m_handle = handle;

            // Create cache values
            this.m_slotState = new(initialState, TimeSpan.FromMinutes(1));
            this.m_slotOccupant = new(initialOccupant, TimeSpan.FromMinutes(1));

            // Set team
            this.SlotTeam = team;

        }

        [RemotableMethod]
        public void ClearOccupant()
            => this.m_slotOccupant.Null();

        [RemotableMethod]
        public bool SetOccupant(ILobbyParticipant occupant) {
            this.m_slotOccupant.SetCachedValue(occupant);
            return true;
        }

        [RemotableMethod]
        public bool SetState(TeamSlotState newState) {
            this.m_slotState.SetCachedValue(newState);
            return true;
        }

    }

}
