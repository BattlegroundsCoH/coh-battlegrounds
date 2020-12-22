using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Battlegrounds.Online.Services {
    
    /// <summary>
    /// Represents a lobby that can be connected to.
    /// </summary>
    public class ConnectableLobby {

        /// <summary>
        /// Represents a player that's connected to a <see cref="ConnectableLobby"/>.
        /// </summary>
        public record ConnectableLobbyPlayer(int pIndex, ulong sIndex, string pName, string pFaction, string cName, int pTIndex);

        private string m_lobbyMap;

        /// <summary>
        /// The server-assigned GUID given to the lobby.
        /// </summary>
        public string lobby_guid { get; }

        /// <summary>
        /// The name of the lobby.
        /// </summary>
        public string lobby_name { get; }
        /// <summary>
        /// Is the lobby password protected.
        /// </summary>
        public bool lobby_passwordProtected { get; }

        /// <summary>
        /// The currently selected map (Not formatted)
        /// </summary>
        public string lobby_map => this.m_lobbyMap;

        /// <summary>
        /// The lobby players currently in the lobby (Player name, team index, ...).
        /// </summary>
        public List<ConnectableLobbyPlayer> lobby_players;

        /// <summary>
        /// Gets number of players in player list
        /// </summary>
        public int lobby_player_count => this.lobby_players.Count;

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
            this.m_lobbyMap = "<none>";
        }

        public void Update() {
            try {
                this.lobby_players.Clear();
                Message message = new Message(MessageType.LOBBY_INFO, this.lobby_guid);
                MessageSender.SendMessage(AddressBook.GetLobbyServer(), message, (a, msg) => {
                    Trace.WriteLine(msg, "ConnectableLobby");
                    var mapRegMatch = Regex.Match(msg.Argument1, @"\(m:(?<map>(\w|_|-|<|>|x07)*)\)");
                    if (mapRegMatch.Success) {
                        this.m_lobbyMap = mapRegMatch.Groups["map"].Value;
                        Trace.WriteLine($"{this.lobby_guid}-map: {this.m_lobbyMap}", "ConnectableLobby");
                    } else {
                        Trace.WriteLine($"{this.lobby_guid}-map: Failed to read map", "ConnectableLobby");
                    }
                    var matches = Regex.Matches(msg.Argument1, @"\(s:(?<s>\d+);n:""(?<n>(\s|\S)+)"";i:(?<i>\d+);t:(?<t>-?\d+);f:(?<f>\w*);c:""(?<c>(\s|\S)*)""\)");
                    if (matches.Count > 0) {
                        for (int i = 0; i < matches.Count; i++) {
                            if (!ulong.TryParse(matches[i].Groups["s"].Value, out ulong sID)) {
                                Trace.WriteLine($"{this.lobby_guid}: Failed to read steam ID ({matches[i].Groups["s"].Value})", "ConnectableLobby");
                                break;
                            }
                            if (!int.TryParse(matches[i].Groups["i"].Value, out int pID)) {
                                Trace.WriteLine($"{this.lobby_guid}: Failed to read lobby ID ({matches[i].Groups["i"].Value})", "ConnectableLobby");
                                break;
                            }
                            if (!int.TryParse(matches[i].Groups["t"].Value, out int tID)) {
                                Trace.WriteLine($"{this.lobby_guid}: Failed to read team ID ({matches[i].Groups["t"].Value})", "ConnectableLobby");
                                break;
                            }
                            this.lobby_players.Add(new ConnectableLobbyPlayer(pID, sID, matches[i].Groups["n"].Value, matches[i].Groups["f"].Value, matches[i].Groups["c"].Value, tID));
                        }
                    } else {
                        Trace.WriteLine($"{this.lobby_guid}: Failed to read players", "ConnectableLobby");
                    }
                });
            } catch { /* do something here? */ }
        }

    }

}
