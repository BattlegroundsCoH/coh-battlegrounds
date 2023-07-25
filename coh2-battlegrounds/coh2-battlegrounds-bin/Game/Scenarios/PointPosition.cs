using System.Text.Json.Serialization;

namespace Battlegrounds.Game.Scenarios;

/// <summary>
/// 
/// </summary>
public readonly struct PointPosition {

    /// <summary>
    /// 
    /// </summary>
    [JsonInclude]
    public readonly GamePosition Position;

    /// <summary>
    /// 
    /// </summary>
    [JsonInclude]
    public readonly ushort Owner;

    /// <summary>
    /// 
    /// </summary>
    [JsonInclude]
    public readonly string EntityBlueprint;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Position"></param>
    /// <param name="Owner"></param>
    /// <param name="EntityBlueprint"></param>
    [JsonConstructor]
    public PointPosition(GamePosition Position, ushort Owner, string EntityBlueprint) {
        this.Position = Position;
        this.Owner = Owner;
        this.EntityBlueprint = EntityBlueprint;
    }

}
