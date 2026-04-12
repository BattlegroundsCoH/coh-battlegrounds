using Battlegrounds.Models.Blueprints;

namespace Battlegrounds.Models.Companies;

/// <summary>
/// Represents an item that has been captured, including identifying information, the capturing squad, and the time of
/// capture.
/// </summary>
public sealed class CapturedItem {

    /// <summary>
    /// Gets the unique identifier for the captured item.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Gets the blueprint that defines the item's entity configuration.
    /// </summary>
    public EntityBlueprint? ItemBlueprint { get; init; } // TODO: When C# introduces union types, this should be a union of EntityBlueprint and SlotItemBlueprint

    /// <summary>
    /// Gets the identifier of the squad that captured the item.
    /// </summary>
    /// <remarks>A value of -1 indicates that the capturing squad is unknown, not tracked, or no longer
    /// exists.</remarks>
    public int CapturedBySquadId { get; init; } // If -1 the squad that captured the item is unknown (not tracked, dead, etc.)

    /// <summary>
    /// Gets the date and time when the item was captured.
    /// </summary>
    public DateTime CapturedAt { get; init; }

}
