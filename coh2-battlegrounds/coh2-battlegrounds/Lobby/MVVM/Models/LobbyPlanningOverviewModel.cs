using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using Battlegrounds.AI;
using Battlegrounds.Functional;
using Battlegrounds.Game;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Modding;
using Battlegrounds.Modding.Content;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Util;

using BattlegroundsApp.Controls;
using BattlegroundsApp.Lobby.MatchHandling;
using BattlegroundsApp.Lobby.Planning;
using BattlegroundsApp.MVVM;
using BattlegroundsApp.Utilities;

using static BattlegroundsApp.Lobby.MVVM.Models.LobbyAuxModels;

namespace BattlegroundsApp.Lobby.MVVM.Models;

public record LobbyPlanningOverviewModelInput(LobbyModel Model, LobbyChatSpectatorModel Chat, ILobbyHandle Handle) {
    public Scenario Scenario => this.Model.Scenario ?? throw new Exception("No scenario was set!");
    public ModPackage Package => this.Model.ModPackage;
    public Gamemode Gamemode => this.Model.Gamemode;
}

public class LobbyPlanningOverviewModel : ViewModelBase {

    public record LobbyPlanningAction(PlanningObjectiveType ObjectiveType, RelayCommand Click) {

        public ImageSource? Icon => this.ObjectiveType switch {
            PlanningObjectiveType.OT_Attack => LobbyVisualsLookup.ObjectiveTypes[0],
            PlanningObjectiveType.OT_Defend => LobbyVisualsLookup.ObjectiveTypes[1],
            PlanningObjectiveType.OT_Support => LobbyVisualsLookup.ObjectiveTypes[2],
            _ => null
        };
        public string Name => this.ObjectiveType switch {
            PlanningObjectiveType.OT_Attack => "Attack Objective",
            PlanningObjectiveType.OT_Defend => "Defend Objective",
            PlanningObjectiveType.OT_Support => "Support Objective",
            _ => string.Empty
        };
    }

    public record LobbyPlanningDefence(ImageSource? Icon, string Name, RelayCommand Click);

    public record LobbyPlanningParticipantDisplay(ImageSource? ArmyIcon, string Name, string CompanyName, int Row, int Column, ImageSource? SpawnPosIcon);

    public record LobbyPlanningMinimapItem(EntityBlueprint EntityBlueprint, ushort Owner, GamePosition WorldPos, Scenario Scenario);

    public class LobbyPlanningCompanyDisplay : ILocLabelArgumentsObject, INotifyPropertyChanged {
        public event ObjectChangedEventHandler? ObjectChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name { get; }
        public CapacityValue Cap { get; }
        public LobbyPlanningCompanyDisplay(string CompanyName, CapacityValue Cap) {
            this.Name = CompanyName;
            this.Cap = Cap;
            this.Cap.ObjectChanged += (a, b) => this.ObjectChanged?.Invoke(a, b);
        }
        public object[] ToArgs() => this.Cap.ToArgs().Prepend(this.Name);
        public void Update() {
            this.PropertyChanged?.Invoke(this, new(nameof(Cap)));
        }
    }

    private readonly ILobbyPlanningHandle m_planHandle;
    private readonly LobbyPlanningOverviewModelInput m_data;
    private readonly LobbyPlanningContextHandler m_planningContext;
    private readonly bool m_isDefending;

    public override bool SingleInstanceOnly => false;

    public override bool KeepAlive => false;

    public string LobbyTitle => this.m_data.Model.LobbyTitle;

    public ImageSource? ScenarioPreview => LobbySettingsLookup.TryGetMapSource(this.m_data.Scenario);

    public RelayCommand ReturnLobbyCommand { get; }

    public RelayCommand BeginMatchCommand { get; }

    public LobbyMutButton StartMatchButton => this.m_data.Model.StartMatchButton;

    public ObservableCollection<LobbyPlanningDefence> DefenceStructures { get; }

    public ObservableCollection<LobbyPlanningAction> PlanningActions { get; }

