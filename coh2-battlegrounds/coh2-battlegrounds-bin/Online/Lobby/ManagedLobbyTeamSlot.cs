namespace Battlegrounds.Online.Lobby {
    
    public enum ManagedLobbyTeamSlotState {
        Open,
        Occupied,
    }

    public class ManagedLobbyTeamSlot {

        private ManagedLobbyMember m_occupant;
        private int m_index;

        public ManagedLobbyMember Occupant => this.m_occupant;

        public ManagedLobbyTeamSlotState State => (this.m_occupant is null) ? ManagedLobbyTeamSlotState.Open : ManagedLobbyTeamSlotState.Occupied;

        public int SlotIndex => this.m_index;

        public ManagedLobbyTeamSlot(int slotIndex) {
            this.m_index = slotIndex;
            this.m_occupant = null;
        }

        public void SetOccupant(ManagedLobbyMember member) => this.m_occupant = member;

    }

}
