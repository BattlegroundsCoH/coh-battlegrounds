namespace Battlegrounds.Networking.LobbySystem.Roles.Host {

    public class HostedLobbyTeamSlot : ILobbyTeamSlot {

        ILobbyParticipant m_participant;
        TeamSlotState m_state;

        public event ObservableValueChangedHandler<ILobbyTeamSlot> ValueChanged;
        public event ObservableMethodInvokedHandler<ILobbyTeamSlot> MethodInvoked;

        public ILobbyTeam SlotTeam { get; }

        public TeamSlotState SlotState => this.m_state;

        public ILobbyParticipant SlotOccupant => this.m_participant;

        public HostedLobbyTeamSlot(ILobbyTeam team) {

            // Set team
            this.SlotTeam = team;

            // Set initial state
            this.m_state = TeamSlotState.Open;
            this.m_participant = null;

        }

        public bool SetOccupant(ILobbyParticipant occupant) {

            // If already occupied, it's not possible
            if (this.m_participant is not null) {
                return false;
            }

            // Check if open, and assign occupant if that is the case
            if (this.m_state is TeamSlotState.Open) {

                // Update participant and state
                this.m_participant = occupant;
                this.m_participant.ValueChanged += this.OnOccupantChange;
                this.m_state = TeamSlotState.Occupied;
                
                // Invoke value changed
                this.ValueChanged?.Invoke(this, new(nameof(this.SlotState), this.m_state));

                // Return true
                return true;
            }

            // base case is default
            return false;

        }

        public bool SetState(TeamSlotState newState) {

            // If occuped, set as occupied and reutrn file
            if (this.m_participant is not null) {
                this.m_state = TeamSlotState.Occupied;
                return newState != TeamSlotState.Occupied;
            }

            // Set state and return true
            this.m_state = newState;

            // Invoke value changed
            this.ValueChanged?.Invoke(this, new(nameof(this.SlotState), this.m_state));

            return true;

        }

        public void ClearOccupant() {

            // Unhook event
            if (this.m_participant is not null) {
                this.m_participant.ValueChanged -= this.OnOccupantChange;
            }

            // Set values
            this.m_participant = null;
            this.m_state = TeamSlotState.Open;

            // Invoke value changed
            this.ValueChanged?.Invoke(this, new(nameof(this.SlotState), this.m_state));

        }

        private void OnOccupantChange(ILobbyParticipant participant, ObservableValueChangedEventArgs eventArgs) {
            
            // Make sure we're triggering on this occupant
            if (participant == this.m_participant) {
                this.ValueChanged?.Invoke(this, eventArgs); // Propogate to slot listener
            }

        }

    }

}
