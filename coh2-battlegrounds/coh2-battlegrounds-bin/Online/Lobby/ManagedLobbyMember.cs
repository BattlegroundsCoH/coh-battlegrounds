using System;

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
        /// Check if this instance of <see cref="ManagedLobbyMember"/> can be considered the same player as the <see cref="ManagedLobbyMember"/> instance being compared to.
        /// </summary>
        /// <param name="other">The other <see cref="ManagedLobbyMember"/> instance to check.</param>
        /// <returns></returns>
        public abstract bool IsSamePlayer(ManagedLobbyMember other);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="faction"></param>
        public abstract void UpdateFaction(string faction);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="strength"></param>
        public abstract void UpdateCompany(string name, double strength);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        public abstract void UpdateTeamPosition(int position);
    
    }

}
