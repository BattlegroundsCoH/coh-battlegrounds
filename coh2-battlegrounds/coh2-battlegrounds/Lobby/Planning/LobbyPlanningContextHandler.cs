using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

using Battlegrounds.Game;
using Battlegrounds.Game.Database;
using Battlegrounds.Modding.Content;
using Battlegrounds.Networking.LobbySystem;

using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Lobby.Planning;

public record LobbyPlanningUnit(ushort CompanyId, ImageSource? Symbol, string Name, int Rank, RelayCommand Click);

public class LobbyPlanningContextHandler {

    private readonly ILobbyPlanningHandle m_handle;
    private readonly Scenario m_scenario;
    private (EntityBlueprint? e, SquadBlueprint? s, FactionDefence? d, ushort i)? m_currentPlacement;

    [MemberNotNullWhen(true, nameof(PlaceElementData))]
    public bool HasPlaceElement {
        get => this.m_currentPlacement is not null;
        set => this.m_currentPlacement = value is false ? null : this.m_currentPlacement;
    }

    [MemberNotNullWhen(true, nameof(PlaceElementData), nameof(PlaceElementBlueprint))]
    public bool HasEntityPlacement {
        get => this.m_currentPlacement.HasValue && this.m_currentPlacement.Value.e is not null;
    }

    [MemberNotNullWhen(true, nameof(PlaceElementSquadBlueprint))]
    public bool HasSquadPlacement {
        get => this.m_currentPlacement.HasValue && this.m_currentPlacement.Value.s is not null;
    }

    public Pool<LobbyPlanningUnit> PreplacableUnits { get; }

    public bool RequiresSecond {
        get {
            if (this.HasEntityPlacement) {
                return this.m_currentPlacement.HasValue && (PlaceElementData.Value.IsLinePlacement || PlaceElementData.Value.IsDirectional);
            } else if (this.HasSquadPlacement) {
                return this.PlaceElementSquadBlueprint.IsTeamWeapon;
            }
            return false;
        }
    }

    public FactionDefence? PlaceElementData => this.m_currentPlacement.HasValue ? this.m_currentPlacement.Value.d : null;

    public EntityBlueprint? PlaceElementBlueprint => this.m_currentPlacement.HasValue ? this.m_currentPlacement.Value.e : null;

    public SquadBlueprint? PlaceElementSquadBlueprint => this.m_currentPlacement.HasValue ? this.m_currentPlacement.Value.s : null;

    public ushort PlaceElementSquadId => this.m_currentPlacement.HasValue ? this.m_currentPlacement.Value.i : ushort.MinValue;

    public ObservableCollection<LobbyPlanningObject> Elements { get; }

    public LobbyPlanningContextHandler(ILobbyPlanningHandle handle, Scenario scenario) {

        // Set handle
        this.m_handle = handle;

        // Set scenario
        this.m_scenario = scenario;

        // Init collections
        this.Elements = new();
        this.PreplacableUnits = new();

    }

    public void PickPlaceElement(EntityBlueprint ebp, FactionDefence defence) {
        this.m_currentPlacement = (ebp, null, defence, ushort.MaxValue);
    }

    public void PickPlaceElement(SquadBlueprint sbp, ushort cid) {
        this.m_currentPlacement = (null, sbp, null, cid);
    }

    public void PickPlaceElement(PlanningObjectiveType objectiveType) {

    }

    public void PlaceElement(Point point, Point? other = null) {

        // Grab self
        var self = this.m_handle.Handle.Self.ID;

        // Translate points
        GamePosition spawn = this.m_scenario.FromMinimapPosition(768, 768, point.X, point.Y);
        GamePosition? lookat = other is null ? null : this.m_scenario.FromMinimapPosition(768, 768, other.Value.X, other.Value.Y);

        // Decide what to do
        if (this.HasEntityPlacement) {

            var e = this.PlaceElementBlueprint;
            var d = this.PlaceElementData.Value;

            // Grab index
            int i = this.m_handle.CreatePlanningStructure(self, e.Name, d.IsDirectional, spawn, lookat);

            this.Elements.Add(new(i, self, e, point, other));
            this.m_currentPlacement = null;

        } else if (this.HasSquadPlacement) {

            var s = this.PlaceElementSquadBlueprint;

            this.PreplacableUnits.Pick(x => x.CompanyId == this.PlaceElementSquadId);

            // Grab index
            int i = this.m_handle.CreatePlanningSquad(self, s.Name, this.PlaceElementSquadId, spawn, lookat);

            this.Elements.Add(new(i, self, s, point, other));
            this.m_currentPlacement = null;

        }

    }

}
