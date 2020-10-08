using System;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using System.Windows.Controls;
using Battlegrounds;
using Battlegrounds.Game;
using BattlegroundsApp.Views.ViewComponent;
using Battlegrounds.Game.Battlegrounds;
using BattlegroundsApp.LocalData;
using Battlegrounds.Game.Gameplay;

namespace BattlegroundsApp.Models {
    
    public class LobbyTeamManagementModel {

        private Grid m_teamGrid;
        private int m_maxPlayerCount;
        private Dictionary<Lobby.LobbyTeam.TeamType, List<PlayercardView>> m_teamSetup;

        public LobbyTeamManagementModel(Grid teamGrid) {
            this.m_teamGrid = teamGrid;
            this.m_teamSetup = new Dictionary<Lobby.LobbyTeam.TeamType, List<PlayercardView>>() {
                [Lobby.LobbyTeam.TeamType.Allies] = new List<PlayercardView>(),
                [Lobby.LobbyTeam.TeamType.Axis] = new List<PlayercardView>(),
            };
            for (int i = 0; i < 4; i++) {
                PlayercardView alliesView = new PlayercardView();
                alliesView.SetValue(Grid.ColumnProperty, 0);
                alliesView.SetValue(Grid.RowProperty, i);
                PlayercardView axisView = new PlayercardView();
                axisView.SetValue(Grid.ColumnProperty, 1);
                axisView.SetValue(Grid.RowProperty, i);
                this.m_teamSetup[Lobby.LobbyTeam.TeamType.Allies].Add(alliesView);
                this.m_teamSetup[Lobby.LobbyTeam.TeamType.Axis].Add(axisView);
                this.m_teamGrid.Children.Add(alliesView);
                this.m_teamGrid.Children.Add(axisView);
            }
            this.SetMaxPlayers(2);
        }

        public void SetMaxPlayers(int count) {
            Contract.Requires(count > 0);
            Contract.Requires(count <= 8);
            Contract.Requires(count % 2 == 0);
            this.m_maxPlayerCount = count;
            for (int i = 0; i < 4; i++) {
                this.m_teamSetup[Lobby.LobbyTeam.TeamType.Allies][i].Visibility = i < (count / 2) ? Visibility.Visible : Visibility.Collapsed;
                this.m_teamSetup[Lobby.LobbyTeam.TeamType.Axis][i].Visibility = i < (count / 2) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public void UpdateTeamview(Lobby lobby, bool isHost) {

            foreach (var pair in this.m_teamSetup) {

                var team = lobby.GetTeam(pair.Key);

                for (int i = 0; i < 4; i++) {
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

    }

}
