using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;
using System;
using Battlegrounds.Game.Blueprints;
using Battlegrounds.Functional;
using Battlegrounds.Logging;

namespace Battlegrounds.DataLocal.Generator;

/// <summary>
/// Static utility class for generating initial company generators.
/// </summary>
public static class InitialCompanyCreator {

    private static readonly Logger logger = Logger.CreateLogger();

    /// <summary>
    /// Initialise basic companies for first time use.
    /// </summary>
    public static void Init() {

        try {

            // Grab package
            var package = BattlegroundsContext.ModManager.GetPackageOrError("mod_bg");

            // Create default CoH2 companies
            CreateDefaultSovietCompany(package);
            CreateDefaultGermanCompany(package);

            // Create default CoH3 companies
            CreateDefaultCoH3BritishCompany(package);
            CreateDefaultCoH3DAKCompany(package);

        } catch (Exception ex) {
            logger.Error(ex);
        }

    }

    private static void CreateDefaultSovietCompany(IModPackage package) {

        // Grab faction
        var sov = Faction.Soviet;

        // Grab type
        var typ = package.GetCompanyType(sov, "sov_rifles") ?? throw new Exception("Failed to fetch company type");

        // Create builder
        CompanyBuilder builder = CompanyBuilder.NewCompany("Default Soviet", typ, CompanyAvailabilityType.MultiplayerOnly, sov, ModGuid.BattlegroundsTuning);

        // Initial phase
        AddUnit(builder, "frontoviki_squad_bg", DeploymentPhase.PhaseInitial, DeploymentRole.DirectCommand);
        AddUnit(builder, "frontoviki_squad_bg", DeploymentPhase.PhaseInitial, DeploymentRole.DirectCommand,
                DeploymentMethod.DeployAndExit, "zis_6_transport_truck_bg", new string[] { "ppsh-41_sub_machine_gun_upgrade_bg" });
        AddUnit(builder, "conscript_squad_bg", DeploymentPhase.PhaseInitial, DeploymentRole.DirectCommand);
        AddUnit(builder, "conscript_squad_bg", DeploymentPhase.PhaseInitial, DeploymentRole.DirectCommand);
        AddUnit(builder, "conscript_squad_bg", DeploymentPhase.PhaseInitial, DeploymentRole.DirectCommand);
        AddUnit(builder, "conscript_squad_bg", DeploymentPhase.PhaseInitial, DeploymentRole.DirectCommand);

        AddUnit(builder, "combat_engineer_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);
        AddUnit(builder, "combat_engineer_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);
        AddUnit(builder, "combat_engineer_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);

        AddUnit(builder, "recon_team_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "recon_team_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        AddUnit(builder, "guards_troops_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "guards_troops_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "guards_troops_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand, 
                DeploymentMethod.DeployAndStay, "zis_6_transport_truck_bg", new string[] { "dp-28_lmg_upgrade_bg" });

        AddUnit(builder, "penal_battalion_bg", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);
        AddUnit(builder, "penal_battalion_bg", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);
        AddUnit(builder, "penal_battalion_bg", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);
        AddUnit(builder, "penal_battalion_bg", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);

        AddUnit(builder, "tank_buster_bg", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);
        AddUnit(builder, "tank_buster_bg", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);

        AddUnit(builder, "dshk_38_hmg_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "dshk_38_hmg_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        AddUnit(builder, "m1910_maxim_heavy_machine_gun_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        AddUnit(builder, "m1942_zis-3_76mm_at_gun_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "m1942_zis-3_76mm_at_gun_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "m1942_zis-3_76mm_at_gun_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand,
                DeploymentMethod.DeployAndExit, "zis_6_transport_truck_bg");

        AddUnit(builder, "m1937_152mm_ml_20_artillery_bg", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole,
                DeploymentMethod.DeployAndStay, "zis_6_transport_truck_bg");

        AddUnit(builder, "pm-82_41_mortar_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "hm-120_38_mortar_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "hm-120_38_mortar_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        AddUnit(builder, "katyusha_rocket_truck_bg", DeploymentPhase.PhaseStandard, DeploymentRole.ReserveRole);
        AddUnit(builder, "katyusha_rocket_truck_bg", DeploymentPhase.PhaseStandard, DeploymentRole.ReserveRole);

        AddUnit(builder, "kv-1_bg", DeploymentPhase.PhaseStandard, DeploymentRole.ReserveRole);

        AddUnit(builder, "su-85_bg", DeploymentPhase.PhaseStandard, DeploymentRole.ReserveRole);
        AddUnit(builder, "su-85_bg", DeploymentPhase.PhaseStandard, DeploymentRole.ReserveRole);

        AddUnit(builder, "t_34_85_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);
        AddUnit(builder, "t_34_85_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);
        AddUnit(builder, "t_34_85_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);

        AddUnit(builder, "commissar_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "soviet_officer_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        // Build company
        var company = builder.Commit().Result;

        // Save
        Companies.SaveCompany(company);

        // Register
        Companies.Register(company);

    }

    private static void CreateDefaultGermanCompany(IModPackage package) {

        // Grab faction
        var ger = Faction.Wehrmacht;

        // Grab type
        var typ = package.GetCompanyType("ost_rifles") ?? throw new Exception("Failed to fetch company type");

        // Create builder
        CompanyBuilder builder = CompanyBuilder.NewCompany("Default German", typ, CompanyAvailabilityType.MultiplayerOnly, ger, ModGuid.BattlegroundsTuning);

        // Initial phase
        AddUnit(builder, "grenadier_squad_bg", DeploymentPhase.PhaseInitial, DeploymentRole.DirectCommand);
        AddUnit(builder, "grenadier_squad_bg", DeploymentPhase.PhaseInitial, DeploymentRole.DirectCommand,
                DeploymentMethod.DeployAndExit, "opel_blitz_transport_squad_bg");
        AddUnit(builder, "grenadier_squad_bg", DeploymentPhase.PhaseInitial, DeploymentRole.DirectCommand);
        AddUnit(builder, "ostruppen_squad_bg", DeploymentPhase.PhaseInitial, DeploymentRole.DirectCommand,
                DeploymentMethod.DeployAndExit, "opel_blitz_transport_squad_bg");
        AddUnit(builder, "ostruppen_squad_bg", DeploymentPhase.PhaseInitial, DeploymentRole.DirectCommand,
                DeploymentMethod.DeployAndExit, "opel_blitz_transport_squad_bg");
        AddUnit(builder, "pioneer_squad_bg", DeploymentPhase.PhaseInitial, DeploymentRole.DirectCommand);

        AddUnit(builder, "pioneer_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "pioneer_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        AddUnit(builder, "ost_tankhunter_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "ost_tankhunter_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        AddUnit(builder, "stormtrooper_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "stormtrooper_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        AddUnit(builder, "assault_grenadier_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "assault_grenadier_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        AddUnit(builder, "jaeger_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "jaeger_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand,
                DeploymentMethod.DeployAndExit, "opel_blitz_transport_squad_bg", new string[] { "panzer_grenadier_panzershreck_atw_item_mp" });
        AddUnit(builder, "jaeger_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand,
                DeploymentMethod.DeployAndExit, "opel_blitz_transport_squad_bg", new string[] { "panzerbusche_39_mp" });

        AddUnit(builder, "ostruppen_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "ostruppen_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "ostruppen_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        AddUnit(builder, "mg42_heavy_machine_gun_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "mg42_heavy_machine_gun_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        AddUnit(builder, "pak40_75mm_at_gun_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "pak40_75mm_at_gun_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "pak40_75mm_at_gun_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        AddUnit(builder, "mortar_team_81mm_bg", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);
        AddUnit(builder, "mortar_team_81mm_bg", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);
        AddUnit(builder, "mortar_team_81mm_bg", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);

        AddUnit(builder, "pak43_88mm_at_gun_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        AddUnit(builder, "howitzer_105mm_le_fh18_artillery_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand,
                DeploymentMethod.DeployAndStay, "opel_blitz_transport_squad_bg");

        AddUnit(builder, "brummbar_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.ReserveRole);
        AddUnit(builder, "brummbar_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.ReserveRole);

        AddUnit(builder, "mortar_250_halftrack_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        AddUnit(builder, "panzer_iv_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);
        AddUnit(builder, "panzer_iv_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);
        AddUnit(builder, "panzer_iv_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);

        AddUnit(builder, "tiger_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        AddUnit(builder, "sdkfz_234_puma_ost_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        AddUnit(builder, "assault_officer_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "luftwaffe_officer_squad_bg", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        // Build company
        var company = builder.Commit().Result;

        // Save
        Companies.SaveCompany(company);

        // Register
        Companies.Register(company);

    }

    private static void CreateDefaultCoH3BritishCompany(IModPackage package) {

        // Grab faction
        var ger = Faction.BritishAfrica;

        // Grab type
        var typ = package.GetCompanyType("ukf3_rifles") ?? throw new Exception("Failed to fetch company type");

        // Create builder
        CompanyBuilder builder = CompanyBuilder.NewCompany("Desert Rats", typ, CompanyAvailabilityType.MultiplayerOnly, ger, ModGuid.BattlegroundsTuning);

        // Initial phase
        AddUnit(builder, "SBP.BRITISH.TOMMY_UK", DeploymentPhase.PhaseInitial, DeploymentRole.DirectCommand);
        AddUnit(builder, "SBP.BRITISH.TOMMY_UK", DeploymentPhase.PhaseInitial, DeploymentRole.DirectCommand, upgrades: new string[] { "UPGRADE.BRITISH.LMG_BREN_TOMMY_UK" });
        AddUnit(builder, "SBP.BRITISH.TOMMY_UK", DeploymentPhase.PhaseInitial, DeploymentRole.DirectCommand);
        AddUnit(builder, "SBP.BRITISH.TOMMY_UK", DeploymentPhase.PhaseInitial, DeploymentRole.DirectCommand);
        AddUnit(builder, "SBP.BRITISH.TOMMY_UK", DeploymentPhase.PhaseInitial, DeploymentRole.DirectCommand);
        AddUnit(builder, "SBP.BRITISH.TOMMY_UK", DeploymentPhase.PhaseInitial, DeploymentRole.DirectCommand);

        AddUnit(builder, "SBP.BRITISH.SAPPER_UK", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);
        AddUnit(builder, "SBP.BRITISH.SAPPER_UK", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);
        AddUnit(builder, "SBP.BRITISH.SAPPER_UK", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);

        AddUnit(builder, "SBP.BRITISH_AFRICA.CANADIAN_HEAVY_INFANTRY_AFRICA_UK", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "SBP.BRITISH_AFRICA.CANADIAN_HEAVY_INFANTRY_AFRICA_UK", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "SBP.BRITISH_AFRICA.CANADIAN_HEAVY_INFANTRY_AFRICA_UK", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        AddUnit(builder, "SBP.BRITISH.TOMMY_UK", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole, upgrades: new[] { "UPGRADE.BRITISH.BOYS_ANTI_TANK_RIFLES_TOMMY_UK" });
        AddUnit(builder, "SBP.BRITISH.TOMMY_UK", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole, upgrades: new[] { "UPGRADE.BRITISH.BOYS_ANTI_TANK_RIFLES_TOMMY_UK" });

        AddUnit(builder, "SBP.BRITISH.HMG_VICKERS_UK", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "SBP.BRITISH.HMG_VICKERS_UK", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        AddUnit(builder, "SBP.BRITISH.AT_GUN_6PDR_UK", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "SBP.BRITISH.AT_GUN_6PDR_UK", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "SBP.BRITISH.AT_GUN_6PDR_UK", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        AddUnit(builder, "SBP.BRITISH.MORTAR_81MM_UK", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "SBP.BRITISH.MORTAR_81MM_UK", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);
        AddUnit(builder, "SBP.BRITISH.MORTAR_81MM_UK", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        AddUnit(builder, "SBP.BRITISH.BISHOP_UK", DeploymentPhase.PhaseStandard, DeploymentRole.ReserveRole);
        AddUnit(builder, "SBP.BRITISH.BISHOP_UK", DeploymentPhase.PhaseStandard, DeploymentRole.ReserveRole);

        AddUnit(builder, "SBP.BRITISH.MATILDA_UK", DeploymentPhase.PhaseStandard, DeploymentRole.ReserveRole);
        AddUnit(builder, "SBP.BRITISH.MATILDA_UK", DeploymentPhase.PhaseStandard, DeploymentRole.ReserveRole);

        AddUnit(builder, "SBP.BRITISH.ARCHER_UK", DeploymentPhase.PhaseStandard, DeploymentRole.ReserveRole);
        AddUnit(builder, "SBP.BRITISH.ARCHER_UK", DeploymentPhase.PhaseStandard, DeploymentRole.ReserveRole);

        AddUnit(builder, "SBP.BRITISH.CRUSADER_57MM_UK", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);
        AddUnit(builder, "SBP.BRITISH.CRUSADER_57MM_UK", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);
        AddUnit(builder, "SBP.BRITISH.CRUSADER_57MM_UK", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);
        AddUnit(builder, "SBP.BRITISH.CRUSADER_57MM_UK", DeploymentPhase.PhaseStandard, DeploymentRole.SupportRole);

        AddUnit(builder, "SBP.BRITISH_AFRICA.OFFICER_AFRICA_UK", DeploymentPhase.PhaseStandard, DeploymentRole.DirectCommand);

        // Build company
        var company = builder.Commit().Result;

        // Save
        Companies.SaveCompany(company);

        // Register
        Companies.Register(company);

    }

    private static void CreateDefaultCoH3DAKCompany(IModPackage package) {

    }

    private static void AddUnit(CompanyBuilder cb, string bp, DeploymentPhase dp, DeploymentRole role, DeploymentMethod dm = DeploymentMethod.None, string transport = "", string[]? upgrades = null) {
        var sbp = cb.BlueprintDatabase.FromBlueprintName<SquadBlueprint>(bp);
        var tsbp = string.IsNullOrEmpty(transport) ? null : cb.BlueprintDatabase.FromBlueprintName<SquadBlueprint>(transport);

        cb.AddUnit(
            UnitBuilder.NewUnit(sbp)
                .SetDeploymentPhase(dp)
                .SetDeploymentRole(role)
                .SetDeploymentMethod(dm)
                .SetTransportBlueprint(tsbp)
                .AddUpgrade(upgrades?.Map(cb.BlueprintDatabase.FromBlueprintName<UpgradeBlueprint>) ?? Array.Empty<UpgradeBlueprint>())
       );
    }

}
