using Battlegrounds.Game.Database.Management;

namespace Battlegrounds.Game.Blueprints;

/// <summary>
/// The type a <see cref="Blueprint"/> may represent in the <see cref="BlueprintManager"/>.
/// </summary>
public enum BlueprintType {

    /// <summary>
    /// Ability Blueprint
    /// </summary>
    ABP,

    /// <summary>
    /// Upgrade Blueprint
    /// </summary>
    UBP,

    /// <summary>
    /// Critical Blueprint
    /// </summary>
    CBP,

    /// <summary>
    /// Slot Item Blueprint
    /// </summary>
    IBP,

    /// <summary>
    /// Entity Blueprint
    /// </summary>
    EBP = 16,

    /// <summary>
    /// Squad Blueprint
    /// </summary>
    SBP = 32,

    /// <summary>
    /// Weapon Blueprint
    /// </summary>
    WBP = 64,

}
