namespace Battlegrounds;

/// <summary>
/// Static utility class containing define values for various purposes.
/// </summary>
public static class BattlegroundsDefine {

    /// <summary>
    /// Cost modifier to apply to unit costs (per veterancy rank)
    /// </summary>
    public static readonly float VET_COSTMODIFIER = .005f;

    /// <summary>
    /// Maximum amount of units that can be in a company.
    /// </summary>
    public static readonly int COMPANY_MAX = 40;

    /// <summary>
    /// Maximum amount of units allowed in a role by default.
    /// </summary>
    public static readonly int COMPANY_ROLE_MAX = int.MaxValue;

    /// <summary>
    /// The max amount of initially deployed units.
    /// </summary>
    public static readonly int COMPANY_DEFAULT_INITIAL = 6;

}
