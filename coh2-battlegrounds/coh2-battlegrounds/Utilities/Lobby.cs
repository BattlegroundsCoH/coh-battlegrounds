using System;
using System.Collections.Generic;
using System.Diagnostics;

using Battlegrounds.Functional;
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

        public string _lobbyGamemode { get; set; }

        public int _lobbyGamemodeOption { get; set; }

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
        /// <param name="playerSteamID"></param>
        /// <returns></returns>
        public LobbyTeam.TeamType GetPlayerTeam(ulong playerSteamID) {
            foreach (var pair in m_lobbyTeams) {
                if (pair.Value.Players.FindIndex(x => x.SteamID == playerSteamID) >= 0) {
                    return pair.Key;
                }
            }
            return LobbyTeam.TeamType.Undefined;
        }

        /// <summary>
        /// Move the local player to a new team.
        /// </summary>
        /// <param name="smh">The <see cref="ServerMessageHandler"/> connected to the lobby.</param>
        /// <param name="playerID">The ID of the player to move. Can only move self. This is for validation purposes.</param>
        /// <param name="type">The new <see cref="LobbyTeam.TeamType"/> to assign the player.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public void MoveTeam(ServerMessageHandler smh, ulong playerID, LobbyTeam.TeamType type) {
            if (playerID == smh.Lobby.Self.ID) {
                var currentTeam = this.GetPlayerTeam(playerID);
                if (currentTeam != type) {
                    if (this.m_lobbyTeams[currentTeam].Players.Find(x => x.SteamID == playerID) is LobbyPlayer player) {
                        this.m_lobbyTeams[currentTeam].Players.Remove(player);
                        this.m_lobbyTeams[type].Players.Add(player);
                        smh.Lobby.SendMetaMessage("TeamPositionChanged");
                    } else {
                        throw new ArgumentOutOfRangeException($"Unknown player #{playerID}");
                    }
                }
            }
        }

        /// <summary>
        /// Update lobby such that the local data is updated to reflect the server lobby data or the local data is "uploaded" to the server lobby data.
        /// </summary>
        /// <param name="smh">The <see cref="ServerMessageHandler"/> that handles messages to and from the server.</param>
        public void UpdateLobby(ServerMessageHandler smh) {
            this.FetchLobbyInfo(smh); // will only fire if not host
            this.UpdateLobbyInfo(smh); // will only fire if host
        }

        /*
         ** Just making the note now : There's a high chance we're going to have some sync problems.
         */

        private async void FetchLobbyInfo(ServerMessageHandler smh) {
            if (!smh.Lobby.IsHost) {
                Trace.WriteLine("Updating lobby information....");
                smh.Lobby.GetSelectedMap(false, x => this.m_lobbySelectedMapFilename = x);
                smh.Lobby.GetSelectedGamemode(x => this._lobbyGamemode = x);
                smh.Lobby.GetSelectedGamemodeOption(x => int.TryParse(x, out int o).Then(() => this._lobbyGamemodeOption = o));
                int player_count = await smh.Lobby.GetPlayersInLobbyAsync();
                ulong[] steamIndicies = await smh.Lobby.GetPlayerIDsAsync();
                string[] steamNames = await smh.Lobby.GetPlayerNamesAsync();
                this.ClearTeams();
                for (int i = 0; i < player_count; i++) {
                    (int teamIndex, string faction, string company) = await smh.Lobby.GetPlayerdata(i);
                    this.m_lobbyTeams[(LobbyTeam.TeamType)teamIndex].Players.Add(new LobbyPlayer(i, steamIndicies[i], steamNames[i], faction, company));
                }
            }
        }

        private void UpdateLobbyInfo(ServerMessageHandler smh) {
            if (smh.Lobby.IsHost) {
                Trace.WriteLine("Updating lobby information...");
                smh.Lobby.SetLobbyInformation("selected_map", this.m_lobbySelectedMapFilename);
                smh.Lobby.SetLobbyInformation("selected_wc", this._lobbyGamemode);
                smh.Lobby.SetLobbyInformation("selected_wcs", this._lobbyGamemodeOption);
                for (int i = (int)LobbyTeam.TeamType.Undefined; i < (int)LobbyTeam.TeamType.Axis; i++) {
                    LobbyTeam.TeamType t = (LobbyTeam.TeamType)i;
                    for (int j = 0; j < this.m_lobbyTeams[t].Players.Count; j++) {
                        smh.Lobby.SetLobbyInformation($"tid{this.m_lobbyTeams[t].Players[j].LobbyIndex}", (int)t);
                        smh.Lobby.SetLobbyInformation($"fac{this.m_lobbyTeams[t].Players[j].LobbyIndex}", this.m_lobbyTeams[t].Players[j].Faction);
                        smh.Lobby.SetLobbyInformation($"com{this.m_lobbyTeams[t].Players[j].LobbyIndex}", this.m_lobbyTeams[t].Players[j].CompanyName);
                    }
                }
            }
        }

        private void ClearTeams() {
            this.m_lobbyTeams[LobbyTeam.TeamType.Allies].Players.Clear();
            this.m_lobbyTeams[LobbyTeam.TeamType.Axis].Players.Clear();
            this.m_lobbyTeams[LobbyTeam.TeamType.Spectator].Players.Clear();
            this.m_lobbyTeams[LobbyTeam.TeamType.Undefined].Players.Clear();
        }

    }

}
