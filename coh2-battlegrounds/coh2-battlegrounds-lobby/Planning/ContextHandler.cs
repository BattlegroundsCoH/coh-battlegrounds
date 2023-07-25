using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows;
using Battlegrounds.Game;
using Battlegrounds.Misc.Collections;
using Battlegrounds.Misc.Values;
using Battlegrounds.Modding.Content;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.UI;
using Battlegrounds.UI.Converters;
using Battlegrounds.Game.Blueprints;
using Battlegrounds.Modding;
using Battlegrounds.Game.Scenarios;

namespace Battlegrounds.Lobby.Planning;

public record PlanningUnit(ushort CompanyId, ImageSource? Symbol, string Name, int Rank, RelayCommand Click);

public sealed class ContextHandler {

    private abstract record PlacementCase();
    private record EntityPlacement(EntityBlueprint Ebp, FactionDefence Def) : PlacementCase();
    private record SquadPlacement(SquadBlueprint Sbp, ushort Cid) : PlacementCase();
    private record ObjectivePlacement(PlanningObjectiveType ObjectiveType) : PlacementCase();

    private readonly List<int> m_objectiveElements;
    private readonly ILobbyPlanningHandle m_handle;
    private readonly IScenario m_scenario;
    private readonly Dictionary<string, CapacityValue> m_entityCapacities;
    private PlacementCase? m_currentPlacement;

    public Size MinimapRenderSize { get; set; }

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

    public Pool<PlanningUnit> PreplacableUnits { get; }

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

    public int PlacementWidth => this.m_currentPlacement is EntityPlacement e ? e.Def.Width : 1;

    public EntityBlueprint? PlaceElementBlueprint => this.m_currentPlacement is EntityPlacement e ? e.Ebp : null;

    public SquadBlueprint? PlaceElementSquadBlueprint => this.m_currentPlacement is SquadPlacement s ? s.Sbp : null;

    public PlanningObjectiveType PlaceElemtObjectiveType => this.m_currentPlacement is ObjectivePlacement o ? o.ObjectiveType : PlanningObjectiveType.None;

    public ushort PlaceElementSquadId => this.m_currentPlacement is SquadPlacement s ? s.Cid : ushort.MinValue;

    public ObservableCollection<PlanningObject> Elements { get; }

    public ulong SelfId => this.m_handle.Handle.Self.ID;

    public IModPackage Package { get; }

    public ContextHandler(ILobbyPlanningHandle handle, IScenario scenario, IModPackage package) {

        // Set handle
        this.m_handle = handle;

        // Set scenario
        this.m_scenario = scenario;

        // Init collections
        this.Elements = new();
        this.PreplacableUnits = new();

        // Init fields
        this.m_objectiveElements = new();

        // Init base capacity handlers
        this.m_entityCapacities = new();

        // Init base case
        this.MinimapRenderSize = new Size(512, 512);

        // Set package
        this.Package = package;

    }

    public CapacityValue GetSelfCapacity(string ebp)
        => this.m_entityCapacities.GetValueOrDefault(ebp, new(0, 0));

    public void SetSelfCapacity(string ebp, CapacityValue cap)
        => this.m_entityCapacities[ebp] = cap;

    public int GetSelfPlaceCount(EntityBlueprint ebp) {
        int count = 0;
        for (int i = 0; i < this.Elements.Count; i++) {
            if (this.Elements[i].Owner == this.SelfId && this.Elements[i].Blueprint == ebp)
                count += this.Elements[i].Weight;
        }
        return count;
    }

    public void PickPlaceElement(EntityBlueprint ebp, FactionDefence defence) {

        // Bail if already at capacity
        var cap = this.GetSelfCapacity(ebp.Name);
        if (!cap.Test(1)) {
            return;
        }

        // Set placement
        this.m_currentPlacement = new EntityPlacement(ebp, defence);

    }

    public void PickPlaceElement(SquadBlueprint sbp, ushort cid) {

        // Bail if units are unavailable
        if (this.PreplacableUnits.Picked >= 10) {
            return;
        }

        // Set placement
        this.m_currentPlacement = new SquadPlacement(sbp, cid);

    }

    public void PickPlaceElement(PlanningObjectiveType objectiveType) {
        this.m_currentPlacement = new ObjectivePlacement(objectiveType);
    }

