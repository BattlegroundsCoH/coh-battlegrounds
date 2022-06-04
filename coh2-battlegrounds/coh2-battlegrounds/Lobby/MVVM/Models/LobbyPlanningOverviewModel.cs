using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding.Content;
using Battlegrounds.Networking.LobbySystem;

using BattlegroundsApp.Lobby.Planning;
using BattlegroundsApp.MVVM;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Lobby.MVVM.Models;

public class LobbyPlanningOverviewModel : ViewModelBase {

    public record LobbyPlanningAction(string Icon);

    public record LobbyPlanningDefence(ImageSource? Icon, string Name, RelayCommand Click);

    private readonly LobbyModel m_lobby;
    private readonly LobbyPlanningContextHandler m_planningContext;

    public override bool SingleInstanceOnly => true;

    public override bool KeepAlive => false;

    public string LobbyTitle => this.m_lobby.LobbyTitle;

    public ImageSource? ScenarioPreview => LobbySettingsLookup.TryGetMapSource(this.m_lobby.Scenario);

    public RelayCommand ReturnLobbyCommand { get; }

    public ObservableCollection<LobbyPlanningDefence> DefenceStructures { get; }

    public ObservableCollection<LobbyPlanningAction> PlanningActions { get; }

    public LobbyPlanningContextHandler ContextHandler => this.m_planningContext;

    public LobbyPlanningOverviewModel(LobbyModel lobbyModel) {

        // Set lobby
        this.m_lobby = lobbyModel;

        // Set planning context handler
        this.m_planningContext = new();

        // Set return command
        this.ReturnLobbyCommand = new(() => App.ViewManager.UpdateDisplay(AppDisplayTarget.Right, this.m_lobby));

        // Create Lists
        this.DefenceStructures = new();
        this.PlanningActions = new();

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

            // Add entities
            for (int i = 0; i < defData.Length; i++) {

                // Grab copy of defence element
                var data = defData[i];

                // Grab EBP
                var ebp = BlueprintManager.FromBlueprintName<EntityBlueprint>(data.EntityBlueprint);

                // Create display data from mod data
                var handler = () => this.m_planningContext.PickPlaceElement(ebp, data);
                var planData = new LobbyPlanningDefence(App.ResourceHandler.GetIcon("entity_icons", ebp.UI.Icon), ebp.UI.ScreenName, new(handler));

                // Add
                this.DefenceStructures.Add(planData);

            }

        }

    }

}
