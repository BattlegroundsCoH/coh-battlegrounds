using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using System.Windows.Controls;
using Battlegrounds;
using BattlegroundsApp.Views.ViewComponent;

namespace BattlegroundsApp.Models {
    
    public class LobbyTeamManagementModel {

        private Grid m_teamGrid;
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
        }

        public void SetMaxPlayers(int count) {
            Contract.Requires(count > 0);
            Contract.Requires(count <= 8);
            Contract.Requires(count % 2 == 0);
            for (int i = 0; i < count; i++) {
                this.m_teamSetup[Lobby.LobbyTeam.TeamType.Allies][^(i + 1)].Visibility = System.Windows.Visibility.Collapsed;
                this.m_teamSetup[Lobby.LobbyTeam.TeamType.Axis][^(i + 1)].Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public void UpdateTeamview(Lobby lobby) {

            foreach (var pair in this.m_teamSetup) {

                Lobby.LobbyTeam team = lobby.GetTeam(pair.Key);

                int i = 0;
                foreach (Lobby.LobbyPlayer player in team.Players) {
                    pair.Value[i].SetPlayerdata(player.Name, player.Faction, player.SteamID == BattlegroundsInstance.LocalSteamuser.ID);
                    i++;
                }

            }

            return;

        }

    }

}
