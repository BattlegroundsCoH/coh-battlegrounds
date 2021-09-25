using System.Linq;

using Battlegrounds.Functional;
using Battlegrounds.Networking.Remoting.Objects;
using Battlegrounds.Networking.Remoting.Reflection;

namespace Battlegrounds.Networking.LobbySystem.Roles.Participant {
    
    public class RemoteLobbyTeam : ILobbyTeam, IRemoteReference {

        private readonly IRemoteHandle m_handle;

        public IObjectID ObjectID { get; }

        public int SlotCapacity => this.Slots.Count(x => x.SlotState is not TeamSlotState.Disabled);

        public int SlotCount => this.Slots.Count(x => x.SlotState is TeamSlotState.Occupied);

        public ILobbyTeamSlot[] Slots { get; }

        public ILobby Lobby { get; }

        public RemoteLobbyTeam(IObjectID objectID, IRemoteHandle remoteHandle, ILobby lobby) {

            // Set lobby
            this.Lobby = lobby;

            // Set remoting data
            this.ObjectID = objectID;
            this.m_handle = remoteHandle;

            // Alloc slots
            this.Slots = new ILobbyTeamSlot[4];

        }

        public ILobbyParticipant[] GetTeamMembers()
            => this.Slots.Filter(x => x.SlotState is TeamSlotState.Occupied).Map(x => x.SlotOccupant);

        public bool IsTeamMember(ILobbyParticipant participant)
            => this.Slots.Any(x => x.SlotOccupant.Equals(participant));
        
        [RemotableMethod]
        public bool Remove(ILobbyParticipant participant) {
            for (int i = 0; i < this.Slots.Length; i++) {
                if (this.Slots[i].SlotOccupant?.Equals(participant) ?? false) {
                    this.Slots[i].ClearOccupant();
                    return true;
                }
            }
            return false;
        }

        public bool SetCapacity(int capacity) => true;

        /// <summary>
        /// 
        /// </summary>
        public void SetAllSlots() { }

    }

}
