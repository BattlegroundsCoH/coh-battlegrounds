using System;
using System.IO;
using System.Threading;

using Battlegrounds;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.DataSource.Replay;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match;
using Battlegrounds.Game.Match.Data;
using Battlegrounds.Json;
using Battlegrounds.Modding;

namespace coh2_battlegrounds_console {

    class Program {

        static bool recent_analysis;
        static string recent_file = null;
        static bool compile_json;

        static bool campaign_compile;
        static string campaign_compile_file = null;

        static bool extract_coh2_maps;

        static bool do_test_companies;

        static string output_path = null;

        static ITuningMod tuningMod;

        static void Main(string[] args) {

            // Write args
            Console.WriteLine(string.Join(' ', args));
            ParseArguments(args);

            if (recent_analysis) {
                LoadBGAndProceed();
                Console.WriteLine("Parsing latest replay file");
                string target = ReplayMatchData.LATEST_REPLAY_FILE;
                if (!string.IsNullOrEmpty(recent_file)) {
                    if (File.Exists(recent_file)) {
                        target = recent_file;
                    }
                }
                ReplayFile replayFile = new ReplayFile(target);
                Console.WriteLine($"Load Replay: {replayFile.LoadReplay()}");
                Console.WriteLine($"Partial: {replayFile.IsPartial}");
                if (compile_json) {
                    Console.WriteLine("Compiling to json playback");
                    ReplayMatchData playback = new ReplayMatchData(new NullSession());
                    playback.SetReplayFile(replayFile);
                    if (playback.ParseMatchData()) {
                        JsonPlayback events = new JsonPlayback(playback);
                        File.WriteAllText("playback.json", events.SerializeAsJson());
                        Console.WriteLine("Saved to .json");
                    } else {
                        Console.WriteLine("Failed to compile to json...");
                    }
                }
                Console.ReadLine();
            } else if (campaign_compile) {
                LoadBGAndProceed();
                CampaignCompiler.Output = output_path;
                CampaignCompiler.Compile(campaign_compile_file);
            } else if (extract_coh2_maps) {
                MapExtractor.Output = output_path;
                MapExtractor.Extract();
            } else if (do_test_companies) {

                LoadBGAndProceed();

                Company testCompany = CreateSovietCompany();
                Company germanCompany = CreateGermanCompany();

                File.WriteAllText("26th_Rifle_Division.json", CompanySerializer.GetCompanyAsJson(testCompany, true));
                File.WriteAllText("69th_panzer.json", CompanySerializer.GetCompanyAsJson(germanCompany, true));

            }

        }

        private static void LoadBGAndProceed() {

            // Load BG
            BattlegroundsInstance.LoadInstance();

            // Important this is done
            DatabaseManager.LoadAllDatabases(null);

            // Wait for database to load
            while (!DatabaseManager.DatabaseLoaded) {
                Thread.Sleep(100);
            }

            tuningMod = ModManager.GetMod<ITuningMod>(ModManager.GetPackage("mod_bg").TuningGUID);

        }

