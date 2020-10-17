using System;
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
            /// The player's selected company name
            /// </summary>
            public string player_company;

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
            public ConnectableLobbyPlayer(int pIndex, ulong sIndex, string pName, string pFaction, string cName, int pTIndex) {
                this.player_name = pName;
                this.player_faction = pFaction;
                this.player_index = pIndex;
                this.player_steam_index = sIndex;
                this.player_team_index = pTIndex;
                this.player_company = cName;
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
        /// The currently selected map (Not formatted)
        /// </summary>
        public string lobby_map;

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
            this.lobby_map = "<none>";
            this.lobby_passwordProtected = isPsswProtected;
            this.lobby_players = new List<ConnectableLobbyPlayer>();
        }

        public void Update() {
            try {
                this.lobby_players.Clear();
                Message message = new Message(MessageType.LOBBY_INFO, this.lobby_guid);
                MessageSender.SendMessage(AddressBook.GetLobbyServer(), message, (a, msg) => {
                    string work = msg.Argument1.Replace("\x07", "x07");
                    var mapRegMatch = Regex.Match(work, @"m:""(?<map>(\w|_|-|<|>|x07)*)""");
                    if (mapRegMatch.Success) {
                        this.lobby_map = mapRegMatch.Groups["map"].Value.Replace("x07", "\x07");
                    }
                    var matches = Regex.Matches(work, @"\(s:(?<s>\d+);n:""(?<n>(\s|\S)+)"";i:(?<i>\d+);t:(?<t>-?\d+);f:(?<f>\w*);c:""(?<c>(\s|\S)*)""\)");
                    if (matches.Count > 0) {
                        for (int i = 0; i < matches.Count; i++) {
                            if (!ulong.TryParse(matches[i].Groups["s"].Value, out ulong sID)) {
                                break;
                            }
                            if (!int.TryParse(matches[i].Groups["i"].Value, out int pID)) {
                                break;
                            }
                            if (!int.TryParse(matches[i].Groups["t"].Value, out int tID)) {
                                break;
                            } // TODO: Save company name
                            this.lobby_players.Add(new ConnectableLobbyPlayer(pID, sID, matches[i].Groups["n"].Value, matches[i].Groups["f"].Value, matches[i].Groups["c"].Value, tID));
                        }
                    }
                });
            } catch { /* do something here? */ }
        }

    }

}
