using System;
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
        private List<ConnectableLobbyPlayer> m_lobby_players;
        private int m_lobbyPlayerCap;

        /// <summary>
        /// The server-assigned GUID given to the lobby.
        /// </summary>
        public string LobbyGUID { get; }

        /// <summary>
        /// The name of the lobby.
        /// </summary>
        public string LobbyName { get; }
        /// <summary>
        /// Is the lobby password protected.
        /// </summary>
        public bool IsPasswordProtected { get; }

        /// <summary>
        /// The currently selected map (Not formatted)
        /// </summary>
        public string LobbyMap => this.m_lobbyMap;

        /// <summary>
        /// 
        /// </summary>
        public string LobbyPlayers => $"{this.m_lobby_players.Count}/{this.m_lobbyPlayerCap}";

        /// <summary>
        /// New standard <see cref="ConnectableLobby"/> instance without player data.
        /// </summary>
        /// <param name="gid">The lobby GUID that's used by the server to identify the lobby.</param>
        /// <param name="name">The assigned name of the lobby.</param>
        /// <param name="isPsswProtected">Boolean flag makring whether the lobby is password protected or not.</param>
        public ConnectableLobby(string gid, string name, bool isPsswProtected) {
            this.LobbyGUID = gid;
            this.LobbyName = name;
            this.m_lobbyPlayerCap = 0;
            this.IsPasswordProtected = isPsswProtected;
            this.m_lobby_players = new List<ConnectableLobbyPlayer>();
            this.m_lobbyMap = "<none>";
        }

        public void Update(Action<ConnectableLobby> updateFinished) {
            try {
                this.m_lobby_players.Clear();
                Message message = new Message(MessageType.LOBBY_INFO, this.LobbyGUID);
                MessageSender.SendMessage(AddressBook.GetLobbyServer(), message, (a, msg) => {
                    var mapRegMatch = Regex.Match(msg.Argument1, @"\(m:(?<map>(\w|\d|_|-|<|>)*)\)");
                    if (mapRegMatch.Success) {
                        this.m_lobbyMap = mapRegMatch.Groups["map"].Value;
                    } else {
                        Trace.WriteLine($"{this.LobbyGUID}-map: Failed to read map", "ConnectableLobby");
                    }
                    var capRegMatch = Regex.Match(msg.Argument1, @"\(cap:(?<cap>(\d*))\)");
                    if (capRegMatch.Success) {
                        if (int.TryParse(capRegMatch.Groups["cap"].Value, out int capValue)) {
                            this.m_lobbyPlayerCap = capValue;
                        }
                    } else {
                        Trace.WriteLine($"{this.LobbyGUID}-map: Failed to read capacity", "ConnectableLobby");
                    }
                    var matches = Regex.Matches(msg.Argument1, @"\(s:(?<s>\d+);n:""(?<n>(\s|\S)+)"";i:(?<i>\d+);t:(?<t>-?\d+);f:(?<f>\w*);c:""(?<c>(\s|\S)*)""\)");
                    if (matches.Count > 0) {
                        for (int i = 0; i < matches.Count; i++) {
                            if (!ulong.TryParse(matches[i].Groups["s"].Value, out ulong sID)) {
                                Trace.WriteLine($"{this.LobbyGUID}: Failed to read steam ID ({matches[i].Groups["s"].Value})", "ConnectableLobby");
                                break;
                            }
                            if (!int.TryParse(matches[i].Groups["i"].Value, out int pID)) {
                                Trace.WriteLine($"{this.LobbyGUID}: Failed to read lobby ID ({matches[i].Groups["i"].Value})", "ConnectableLobby");
                                break;
                            }
                            if (!int.TryParse(matches[i].Groups["t"].Value, out int tID)) {
                                Trace.WriteLine($"{this.LobbyGUID}: Failed to read team ID ({matches[i].Groups["t"].Value})", "ConnectableLobby");
                                break;
                            }
                            this.m_lobby_players.Add(new ConnectableLobbyPlayer(pID, sID, matches[i].Groups["n"].Value, matches[i].Groups["f"].Value, matches[i].Groups["c"].Value, tID));
                        }
                    } else {
                        Trace.WriteLine($"{this.LobbyGUID}: Failed to read players", "ConnectableLobby");
                    }
                    updateFinished?.Invoke(this);
                });
            } catch { /* do something here? */ }
        }

    }

}
