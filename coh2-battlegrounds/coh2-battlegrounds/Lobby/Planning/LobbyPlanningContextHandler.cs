using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

using Battlegrounds.Game;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Modding.Content;
using Battlegrounds.Networking.LobbySystem;

using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Lobby.Planning;

public record LobbyPlanningUnit(ushort CompanyId, ImageSource? Symbol, string Name, int Rank, RelayCommand Click);

public class LobbyPlanningContextHandler {

    private abstract record PlacementCase();
    private record EntityPlacement(EntityBlueprint Ebp, FactionDefence Def) : PlacementCase();
    private record SquadPlacement(SquadBlueprint Sbp, ushort Cid) : PlacementCase();
    private record ObjectivePlacement(PlanningObjectiveType ObjectiveType) : PlacementCase();

    private readonly List<int> m_objectiveElements;
    private readonly ILobbyPlanningHandle m_handle;
    private readonly Scenario m_scenario;
    private PlacementCase? m_currentPlacement;

    public bool HasPlaceElement {
        get => this.m_currentPlacement is not null;
        set => this.m_currentPlacement = value is false ? null : this.m_currentPlacement;
    }

    [MemberNotNullWhen(true, nameof(PlaceElementBlueprint))]
    public bool HasEntityPlacement => this.m_currentPlacement is EntityPlacement;

    [MemberNotNullWhen(true, nameof(PlaceElementSquadBlueprint))]
    public bool HasSquadPlacement => this.m_currentPlacement is SquadPlacement;

    [MemberNotNullWhen(true, nameof(PlaceElementSquadBlueprint))]
    public bool HasObjectivePlacement => this.m_currentPlacement is ObjectivePlacement;

    public Pool<LobbyPlanningUnit> PreplacableUnits { get; }

    public bool RequiresSecond {
        get {
            if (this.m_currentPlacement is EntityPlacement e) {
                return e.Def.IsLinePlacement || e.Def.IsDirectional;
            } else if (this.m_currentPlacement is SquadPlacement s) {
                return s.Sbp.IsTeamWeapon;
            }
            return false;
        }
    }

    public bool IsLinePlacement => this.m_currentPlacement is EntityPlacement e && e.Def.IsLinePlacement;

    public EntityBlueprint? PlaceElementBlueprint => this.m_currentPlacement is EntityPlacement e ?  e.Ebp : null;

    public SquadBlueprint? PlaceElementSquadBlueprint => this.m_currentPlacement is SquadPlacement s ? s.Sbp : null;

    public PlanningObjectiveType PlaceElemtObjectiveType => this.m_currentPlacement is ObjectivePlacement o ? o.ObjectiveType : PlanningObjectiveType.None;

    public ushort PlaceElementSquadId => this.m_currentPlacement is SquadPlacement s ? s.Cid : ushort.MinValue;

    public ObservableCollection<LobbyPlanningObject> Elements { get; }

    public ulong SelfId => this.m_handle.Handle.Self.ID;

    public LobbyPlanningContextHandler(ILobbyPlanningHandle handle, Scenario scenario) {

        // Set handle
        this.m_handle = handle;

        // Set scenario
        this.m_scenario = scenario;

        // Init collections
        this.Elements = new();
        this.PreplacableUnits = new();

        // Init fields
        this.m_objectiveElements = new();

    }

    public void PickPlaceElement(EntityBlueprint ebp, FactionDefence defence) {
        this.m_currentPlacement = new EntityPlacement(ebp, defence);
    }

    public void PickPlaceElement(SquadBlueprint sbp, ushort cid) {
        this.m_currentPlacement = new SquadPlacement(sbp, cid);
    }

    public void PickPlaceElement(PlanningObjectiveType objectiveType) {
        this.m_currentPlacement = new ObjectivePlacement(objectiveType);
    }