    public int GetIngameCount(Point start, Point end, int cellWidth) {

        // Compute ingame positions
        GamePosition spawn = this.m_scenario.FromMinimapPosition(this.MinimapRenderSize.Width, this.MinimapRenderSize.Height, start.X, start.Y);
        GamePosition lookat = this.m_scenario.FromMinimapPosition(this.MinimapRenderSize.Width, this.MinimapRenderSize.Height, end.X, end.Y);

        // Compute distance
        return GetIngameCount(spawn, lookat, cellWidth);

    }

    public static int GetIngameCount(GamePosition spawn, GamePosition lookat, int cellWidth) {

        // Compute distance
        double distance = spawn.DistanceTo(lookat);

        // Return
        return (int)Math.Floor(distance / cellWidth);

    }

    public int PlaceElement(Size mmSize, Point point, Point? other = null) {

        // Grab self
        var self = this.m_handle.Handle.Self.ID;

        // Translate points
        GamePosition spawn = this.m_scenario.FromMinimapPosition(mmSize.Width, mmSize.Height, point.X, point.Y);
        GamePosition? lookat = other is null ? null : this.m_scenario.FromMinimapPosition(mmSize.Width, mmSize.Height, other.Value.X, other.Value.Y);

        // Define placed index
        int i = -1;

        // Decide what to do
        if (this.m_currentPlacement is EntityPlacement ep) {

            // Grab index
            i = this.m_handle.CreatePlanningStructure(self, ep.Ebp.Name, ep.Def.IsDirectional, spawn, lookat);

            // Grab ingame count
            int count = ep.Def.IsLinePlacement ? GetIngameCount(spawn, lookat!.Value, ep.Def.Width) : 1;

            // Add planning structure
            this.Elements.Add(new(i, self, ep.Ebp, point, other, ep.Def.IsLinePlacement, weight: count));
            this.m_currentPlacement = null;

        } else if (this.m_currentPlacement is SquadPlacement sp) {

            // Pick
            var poolItem = this.PreplacableUnits.Find(x => x.CompanyId == this.PlaceElementSquadId);
            if (poolItem is null)
                return -1;

            // Pick
            this.PreplacableUnits.Pick(poolItem);

            // Grab index
            i = this.m_handle.CreatePlanningSquad(self, sp.Sbp.Name, this.PlaceElementSquadId, spawn, lookat);

            // Add squad placement
            this.Elements.Add(new(i, self, sp.Sbp, point, other, companyId: sp.Cid) { ClientTag = poolItem });
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

        // Remove visual
        this.RemoveElementVisuals(elemId);

        // Remove from handler
        this.m_handle.RemovePlanElement(elemId);

        // Log
        Trace.WriteLine($"Removing plan element {elemId}", nameof(ContextHandler));

        // Correct object order
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

    public void AddElementVisuals(Size mmSize, ILobbyPlanElement planElement) {

        // extract 'spawn' position
        var spawn = this.m_scenario.ToMinimapPosition(mmSize.Width, mmSize.Height, planElement.SpawnPosition).ToPoint();
        Point? lookat = planElement.LookatPosition is GamePosition p ? this.m_scenario.ToMinimapPosition(mmSize.Width, mmSize.Height, p).ToPoint() : null;

        // Determine placement type and add
        if (planElement.ObjectiveType is not PlanningObjectiveType.None) {

            // Add element
            this.Elements.Add(new(planElement.ElementId, planElement.ElementOwnerId, spawn, planElement.ObjectiveType, planElement.ObjectiveOrder));

        } else if (planElement.IsEntity) {

            // entity
            var ebp = this.Package.GetDataSource().GetBlueprints(GameCase.CompanyOfHeroes2).FromBlueprintName<EntityBlueprint>(planElement.Blueprint);

            // Add element
            this.Elements.Add(new(planElement.ElementId, planElement.ElementOwnerId, ebp, spawn, lookat, !planElement.IsDirectional));

        } else {

            // squad
            var sbp = this.Package.GetDataSource().GetBlueprints(GameCase.CompanyOfHeroes2).FromBlueprintName<SquadBlueprint>(planElement.Blueprint);

            // Add element
            this.Elements.Add(new(planElement.ElementId, planElement.ElementOwnerId, sbp, spawn, lookat, companyId: planElement.CompanyId));

        }

    }

}
