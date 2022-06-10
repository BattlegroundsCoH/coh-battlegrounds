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
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Modding;
using Battlegrounds.Networking.LobbySystem;

using BattlegroundsApp.Lobby.MatchHandling;
using BattlegroundsApp.Lobby.Planning;
using BattlegroundsApp.MVVM;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Lobby.MVVM.Models;

public record LobbyPlanningOverviewModelInput(LobbyModel Model, LobbyChatSpectatorModel Chat, ILobbyHandle Handle) {
    public Scenario Scenario => this.Model.Scenario ?? throw new Exception("No scenario was set!");
    public ModPackage Package => this.Model.ModPackage;
}

public class LobbyPlanningOverviewModel : ViewModelBase {

    public record LobbyPlanningAction(string Icon);

    public record LobbyPlanningDefence(ImageSource? Icon, string Name, RelayCommand Click);

    private readonly LobbyPlanningOverviewModelInput m_data;
    private readonly LobbyPlanningContextHandler m_planningContext;

    public override bool SingleInstanceOnly => true;

    public override bool KeepAlive => false;

    public string LobbyTitle => this.m_data.Model.LobbyTitle;

    public ImageSource? ScenarioPreview => LobbySettingsLookup.TryGetMapSource(this.m_data.Scenario);

    public RelayCommand ReturnLobbyCommand { get; }

    public RelayCommand BeginMatchCommand { get; }

    public ObservableCollection<LobbyPlanningDefence> DefenceStructures { get; }

    public ObservableCollection<LobbyPlanningAction> PlanningActions { get; }

    public Pool<LobbyPlanningUnit> PreplacableUnits => this.ContextHandler.PreplacableUnits;

    public CapacityValue UnitCapacity { get; }

    public LobbyPlanningContextHandler ContextHandler => this.m_planningContext;

    public LobbyPlanningOverviewModel(LobbyPlanningOverviewModelInput input) {

        // Set handles
        this.m_data = input;

        // Set planning context handler
        this.m_planningContext = new();

        // Set return command
        this.ReturnLobbyCommand = new(() => App.ViewManager.UpdateDisplay(AppDisplayTarget.Right, this.m_data.Model));
        this.BeginMatchCommand = new(this.BeginMatch);

        // Create Lists
        this.DefenceStructures = new();
        this.PlanningActions = new();

        // Create capacity
        this.UnitCapacity = new(10, () => this.PreplacableUnits.Count);

        // Grab self
        if (this.m_data.Model.GetSelf() is not ILobbySlot self) {
            return;
        }

        // Get faction str
        var factionStr = self.Occupant?.Company?.Army ?? string.Empty;
        if (string.IsNullOrEmpty(factionStr)) {
            return;
        }

        // Get gamemode
        var gamemode = this.m_data.Package.Gamemodes[this.m_data.Model.GamemodeDropdown.Selected];

        // Grab defence structures
        var defData = gamemode.PlanningEntities[factionStr];

        // Grab if defender
        var isDefender = true; // TODO: Implement proper check

        // If defender
        if (isDefender) {

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

            // Get company
            if (this.m_data.Model.TryGetSelectedCompany(out _) is not Company company) {
                return;
            }

            // Add units
            for (int i = 0; i < company.Units.Length; i++) {

                // Grab unit
                var unit = company.Units[i];

                // Create place handler
                var handler = () => this.m_planningContext.PickPlaceElement(unit.SBP, unit.SquadID);

                // Add
                this.PreplacableUnits.Register(new(unit.SquadID, null, unit.GetName(), unit.VeterancyRank, new(handler)));

            }

        } else {

            // TODO:

        }

    }

    private void BeginMatch() {

        if (this.m_data.Model is LobbyHostModel hostModel) {

            // Set lobby status here
            this.m_data.Handle.SetLobbyState(LobbyState.Starting);

            // Get play model
            var play = PlayModelFactory.GetModel(this.m_data.Handle, this.m_data.Chat, hostModel.UploadGamemodeCallback);

            // prepare
            play.Prepare(this.m_data.Package, hostModel.BeginMatch, x => hostModel.EndMatch(x is IPlayModel y ? y : throw new ArgumentNullException()));

        }

    }

}
