using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Battlegrounds;
using Battlegrounds.Game;
using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Game.Gameplay;
using BattlegroundsApp.LocalData;
using BattlegroundsApp.Views.ViewComponent;

namespace BattlegroundsApp.Models {

    public class LobbyTeamManagementModel {

        public const int MAX_TEAM = 4;

        private Grid m_teamGrid;
        private int m_maxPlayerCount;
        private Dictionary<Lobby.LobbyTeam.TeamType, List<PlayercardView>> m_teamSetup;

        public event Action<Lobby.LobbyTeam.TeamType, PlayercardView, int, string> OnTeamEvent;

        public int TotalPlayerCount => this.m_teamSetup[Lobby.LobbyTeam.TeamType.Allies].Count(x => x.IsOccupied) + this.m_teamSetup[Lobby.LobbyTeam.TeamType.Axis].Count(x => x.IsOccupied);

        public LobbyTeamManagementModel(Grid teamGrid) {
            this.m_teamGrid = teamGrid;
            this.m_teamSetup = new Dictionary<Lobby.LobbyTeam.TeamType, List<PlayercardView>>() {
                [Lobby.LobbyTeam.TeamType.Allies] = new List<PlayercardView>(),
                [Lobby.LobbyTeam.TeamType.Axis] = new List<PlayercardView>(),
            };
            for (int i = 0; i < MAX_TEAM; i++) {
                this.CreatePlayercard(i, Lobby.LobbyTeam.TeamType.Allies);
                this.CreatePlayercard(i, Lobby.LobbyTeam.TeamType.Axis);
            }
            this.SetMaxPlayers(2);
        }

        private void CreatePlayercard(int row, Lobby.LobbyTeam.TeamType type) {
            Contract.Requires(row > 0);
            Contract.Requires(row <= MAX_TEAM);
            Contract.Requires(type == Lobby.LobbyTeam.TeamType.Allies || type == Lobby.LobbyTeam.TeamType.Axis);
            PlayercardView view = new PlayercardView();
            view.SetValue(Grid.ColumnProperty, type == Lobby.LobbyTeam.TeamType.Allies ? 0 : 1);
            view.SetValue(Grid.RowProperty, row);
            view.OnPlayercardEvent += this.OnCardActionHandler;
            this.m_teamSetup[type].Add(view);
            this.m_teamGrid.Children.Add(view);
        }

        public void SetMaxPlayers(int count) {
            Contract.Requires(count > 0);
            Contract.Requires(count <= 8);
            Contract.Requires(count % 2 == 0);
            this.m_maxPlayerCount = count;
            for (int i = 0; i < MAX_TEAM; i++) {
                this.m_teamSetup[Lobby.LobbyTeam.TeamType.Allies][i].Visibility = i < (count / 2) ? Visibility.Visible : Visibility.Collapsed;
                this.m_teamSetup[Lobby.LobbyTeam.TeamType.Axis][i].Visibility = i < (count / 2) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public void UpdateTeamview(Lobby lobby, bool isHost) {

            foreach (var pair in this.m_teamSetup) {

                var team = lobby.GetTeam(pair.Key);

                for (int i = 0; i < MAX_TEAM; i++) {
                    if (i < team.Players.Count) {
                        var player = team.Players[i];
                        pair.Value[i].SetPlayerdata(player.SteamID, player.Name, player.Faction, player.SteamID == BattlegroundsInstance.LocalSteamuser.ID, player.SteamID == 0, isHost);
                    } else {
                        pair.Value[i].SetCardState(i < this.m_maxPlayerCount / 2 ? PlayercardViewstate.Open : PlayercardViewstate.Locked);
                    }
                }

            }

            return;

        }

        public List<SessionParticipant> GetParticipants(Lobby.LobbyTeam.TeamType team) {

            List<SessionParticipant> participants = new List<SessionParticipant>();

            byte i = 0;
            foreach (var player in this.m_teamSetup[team]) {
                if (player.IsOccupied) {
                    if (player.IsAI) {
                        participants.Add(new SessionParticipant(
                            AIDifficulty.AI_Hard, 
                            null, 
                            (team == Lobby.LobbyTeam.TeamType.Allies) ? SessionParticipantTeam.TEAM_ALLIES : SessionParticipantTeam.TEAM_AXIS, 
                            i));
                    } else {
                        participants.Add(new SessionParticipant(
                            player.Playername,
                            player.Playerid,
                            (player.Playerid == BattlegroundsInstance.LocalSteamuser.ID) ? PlayerCompanies.FromNameAndFaction(player.Playercompany, Faction.FromName(player.Playerarmy)) : null,
                            (team == Lobby.LobbyTeam.TeamType.Allies) ? SessionParticipantTeam.TEAM_ALLIES : SessionParticipantTeam.TEAM_AXIS,
                            i));
                    }
                    i++;
                }
            }

            return participants;

        }

        private void OnCardActionHandler(PlayercardView sender, string reason) { 
        
            switch (reason) {
                case "AddAI":
                    if (this.m_teamSetup[Lobby.LobbyTeam.TeamType.Allies].Contains(sender)) {
                        sender.SetAIData(AIDifficulty.AI_Hard, "soviet");
                        OnTeamEvent?.Invoke(Lobby.LobbyTeam.TeamType.Allies, sender, this.TotalPlayerCount, reason);
                    } else {
                        sender.SetAIData(AIDifficulty.AI_Hard, "german");
                        OnTeamEvent?.Invoke(Lobby.LobbyTeam.TeamType.Axis, sender, this.TotalPlayerCount, reason);
                    }
                    break;
                case "ChangeArmy":

                    break;
                case "ChangedCompany":

                    break;
                default:
                    break;
            }

        }

    }

}
