using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding.Content.Companies;
using Battlegrounds.Modding;

using System;

namespace Battlegrounds.Developer.Generator;

public static class CompanyGenerator {

    public static Company CreateSovietCompany(ITuningMod? tuningMod) {

        // Do bail check
        if (tuningMod is null)
            throw new Exception("Tuning mod not defined!");

        // Grab tuning mod GUID
        var g = tuningMod.Guid;

        // Create a dummy company
        CompanyBuilder bld =
            CompanyBuilder.NewCompany("26th Rifle Division", new BasicCompanyType(), CompanyAvailabilityType.MultiplayerOnly, Faction.Soviet, g);

        // Grab blueprints
        var conscripts = BlueprintManager.FromBlueprintName<SquadBlueprint>("conscript_squad_bg");
        var frontoviki = BlueprintManager.FromBlueprintName<SquadBlueprint>("frontoviki_squad_bg");
        var busters = BlueprintManager.FromBlueprintName<SquadBlueprint>("tank_buster_bg");
        var shocks = BlueprintManager.FromBlueprintName<SquadBlueprint>("shock_troops_bg");
        var commissar = BlueprintManager.FromBlueprintName<SquadBlueprint>("commissar_squad_bg");
        var maxim = BlueprintManager.FromBlueprintName<SquadBlueprint>("m1910_maxim_heavy_machine_gun_squad_bg");
        var at = BlueprintManager.FromBlueprintName<SquadBlueprint>("m1942_zis-3_76mm_at_gun_squad_bg");
        var mortar = BlueprintManager.FromBlueprintName<SquadBlueprint>("pm-82_41_mortar_squad_bg");
        var m5 = BlueprintManager.FromBlueprintName<SquadBlueprint>("m5_halftrack_squad_bg");
        var t3476 = BlueprintManager.FromBlueprintName<SquadBlueprint>("t_34_76_squad_bg");
        var t3485 = BlueprintManager.FromBlueprintName<SquadBlueprint>("t_34_85_squad_bg");
        var su85 = BlueprintManager.FromBlueprintName<SquadBlueprint>("su-85_bg");
        var kv = BlueprintManager.FromBlueprintName<SquadBlueprint>("kv-1_bg");

        // Basic infantry
        bld.AddUnit(UnitBuilder.NewUnit(conscripts).SetVeterancyRank(1).AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg").SetDeploymentPhase(DeploymentPhase.PhaseInitial))
        .AddUnit(UnitBuilder.NewUnit(conscripts).SetVeterancyRank(1).AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg").SetDeploymentPhase(DeploymentPhase.PhaseInitial))
        .AddUnit(UnitBuilder.NewUnit(conscripts).SetVeterancyRank(2).AddUpgrade("dp-28_lmg_upgrade_bg").SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(conscripts).SetVeterancyRank(2).AddUpgrade("dp-28_lmg_upgrade_bg").SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(conscripts).SetVeterancyRank(4).AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg").SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(busters).SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(busters).SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(busters).SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(shocks).SetVeterancyRank(4).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(shocks).SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(shocks).SetVeterancyRank(5).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(shocks).SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(shocks).SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(commissar).SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseStandard));

        // Transported Infantry
        bld.AddUnit(UnitBuilder.NewUnit(conscripts)
            .SetTransportBlueprint("zis_6_transport_truck_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(conscripts)
            .SetTransportBlueprint("zis_6_transport_truck_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(3)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(conscripts)
            .SetTransportBlueprint("zis_6_transport_truck_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(4)
            .AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg")
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(conscripts)
            .SetTransportBlueprint("zis_6_transport_truck_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(5)
            .AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg")
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(frontoviki)
            .SetTransportBlueprint("m5_halftrack_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(0)
            .AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg")
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(frontoviki)
            .SetTransportBlueprint("m5_halftrack_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(1)
            .AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg")
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            );

        // Support Weapons
        bld.AddUnit(UnitBuilder.NewUnit(at)
            .SetTransportBlueprint("zis_6_transport_truck_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(at)
            .SetTransportBlueprint("zis_6_transport_truck_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(maxim)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(maxim)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(mortar)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(mortar)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            );

        // Vehicles
        bld.AddUnit(UnitBuilder.NewUnit(m5)
            .SetVeterancyRank(1)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(t3476)
            .SetVeterancyRank(0)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(t3476)
            .SetVeterancyRank(0)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(t3476)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(su85)
            .SetVeterancyRank(1)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(t3485)
            .SetVeterancyRank(4)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(t3485)
            .SetVeterancyRank(5)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(kv)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(kv)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            );

        // Commit changes
        return bld.Commit().Result;
    }

    public static Company CreateGermanCompany(ITuningMod? tuningMod) {

        // Do bail check
        if (tuningMod is null)
            throw new Exception("Tuning mod not defined!");

        // Grab tuning mod GUID
        var g = tuningMod.Guid;

        // Create a dummy company
        CompanyBuilder bld = CompanyBuilder.NewCompany("69th Panzer Kompanie", new BasicCompanyType(), CompanyAvailabilityType.MultiplayerOnly, Faction.Wehrmacht, g);

        // Grab blueprints
        var pioneers = BlueprintManager.FromBlueprintName<SquadBlueprint>("pioneer_squad_bg");
        var sniper = BlueprintManager.FromBlueprintName<SquadBlueprint>("sniper_squad_bg");
        var gren = BlueprintManager.FromBlueprintName<SquadBlueprint>("grenadier_squad_bg");
        var pgren = BlueprintManager.FromBlueprintName<SquadBlueprint>("panzer_grenadier_squad_bg");
        var pak = BlueprintManager.FromBlueprintName<SquadBlueprint>("pak40_75mm_at_gun_squad_bg");
        var mg = BlueprintManager.FromBlueprintName<SquadBlueprint>("mg42_heavy_machine_gun_squad_bg");
        var mortar = BlueprintManager.FromBlueprintName<SquadBlueprint>("mortar_team_81mm_bg");
        var puma = BlueprintManager.FromBlueprintName<SquadBlueprint>("sdkfz_234_puma_ost_bg");
        var panther = BlueprintManager.FromBlueprintName<SquadBlueprint>("panther_squad_bg");
        var pziv = BlueprintManager.FromBlueprintName<SquadBlueprint>("panzer_iv_squad_bg");
        var ostwind = BlueprintManager.FromBlueprintName<SquadBlueprint>("ostwind_squad_bg");
        var tiger = BlueprintManager.FromBlueprintName<SquadBlueprint>("tiger_squad_bg");
        var brumm = BlueprintManager.FromBlueprintName<SquadBlueprint>("brummbar_squad_bg");

        // Basic infantry
        bld.AddUnit(UnitBuilder.NewUnit(pioneers).SetDeploymentPhase(DeploymentPhase.PhaseInitial))
        .AddUnit(UnitBuilder.NewUnit(pioneers).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(pioneers).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(pioneers).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(sniper).SetDeploymentPhase(DeploymentPhase.PhaseInitial))
        .AddUnit(UnitBuilder.NewUnit(sniper).SetDeploymentPhase(DeploymentPhase.PhaseStandard));

        // Transported Infantry
        bld.AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("sdkfz_251_halftrack_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndStay)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("sdkfz_251_halftrack_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndStay)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("sdkfz_251_halftrack_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndStay)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("sdkfz_251_halftrack_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndStay)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(gren)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(2)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(pgren)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(3)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(pgren)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(3)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(pgren)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetVeterancyRank(3)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            );

        // Support Weapons
        bld.AddUnit(UnitBuilder.NewUnit(pak)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(pak)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(pak)
            .SetTransportBlueprint("opel_blitz_transport_squad_bg")
            .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(mg)
            .SetDeploymentPhase(DeploymentPhase.PhaseInitial)
            )
        .AddUnit(UnitBuilder.NewUnit(mg)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(mg)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(mg)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(mortar).SetDeploymentPhase(DeploymentPhase.PhaseStandard))
        .AddUnit(UnitBuilder.NewUnit(mortar).SetDeploymentPhase(DeploymentPhase.PhaseStandard));

        // Vehicles
        bld.AddUnit(UnitBuilder.NewUnit(puma)
            .SetVeterancyRank(1)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(puma)
            .SetVeterancyRank(1)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(panther)
            .SetVeterancyRank(1)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(pziv)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(pziv)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(pziv)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(pziv)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(ostwind)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(ostwind)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(tiger)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(tiger)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(tiger)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            )
        .AddUnit(UnitBuilder.NewUnit(brumm)
            .SetDeploymentPhase(DeploymentPhase.PhaseStandard)
            .SetVeterancyRank(3)
            );

        // Commit changes
        return bld.Commit().Result;
    }

}
