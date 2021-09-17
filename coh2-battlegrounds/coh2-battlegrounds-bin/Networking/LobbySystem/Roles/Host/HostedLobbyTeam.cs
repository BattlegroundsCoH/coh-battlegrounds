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

        public bool SetCapacity(int capacity) { // O(N + (N - C)) ?
            bool[] swaps = new bool[this.Slots.Length];
            for (int i = 0; i < this.Slots.Length; i++) {
                swaps[i] = this.Slots[i].SlotState is TeamSlotState.Occupied;
            }
            for (int i = this.Slots.Length - 1; i >= 0; i--) {
                if (i >= capacity) {
                    if (swaps[i]) {
                        for (int j = capacity; j >= 0; j--) {
                            if (!swaps[j]) {
                                this.SwapSlots(i, j);
                                swaps[j] = false;
                                break;
                            }
                        }
                    }
                    _ = this.Slots[i].SetState(TeamSlotState.Disabled);
                } else if (!swaps[i]) {
                    _ = this.Slots[i].SetState(TeamSlotState.Open);
                }
            }
            return true;
        }

        private void SwapSlots(int fromIndex, int toIndex) {

            // Store temp
            var temp = this.Slots[fromIndex].SlotOccupant;

            // Assign from
            this.Slots[fromIndex].ClearOccupant();
            _ = this.Slots[fromIndex].SetOccupant(this.Slots[toIndex].SlotOccupant);

            // Assign to
            this.Slots[toIndex].ClearOccupant();
            _ = this.Slots[toIndex].SetOccupant(temp);

        }

    }

}
