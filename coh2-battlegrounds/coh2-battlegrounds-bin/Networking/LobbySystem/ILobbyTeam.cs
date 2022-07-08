namespace Battlegrounds.Networking.LobbySystem;

/// <summary>
/// Interface for representing a team in a lobby.
/// </summary>
public interface ILobbyTeam : IHandleObject<ILobbyHandle> {

    /// <summary>
    /// Get the slots of the team.
    /// </summary>
    ILobbySlot[] Slots { get; }

    /// <summary>
    /// Get the team ID.
    /// </summary>
    int TeamID { get; }
    
    /// <summary>
    /// Get the capacity of the team.
    /// </summary>
    int Capacity { get; }
    
    /// <summary>
    /// Get the role of the team.
    /// </summary>
    string TeamRole { get; }

    /// <summary>
    /// Get the <see cref="ILobbySlot"/> representing the slot for a member.
    /// </summary>
    /// <param name="memberID">The Id of the member to fetch the <see cref="ILobbySlot"/> for.</param>
    /// <returns>A <see cref="ILobbySlot"/> for the member or null if no slot is found.</returns>
    ILobbySlot? GetSlotOfMember(ulong memberID);

}
