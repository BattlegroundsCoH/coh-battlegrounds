using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

using Battlegrounds.Functional;

namespace Battlegrounds.Game.Blueprints.Extensions;

/// <summary>
/// 
/// </summary>
public class TypeList : IEnumerable<string> {

    private readonly HashSet<string> m_types;

    private readonly bool m_isHeavyArtyillery;
    private readonly bool m_isAT;
    private readonly bool m_isInfantry;
    private readonly bool m_isVehicle;
    private readonly bool m_isArmour;
    private readonly bool m_isHeavyArmour;
    private readonly bool m_isCrew;
    private readonly bool m_isSpecialInfantry;
    private readonly bool m_isOfficer;
    private readonly bool m_isCommandUnit;
    private readonly bool m_isArtillery;
    private readonly bool m_isSniper;
    private readonly bool m_isTransport;

    /// <summary>
    /// Can the <see cref="SquadBlueprint"/> be considered a heavy artillery piece.
    /// </summary>
    [JsonIgnore]
    public bool IsHeavyArtillery => m_isHeavyArtyillery;

    /// <summary>
    /// Can the <see cref="SquadBlueprint"/> be considered an anti-tank gun.
    /// </summary>
    [JsonIgnore] public bool IsAntiTank => m_isAT;

    /// <summary>
    /// Can the <see cref="SquadBlueprint"/> be considered infantry.
    /// </summary>
    [JsonIgnore] public bool IsInfantry => m_isInfantry;

    /// <summary>
    /// Can the <see cref="SquadBlueprint"/> be considered a vehicle (not a tank).
    /// </summary>
    [JsonIgnore]
    public bool IsVehicle => m_isVehicle;

    /// <summary>
    /// Can the <see cref="SquadBlueprint"/> be considered a tank.
    /// </summary>
    [JsonIgnore] public bool IsArmour => m_isArmour;

    /// <summary>
    /// Can the <see cref="SquadBlueprint"/> be considered a heavy tank.
    /// </summary>
    [JsonIgnore] public bool IsHeavyArmour => m_isHeavyArmour;

    /// <summary>
    /// Can the <see cref="SquadBlueprint"/> be considered a vehicle crew.
    /// </summary>
    [JsonIgnore] public bool IsVehicleCrew => m_isCrew;

    /// <summary>
    /// Can the <see cref="SquadBlueprint"/> be considered a special (elite) infantry.
    /// </summary>
    [JsonIgnore]
    public bool IsSpecialInfantry => m_isSpecialInfantry;

    /// <summary>
    /// Can the <see cref="SquadBlueprint"/> be considered to be an officer unit.
    /// </summary>
    [JsonIgnore] public bool IsOfficer => m_isOfficer;

    /// <summary>
    /// Can the <see cref="SquadBlueprint"/> be considered to be a command unit.
    /// </summary>
    [JsonIgnore] public bool IsCommandUnit => m_isCommandUnit;

    /// <summary>
    /// Can the <see cref="SquadBlueprint"/> be considered to be artillery.
    /// </summary>
    [JsonIgnore] public bool IsArtillery => m_isArtillery;

    /// <summary>
    /// Can the <see cref="SquadBlueprint"/> be considered a sniper unit.
    /// </summary>
    [JsonIgnore] public bool IsSniper => m_isSniper;

    /// <summary>
    /// Can the <see cref="SquadBlueprint"/> be considered a transport unit.
    /// </summary>
    [JsonIgnore] public bool IsTransportVehicle => m_isTransport;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="isTeamWeapon"></param>
    public TypeList(string[] source, bool isTeamWeapon) {

        // Set self data
        m_types = new(source);

        // Define property values
        m_isHeavyArtyillery = source.ContainsWithout("team_weapon",
            "wg_team_weapons", "mortar", "hmg"); // 'wg_team_weapons' is to block the raketenwerfer be considered a heavy artillery piece
        m_isAT = source.Contains("at_gun");
        m_isInfantry = source.Contains("standard_infantry") || (source.Contains("infantry") && !isTeamWeapon);
        m_isHeavyArmour = source.Contains("heavy_tank") || source.Contains("heavy_armor");
        m_isArmour = (source.Contains("vehicle_turret") || source.ContainsWithout("vehicle", "light_vehicle")) && !m_isHeavyArmour;
        m_isVehicle = !m_isArmour && source.Contains("vehicle")
            || source.Contains("light_vehicle") || source.Contains("light_armor");
        m_isCrew = source.Contains("aef_vehicle_crew");
        m_isSpecialInfantry = source.Contains("guard_troops") || source.Contains("shock_troops") || source.Contains("stormtrooper") || source.Contains("elite_infantry");
        m_isOfficer = source.Contains("sov_officer");
        m_isCommandUnit = m_isOfficer || source.Contains("command_panzer");
        m_isArtillery = source.Contains("artillery");
        m_isSniper = source.Contains("sniper_soviet") || source.Contains("sniper_german") || source.Contains("sniper");
        m_isTransport = source.Contains("m5_halftrack") || source.Contains("m3a1_scout_car") || source.Contains("251_halftrack");

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool IsType(string type) => m_types.Contains(type);

    /// <inheritdoc/>
    public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)m_types).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)m_types).GetEnumerator();

    /// <inheritdoc/>
    public override string ToString()
        => $"[{string.Join(';', this)}]";

}