        private static Company CreateSovietCompany() {

            // Create a dummy company
            CompanyBuilder companyBuilder = new CompanyBuilder().NewCompany(Faction.Soviet)
                .ChangeName("26th Rifle Division")
                .ChangeTuningMod(tuningMod.Guid);
            UnitBuilder unitBuilder = new UnitBuilder();

            // Basic infantry
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(1).AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg").SetDeploymentPhase(DeploymentPhase.PhaseInitial).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(1).AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg").SetDeploymentPhase(DeploymentPhase.PhaseInitial).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(2).AddUpgrade("dp-28_lmg_upgrade_bg").SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(2).AddUpgrade("dp-28_lmg_upgrade_bg").SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg").SetVeterancyRank(4).AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg").SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("tank_buster_bg").SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("tank_buster_bg").SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("tank_buster_bg").SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("shock_troops_bg").SetVeterancyRank(4).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("shock_troops_bg").SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("shock_troops_bg").SetVeterancyRank(5).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("shock_troops_bg").SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseB).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("shock_troops_bg").SetVeterancyRank(2).SetDeploymentPhase(DeploymentPhase.PhaseB).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("commissar_squad_bg").SetVeterancyRank(3).SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());

            // Transported Infantry
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg")
                .SetTransportBlueprint("zis_6_transport_truck_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg")
                .SetTransportBlueprint("zis_6_transport_truck_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetVeterancyRank(3)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg")
                .SetTransportBlueprint("zis_6_transport_truck_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetVeterancyRank(4)
                .AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("conscript_squad_bg")
                .SetTransportBlueprint("zis_6_transport_truck_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetVeterancyRank(5)
                .AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("frontoviki_squad_bg")
                .SetTransportBlueprint("m5_halftrack_squad_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetVeterancyRank(0)
                .AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseB)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("frontoviki_squad_bg")
                .SetTransportBlueprint("m5_halftrack_squad_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetVeterancyRank(1)
                .AddUpgrade("ppsh-41_sub_machine_gun_upgrade_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseB)
                .GetAndReset());

            // Support Weapons
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("m1942_zis-3_76mm_at_gun_squad_bg")
                .SetTransportBlueprint("zis_6_transport_truck_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("m1942_zis-3_76mm_at_gun_squad_bg")
                .SetTransportBlueprint("zis_6_transport_truck_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("m1910_maxim_heavy_machine_gun_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("m1910_maxim_heavy_machine_gun_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("pm-82_41_mortar_squad_bg")
                .SetVeterancyRank(2)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("pm-82_41_mortar_squad_bg")
                .SetVeterancyRank(2)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());

            // Vehicles
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("m5_halftrack_squad_bg")
                .SetVeterancyRank(1)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("t_34_76_squad_bg")
                .SetVeterancyRank(0)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("t_34_76_squad_bg")
                .SetVeterancyRank(0)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("t_34_76_squad_bg")
                .SetVeterancyRank(2)
                .SetDeploymentPhase(DeploymentPhase.PhaseB)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("su-85_bg")
                .SetVeterancyRank(1)
                .SetDeploymentPhase(DeploymentPhase.PhaseB)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("t_34_85_squad_bg")
                .SetVeterancyRank(4)
                .SetDeploymentPhase(DeploymentPhase.PhaseB)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("t_34_85_squad_bg")
                .SetVeterancyRank(5)
                .SetDeploymentPhase(DeploymentPhase.PhaseC)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("kv-1_bg")
                .SetVeterancyRank(2)
                .SetDeploymentPhase(DeploymentPhase.PhaseB)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("kv-1_bg")
                .SetVeterancyRank(2)
                .SetDeploymentPhase(DeploymentPhase.PhaseC)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("is-2_bg")
                .SetVeterancyRank(3)
                .SetDeploymentPhase(DeploymentPhase.PhaseC)
                .GetAndReset());

            // Commit changes
            companyBuilder.Commit();
            return companyBuilder.Result;
        }

        private static Company CreateGermanCompany() {

            // Create a dummy company
            CompanyBuilder companyBuilder = new CompanyBuilder().NewCompany(Faction.Wehrmacht)
                .ChangeName("69th Panzer Regiment")
                .ChangeTuningMod(tuningMod.Guid);
            UnitBuilder unitBuilder = new UnitBuilder();

            // Basic infantry
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("pioneer_squad_bg").SetDeploymentPhase(DeploymentPhase.PhaseInitial).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("pioneer_squad_bg").SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("pioneer_squad_bg").SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("pioneer_squad_bg").SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("sniper_squad_bg").SetDeploymentPhase(DeploymentPhase.PhaseInitial).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("sniper_squad_bg").SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());

            // Transported Infantry
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("grenadier_squad_bg")
                .SetTransportBlueprint("sdkfz_251_halftrack_squad_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndStay)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("grenadier_squad_bg")
                .SetTransportBlueprint("sdkfz_251_halftrack_squad_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndStay)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("grenadier_squad_bg")
                .SetTransportBlueprint("sdkfz_251_halftrack_squad_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndStay)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("grenadier_squad_bg")
                .SetTransportBlueprint("sdkfz_251_halftrack_squad_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndStay)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());

            companyBuilder.AddUnit(unitBuilder.SetBlueprint("grenadier_squad_bg")
                .SetTransportBlueprint("opel_blitz_transport_squad_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetVeterancyRank(2)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("grenadier_squad_bg")
                .SetTransportBlueprint("opel_blitz_transport_squad_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetVeterancyRank(2)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("grenadier_squad_bg")
                .SetTransportBlueprint("opel_blitz_transport_squad_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetVeterancyRank(2)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("grenadier_squad_bg")
                .SetTransportBlueprint("opel_blitz_transport_squad_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetVeterancyRank(2)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());

            companyBuilder.AddUnit(unitBuilder.SetBlueprint("panzer_grenadier_squad_bg")
                .SetTransportBlueprint("opel_blitz_transport_squad_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetVeterancyRank(3)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("panzer_grenadier_squad_bg")
                .SetTransportBlueprint("opel_blitz_transport_squad_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetVeterancyRank(3)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("panzer_grenadier_squad_bg")
                .SetTransportBlueprint("opel_blitz_transport_squad_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetVeterancyRank(3)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());

            // Support Weapons
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("pak40_75mm_at_gun_squad_bg")
                .SetTransportBlueprint("opel_blitz_transport_squad_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("pak40_75mm_at_gun_squad_bg")
                .SetTransportBlueprint("opel_blitz_transport_squad_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("pak40_75mm_at_gun_squad_bg")
                .SetTransportBlueprint("opel_blitz_transport_squad_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndExit)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("mg42_heavy_machine_gun_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseInitial)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("mg42_heavy_machine_gun_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("mg42_heavy_machine_gun_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("mg42_heavy_machine_gun_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());

            companyBuilder.AddUnit(unitBuilder.SetBlueprint("mortar_team_81mm_bg").SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("mortar_team_81mm_bg").SetDeploymentPhase(DeploymentPhase.PhaseA).GetAndReset());

            // Vehicles
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("sdkfz_234_puma_ost_bg")
                .SetVeterancyRank(1)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("sdkfz_234_puma_ost_bg")
                .SetVeterancyRank(1)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());

            companyBuilder.AddUnit(unitBuilder.SetBlueprint("panther_squad_bg")
                .SetVeterancyRank(1)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());

            companyBuilder.AddUnit(unitBuilder.SetBlueprint("panzer_iv_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("panzer_iv_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("panzer_iv_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("panzer_iv_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());

            companyBuilder.AddUnit(unitBuilder.SetBlueprint("ostwind_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("ostwind_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());

            companyBuilder.AddUnit(unitBuilder.SetBlueprint("tiger_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("tiger_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("tiger_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());

            companyBuilder.AddUnit(unitBuilder.SetBlueprint("brummbar_squad_bg")
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .SetVeterancyRank(3)
                .GetAndReset());

            // Artillery
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("howitzer_105mm_le_fh18_artillery_bg")
                .SetTransportBlueprint("opel_blitz_transport_squad_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndStay)
                .SetVeterancyRank(3)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());
            companyBuilder.AddUnit(unitBuilder.SetBlueprint("howitzer_105mm_le_fh18_artillery_bg")
                .SetTransportBlueprint("opel_blitz_transport_squad_bg")
                .SetDeploymentMethod(DeploymentMethod.DeployAndStay)
                .SetVeterancyRank(3)
                .SetDeploymentPhase(DeploymentPhase.PhaseA)
                .GetAndReset());

            // Commit changes
            companyBuilder.Commit();
            return companyBuilder.Result;
        }

        private static void ParseArguments(string[] args) {

            for (int i = 0; i < args.Length; i++) {

                if (args[i].CompareTo("-recent_analysis") == 0) {
                    recent_analysis = true;
                    if (i + 1 < args.Length) {
                        if (args[i + 1][0] != '-') {
                            recent_file = args[i + 1];
                        }
                    }
                } else if (args[i].CompareTo("-json") == 0) {
                    compile_json = true;
                } else if (args[i].CompareTo("-campaign") == 0) {
                    campaign_compile = true;
                    if (i + 1 < args.Length) {
                        campaign_compile_file = args[i + 1];
                    } else {
                        Console.WriteLine("Cannot compile campaign - none specified!");
                        campaign_compile = false;
                    }
                } else if (args[i].CompareTo("-coh2-extract-maps") == 0) {
                    extract_coh2_maps = true;
                } else if (args[i].CompareTo("-test_companies") == 0) {
                    do_test_companies = true;
                } else if (args[i].CompareTo("-o") == 0) {
                    if (i + 1 < args.Length) {
                        output_path = args[i + 1];
                    } else {
                        Environment.Exit(-1);
                    }
                }

            }

        }

    }

}
