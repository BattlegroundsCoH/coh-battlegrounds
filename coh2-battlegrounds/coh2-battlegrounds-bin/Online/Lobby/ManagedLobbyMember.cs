namespace Battlegrounds.Online.Lobby {
    
    /// <summary>
    /// Abstract representation of a member of a <see cref="ManagedLobby"/>.
    /// </summary>
    public abstract class ManagedLobbyMember {

        /// <summary>
        /// The unique identifier of the <see cref="ManagedLobbyMember"/>. Indicies below 32 are assumed to be of type <see cref="AILobbyMember"/> while above is of type <see cref="HumanLobbyMember"/>.
        /// </summary>
        public abstract ulong ID { get; }

        /// <summary>
        /// The display name of the <see cref="ManagedLobbyMember"/>.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The <see cref="ManagedLobbyMember"/>'s faction.
        /// </summary>
        public abstract string Faction { get; }

        /// <summary>
        /// The <see cref="ManagedLobbyMember"/>'s selected company's name.
        /// </summary>
        public abstract string CompanyName { get; }

        /// <summary>
        /// The <see cref="ManagedLobbyMember"/>'s selected company's strength
        /// </summary>
        public abstract double CompanyStrength { get; }

        /// <summary>
        /// Check if this instance of <see cref="ManagedLobbyMember"/> can be considered the same player as the <see cref="ManagedLobbyMember"/> instance being compared to.
        /// </summary>
        /// <param name="other">The other <see cref="ManagedLobbyMember"/> instance to check.</param>
        /// <returns></returns>
        public abstract bool IsSamePlayer(ManagedLobbyMember other);

        /// <summary>
        /// Update the faction of the <see cref="ManagedLobbyMember"/>.
        /// </summary>
        /// <param name="faction">The new faction to set.</param>
        /// <remarks>
        /// No validation is applied!
        /// </remarks>
        public abstract void UpdateFaction(string faction);

        /// <summary>
        /// Update the company used by the <see cref="ManagedLobbyMember"/>.
        /// </summary>
        /// <param name="name">The display name of the company</param>
        /// <param name="strength">The strength of the company.</param>
        /// <remarks>
        /// No validation is applied!
        /// </remarks>
        public abstract void UpdateCompany(string name, double strength);

    }

}
