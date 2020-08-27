using System.Collections.Generic;
using System.Diagnostics;

using Battlegrounds.Game.Database;
using Battlegrounds.Online.Services;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp {
    
    public class Lobby {

        public class LobbyTeam {

            public enum TeamType {
                Undefined = -1,
                Spectator = 0,
                Allies = 1,
                Axis = 2
            }

            public List<LobbyPlayer> Players { get; }

            public LobbyTeam() {
                this.Players = new List<LobbyPlayer>();
            }

        }

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
        public class LobbyPlayer {

            public ulong SteamID { get; }

            public string Name { get; }

            public string Faction { get; }

            public string CompanyName { get; }

            public int LobbyIndex { get; }

            public LobbyPlayer(int lobID, ulong id, string name, string faction, string company) {
                this.LobbyIndex = lobID;
                this.SteamID = id;
                this.Name = name;
                this.CompanyName = company;
                this.Faction = faction;
            }

            public static bool operator==(LobbyPlayer a, LobbyPlayer b) {
                if (a is LobbyPlayer c && b is LobbyPlayer d) {
                    return c.SteamID == d.SteamID;
                } else {
                    return false;
                }
            }
            public static bool operator !=(LobbyPlayer a, LobbyPlayer b) => !(a == b);

        }
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()

        private string m_lobbySelectedMapFilename;
        private Dictionary<LobbyTeam.TeamType, LobbyTeam> m_lobbyTeams;

        public string _lobbyName { get; set; }
        
        public string _lobbyPlayers { get; set; }
        
        public bool _lobbyPasswordProtected { get; set; }
        
        public string _lobbyGuid { get; set; }

        public string _lobbyMap => ScenarioList.FromFilename(this.m_lobbySelectedMapFilename).Name;

        public string _lobbyMapFile { get => this.m_lobbySelectedMapFilename; set => this.m_lobbySelectedMapFilename = value; }

        /// <summary>
        /// New basic <see cref="Lobby"/> instance with a basic setup.
        /// </summary>
        /// <param name="baseLobby">The <see cref="ConnectableLobby"/> that was connected with.</param>
        public Lobby(ConnectableLobby baseLobby) {
            this._lobbyName = baseLobby.lobby_name;
            this._lobbyGuid = baseLobby.lobby_guid;
            this._lobbyPasswordProtected = baseLobby.lobby_passwordProtected;
            this._lobbyPlayers = baseLobby.lobby_players.Count.ToString();
            this.m_lobbySelectedMapFilename = baseLobby.lobby_map;
            this.m_lobbyTeams = new Dictionary<LobbyTeam.TeamType, LobbyTeam>() {
                [LobbyTeam.TeamType.Undefined] = new LobbyTeam(),
                [LobbyTeam.TeamType.Spectator] = new LobbyTeam(),
                [LobbyTeam.TeamType.Allies] = new LobbyTeam(),
                [LobbyTeam.TeamType.Axis] = new LobbyTeam(),
            };
            for (int i = 0; i < baseLobby.lobby_players.Count; i++) {
                LobbyPlayer player = new LobbyPlayer(
                    baseLobby.lobby_players[i].player_index,
                    baseLobby.lobby_players[i].player_steam_index,
                    baseLobby.lobby_players[i].player_name,
                    baseLobby.lobby_players[i].player_faction,
                    baseLobby.lobby_players[i].player_company
                    );
                this.m_lobbyTeams[(LobbyTeam.TeamType)baseLobby.lobby_players[i].player_index].Players.Add(player);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="smh"></param>
        /// <param name="name"></param>
        public void PlayerConnected(ServerMessageHandler smh, string name) {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="smh"></param>
        /// <param name="playerID"></param>
        /// <param name="type"></param>
        public void MoveTeam(ServerMessageHandler smh, ulong playerID, LobbyTeam.TeamType type) {

        }

        /// <summary>
        /// Update lobby such that the local data is updated to reflect the server lobby data or the local data is "uploaded" to the server lobby data.
        /// </summary>
        /// <param name="smh">The <see cref="ServerMessageHandler"/> that handles messages to and from the server.</param>
        public void UpdateLobby(ServerMessageHandler smh) {
            this.FetchLobbyInfo(smh); // will inly fire if not host
            this.UpdateLobbyInfo(smh); // will only fire if host
        }

        private void FetchLobbyInfo(ServerMessageHandler smh) {
            if (!smh.Lobby.IsHost) {
                Trace.WriteLine("Updating lobby information....");
                smh.Lobby.GetSelectedMap(false, x => this.m_lobbySelectedMapFilename = x);
            }
        }

        private void UpdateLobbyInfo(ServerMessageHandler smh) {
            if (smh.Lobby.IsHost) {
                Trace.WriteLine("Updating lobby information...");
            }
        }

    }

}
