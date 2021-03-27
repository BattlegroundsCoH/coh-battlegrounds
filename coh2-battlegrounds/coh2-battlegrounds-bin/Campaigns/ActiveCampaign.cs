using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

using Battlegrounds.Campaigns.Organisations;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Json;
using Battlegrounds.Locale;
using Battlegrounds.Lua;
using Battlegrounds.Gfx;
using Battlegrounds.Game.Database;

namespace Battlegrounds.Campaigns {

    /// <summary>
    /// 
    /// </summary>
    public enum CampaignArmyTeam {
        TEAM_NEUTRAL,
        TEAM_ALLIES,
        TEAM_AXIS,
    }

    /// <summary>
    /// 
    /// </summary>
    public class ActiveCampaign : IJsonObject {

        private string m_locSourceID;
        private ushort m_unitCampaignID;

        private HashSet<string> m_allowedSummerAtmospheres;
        private HashSet<string> m_allowedWinterAtmospheres;

        public class Player { }

        /// <summary>
        /// 
        /// </summary>
        public CampaignMap PlayMap { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public Army[] Armies { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public List<GfxMap> GfxMaps { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public Localize Locale { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public ActiveCampaignTurnData Turn { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int DifficultyLevel { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public LuaState LuaState { get; }

        private ActiveCampaign() {
            
            // Create state
            this.LuaState = new LuaState("base", "math", "table", "string");
            
            // Register userdata
            this.LuaState.RegisterUserdata(typeof(CampaignMap));
            this.LuaState.RegisterUserdata(typeof(CampaignMapNode));
            this.LuaState.RegisterUserdata(typeof(Formation));

            // Register constants
            this.LuaState._G["TEAM_AXIS"] = LuaMarshal.ToLuaValue(CampaignArmyTeam.TEAM_AXIS);
            this.LuaState._G["TEAM_ALLIES"] = LuaMarshal.ToLuaValue(CampaignArmyTeam.TEAM_ALLIES);
            this.LuaState._G["TEAM_NEUTRAL"] = LuaMarshal.ToLuaValue(CampaignArmyTeam.TEAM_NEUTRAL);
            this.LuaState._G["FILTER_NEVER"] = new LuaNumber(double.MaxValue);
            this.LuaState._G["FILTER_OK"] = new LuaNumber(1.0);

        }

        public Scenario PickScenario(List<CampaignMapNode.NodeMap> maps) {
            if (maps.Count == 1) {
                if (ScenarioList.TryFindScenario(maps[0].MapName, out Scenario singleScenario)) {
                    return singleScenario;
                }
            } else {

            }
            return null;
        }

        public void CreateArmy(int index, ref uint divCount, CampaignPackage.ArmyData army) {
            this.Armies[index] = new Army(army.Army.IsAllied ? CampaignArmyTeam.TEAM_ALLIES : CampaignArmyTeam.TEAM_AXIS) {
                Name = this.Locale.GetString(army.Name),
                Description = this.Locale.GetString(army.Desc),
                Faction = army.Army,
            };
            uint tmp = divCount;
            army.FullArmyData?.Pairs((k, v) => {
                switch (k.Str()) {
                    case "templates":
                        (v as LuaTable).Pairs((k, v) => {
                            this.Armies[index].NewRegimentTemplate(k.Str(), v as LuaTable);
                        });
                        break;
                    case "army":
                        var vt = v as LuaTable;
                        this.Armies[index].ArmyName = new LocaleKey(vt["name"].Str(), this.m_locSourceID);
                        (vt["divisions"] as LuaTable).Pairs((k, v) => {
                            var vt = v as LuaTable;
                            uint divID = tmp;
                            
                            this.Armies[index].NewDivision(divID, 
                                new LocaleKey(k.Str(), this.m_locSourceID), 
                                vt["tmpl"].Str(), 
                                vt["max_move"], 
                                vt["regiments"] as LuaTable, 
                                ref this.m_unitCampaignID);

                            tmp++;
                            if (vt["deploy"] is not LuaNil) {
                                if (vt["deploy"] is LuaTable tableAt) {
                                    this.Armies[index].DeployDivision(divID, new List<string>(tableAt.ToArray().Select(x => x.Str())), this.PlayMap);
                                } else if (vt["deploy"] is LuaString strAt) {
                                    this.Armies[index].DeployDivision(divID, new List<string>() { strAt.Str() }, this.PlayMap);
                                } else {
                                    Trace.WriteLine($"Attempted to spawn '{k.Str()}' using unsupported datatype!", nameof(ActiveCampaign));
                                }
                            }
                        });
                        break;
                    default:
                        Trace.WriteLine($"Unrecognized army entry '{k.Str()}'", nameof(ActiveCampaign));
                        break;
                }
            });
            divCount = tmp;
        }

        public string ToJsonReference() => throw new NotSupportedException();

        public static ActiveCampaign FromPackage(CampaignPackage package, CampaignMode mode, int difficulty, ActiveCampaignStartData createArgs) {

            // Create initial campaign data
            ActiveCampaign campaign = new ActiveCampaign {
                PlayMap = new CampaignMap(package.MapData),
                Armies = new Army[package.CampaignArmies.Length],
                Locale = package.LocaleManager,
                DifficultyLevel = difficulty,
                GfxMaps = package.GfxResources,
                m_locSourceID = package.LocaleSourceID,
                m_unitCampaignID = 0,
            };

            // Create campaign nodes
            campaign.PlayMap.BuildNetwork();

            // Counter to keep track of diviions
            uint divisionCount = 0;

            // Create the armies
            for (int i = 0; i < package.CampaignArmies.Length; i++) {
                campaign.CreateArmy(i, ref divisionCount, package.CampaignArmies[i]);
            }

            // Define start team
            CampaignArmyTeam startTeam = GetArmyTeamFromFaction(package.NormalStartingSide);

            // Create mode-specific data
            if (mode == CampaignMode.Singleplayer) {

                // Determine starting team
                if (createArgs.PlayAs != package.NormalStartingSide) {
                    if (startTeam == CampaignArmyTeam.TEAM_AXIS) {
                        startTeam = CampaignArmyTeam.TEAM_ALLIES;
                    } else {
                        startTeam = CampaignArmyTeam.TEAM_AXIS;
                    }
                }

            }

            // Create turn data
            campaign.Turn = new ActiveCampaignTurnData(startTeam, new[] {
                package.CampaignTurnData.Start,
                package.CampaignTurnData.End
            }, package.CampaignTurnData.TurnLength);
            campaign.Turn.SetWinterDates(new[] { package.CampaignWeatherData.WinterStart, package.CampaignWeatherData.WinterEnd });

            // Set atmospheres
            campaign.m_allowedSummerAtmospheres = package.CampaignWeatherData.SummerAtmospheres;
            campaign.m_allowedWinterAtmospheres = package.CampaignWeatherData.WinterAtmospheres;

            // Set lua stuff
            campaign.LuaState._G["Map"] = new LuaUserObject(campaign.PlayMap);

            // Return campaign
            return campaign;

        }

        public static CampaignArmyTeam GetArmyTeamFromFaction(string faction)
            => Faction.IsAlliedFaction(faction) ? CampaignArmyTeam.TEAM_ALLIES : CampaignArmyTeam.TEAM_AXIS;

    }

}
