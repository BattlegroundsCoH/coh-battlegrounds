using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Battlegrounds.Online.Services {
    
    /// <summary>
    /// Represents a lobby that can be connected to.
    /// </summary>
    public class ConnectableLobby {

        /// <summary>
        /// Represents a player that's connected to a <see cref="ConnectableLobby"/>.
        /// </summary>
        public class ConnectableLobbyPlayer {
            
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
            /// The player's unique steam index.
            /// </summary>
            public ulong player_steam_index;
            
            /// <summary>
            /// The player's team index.
            /// </summary>
            public int player_team_index;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pIndex"></param>
            /// <param name="sIndex"></param>
            /// <param name="pName"></param>
            /// <param name="pFaction"></param>
            /// <param name="pTIndex"></param>
            public ConnectableLobbyPlayer(int pIndex, ulong sIndex, string pName, string pFaction, int pTIndex) {
                this.player_name = pName;
                this.player_faction = pFaction;
                this.player_index = pIndex;
                this.player_steam_index = sIndex;
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
        /// The lobby players currently in the lobby (Player name, team index, ...).
        /// </summary>
        public List<ConnectableLobbyPlayer> lobby_players;

        /// <summary>
        /// New standard <see cref="ConnectableLobby"/> instance without player data.
        /// </summary>
        /// <param name="gid">The lobby GUID that's used by the server to identify the lobby.</param>
        /// <param name="name">The assigned name of the lobby.</param>
        /// <param name="isPsswProtected">Boolean flag makring whether the lobby is password protected or not.</param>
        public ConnectableLobby(string gid, string name, bool isPsswProtected) {
            this.lobby_guid = gid;
            this.lobby_name = name;
            this.lobby_passwordProtected = isPsswProtected;
            this.lobby_players = new List<ConnectableLobbyPlayer>();
        }

        public void Update() {
            try {
                this.lobby_players.Clear();
                Message message = new Message(Message_Type.LOBBY_INFO, this.lobby_guid);
                MessageSender.SendMessage(AddressBook.GetLobbyServer(), message, (a, msg) => {
                    var matches = Regex.Match(msg.Argument1, @"");
                    if (matches.Success) {

                    }
                });
            } catch { /* do something here? */ }
        }

    }

}