    public int PlaceElement(Point point, Point? other = null) {

        // Grab self
        var self = this.m_handle.Handle.Self.ID;

        // Translate points
        GamePosition spawn = this.m_scenario.FromMinimapPosition(768, 768, point.X, point.Y);
        GamePosition? lookat = other is null ? null : this.m_scenario.FromMinimapPosition(768, 768, other.Value.X, other.Value.Y);

        // Define placed index
        int i = -1;

        // Decide what to do
        if (this.m_currentPlacement is EntityPlacement ep) {

            // Grab index
            i = this.m_handle.CreatePlanningStructure(self, ep.Ebp.Name, ep.Def.IsDirectional, spawn, lookat);

            this.Elements.Add(new(i, self, ep.Ebp, point, other, ep.Def.IsLinePlacement));
            this.m_currentPlacement = null;

        } else if (this.m_currentPlacement is SquadPlacement sp) {

            this.PreplacableUnits.Pick(x => x.CompanyId == this.PlaceElementSquadId);

            // Grab index
            i = this.m_handle.CreatePlanningSquad(self, sp.Sbp.Name, this.PlaceElementSquadId, spawn, lookat);

            this.Elements.Add(new(i, self, sp.Sbp, point, other));
            this.m_currentPlacement = null;

        } else if (this.m_currentPlacement is ObjectivePlacement op) {

            // Grab index
            i = this.m_handle.CreatePlanningObjective(self, op.ObjectiveType, this.m_objectiveElements.Count, spawn);

            // Add to objective list
            this.m_objectiveElements.Add(i);

            // Reset placement data
            this.m_currentPlacement = null;

            // Add element
            this.Elements.Add(new(i, self, point, op.ObjectiveType, this.m_objectiveElements.Count));

        }

        return i;

    }

    public void RemoveElement(int elemId) {
        this.RemoveElementVisuals(elemId);
        this.m_handle.RemovePlanElement(elemId);
        if (this.m_objectiveElements.IndexOf(elemId) is int i && i >= 0) {
            this.m_objectiveElements.RemoveAt(i);
            for (int j = i; j < this.m_objectiveElements.Count; j++) {
                this.m_handle.GetPlanElement(this.m_objectiveElements[j])?.SetObjectiveOrder(j);
            }
        }
    }

    public void RemoveElementVisuals(int elemId) {
        for (int i = 0; i < this.Elements.Count; i++) {
            if (this.Elements[i].ObjectId == elemId) {
                this.Elements.RemoveAt(i);
                break;
            }
        }
    }

    public void AddElementVisuals(ILobbyPlanElement planElement) {
        if (planElement.ObjectiveType is not PlanningObjectiveType.None) {

            // objective
            var spawn = this.m_scenario.ToMinimapPosition(768, 768, planElement.SpawnPosition);

            // Add element
            this.Elements.Add(new(planElement.ElementId, planElement.ElementOwnerId, spawn.ToPoint(), planElement.ObjectiveType, planElement.ObjectiveOrder));

        } else if (planElement.IsEntity) {
            
            // entity
            var ebp = BlueprintManager.FromBlueprintName<EntityBlueprint>(planElement.Blueprint);
            var spawn = this.m_scenario.ToMinimapPosition(768, 768, planElement.SpawnPosition);
            Point? lookat = planElement.LookatPosition is GamePosition p ? this.m_scenario.ToMinimapPosition(768, 768, p).ToPoint() : null;
        
            // Add element
            this.Elements.Add(new(planElement.ElementId, planElement.ElementOwnerId, ebp, spawn.ToPoint(), lookat));
        
        } else {
            
            // squad
            var sbp = BlueprintManager.FromBlueprintName<SquadBlueprint>(planElement.Blueprint);
            var spawn = this.m_scenario.ToMinimapPosition(768, 768, planElement.SpawnPosition);
            Point? lookat = planElement.LookatPosition is GamePosition p ? this.m_scenario.ToMinimapPosition(768, 768, p).ToPoint() : null;
            
            // Add element
            this.Elements.Add(new(planElement.ElementId, planElement.ElementOwnerId, sbp, spawn.ToPoint(), lookat));

        }
    }

}
