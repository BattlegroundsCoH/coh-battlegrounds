using System.Collections.Generic;

namespace Battlegrounds.Online.Services {
    
    /// <summary>
    /// Represents a lobby that can be connected to.
    /// </summary>
    public struct ConnectableLobby {

        /// <summary>
        /// Represents a player that's connected to a <see>ConnectableLobby</see>
        /// </summary>
        public struct ConnectableLobbyPlayer {
            
            /// <summary>
            /// The UTF-8 string representation of the connected player's name.
            /// </summary>
            public string player_name;
            
            /// <summary>
            /// The the current faction ID selected by the player.
            /// </summary>
            public string player_faction;
            
            /// <summary>
            /// The player's index in the lobby.
            /// </summary>
            public int player_index;
            
            /// <summary>
            /// The player's team index.
            /// </summary>
            public int player_team_index;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pIndex"></param>
            /// <param name="pName"></param>
            /// <param name="pFaction"></param>
            /// <param name="pTIndex"></param>
            public ConnectableLobbyPlayer(int pIndex, string pName, string pFaction, int pTIndex) {
                this.player_name = pName;
                this.player_faction = pFaction;
                this.player_index = pIndex;
                this.player_team_index = pTIndex;
            }

        }

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

        /// <summary>
        /// The lobby players currently in the lobby (Player name, team index).
        /// </summary>
        public List<ConnectableLobbyPlayer> lobby_players;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gid"></param>
        /// <param name="name"></param>
        /// <param name="isPsswProtected"></param>
        public ConnectableLobby(string gid, string name, bool isPsswProtected) {
            this.lobby_guid = gid;
            this.lobby_name = name;
            this.lobby_passwordProtected = isPsswProtected;
            this.lobby_players = new List<ConnectableLobbyPlayer>();
        }

    }

}
