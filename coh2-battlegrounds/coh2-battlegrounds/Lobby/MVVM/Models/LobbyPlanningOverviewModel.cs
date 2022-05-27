using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using Battlegrounds.Game.Gameplay;
using Battlegrounds.Networking.LobbySystem;

using BattlegroundsApp.MVVM;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Lobby.MVVM.Models;

public class LobbyPlanningOverviewModel : ViewModelBase {

    private readonly LobbyModel m_lobby;

    public override bool SingleInstanceOnly => true;

    public override bool KeepAlive => false;

    public string LobbyTitle => this.m_lobby.LobbyTitle;

    public ImageSource? ScenarioPreview => LobbySettingsLookup.TryGetMapSource(this.m_lobby.Scenario);

    public RelayCommand ReturnLobbyCommand { get; }

    public LobbyPlanningOverviewModel(LobbyModel lobbyModel) {

        // Set lobby
        this.m_lobby = lobbyModel;

        // Set return command
        this.ReturnLobbyCommand = new(() => App.ViewManager.UpdateDisplay(AppDisplayTarget.Right, this.m_lobby));

        // Grab self
        if (lobbyModel.GetSelf() is not ILobbySlot self) {
            return;
        }

        // Get faction str
        var factionStr = self.Occupant?.Company?.Army ?? string.Empty;
        if (string.IsNullOrEmpty(factionStr)) {
            return;
        }

        // Get mod package
        var package = lobbyModel.ModPackage;

        // Get gamemode
        var gamemode = package.Gamemodes[lobbyModel.GamemodeDropdown.Selected];

        // If has planning
        if (gamemode.Planning) {

            // Grab defence structures
            var defData = gamemode.PlanningEntities[factionStr];

        }

    }

}
