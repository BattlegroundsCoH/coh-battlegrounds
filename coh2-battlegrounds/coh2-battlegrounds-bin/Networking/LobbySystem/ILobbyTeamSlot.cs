namespace Battlegrounds.Networking.LobbySystem {

    /// <summary>
    /// Enum representing all possible states a <see cref="ILobbyTeamSlot"/> instance may take.
    /// </summary>
    public enum TeamSlotState {

        /// <summary>
        /// Open state
        /// </summary>
        Open,

        /// <summary>
        /// Occupied state
        /// </summary>
        Occupied,

        /// <summary>
        /// Locked state
        /// </summary>
        Locked,

        /// <summary>
        /// Disabled state
        /// </summary>
        Disabled,

    }

    /// <summary>
    /// Interface representing a team slot on a <see cref="ILobbyTeam"/>.
    /// </summary>
    public interface ILobbyTeamSlot {

        /// <summary>
        /// Get the team the slot belongs to.
        /// </summary>
        ILobbyTeam SlotTeam { get; }

        /// <summary>
        /// Get the state of the team slot.
        /// </summary>
        TeamSlotState SlotState { get; }

        /// <summary>
        /// Get the occupant of the team slot.
        /// </summary>
        ILobbyParticipant SlotOccupant { get; }

        /// <summary>
        /// Set the occupant who will occupy the slot.
        /// </summary>
        /// <param name="occupant">The occupant that will now occupy the slot.</param>
        /// <returns>If occupant was updated, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
        bool SetOccupant(ILobbyParticipant occupant);

        /// <summary>
        /// Set the state of the team slot.
        /// </summary>
        /// <param name="newState">The new state of the slot.</param>
        /// <returns>If state was updated, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
        bool SetState(TeamSlotState newState);

    }

}
