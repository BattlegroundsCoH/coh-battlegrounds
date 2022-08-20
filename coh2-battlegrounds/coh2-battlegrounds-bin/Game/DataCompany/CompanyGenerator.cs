using System;
using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;
using Battlegrounds.Modding.Content.Companies;

namespace Battlegrounds.Game.DataCompany;

/// <summary>
/// Utility class for generating random <see cref="Company"/> data ready for ingame-use.
/// </summary>
public static class CompanyGenerator {

    // The random instance to use when making random decisions
    private static readonly Random __random = new Random();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="army"></param>
    /// <param name="type"></param>
    /// <param name="tuningGUID"></param>
    /// <param name="borrowVanilla"></param>
    /// <param name="allowVeterancy"></param>
    /// <returns></returns>
    public static Company Generate(Faction army, ModPackage package, bool allowVeterancy, bool allowCapturedEquipment) {

        // Create the company
        CompanyBuilder builder = CompanyBuilder.NewCompany("Auto-generated Company", new BasicCompanyType(), CompanyAvailabilityType.AnyMode, army, package.TuningGUID);

        // Our selection query
        bool selectQuery(Blueprint x) {
            if (x is SquadBlueprint sbp) {
                return sbp.Army == army.Name && sbp.PBGID.Mod.GUID == package.TuningGUID;
            } else {
                return false;
            }
        }

        // Select all the squad blueprints we can use
        IEnumerable<SquadBlueprint> blueprintPool = BlueprintManager.Select(selectQuery).Select(x => x as SquadBlueprint).ToArray().NotNull();

        // Get the max units we can deploy (not representative of standard-human company design!)
        int remaining = Company.MAX_SIZE;
        int max_tanks = 10;
        int max_vehicles = 4;
        int max_command_units = 1;
        int max_specialized_infantry = 4;
        int max_heavy_tank = 2;
        int max_support = 6;
        int max_transport_use = 4;

        // While we can add a unit
        while (remaining >= 0) {

            int unit_type = __random.Next(0, 52);
            byte vet_level = (allowVeterancy) ? (byte)__random.Next(0, 6) : (byte)0;

            if (unit_type is >= 10 and <= 28) { // infantry

                SquadBlueprint inf_blueprint = blueprintPool.Where(
                    x => x.Types.IsInfantry && !x.Types.IsVehicleCrew && !x.Types.IsSpecialInfantry && !x.Types.IsCommandUnit && !x.Types.IsSniper).Random(__random);
                SquadBlueprint? transportbp = null;
                DeploymentMethod deployMethod = DeploymentMethod.None;

                if (__random.Next(0, 50) <= 24 && max_transport_use > 0) {
                    transportbp = blueprintPool.Where(x => x.Types.IsTransportVehicle).Random(__random);
                    deployMethod = DeploymentMethod.DeployAndExit;
                    max_transport_use--;
                }

                // Add unit
                builder.AddUnit(
                    UnitBuilder.NewUnit(inf_blueprint).
                    SetVeterancyRank(vet_level).
                    SetTransportBlueprint(transportbp).
                    SetDeploymentMethod(deployMethod));

                remaining--;

            } else if (unit_type >= 3 && unit_type <= 9 && max_specialized_infantry > 0) { // special infantry

                SquadBlueprint sinf_blueprint = blueprintPool.Where(x => x.Types.IsSpecialInfantry || x.Types.IsSniper).Random(__random);
                SquadBlueprint? transportbp = null;
                DeploymentMethod deployMethod = DeploymentMethod.None;

                if (__random.Next(0, 40) <= 24 && max_transport_use > 0) {
                    transportbp = blueprintPool.Where(x => x.Types.IsTransportVehicle).Random(__random);
                    deployMethod = DeploymentMethod.DeployAndExit;
                    max_transport_use--;
                }

                // Add unit
                builder.AddUnit(
                    UnitBuilder.NewUnit(sinf_blueprint).
                    SetVeterancyRank(vet_level).
                    SetTransportBlueprint(transportbp).
                    SetDeploymentMethod(deployMethod));

                max_specialized_infantry--;
                remaining--;

            } else if (unit_type >= 0 && unit_type <= 2 && max_command_units > 0) { // command unit

                SquadBlueprint cmd_bp = blueprintPool.Where(x => x.Types.IsCommandUnit).Random(__random);
                if (cmd_bp != null) {

                    // Add unit
                    builder.AddUnit(
                        UnitBuilder.NewUnit(cmd_bp).
                        SetVeterancyRank(vet_level));

                    max_command_units--;
                    remaining--;

                }

            } else if (unit_type >= 29 && unit_type <= 34 && max_support > 0) { // support

                SquadBlueprint support = blueprintPool.Where(x => x.IsTeamWeapon && !x.Types.IsHeavyArtillery).Random(__random);
                SquadBlueprint? transport_blueprint = null;
                DeploymentMethod deployMethod = DeploymentMethod.None;

                if (support.Types.IsAntiTank && __random.Next(0, 50) <= 24 && max_transport_use > 0) {
                    transport_blueprint = blueprintPool.Where(x => x.Types.IsTransportVehicle).Random(__random);
                    deployMethod = DeploymentMethod.DeployAndExit;
                    max_transport_use--;
                }

                // Add unit
                builder.AddUnit(
                    UnitBuilder.NewUnit(support).
                    SetVeterancyRank(vet_level).
                    SetTransportBlueprint(transport_blueprint).
                    SetDeploymentMethod(deployMethod));

                max_support--;
                remaining--;

            } else if (unit_type >= 38 && unit_type <= 45 && max_tanks > 0) { // tanks

                SquadBlueprint tank_blueprint = blueprintPool.Where(x => x.Types.IsArmour || !x.Types.IsVehicle).Random(__random);

                // Add unit
                builder.AddUnit(
                    UnitBuilder.NewUnit(tank_blueprint).
                    SetVeterancyRank(vet_level));

                max_tanks--;
                remaining--;

            } else if (unit_type >= 46 && unit_type <= 50 && max_vehicles > 0) { // vehicles

                SquadBlueprint vehicle_blueprint = blueprintPool.Where(x => !x.Types.IsArmour || x.Types.IsVehicle).Random(__random);

                // Add unit
                builder.AddUnit(
                    UnitBuilder.NewUnit(vehicle_blueprint).
                    SetVeterancyRank(vet_level));

                max_vehicles--;
                remaining--;

            } else if (max_heavy_tank > 0) { // heavy tank

                SquadBlueprint hv_blueprint = blueprintPool.Where(x => x.Types.IsHeavyArmour).Random(__random);

                // Add unit
                builder.AddUnit(
                    UnitBuilder.NewUnit(hv_blueprint).
                    SetVeterancyRank(vet_level));

                max_heavy_tank--;
                remaining--;

            }

        }

        // Commit changes and return the company
        return builder.Commit().Result;

    }

}
