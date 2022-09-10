using System;
using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Compiler;

/// <summary>
/// Standard implementation for compiling a company into Lua format.
/// </summary>
public class CompanyCompiler : ICompanyCompiler {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="company"></param>
    /// <param name="isAIPlayer"></param>
    /// <param name="indexOnTeam"></param>
    /// <param name="customNames"></param>
    /// <returns></returns>
    public Dictionary<string, object> CompileToLua(Company company, bool isAIPlayer, byte indexOnTeam, IList<string> customNames) {

        // Grab units
        var units = company.Units;
        Array.Sort(units, this.CompareUnit);

        // Set unit names
        for (int i = 0; i < units.Length; i++) {
            int j = customNames.IndexOf(units[i].CustomName);
            if (j != -1) {
                units[i] = UnitBuilder.EditUnit(units[i]).SetCustomName($"bg_custom_name_{(j+1):000}").Commit(units[i].SquadID).Result;
            }
        }

        // Get unit abilities
        var uabps = company.GetSpecialUnitAbilities();

        // Get artillery
        var artillery = company.Abilities.Where(x => x.Category is AbilityCategory.Artillery)
            .Union(uabps.Where(x => x.Category is AbilityCategory.Artillery)).ToArray();

        // Get air
        var air = company.Abilities.Where(x => x.Category is AbilityCategory.AirSupport)
            .Union(uabps.Where(x => x.Category is AbilityCategory.AirSupport)).ToArray();

        // Create result
        Dictionary<string, object> result = new() {
            ["name"] = company.Name,
            ["style"] = company.Type,
            ["army"] = company.Army.Name,
            ["specials"] = new Dictionary<string, object>() {
                ["artillery"] = artillery,
                ["air"] = air,
            },
            ["upgrades"] = company.Upgrades.Select(x => x.GetScarName()),
            ["modifiers"] = company.Modifiers,
            ["units"] = units
        };

        // Return result
        return result;

    }

    protected virtual int CompareUnit(Squad lhs, Squad rhs) {
        string catlhs = lhs.GetCategory(true);
        string catrhs = rhs.GetCategory(true);
        int cilhs = catlhs.CompareTo("infantry") == 0 ? 0 : (catlhs.CompareTo("team_weapon") == 0 ? 1 : 2);
        int cirhs = catrhs.CompareTo("infantry") == 0 ? 0 : (catrhs.CompareTo("team_weapon") == 0 ? 1 : 2);
        if (cirhs == cilhs) {
            int order = lhs.Blueprint.Name.CompareTo(rhs.Blueprint.Name);
            if (order == 0) {
                return lhs.VeterancyRank - rhs.VeterancyRank;
            } else {
                if (lhs.SBP.Types.IsCommandUnit) {
                    return int.MaxValue;
                } else {
                    return order;
                }
            }
        } else {
            return cilhs - cirhs;
        }
    }

}
