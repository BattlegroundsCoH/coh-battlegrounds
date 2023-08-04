using System.Text.Json.Serialization;

using Battlegrounds.Data.Generators.Lua.RuntimeServices;
using Battlegrounds.Functional;
using Battlegrounds.Game.Blueprints;
using Battlegrounds.Game.Gameplay.DataConverters;

namespace Battlegrounds.Game.Gameplay;

/// <summary>
/// Special ability category an ability may belong to.
/// </summary>
public enum AbilityCategory {

    /// <summary>
    /// The category is undefined (Will not be compiled).
    /// </summary>
    Undefined,

    /// <summary>
    /// Show up as a "commander" ability (Currently not compiled - CoDiEx 05/08/21).
    /// </summary>
    Default,

    /// <summary>
    /// Be included as an artillery upgrade.
    /// </summary>
    Artillery,

    /// <summary>
    /// Be included as an air support.
    /// </summary>
    AirSupport,

    /// <summary>
    /// The ability is a unit ability (Should not be compiled).
    /// </summary>
    Unit,

}

/// <summary>
/// Represents a <see cref="Ability"/> ingame ability.
/// </summary>
[LuaConverter(typeof(AbilityConverter))]
public class Ability {

    /// <summary>
    /// Get the <see cref="AbilityBlueprint"/> being granted by the <see cref="Ability"/>.
    /// </summary>
    public AbilityBlueprint ABP { get; }

    /// <summary>
    /// Get the <see cref="UpgradeBlueprint"/> required in order to unlock the ability.
    /// </summary>
    public UpgradeBlueprint UnlockUpgrade { get; }

    /// <summary>
    /// Get the unit blueprint that grants this ability.
    /// </summary>
    public SquadBlueprint[] GrantingBlueprints { get; }

    /// <summary>
    /// The <see cref="AbilityCategory"/> the <see cref="Ability"/> will belong to.
    /// </summary>
    public AbilityCategory Category { get; }

    /// <summary>
    /// The amount of uses a player has during each match.
    /// </summary>
    public int MaxUse { get; }

    /// <summary>
    /// Get or set the amount of times this special ability has been used.
    /// </summary>
    [JsonIgnore]
    public int UsedCount { get; set; }

    /// <summary>
    /// Instantiate a new <see cref="Ability"/> with predefined <see cref="AbilityCategory"/> and use count.
    /// </summary>
    /// <param name="ABP">The ability blueprint.</param>
    /// <param name="UnlockUpgrade">The upgrade that will unlock this ability</param>
    /// <param name="Category">The category.</param>
    /// <param name="MaxUse">The maximum amount of uses each match.</param>
    /// <param name="UsedCount">The amount of times this has been used.</param>
    public Ability(AbilityBlueprint ABP, string UnlockUpgrade, string[] GrantingBlueprints, AbilityCategory Category, int MaxUse, int UsedCount = -1) {
        var package = BattlegroundsContext.ModManager.GetPackageFromGuid(ABP.PBGID.Mod, ABP.Game) ?? throw new System.Exception();
        var ds = package.GetDataSource().GetBlueprints(ABP.Game);
        this.ABP = ABP;
        this.Category = Category;
        this.UnlockUpgrade = ds.FromBlueprintName<UpgradeBlueprint>(UnlockUpgrade);
        this.GrantingBlueprints = GrantingBlueprints.Map(x => ds.FromBlueprintName<SquadBlueprint>(x));
        this.MaxUse = MaxUse;
        this.UsedCount = UsedCount;
    }

    /// <summary>
    /// Instantiate a new <see cref="Ability"/> with predefined <see cref="AbilityCategory"/> and use count.
    /// </summary>
    /// <param name="ABP">The ability blueprint.</param>
    /// <param name="UnlockUpgrade">The upgrade that will unlock this ability</param>
    /// <param name="Category">The category.</param>
    /// <param name="MaxUse">The maximum amount of uses each match.</param>
    [JsonConstructor]
    public Ability(AbilityBlueprint ABP, UpgradeBlueprint UnlockUpgrade, SquadBlueprint[] GrantingBlueprints, AbilityCategory Category, int MaxUse) {
        this.ABP = ABP;
        this.Category = Category;
        this.UnlockUpgrade = UnlockUpgrade;
        this.GrantingBlueprints = GrantingBlueprints;
        this.MaxUse = MaxUse;
        this.UsedCount = -1;
    }

}
