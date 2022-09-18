using System.Text.Json.Serialization;

namespace Battlegrounds.Modding.Content;

/// <summary>
/// AI placement type to help the AI decide what to place
/// </summary>
public enum AIPlacementType {

    /// <summary>
    /// Placement type is undefined.
    /// </summary>
    Undefined,

    /// <summary>
    /// Bunker placement (Area defence, lockdown zones).
    /// </summary>
    Bunker,

    /// <summary>
    /// Barricade (Blocks/slows movement).
    /// </summary>
    Barricade,

    /// <summary>
    /// Mine (Slows or blocks path movement for attackers).
    /// </summary>
    Mine

}

/// <summary>
/// Readonly struct representing a defence entity for a faction.
/// </summary>
public readonly struct FactionDefence {

    /// <summary>
    /// Get the name of the entity blueprint to place ingame
    /// </summary>
    public string EntityBlueprint { get; }

    /// <summary>
    /// Get if the entity should be given a direction when placed.
    /// </summary>
    public bool IsDirectional { get; }

    /// <summary>
    /// Get if the planner should allow line placement of this defence type.
    /// </summary>
    /// <remarks>
    /// Should be true for barbed wire and trenches.
    /// </remarks>
    public bool IsLinePlacement { get; }

    /// <summary>
    /// Get the max amount of placements that can be made of this defence type.
    /// </summary>
    public int MaxPlacement { get; }

    /// <summary>
    /// Get the list of unit types this defence allows to pre-garrison.
    /// </summary>
    public string[] PreGarrisonUnitTypes { get; }

    /// <summary>
    /// Get the AI placement type.
    /// </summary>
    public AIPlacementType PlacementType { get; }

    /// <summary>
    /// Get the width of the element.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Initialise a new <see cref="FactionDefence"/> instance with specified parameters.
    /// </summary>
    /// <param name="EntityBlueprint">The name of the EBP.</param>
    /// <param name="IsDirectional">Directional placement.</param>
    /// <param name="IsLinePlacement">Line Placement.</param>
    /// <param name="MaxPlacement">Max placement count.</param>
    /// <param name="PreGarrisonUnitTypes">Allowed pre-garrison units.</param>
    [JsonConstructor]
    public FactionDefence(string EntityBlueprint, bool IsDirectional, bool IsLinePlacement, int MaxPlacement, string[] PreGarrisonUnitTypes, 
        AIPlacementType PlacementType, int Width) {
        this.EntityBlueprint = EntityBlueprint;
        this.IsDirectional = IsDirectional;
        this.IsLinePlacement = IsLinePlacement;
        this.MaxPlacement = MaxPlacement is -1 ? int.MaxValue : MaxPlacement;
        this.PreGarrisonUnitTypes = PreGarrisonUnitTypes;
        this.PlacementType = PlacementType;
        this.Width = Width;
    }

    public override string ToString() => this.EntityBlueprint;

}