    public Pool<LobbyPlanningUnit> PreplacableUnits => this.ContextHandler.PreplacableUnits;

    public LobbyPlanningCompanyDisplay PlanningDisplay { get; } = new("", new(0,0));

    public LobbyPlanningContextHandler ContextHandler => this.m_planningContext;

    public ObservableCollection<LobbyPlanningParticipantDisplay> Attackers { get; }

    public ObservableCollection<LobbyPlanningParticipantDisplay> Defenders { get; }

    public TaggedObservableCollection<LobbyPlanningMinimapItem> MinimapItems { get; }

    public Visibility IsUnitsVisible => this.m_planHandle.IsDefender ? Visibility.Visible : Visibility.Collapsed;

    public Visibility IsDefencesVisible => this.m_planHandle.IsDefender ? Visibility.Visible : Visibility.Collapsed;

    public Visibility IsPlanningVisible => this.m_planHandle.IsAttacker ? Visibility.Visible : Visibility.Collapsed;

    public LobbyPlanningOverviewModel(LobbyPlanningOverviewModelInput input) {

        // Set handles
        this.m_data = input;

        // Error if plan handle does not exist
        if (this.m_data.Handle.PlanningHandle is not ILobbyPlanningHandle pHandle) {
            throw new Exception("Failed to create planning overview model because no planning handle was created by lobby!");
        }

        // Grab handle
        this.m_planHandle = pHandle;

        // Set planning context handler
        this.m_planningContext = new(this.m_planHandle, input.Scenario);

        // Set return command
        this.ReturnLobbyCommand = new(() => App.ViewManager.UpdateDisplay(AppDisplayTarget.Right, this.m_data.Model));
        this.BeginMatchCommand = new(this.BeginMatch);

        // Create Lists
        this.DefenceStructures = new();
        this.PlanningActions = new();
        this.MinimapItems = new();
        this.Attackers = new();
        this.Defenders = new();

        // Init teams
        this.SetupTeams(this.m_data.Handle);

        // Grab self
        if (this.m_data.Model.GetSelf() is not ILobbySlot self) {
            return;
        }

        // Create capacity
        this.PlanningDisplay = new(self.Occupant?.Company?.Name ?? string.Empty, new(10, () => this.PreplacableUnits.Picked));
        this.PreplacableUnits.OnPick += (_, _) => this.PlanningDisplay.Update();
        this.PreplacableUnits.OnUnPick += (_, _) => this.PlanningDisplay.Update();

        // Get faction str
        var factionStr = self.Occupant?.Company?.Army ?? string.Empty;
        if (string.IsNullOrEmpty(factionStr)) {
            return;
        }

        // Get gamemode
        var gamemode = this.m_data.Gamemode;

        // Grab defence structures
        var defData = gamemode.PlanningEntities[factionStr];

        // Grab if defender
        this.m_isDefending = this.m_planHandle.IsDefender;

        // If defender
        if (this.m_isDefending) {

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

                // Grab icon
                var ico = App.ResourceHandler.GetIcon("unit_icons", unit.SBP.UI.Icon);

                // Add
                this.PreplacableUnits.Register(new(unit.SquadID, ico, unit.GetName(), unit.VeterancyRank, new(handler)));

            }

        } else {

            // Create basic objective types
            this.PlanningActions.Add(new(PlanningObjectiveType.OT_Attack, new(() => this.m_planningContext.PickPlaceElement(PlanningObjectiveType.OT_Attack))));
            this.PlanningActions.Add(new(PlanningObjectiveType.OT_Defend, new(() => this.m_planningContext.PickPlaceElement(PlanningObjectiveType.OT_Defend))));

            // If more than one team member, add support objective
            if (pHandle.TeamSize > 1) {
                this.PlanningActions.Add(new(PlanningObjectiveType.OT_Support, new(() => this.m_planningContext.PickPlaceElement(PlanningObjectiveType.OT_Support))));
            }

        }

