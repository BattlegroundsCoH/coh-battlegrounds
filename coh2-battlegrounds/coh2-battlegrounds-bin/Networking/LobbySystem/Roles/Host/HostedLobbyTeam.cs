using System.Linq;

using Battlegrounds.Functional;

namespace Battlegrounds.Networking.LobbySystem.Roles.Host {

    public class HostedLobbyTeam : ILobbyTeam {

        public int SlotCapacity => this.Slots.Count(x => x.SlotState is not TeamSlotState.Disabled);

        public int SlotCount => this.Slots.Count(x => x.SlotState is TeamSlotState.Occupied);

        public ILobbyTeamSlot[] Slots { get; }

        public ILobby Lobby { get; }

        public HostedLobbyTeam(ILobby lobby) {

            // Set lobby
            this.Lobby = lobby;

            // Create slots
            this.Slots = new ILobbyTeamSlot[] {
                new HostedLobbyTeamSlot(this),
                new HostedLobbyTeamSlot(this),
                new HostedLobbyTeamSlot(this),
                new HostedLobbyTeamSlot(this)
            };

        }

        public ILobbyParticipant[] GetTeamMembers()
            => this.Slots.Filter(x => x.SlotState is TeamSlotState.Occupied)
            .Map(x => x.SlotOccupant);


        public bool IsTeamMember(ILobbyParticipant participant)
            => this.Slots.Any(x => x.SlotOccupant.Equals(participant));

        public bool Remove(ILobbyParticipant participant) {
            for (int i = 0; i < this.Slots.Length; i++) {
                if (this.Slots[i].SlotOccupant?.Equals(participant) ?? false) {
                    this.Slots[i].ClearOccupant();
                    return true;
                }
            }
            return false;
        }

    }

}
