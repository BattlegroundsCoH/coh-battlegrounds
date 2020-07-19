using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Battlegrounds {
    
    /// <summary>
    /// Utility class for generating random <see cref="Company"/> data ready for ingame-use.
    /// </summary>
    public static class CompanyGenerator {

        // The random instance to use when making random decisions
        static Random __random = new Random();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="army"></param>
        /// <param name="type"></param>
        /// <param name="tuningGUID"></param>
        /// <param name="borrowVanilla"></param>
        /// <param name="allowVeterancy"></param>
        /// <returns></returns>
        public static Company Generate(Faction army, CompanyType type, string tuningGUID, bool borrowVanilla, bool allowVeterancy, bool allowCapturedEquipment) {

            // Create the company
            Company company = new Company(null, "Auto-Generated Company", type, army, tuningGUID);

            // Our selection query
            bool selectQuery(Blueprint x) {
                if (x is SquadBlueprint sbp) {
                    string guid = sbp.ModGUID.Replace("-", "");
                    return sbp.Army.CompareTo(army.Name) == 0 && ((borrowVanilla && guid.CompareTo(string.Empty) == 0) || (guid.CompareTo(tuningGUID) == 0));
                } else {
                    return false;
                }
            }

            // Select all the squad blueprints we can use
            IEnumerable<SquadBlueprint> blueprintPool = BlueprintManager.Select(selectQuery).Select(x => x as SquadBlueprint);

            // Get the max units we can deploy (not representative of standard-human company design!)
            int remaining = Company.MAX_SIZE;
            int max_tanks = 10 + (type == CompanyType.Armoured ? 6 : 0);
            int max_vehicles = 4;
            int max_command_units = 1;
            int max_specialized_infantry = 4 + (type == CompanyType.Infantry || type == CompanyType.Airborne ? 4 : 0);
            int max_heavy_tank = 3 + (type == CompanyType.Armoured ? 1 : 0);
            int max_support = 6 + (type == CompanyType.Engineer ? 2 : 0);
            int max_artillery = 4 + (type == CompanyType.Artillery ? 2 : 0);
            int max_transport_use = 4 + ((type == CompanyType.Motorized || type == CompanyType.Mechanized) ? 5 : 0);

            // While we can add a unit
            while (remaining > 0) {

                int unit_type = __random.Next(0, 52);
                byte vet_level = (allowVeterancy) ? (byte)__random.Next(0, 6) : (byte)0;

                if (unit_type >= 10 && unit_type <= 28) { // infantry

                    SquadBlueprint inf_blueprint = blueprintPool.Where(x => x.IsInfantry && !x.IsVehicleCrew && !x.IsSpecialInfantry && !x.IsCommandUnit && !x.IsSniper).Random(__random);
                    SquadBlueprint transportbp = null;
                    DeploymentMethod deployMethod = DeploymentMethod.None;

                    if (__random.Next(0, 50) <= 24 && max_transport_use > 0) {
                        transportbp = blueprintPool.Where(x => x.IsTransportVehicle).Random(__random);
                        deployMethod = (type == CompanyType.Motorized) ? DeploymentMethod.DeployAndExit : DeploymentMethod.DeployAndStay;
                        max_transport_use--;
                    }

                    company.AddSquad(inf_blueprint, transportbp, deployMethod, vet_level, 0, null, null, null);

                    remaining--;

                } else if (unit_type >= 3 && unit_type <= 9 && max_specialized_infantry > 0) { // special infantry

                    SquadBlueprint sinf_blueprint = blueprintPool.Where(x => x.IsSpecialInfantry || x.IsSniper).Random(__random);
                    SquadBlueprint transportbp = null;
                    DeploymentMethod deployMethod = DeploymentMethod.None;

                    if (__random.Next(0, 40) <= 24 && max_transport_use > 0) {
                        transportbp = blueprintPool.Where(x => x.IsTransportVehicle).Random(__random);
                        deployMethod = (type == CompanyType.Motorized) ? DeploymentMethod.DeployAndExit : DeploymentMethod.DeployAndStay;
                        max_transport_use--;
                    }

                    company.AddSquad(sinf_blueprint, transportbp, deployMethod, vet_level, 0, null, null, null);

                    max_specialized_infantry--;
                    remaining--;

                } else if (unit_type >= 0 && unit_type <= 2 && max_command_units > 0) { // command unit

                    SquadBlueprint cmd_bp = blueprintPool.Where(x => x.IsCommandUnit).Random(__random);
                    if (cmd_bp != null) {
                        company.AddSquad(cmd_bp, null, DeploymentMethod.None, vet_level, 0, null, null, null);

                        max_command_units--;
                        remaining--;

                    }

                } else if (unit_type >= 29 && unit_type <= 34 && max_support > 0) { // support

                    SquadBlueprint support = blueprintPool.Where(x => x.IsTeamWeapon).Random(__random);
                    SquadBlueprint transport_blueprint = null;
                    DeploymentMethod deployMethod = DeploymentMethod.None;

                    if (support.IsAntiTank &&__random.Next(0, 50) <= 24 && max_transport_use > 0) {
                        transport_blueprint = blueprintPool.Where(x => x.IsTransportVehicle).Random(__random);
                        deployMethod = DeploymentMethod.DeployAndStay;
                        max_transport_use--;
                    }

                    company.AddSquad(support, transport_blueprint, deployMethod, vet_level, 0, null, null, null);

                    max_support--;
                    remaining--;

                } else if (unit_type >= 35 && unit_type <= 39 && max_artillery > 0) { // artillery

                    SquadBlueprint arty_blueprint = blueprintPool.Where(x => x.IsHeavyArtillery || x.IsArtillery).Random(__random);
                    SquadBlueprint transport_blueprint = null;
                    DeploymentMethod deployMethod = DeploymentMethod.None;

                    if (arty_blueprint.IsHeavyArtillery) { // freebie for heavy artillery (no transport use "penalty")
                        transport_blueprint = blueprintPool.Where(x => x.IsTransportVehicle).Random(__random);
                        deployMethod = DeploymentMethod.DeployAndStay;
                    }

                    company.AddSquad(arty_blueprint, transport_blueprint, deployMethod, vet_level, 0, null, null, null);

                    max_artillery--;
                    remaining--;

                } else if (unit_type >= 40 && unit_type <= 46 && max_tanks > 0) { // tanks

                    SquadBlueprint tank_blueprint = blueprintPool.Where(x => x.IsArmour || !x.IsVehicle).Random(__random);
                    company.AddSquad(tank_blueprint, null, DeploymentMethod.None, vet_level, 0, null, null, null);

                    max_tanks--;
                    remaining--;

                } else if (unit_type >= 47 && unit_type <= 50 && max_vehicles > 0) { // vehicles

                    SquadBlueprint tank_blueprint = blueprintPool.Where(x => !x.IsArmour || x.IsVehicle).Random(__random);
                    company.AddSquad(tank_blueprint, null, DeploymentMethod.None, vet_level, 0, null, null, null);

                    max_vehicles--;
                    remaining--;

                } else if (max_heavy_tank > 0) { // heavy tank

                    SquadBlueprint hv_blueprint = blueprintPool.Where(x => x.IsHeavyArmour).Random(__random);
                    company.AddSquad(hv_blueprint, null, DeploymentMethod.None, vet_level, 0, null, null, null);

                    max_heavy_tank--;
                    remaining--;

                }

            }

            // Return the company
            return company;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="army"></param>
        /// <param name="tuningGUID"></param>
        /// <param name="borrowVanilla"></param>
        /// <param name="allowVeterancy"></param>
        /// <returns></returns>
        public static Company Generate(Faction army, string tuningGUID, bool borrowVanilla, bool allowVeterancy, bool allowCapturedEquipment)
            => Generate(army, CompanyType.Unspecified, tuningGUID, borrowVanilla, allowVeterancy, allowCapturedEquipment);

    }

}
