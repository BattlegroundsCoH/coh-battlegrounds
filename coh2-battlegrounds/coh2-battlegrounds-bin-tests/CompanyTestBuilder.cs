using System;

using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;

namespace coh2_battlegrounds_bin_tests {
    public static class CompanyTestBuilder {
        public static Company CreateSovietCompany() {

            // Create a dummy company
            CompanyBuilder companyBuilder = new CompanyBuilder().NewCompany(Faction.Soviet)
                .ChangeName("26th Rifle Division")
                .ChangeUser("CoDiEx")
                .ChangeTuningMod("142b113740474c82a60b0a428bd553d5");
            UnitBuilder unitBuilder = new UnitBuilder();

            // Basic infantry
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetDeploymentPhase(DeploymentPhase.PhaseInitial).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetDeploymentPhase(DeploymentPhase.PhaseInitial).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(1).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(4).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg")
                .SetVeterancyRank(3)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .AddSlotItem("dp-28_light_machine_gun_package_bg").GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(5).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseC).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseC).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("t_34_76_squad_bg").CreateAndGetCrew(GetCrew).SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseB).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg")
                .SetVeterancyRank(2)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .AddSlotItem("dp-28_light_machine_gun_package_bg")
                .AddSlotItem("ppsh41_assault_package_bg").GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(1).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(4).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(5).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg")
                .SetVeterancyRank(2)
                .SetDeploymentPhase(DeploymentPhase.PhaseB)
                .AddSlotItem("dp-28_light_machine_gun_package_bg").GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseB).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseB).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("t_34_76_squad_bg").CreateAndGetCrew(GetCrew).SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseB).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("t_34_76_squad_bg").CreateAndGetCrew(GetCrew).SetVeterancyRank(1).SetDeploymentPhase(DeploymentPhase.PhaseC).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("t_34_76_squad_bg").CreateAndGetCrew(GetCrew).SetVeterancyRank(4).SetDeploymentPhase(DeploymentPhase.PhaseC).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(5).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseC).GetAndReset());

            // Commit changes
            companyBuilder.Commit();
            var result = companyBuilder.Result;
            return result;

        }

        private static UnitBuilder GetCrew(UnitBuilder builder) => builder;

        public static Company CreateAdvancedSovietCompany() {

            // Create a dummy company
            CompanyBuilder companyBuilder = new CompanyBuilder().NewCompany(Faction.Soviet)
                .ChangeName("11th Motorized Division")
                .ChangeTuningMod("142b113740474c82a60b0a428bd553d5");
            UnitBuilder unitBuilder = new UnitBuilder();

            // Basic infantry
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetDeploymentPhase(DeploymentPhase.PhaseInitial).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseInitial)
                .SetVeterancyRank(2)
                .SetCombatTime(TimeSpan.FromMinutes(21.4))
                .AddSlotItem("ppsh41_assault_package_bg")
                .AddSlotItem("ppsh41_assault_package_bg")
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseInitial)
                .SetVeterancyRank(4)
                .SetCombatTime(TimeSpan.FromMinutes(29.27))
                .AddSlotItem("ppsh41_assault_package_bg")
                .AddSlotItem("ppsh41_assault_package_bg")
                .GetAndReset());

            // Phase 1
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg")
                .SetTransportBlueprint("zis_6_transport_truck_bg")
                .SetVeterancyRank(1)
                .SetCombatTime(TimeSpan.FromMinutes(7.8))
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg")
                .SetTransportBlueprint("zis_6_transport_truck_bg")
                .SetVeterancyRank(1)
                .SetCombatTime(TimeSpan.FromMinutes(7.1))
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg")
                .SetTransportBlueprint("zis_6_transport_truck_bg")
                .SetVeterancyRank(2)
                .SetCombatTime(TimeSpan.FromMinutes(16.11))
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .AddSlotItem("ppsh41_assault_package_bg")
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg")
                .SetTransportBlueprint("zis_6_transport_truck_bg")
                .SetVeterancyRank(2)
                .SetCombatTime(TimeSpan.FromMinutes(15.89))
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .AddSlotItem("ppsh41_assault_package_bg")
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("dshk_38_hmg_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("dshk_38_hmg_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("hm-120_38_mortar_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("m1937_53-k_45mm_at_gun_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("m1937_53-k_45mm_at_gun_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("combat_engineer_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("combat_engineer_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .SetVeterancyRank(1)
                .SetCombatTime(TimeSpan.FromMinutes(9.46))
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("t_34_76_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .SetVeterancyRank(2).SetCombatTime(TimeSpan.FromMinutes(34.76))
                .CreateAndGetCrew(
                    x => x.SetBlueprint("soviet_male_vehicle_driver_bg").SetCombatTime(TimeSpan.FromMinutes(34.76)).SetVeterancyRank(3)
                ).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("t_34_76_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .SetVeterancyRank(2).SetCombatTime(TimeSpan.FromMinutes(34.85))
                .CreateAndGetCrew(
                    x => x.SetBlueprint("soviet_male_vehicle_driver_bg").SetCombatTime(TimeSpan.FromMinutes(34.85)).SetVeterancyRank(3)
                ).GetAndReset());

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
