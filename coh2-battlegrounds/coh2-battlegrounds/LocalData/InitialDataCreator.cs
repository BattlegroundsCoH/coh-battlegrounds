using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

namespace BattlegroundsApp.LocalData;

/// <summary>
/// Static utility class for creating initial data on first startup
/// </summary>
public static class InitialDataCreator {

    public static void Init() {

        // Wait for database to be loaded
        DatabaseManager.LoadedCallback(() => {

            // Create default companies
            CreateDefaultSovietCompany();
            CreateDefaultGermanCompany();

        });

    }

    private static void CreateDefaultSovietCompany() {

        // Create builder
        CompanyBuilder builder = CompanyBuilder.NewCompany("Default Soviet", CompanyType.Infantry, CompanyAvailabilityType.MultiplayerOnly, Faction.Soviet, ModGuid.BattlegroundsTuning);

        // Initial phase
        AddUnit(builder, "combat_engineer_squad_bg", DeploymentPhase.PhaseInitial);
        AddUnit(builder, "conscript_squad_bg", DeploymentPhase.PhaseInitial, DeploymentMethod.DeployAndExit, "zis_6_transport_truck_bg");
        AddUnit(builder, "conscript_squad_bg", DeploymentPhase.PhaseInitial);
        AddUnit(builder, "m1910_maxim_heavy_machine_gun_squad_bg", DeploymentPhase.PhaseInitial);
        AddUnit(builder, "m1942_zis-3_76mm_at_gun_squad_bg", DeploymentPhase.PhaseInitial, DeploymentMethod.DeployAndExit, "zis_6_transport_truck_bg");

        // Phase A
        AddUnit(builder, "t_34_76_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "t_34_76_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "t_34_85_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "t_34_85_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "m1937_53-k_45mm_at_gun_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "m1942_zis-3_76mm_at_gun_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "m1910_maxim_heavy_machine_gun_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "m1910_maxim_heavy_machine_gun_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "penal_battalion_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "penal_battalion_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "penal_battalion_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "penal_battalion_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "conscript_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "conscript_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "conscript_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "combat_engineer_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "combat_engineer_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "combat_engineer_squad_bg", DeploymentPhase.PhaseA);

        // Phase B
        AddUnit(builder, "su-85_bg", DeploymentPhase.PhaseB);
        AddUnit(builder, "pm-82_41_mortar_squad_bg", DeploymentPhase.PhaseB);
        AddUnit(builder, "pm-82_41_mortar_squad_bg", DeploymentPhase.PhaseB);
        AddUnit(builder, "dshk_38_hmg_squad_bg", DeploymentPhase.PhaseB);
        AddUnit(builder, "dshk_38_hmg_squad_bg", DeploymentPhase.PhaseB);
        AddUnit(builder, "m1910_maxim_heavy_machine_gun_squad_bg", DeploymentPhase.PhaseB);
        AddUnit(builder, "guards_troops_bg", DeploymentPhase.PhaseB);
        AddUnit(builder, "guards_troops_bg", DeploymentPhase.PhaseB);
        AddUnit(builder, "guards_troops_bg", DeploymentPhase.PhaseB);

        // Phase C
        AddUnit(builder, "is-2_bg", DeploymentPhase.PhaseC);
        AddUnit(builder, "m1937_152mm_ml_20_artillery_bg", DeploymentPhase.PhaseC, DeploymentMethod.DeployAndStay, "zis_6_transport_truck_bg");
        AddUnit(builder, "m1937_152mm_ml_20_artillery_bg", DeploymentPhase.PhaseC, DeploymentMethod.DeployAndStay, "zis_6_transport_truck_bg");
        AddUnit(builder, "hm-120_38_mortar_squad_bg", DeploymentPhase.PhaseC);
        AddUnit(builder, "sniper_team_bg", DeploymentPhase.PhaseC);
        AddUnit(builder, "sniper_team_bg", DeploymentPhase.PhaseC);
        AddUnit(builder, "shock_troops_bg", DeploymentPhase.PhaseC);
        AddUnit(builder, "shock_troops_bg", DeploymentPhase.PhaseC);

        // Save
        PlayerCompanies.SaveCompany(builder.Commit().Result);

    }

    private static void CreateDefaultGermanCompany() {
        
        // Create builder
        CompanyBuilder builder = CompanyBuilder.NewCompany("Default German", CompanyType.Infantry, CompanyAvailabilityType.MultiplayerOnly, Faction.Wehrmacht, ModGuid.BattlegroundsTuning);

        // Initial phase
        AddUnit(builder, "pioneer_squad_bg", DeploymentPhase.PhaseInitial);
        AddUnit(builder, "grenadier_squad_bg", DeploymentPhase.PhaseInitial, DeploymentMethod.DeployAndExit, "opel_blitz_transport_squad_bg");
        AddUnit(builder, "grenadier_squad_bg", DeploymentPhase.PhaseInitial);
        AddUnit(builder, "mg42_heavy_machine_gun_squad_bg", DeploymentPhase.PhaseInitial);

        // Phase A
        AddUnit(builder, "pioneer_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "pioneer_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "grenadier_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "grenadier_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "panzer_grenadier_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "panzer_grenadier_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "assault_grenadier_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "assault_grenadier_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "ost_tankhunter_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "ost_tankhunter_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "mg42_heavy_machine_gun_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "mg42_heavy_machine_gun_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "mg42_heavy_machine_gun_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "mortar_team_81mm_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "pak40_75mm_at_gun_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "pak40_75mm_at_gun_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "pak40_75mm_at_gun_squad_bg", DeploymentPhase.PhaseA);
        AddUnit(builder, "sdkfz_234_puma_ost_bg", DeploymentPhase.PhaseA);

        // Phase B
        AddUnit(builder, "panzer_grenadier_squad_bg", DeploymentPhase.PhaseB);
        AddUnit(builder, "panzer_grenadier_squad_bg", DeploymentPhase.PhaseB);
        AddUnit(builder, "stormtrooper_squad_bg", DeploymentPhase.PhaseB);
        AddUnit(builder, "stormtrooper_squad_bg", DeploymentPhase.PhaseB);
        AddUnit(builder, "sniper_squad_bg", DeploymentPhase.PhaseB);
        AddUnit(builder, "mg42_heavy_machine_gun_squad_bg", DeploymentPhase.PhaseB);
        AddUnit(builder, "mortar_team_81mm_bg", DeploymentPhase.PhaseB);
        AddUnit(builder, "pak40_75mm_at_gun_squad_bg", DeploymentPhase.PhaseB);
        AddUnit(builder, "pak40_75mm_at_gun_squad_bg", DeploymentPhase.PhaseB);
        AddUnit(builder, "ostwind_squad_bg", DeploymentPhase.PhaseB);
        AddUnit(builder, "panzerwerfer_squad_bg", DeploymentPhase.PhaseB);
        AddUnit(builder, "panzer_iv_squad_bg", DeploymentPhase.PhaseB);
        AddUnit(builder, "panzer_iv_squad_bg", DeploymentPhase.PhaseB);

        // Phase C
        AddUnit(builder, "sniper_squad_bg", DeploymentPhase.PhaseC);
        AddUnit(builder, "luftwaffe_officer_squad_bg", DeploymentPhase.PhaseC);
        AddUnit(builder, "howitzer_105mm_le_fh18_artillery_bg", DeploymentPhase.PhaseC, DeploymentMethod.DeployAndStay, "opel_blitz_transport_squad_bg");
        AddUnit(builder, "howitzer_105mm_le_fh18_artillery_bg", DeploymentPhase.PhaseC, DeploymentMethod.DeployAndStay, "opel_blitz_transport_squad_bg");
        AddUnit(builder, "tiger_command_squad_bg", DeploymentPhase.PhaseC);

        // Save
        PlayerCompanies.SaveCompany(builder.Commit().Result);

    }

    private static void AddUnit(CompanyBuilder cb, string bp, DeploymentPhase dp, DeploymentMethod dm = DeploymentMethod.None, string transport = "") {
        var sbp = BlueprintManager.FromBlueprintName<SquadBlueprint>(bp);
        var tsbp = string.IsNullOrEmpty(transport) ? null : BlueprintManager.FromBlueprintName<SquadBlueprint>(transport);
        cb.AddUnit(
            UnitBuilder.NewUnit(sbp)
            .SetDeploymentPhase(dp).SetDeploymentMethod(dm).SetTransportBlueprint(tsbp));
    }

}
