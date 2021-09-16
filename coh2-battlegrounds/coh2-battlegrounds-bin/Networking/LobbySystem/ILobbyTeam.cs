namespace Battlegrounds.Networking.LobbySystem {

    /// <summary>
    /// Interface representing a team in a <see cref="ILobby"/>.
    /// </summary>
    public interface ILobbyTeam {

        /// <summary>
        /// The max amount of slots available in a team.
        /// </summary>
        public const int MAXSLOTS = 4;

        /// <summary>
        /// Get the amount of slots available.
        /// </summary>
        int SlotCapacity { get; }

        /// <summary>
        /// Get the amount of slots that are currently occupied.
        /// </summary>
        int SlotCount { get; }

        /// <summary>
        /// Get the slots available on the team.
        /// </summary>
        ILobbyTeamSlot[] Slots { get; }

        /// <summary>
        /// Get the lobby instance owning this team.
        /// </summary>
        ILobby Lobby { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ILobbyParticipant[] GetTeamMembers();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="participant"></param>
        /// <returns></returns>
        bool IsTeamMember(ILobbyParticipant participant);

    }

}
