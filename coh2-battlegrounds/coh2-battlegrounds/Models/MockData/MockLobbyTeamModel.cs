using System;
using System.Linq;

using Battlegrounds.Networking.Lobby;
using Battlegrounds.Networking.Remoting.Query;
using Battlegrounds.Networking.Requests;

namespace BattlegroundsApp.Models.MockData {

    public class MockLobbyTeamModel : ILobbyTeam {

        private readonly CommandQueryResultVector m_data;
        private readonly ILobbyTeamSlot[] m_slots;

        public int Capacity { get; }

        public int Size => this.m_slots.Count(x => x.SlotState is LobbyTeamSlotState.OCCUPIED);

        public int TeamIndex { get; }

        public bool HasOpenSlot => this.m_slots.Any(x => x.SlotState is LobbyTeamSlotState.OPEN);

        public ILobbyTeamSlot[] Slots => this.m_slots;

        public IRequestHandler RequestHandler { get; }

        public MockLobbyTeamModel(int tid, LobbyHandler handler, CommandQueryResultVector dataVector) {

            // Set data
            this.RequestHandler = handler.RequestHandler;
            this.m_data = dataVector.Reverse();

            // Set Team Index
            this.TeamIndex = tid;

            // Set capacity
            this.Capacity = this.m_data.Dimensions;
            this.m_slots = new ILobbyTeamSlot[this.Capacity];

            // Parse vector data
            for (int i = 0; i < this.m_data.Dimensions; i++) {

                // Get the slot data
                var slot = (this.m_data[i] as CommandQueryResultVector).Reverse();
                var (slotState, id, name, army, company, companyValue) = slot.ToTuple<string, ulong, string, string, string, double>();
                if (slotState is not "OCCUPIED") {
                    this.m_slots[i] = new MockLobbyTeamSlotModel(slotState, null);
                } else {

                    if (id == handler.Self.ID) {
                        this.m_slots[i] = new MockLobbyTeamSlotModel(slotState, handler.Self);
                    } else {
                        this.m_slots[i] = new MockLobbyTeamSlotModel(slotState, new MockLobbyTeamMemberModel(id, name, army, company, companyValue));
                    }

                }

            }

        }

        public bool CanSetCapacity(int capacity) => throw new NotSupportedException();

        public ILobbyTeamSlot GetSlotAt(int index) => this.m_slots[index];

        public bool IsMember(ILobbyMember member) {
            for (int i = 0; i < this.Slots.Length; i++) {
                if (this.Slots[i].SlotState is LobbyTeamSlotState.OCCUPIED && this.Slots[i].SlotOccupant.ID == member.ID) {
                    return true;
                }
            }
            return false;
        }

        public void JoinTeam(ILobbyMember member) => throw new NotSupportedException();

        public void LeaveTeam(ILobbyMember member) => throw new NotSupportedException();

        public void SetCapacity(int capacity) => throw new NotSupportedException();

        public void SwapSlots(ILobbyMember from, int to) => throw new NotSupportedException();

        public void SwapSlots(int from, int to) => throw new NotSupportedException();

    }

}
