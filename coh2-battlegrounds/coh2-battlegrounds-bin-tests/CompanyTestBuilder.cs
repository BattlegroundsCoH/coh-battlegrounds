using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds;
using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Game.Database;
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
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(5).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseC).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseC).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("t_34_76_squad_bg").SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseB).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(1).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(4).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(5).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseB).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseB).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseB).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("t_34_76_squad_bg").SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseB).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("t_34_76_squad_bg").SetVeterancyRank(1).SetDeploymentPhase(DeploymentPhase.PhaseC).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("t_34_76_squad_bg").SetVeterancyRank(4).SetDeploymentPhase(DeploymentPhase.PhaseC).GetAndReset());
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
    }
}
