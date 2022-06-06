using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

using Battlegrounds.Game.Database;
using Battlegrounds.Modding.Content;

namespace BattlegroundsApp.Lobby.Planning;

public class LobbyPlanningContextHandler {

    private (EntityBlueprint e, FactionDefence d)? m_currentPlacement;

    [MemberNotNullWhen(true, nameof(PlaceElementData), nameof(PlaceElementBlueprint))]
    public bool HasPlaceElement {
        get => this.m_currentPlacement is not null;
        set => this.m_currentPlacement = value is false ? null : this.m_currentPlacement;
    }

    public bool RequiresSecond
        => this.m_currentPlacement.HasValue && (this.m_currentPlacement.Value.d.IsLinePlacement || this.m_currentPlacement.Value.d.IsDirectional);

    public FactionDefence? PlaceElementData => this.m_currentPlacement.HasValue ? this.m_currentPlacement.Value.d : null;

    public EntityBlueprint? PlaceElementBlueprint => this.m_currentPlacement.HasValue ? this.m_currentPlacement.Value.e : null;

    public ObservableCollection<LobbyPlanningObject> Elements { get; }

    public LobbyPlanningContextHandler() {
        this.Elements = new();
    }

    public void PickPlaceElement(EntityBlueprint ebp, FactionDefence defence) {
        this.m_currentPlacement = (ebp, defence);
    }

    public bool PlaceElement(Point point, Point? other = null) {

        if (!this.m_currentPlacement.HasValue) {
            return false;
        }

        var obj = this.m_currentPlacement.Value;

        var isPlaced = !(obj.d.IsLinePlacement || obj.d.IsDirectional);
        if (isPlaced) {
            this.Elements.Add(new(0, obj.e, point, other));
            this.m_currentPlacement = null;
        }

        return isPlaced;

    }

}