        // Subscribe to remote events
        this.m_planHandle.PlanElementAdded += this.PlanElementAdded;
        this.m_planHandle.PlanElementRemoved += this.PlanElementRemoved;

        // Finally, display world elements
        App.Current.Dispatcher.Invoke(this.DisplayWorldElements);

    }

    private void SetupTeams(ILobbyHandle handle) {

        // Grab allies
        var allies = handle.Allies;

        // Grab axis
        var axis = handle.Axis;

        // Grab proper setups
        var attackers = handle.AreTeamRolesSwapped() ? axis : allies;
        var defenders = handle.AreTeamRolesSwapped() ? allies : axis;

        // Compute index
        var posIndex = new Dictionary<ILobbyMember, int>();
        for (int k = 0; k < allies.Slots.Length; k++) {
            if (allies.Slots[k].Occupant is { } mem) {
                posIndex[mem] = k;
            }
        }
        for (int k = 0; k < axis.Slots.Length; k++) {
            if (axis.Slots[k].Occupant is { } mem) {
                posIndex[mem] = k + allies.Capacity;
            }
        }

        // Map attackers
        (ValRef<int> i, ValRef<int> j) = (0,0);
        attackers.Slots.Filter(x => x.IsOccupied)
            .MapNotNull(x => x.Occupant).Map(x => OccupantToDisplay(x, i, j, false, posIndex[x])).ForEach(this.Attackers.Add);

        // Map defenders
        (i, j) = (0, 0);
        defenders.Slots.Filter(x => x.IsOccupied)
            .MapNotNull(x => x.Occupant).Map(x => OccupantToDisplay(x, i, j, true, posIndex[x])).ForEach(this.Defenders.Add);

    }

    private static LobbyPlanningParticipantDisplay OccupantToDisplay(ILobbyMember occupant, ValRef<int> i, ValRef<int> j, bool columInverse, int pid) {

        // Compute row and column
        int row = i.GetAndChange(x => x + 1);
        int col = j.GetAndChange(x => i > 1 ? x + 1 : x);
        if (j != col) {
            i.Value = 0;
        }

        // Grab name
        string name = occupant.AILevel switch {
            1 => AIDifficulty.AI_Easy.GetIngameDisplayName(),
            2 => AIDifficulty.AI_Standard.GetIngameDisplayName(),
            3 => AIDifficulty.AI_Hard.GetIngameDisplayName(),
            4 => AIDifficulty.AI_Expert.GetIngameDisplayName(),
            _ => occupant.DisplayName
        };

        // Invert if requested
        int c = columInverse ? (1 - col) : col;

        // Get pos icon
        var p = App.ResourceHandler.GetIcon("minimap_icons", $"Icons_minimap_mm_starting_point_{pid+1}");

        // Make sure we have a valid company to get remaining data from
        if (occupant.Company is ILobbyCompany company) {
            return new LobbyPlanningParticipantDisplay(LobbyVisualsLookup.FactionIcons[company.Army], name, company.Name, row, c, p);
        } else {
            return new LobbyPlanningParticipantDisplay(null, occupant.DisplayName, string.Empty, row, c, p);
        }

    }

    private async void DisplayWorldElements() {
        
        // Grab points
        var points = await Task.Run(() => this.m_data.Scenario.Points.Map(x => (x.Position, x.Owner switch {
            >= 1000 and < 1008 => (ushort)(x.Owner - 1000),
            _ => ushort.MaxValue
        }, BlueprintManager.FromBlueprintName<EntityBlueprint>(x.EntityBlueprint))));

        // Display basic information
        points.Map(x => new LobbyPlanningMinimapItem(x.Item3, x.Item2, x.Position, this.m_data.Scenario)).ForEach(this.MinimapItems.Add);

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

    private void PlanElementRemoved(int elementId) 
        => this.m_planningContext.RemoveElementVisuals(elementId);

    private void PlanElementAdded(ILobbyPlanElement planElement)
        => this.m_planningContext.AddElementVisuals(planElement);

}
