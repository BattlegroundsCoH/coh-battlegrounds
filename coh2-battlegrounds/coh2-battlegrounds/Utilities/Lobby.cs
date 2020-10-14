using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Battlegrounds.Functional;
using Battlegrounds.Game;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Online.Services;
using Battlegrounds.Steam;
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

            /// <summary>
            /// The <see cref="LobbyPlayer"/>'s faction.
            /// </summary>
            /// <remarks>
            /// Do not modify. Call <see cref="SetFaction(ServerMessageHandler, ulong, string)"/>.
            /// </remarks>
            public string Faction { get; set; }

            /// <summary>
            /// The <see cref="LobbyPlayer"/>'s selected company's name.
            /// </summary>
            /// <remarks>
            /// Do not modify. Call <see cref="SetCompanyName(ServerMessageHandler, ulong, string)"/>.
            /// </remarks>
            public string CompanyName { get; set; }

            public int LobbyIndex { get; }

            public AIDifficulty Difficulty { get; set; }

            public LobbyPlayer(int lobID, ulong id, string name, string faction, string company) {
                this.LobbyIndex = lobID;
                this.SteamID = id;
                this.Name = name;
                this.CompanyName = company;
                this.Faction = faction;
                this.Difficulty = AIDifficulty.Human;
            }

            public static bool operator==(LobbyPlayer a, LobbyPlayer b) {
                if (a is LobbyPlayer c && b is LobbyPlayer d) {
                    return c.SteamID == d.SteamID && c.Difficulty == AIDifficulty.Human && d.Difficulty == c.Difficulty;
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

        public string LobbyName { get; set; }
        
        public int LobbyPlayers { get; set; }
        
        public bool LobbyPasswordProtected { get; set; }
        
        public string LobbyGuid { get; set; }

        public string LobbyMap => ScenarioList.FromFilename(this.m_lobbySelectedMapFilename).Name;

        public string LobbyMapFile { get => this.m_lobbySelectedMapFilename; set => this.m_lobbySelectedMapFilename = value; }

        public string LobbyGamemode { get; set; }

        public int LobbyGamemodeOption { get; set; }

        /// <summary>
        /// New basic <see cref="Lobby"/> instance with a basic setup.
        /// </summary>
        /// <param name="baseLobby">The <see cref="ConnectableLobby"/> that was connected with.</param>
        public Lobby(ConnectableLobby baseLobby) {
            this.LobbyName = baseLobby.lobby_name;
            this.LobbyGuid = baseLobby.lobby_guid;
            this.LobbyPasswordProtected = baseLobby.lobby_passwordProtected;
            this.LobbyPlayers = baseLobby.lobby_players.Count;
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

        public Lobby(string guid) {
            this.LobbyName = string.Empty;
            this.LobbyGuid = guid;
            this.m_lobbyTeams = new Dictionary<LobbyTeam.TeamType, LobbyTeam>() {
                [LobbyTeam.TeamType.Undefined] = new LobbyTeam(),
                [LobbyTeam.TeamType.Spectator] = new LobbyTeam(),
                [LobbyTeam.TeamType.Allies] = new LobbyTeam(),
                [LobbyTeam.TeamType.Axis] = new LobbyTeam(),
            };
        }

        public void SetMap(ServerMessageHandler smh, Scenario scenario) { 
            if (smh.Lobby.IsHost) {
                this.m_lobbySelectedMapFilename = scenario.RelativeFilename;
                smh.Lobby.SetLobbyInformation("selected_map", this.m_lobbySelectedMapFilename);
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
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public LobbyTeam GetTeam(LobbyTeam.TeamType type) => this.m_lobbyTeams[type];

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
                        smh.Lobby.UpdateUserInformation("tid", (int)type);
                        smh.Lobby.SendMetaMessage("TeamPositionChanged");
                    } else {
                        throw new ArgumentOutOfRangeException($"Unknown player #{playerID}");
                    }
                }
            }
        }

        public async void AddAI(ServerMessageHandler smh, LobbyTeam.TeamType team, AIDifficulty difficulty, int lobbyID, string army) {
            int lobbyIndex = await smh.Lobby.TryCreateAIPlayer(difficulty, army, (int)team);
            LobbyPlayer player = new LobbyPlayer(lobbyID, (ulong)lobbyIndex, difficulty.GetIngameDisplayName(), army, string.Empty) {
                Difficulty = difficulty
            };
            this.m_lobbyTeams[team].Players.Add(player);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="smh"></param>
        /// <param name="playerID"></param>
        /// <param name="faction"></param>
        public void SetFaction(ServerMessageHandler smh, ulong playerID, string faction) {
            if (this.SetStringServerValue(smh, playerID, "fac", faction)) {
                this.m_lobbyTeams.Aggregate(new List<LobbyPlayer>(), (a, b) => { a.AddRange(b.Value.Players); return a; }).Find(x => x.SteamID == playerID).Faction = faction;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="smh"></param>
        /// <param name="playerID"></param>
        /// <param name="name"></param>
        public void SetCompanyName(ServerMessageHandler smh, ulong playerID, string name) {
            if (this.SetStringServerValue(smh, playerID, "com", name)) {
                this.m_lobbyTeams.Aggregate(new List<LobbyPlayer>(), (a, b) => { a.AddRange(b.Value.Players); return a; }).Find(x => x.SteamID == playerID).CompanyName = name;
            }
        }

        private bool SetStringServerValue(ServerMessageHandler smh, ulong steamID, string key, string val) {
            if (smh.Lobby.Self.ID == steamID) {
                smh.Lobby.UpdateUserInformation(key, val);
                smh.Lobby.SendMetaMessage("StringValueChanged");
                return true;
            } else {
                return false;
            }
        }

        public void CreateHost(SteamUser localSteamuser)
            => this.m_lobbyTeams[LobbyTeam.TeamType.Allies].Players.Add(new LobbyPlayer(0, localSteamuser.ID, localSteamuser.Name, Faction.Soviet, string.Empty));

        /// <summary>
        /// Update lobby such that the local data is updated to reflect the server lobby data or the local data is "uploaded" to the server lobby data.
        /// </summary>
        /// <param name="smh">The <see cref="ServerMessageHandler"/> that handles messages to and from the server.</param>
        public void UpdateLobby(ServerMessageHandler smh, Action<Lobby> onDone) {
            this.FetchLobbyInfo(smh, onDone); // will only fire if not host
            this.UpdateLobbyInfo(smh, onDone); // will only fire if host
        }

        /*
         ** Just making the note now : There's a high chance we're going to have some sync problems.
         */

        private async void FetchLobbyInfo(ServerMessageHandler smh, Action<Lobby> onDone) {
            if (!smh.Lobby.IsHost) {
                Trace.WriteLine("Updating lobby information....");
                smh.Lobby.GetSelectedMap(false, x => this.m_lobbySelectedMapFilename = x);
                smh.Lobby.GetSelectedGamemode(x => this.LobbyGamemode = x);
                smh.Lobby.GetSelectedGamemodeOption(x => int.TryParse(x, out int o).Then(() => this.LobbyGamemodeOption = o));
                int player_count = await smh.Lobby.GetPlayersInLobbyAsync();
                ulong[] steamIndicies = await smh.Lobby.GetPlayerIDsAsync();
                string[] steamNames = await smh.Lobby.GetPlayerNamesAsync();
                this.ClearTeams();
                for (int i = 0; i < player_count; i++) {
                    (int teamIndex, string faction, string company, int diff) = await smh.Lobby.GetPlayerdata(i);
                    if (diff != -1) {
                        this.m_lobbyTeams[(LobbyTeam.TeamType)teamIndex].Players.Add(new LobbyPlayer(i, steamIndicies[i], steamNames[i], faction, company) { Difficulty = (AIDifficulty)diff });
                    } else {
                        this.m_lobbyTeams[(LobbyTeam.TeamType)teamIndex].Players.Add(new LobbyPlayer(i, steamIndicies[i], steamNames[i], faction, company) { Difficulty = AIDifficulty.Human });
                    }
                }
                onDone.Invoke(this);
            }
        }

        private void UpdateLobbyInfo(ServerMessageHandler smh, Action<Lobby> onDone) { // TODO: Update only if there's been a change...
            if (smh.Lobby.IsHost) {
                Trace.WriteLine("Updating lobby information...");
                smh.Lobby.SetLobbyInformation("selected_map", this.m_lobbySelectedMapFilename);
                smh.Lobby.SetLobbyInformation("selected_wc", this.LobbyGamemode);
                smh.Lobby.SetLobbyInformation("selected_wcs", this.LobbyGamemodeOption);
                for (int i = (int)LobbyTeam.TeamType.Undefined; i < (int)LobbyTeam.TeamType.Axis; i++) {
                    LobbyTeam.TeamType t = (LobbyTeam.TeamType)i;
                    for (int j = 0; j < this.m_lobbyTeams[t].Players.Count; j++) {
                        if (this.m_lobbyTeams[t].Players[j].Difficulty != AIDifficulty.Human) {
                            smh.Lobby.SetUserInformation(this.m_lobbyTeams[t].Players[j].LobbyIndex, "ai", (int)this.m_lobbyTeams[t].Players[j].Difficulty);
                        }
                        smh.Lobby.SetUserInformation(this.m_lobbyTeams[t].Players[j].LobbyIndex, "tid", (int)t);
                        smh.Lobby.SetUserInformation(this.m_lobbyTeams[t].Players[j].LobbyIndex, "fac", this.m_lobbyTeams[t].Players[j].Faction);
                        smh.Lobby.SetUserInformation(this.m_lobbyTeams[t].Players[j].LobbyIndex, "com", this.m_lobbyTeams[t].Players[j].CompanyName);
                    }
                }
                onDone.Invoke(this);
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
