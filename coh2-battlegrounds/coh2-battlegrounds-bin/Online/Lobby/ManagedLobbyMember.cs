namespace Battlegrounds.Online.Lobby {
    
    /// <summary>
    /// 
    /// </summary>
    public abstract class ManagedLobbyMember {

        /// <summary>
        /// 
        /// </summary>
        public abstract ulong ID { get; }

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        public abstract int LobbyIndex { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract bool IsSamePlayer(ManagedLobbyMember other);

    }

}
