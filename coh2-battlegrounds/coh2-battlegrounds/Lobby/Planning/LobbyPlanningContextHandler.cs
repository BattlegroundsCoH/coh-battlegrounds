using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

using Battlegrounds.Game.Database;
using Battlegrounds.Modding.Content;

using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Lobby.Planning;

public record LobbyPlanningUnit(ushort CompanyId, ImageSource? Symbol, string Name, int Rank, RelayCommand Click);

public class LobbyPlanningContextHandler {

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

    public LobbyPlanningContextHandler() {
        this.Elements = new();
        this.PreplacableUnits = new();
    }

    public void PickPlaceElement(EntityBlueprint ebp, FactionDefence defence) {
        this.m_currentPlacement = (ebp, null, defence, ushort.MaxValue);
    }

    public void PickPlaceElement(SquadBlueprint sbp, ushort cid) {
        this.m_currentPlacement = (null, sbp, null, cid);
    }

    public void PlaceElement(Point point, Point? other = null) {

        if (this.HasEntityPlacement) {

            var e = this.PlaceElementBlueprint;
            var d = this.PlaceElementData.Value;

            this.Elements.Add(new(0, e, point, other));
            this.m_currentPlacement = null;

        } else if (this.HasSquadPlacement) {

            var s = this.PlaceElementSquadBlueprint;

            this.PreplacableUnits.Pick(x => x.CompanyId == this.PlaceElementSquadId);

            this.Elements.Add(new(0, s, point, other));
            this.m_currentPlacement = null;

        }

    }

}
