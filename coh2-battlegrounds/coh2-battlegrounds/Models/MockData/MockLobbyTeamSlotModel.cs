using System;

using Battlegrounds.Networking.Lobby;

namespace BattlegroundsApp.Models.MockData {

    public class MockLobbyTeamSlotModel : ILobbyTeamSlot {

        public LobbyTeamSlotState SlotState { get; set; }

        public ILobbyMember SlotOccupant { get; set; }

        public MockLobbyTeamSlotModel(string slotState, ILobbyMember member) {

            // Get slot state
            this.SlotState = Enum.Parse<LobbyTeamSlotState>(slotState);

            // Set member
            this.SlotOccupant = member;

        }

    }

}
