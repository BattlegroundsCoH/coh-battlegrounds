namespace Battlegrounds.Online.Services {
    
    /// <summary>
    /// Represents a lobby that can be connected to.
    /// </summary>
    public struct ConnectableLobby {

        /// <summary>
        /// The server-assigned GUID given to the lobby.
        /// </summary>
        public string lobby_guid;

        /// <summary>
        /// The name of the lobby.
        /// </summary>
        public string lobby_name;

        /// <summary>
        /// Is the lobby password protected.
        /// </summary>
        public bool lobby_passwordProtected;

    }

}
