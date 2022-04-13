using System;

using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

namespace coh2_battlegrounds_bin_tests {
    public static class CompanyTestBuilder {
        public static Company CreateSovietCompany() {

            // Grab mod guid
            var guid = ModGuid.FromGuid("142b113740474c82a60b0a428bd553d5");

            // Create a dummy company
            var companyBuilder = CompanyBuilder.NewCompany("26th Rifle Division", CompanyType.Infantry, CompanyAvailabilityType.MultiplayerOnly, Faction.Soviet, guid);

            // Basic infantry
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid).SetDeploymentPhase(DeploymentPhase.PhaseInitial));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid).SetDeploymentPhase(DeploymentPhase.PhaseInitial));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid).SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseA));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid).SetVeterancyRank(1).SetDeploymentPhase(DeploymentPhase.PhaseA));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid).SetVeterancyRank(4).SetDeploymentPhase(DeploymentPhase.PhaseA));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid)
                .SetVeterancyRank(3)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .AddSlotItem("dp-28_light_machine_gun_package_bg"));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid).SetVeterancyRank(5).SetDeploymentPhase(DeploymentPhase.PhaseA));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid).SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseC));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid).SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseC));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("t_34_76_squad_bg", guid).SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseB));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid)
                .SetVeterancyRank(2)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .AddSlotItem("dp-28_light_machine_gun_package_bg")
                .AddSlotItem("ppsh41_assault_package_bg"));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid).SetVeterancyRank(1).SetDeploymentPhase(DeploymentPhase.PhaseA));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid).SetVeterancyRank(4).SetDeploymentPhase(DeploymentPhase.PhaseA));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid).SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseA));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid).SetVeterancyRank(5).SetDeploymentPhase(DeploymentPhase.PhaseA));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid)
                .SetVeterancyRank(2)
                .SetDeploymentPhase(DeploymentPhase.PhaseB)
                .AddSlotItem("dp-28_light_machine_gun_package_bg"));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid).SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseB));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid).SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseB));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("t_34_76_squad_bg", guid).SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseB));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("t_34_76_squad_bg", guid).SetVeterancyRank(1).SetDeploymentPhase(DeploymentPhase.PhaseC));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("t_34_76_squad_bg", guid).SetVeterancyRank(4).SetDeploymentPhase(DeploymentPhase.PhaseC));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid).SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseA));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid).SetVeterancyRank(5).SetDeploymentPhase(DeploymentPhase.PhaseA));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid).SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseA));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid).SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseA));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid).SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseC));

            // Commit changes and return result
            return companyBuilder.Commit().Result;

        }

        public static Company CreateAdvancedSovietCompany() {

            // Grab mod guid
            var guid = ModGuid.FromGuid("142b113740474c82a60b0a428bd553d5");

            // Create a dummy company
            CompanyBuilder companyBuilder = CompanyBuilder.NewCompany("11th Motorized", CompanyType.Motorized, CompanyAvailabilityType.MultiplayerOnly, Faction.Soviet, guid);
  
            // Basic infantry
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid).SetDeploymentPhase(DeploymentPhase.PhaseInitial));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid)
                .SetDeploymentPhase(DeploymentPhase.PhaseInitial)
                .SetVeterancyRank(2)
                .SetCombatTime(TimeSpan.FromMinutes(21.4))
                .AddSlotItem("ppsh41_assault_package_bg")
                .AddSlotItem("ppsh41_assault_package_bg")
                );
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid)
                .SetDeploymentPhase(DeploymentPhase.PhaseInitial)
                .SetVeterancyRank(4)
                .SetCombatTime(TimeSpan.FromMinutes(29.27))
                .AddSlotItem("ppsh41_assault_package_bg")
                .AddSlotItem("ppsh41_assault_package_bg")
                );

            // Phase 1
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid)
                .SetTransportBlueprint("zis_6_transport_truck_bg")
                .SetVeterancyRank(1)
                .SetCombatTime(TimeSpan.FromMinutes(7.8))
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                );
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid)
                .SetTransportBlueprint("zis_6_transport_truck_bg")
                .SetVeterancyRank(1)
                .SetCombatTime(TimeSpan.FromMinutes(7.1))
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                );
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid)
                .SetTransportBlueprint("zis_6_transport_truck_bg")
                .SetVeterancyRank(2)
                .SetCombatTime(TimeSpan.FromMinutes(16.11))
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .AddSlotItem("ppsh41_assault_package_bg")
                );
            companyBuilder.AddUnit(UnitBuilder.NewUnit("conscript_squad_bg", guid)
                .SetTransportBlueprint("zis_6_transport_truck_bg")
                .SetVeterancyRank(2)
                .SetCombatTime(TimeSpan.FromMinutes(15.89))
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .AddSlotItem("ppsh41_assault_package_bg")
                );
            companyBuilder.AddUnit(UnitBuilder.NewUnit("dshk_38_hmg_squad_bg", guid)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                );
            companyBuilder.AddUnit(UnitBuilder.NewUnit("dshk_38_hmg_squad_bg", guid)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                );
            companyBuilder.AddUnit(UnitBuilder.NewUnit("hm-120_38_mortar_squad_bg", guid)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                );
            companyBuilder.AddUnit(UnitBuilder.NewUnit("m1937_53-k_45mm_at_gun_squad_bg", guid)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                );
            companyBuilder.AddUnit(UnitBuilder.NewUnit("m1937_53-k_45mm_at_gun_squad_bg", guid)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                );
            companyBuilder.AddUnit(UnitBuilder.NewUnit("combat_engineer_squad_bg", guid)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                );
            companyBuilder.AddUnit(UnitBuilder.NewUnit("combat_engineer_squad_bg", guid)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .SetVeterancyRank(1)
                .SetCombatTime(TimeSpan.FromMinutes(9.46))
                );
            companyBuilder.AddUnit(UnitBuilder.NewUnit("t_34_76_squad_bg", guid)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .SetVeterancyRank(2).SetCombatTime(TimeSpan.FromMinutes(34.76)));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("t_34_76_squad_bg", guid)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .SetVeterancyRank(2).SetCombatTime(TimeSpan.FromMinutes(34.85)));

            // Commit changes
            companyBuilder.Statistics = new CompanyStatistics() {
                TotalMatchWinCount = 17,
                TotalMatchLossCount = 5,
                TotalMatchCount = 22,
                TotalInfantryLosses = 347,
                TotalVehicleLosses = 9
            };

            // Return
            return companyBuilder.Commit().Result;

        }


        public static Company CreateAdvancedGermanCompany() {

            // Grab mod guid
            var guid = ModGuid.FromGuid("142b113740474c82a60b0a428bd553d5");

            // Create a dummy company
            CompanyBuilder companyBuilder
                = CompanyBuilder.NewCompany("29th Panzer Regiment", CompanyType.Motorized, CompanyAvailabilityType.MultiplayerOnly, Faction.Wehrmacht, guid);

            // Basic infantry
            companyBuilder.AddUnit(UnitBuilder.NewUnit("grenadier_squad_bg", guid).SetDeploymentPhase(DeploymentPhase.PhaseInitial));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("grenadier_squad_bg", guid).SetDeploymentPhase(DeploymentPhase.PhaseInitial));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("grenadier_squad_bg", guid).SetDeploymentPhase(DeploymentPhase.PhaseInitial));
            companyBuilder.AddUnit(UnitBuilder.NewUnit("grenadier_squad_bg", guid).SetDeploymentPhase(DeploymentPhase.PhaseA));

            // Commit changes
            companyBuilder.Commit();
            var result = companyBuilder.Result;
            result.UpdateStatistics(x => {
                x.TotalMatchWinCount = 17;
                x.TotalMatchLossCount = 5;
                x.TotalMatchCount = 22;
                x.TotalInfantryLosses = 347;
                x.TotalVehicleLosses = 9;
                return x;
            });

            return result;

        }

    }
}
