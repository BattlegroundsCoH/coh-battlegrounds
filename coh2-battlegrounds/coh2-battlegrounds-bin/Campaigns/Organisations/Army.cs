using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Battlegrounds.Functional;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Json;
using Battlegrounds.Locale;
using Battlegrounds.Lua;
using Battlegrounds.Util.Lists;

namespace Battlegrounds.Campaigns.Organisations {
    
    /// <summary>
    /// 
    /// </summary>
    public class Army : IJsonObject {

        private record ArmyRegimentalUnitTemplate(string BlueprintName, Range VetRange, double Weight, int Count);
        private record ArmyRegimentalTemplate(string RegimentType, int RegimentUnitSize, int RegimentCompanyCount, 
            ArmyRegimentalUnitTemplate[] WeightedRegimentalUnitTemplates, ArmyRegimentalUnitTemplate[] CountedRegimentalUnitTemplates = null, bool IsArtillery = false);

        private Dictionary<string, List<ArmyRegimentalTemplate>> m_regimentTemplates;

        /// <summary>
        /// Get or initialize the army group name.
        /// </summary>
        public string Name { get; init; }

        public string Description { get; init; }

        public LocaleKey ArmyName { get; set; }

        public List<Division> Divisions { get; init; }

        public Faction Faction { get; init; }

        public CampaignArmyTeam Team { get; }

        public Army(CampaignArmyTeam team) {
            this.Team = team;
            this.Divisions = new List<Division>();
            this.m_regimentTemplates = new Dictionary<string, List<ArmyRegimentalTemplate>>();
        }

        public void NewRegimentTemplate(string templateName, LuaTable templateValue) {

            // Make sure we have a valid name
            int at = templateName.IndexOf('@');
            if (at == -1) {
                Trace.WriteLine($"Regimental template name '{templateName}' is not using standard naming convention and will be skipped.", $"{nameof(Army)}::{nameof(NewRegimentTemplate)}");
                return;
            }

            // Get names
            string templName = templateName[0..at];
            string templType = templateName[(at + 1)..];

            // Register new template if not already in internals
            if (!this.m_regimentTemplates.ContainsKey(templName)) {
                Trace.WriteLine($"Registering new regimental template '{templName}'.", $"{nameof(Army)}::{nameof(NewRegimentTemplate)}");
                this.m_regimentTemplates.Add(templName, new List<ArmyRegimentalTemplate>());
            }

            // Get numerics
            int regSize = (int)(templateValue["size"] as LuaNumber);
            int comCount = (int)(templateValue["companies"] as LuaNumber);
            bool isArty = (templateValue["is_artillery"] as LuaBool)?.IsTrue ?? false;

            // Get unit table and allocate corresponding managed table
            LuaTable units = templateValue["units"] as LuaTable;
            List<ArmyRegimentalUnitTemplate> weightedTemplates = new List<ArmyRegimentalUnitTemplate>();
            List<ArmyRegimentalUnitTemplate> countFixedTemplates = new List<ArmyRegimentalUnitTemplate>();

            // Create units
            units.Pairs((k, v) => {

                int i = (int)((k as LuaNumber) - 1);
                var vt = v as LuaTable;

                string sbp = vt["sbp"].Str();

                var range = (vt["rank_range"] as LuaTable).IfTrue(x => x is not null)
                .ThenDo(x => new Range((int)(x[1] as LuaNumber), (int)(x[2] as LuaNumber)))
                .OrDefaultTo(() => new Range(0, 0));

                if (vt["weight"] is LuaNumber w) {
                    weightedTemplates.Add(new ArmyRegimentalUnitTemplate(sbp, range, w, -1));
                } else if (vt["count"] is LuaNumber c) {
                    weightedTemplates.Add(new ArmyRegimentalUnitTemplate(sbp, range, -1.0, (int)c.AsInteger()));
                } else {
                    Trace.WriteLine($"Unable to add unit {sbp} to regiment template {templName}:{templType}", $"{nameof(Army)}::{nameof(NewRegimentTemplate)}");
                }

            });

            // Add template
            this.m_regimentTemplates[templName].Add(new ArmyRegimentalTemplate(templType, regSize, comCount, weightedTemplates.ToArray(), countFixedTemplates.ToArray(), isArty));

        }

        public void NewDivision(uint divisionID, LocaleKey divisionName, string template, LuaTable regiments) {

            // Make sure it's a valid division
            if (!this.m_regimentTemplates.ContainsKey(template)) {
                Trace.WriteLine(
                    $"Army '{this.ArmyName.LocaleID}' contains no template '{template}' and will therefore skip creating '{divisionName.LocaleID}'", 
                    $"{nameof(Army)}::{nameof(NewDivision)}");
                return;
            }

            // Create division
            Division div = new Division(divisionID, this, divisionName) {
                TemplateName = template
            };

            // Loop table for regiments
            regiments.Pairs((k, v) => {

                // Get template type
                string templType = k.Str();

                if (this.m_regimentTemplates[template].FirstOrDefault(x => x.RegimentType == templType) is ArmyRegimentalTemplate regimentalTemplate) {

                    // Get regimental names
                    string[] regimentNames = (v as LuaTable).ToArray<LuaString>().Select(x => x.Str()).ToArray();

                    // Add all regiments (if it was possible to generate regiment)
                    regimentNames.ForEach(x => GenerateRegiment(x, div, regimentalTemplate).IfTrue(x => x is not null).Then(x => div.Regiments.Add(x)));

                } else {
                    Trace.WriteLine(
                        $"Army '{this.ArmyName.LocaleID}' contains no template sub-type '{templType}' in {template} and will therefore skip creating '{divisionName.LocaleID}'",
                        $"{nameof(Army)}::{nameof(NewDivision)}");
                    return;
                }

            });

            // Add division
            this.Divisions.Add(div);

        }

        private static Regiment GenerateRegiment(string regimentName, Division division, ArmyRegimentalTemplate regimentalTemplate) {

            // Create regiment
            var nameKey = new LocaleKey(regimentName, division.Name.LocaleSource);
            Regiment regiment = new Regiment(division, nameKey, regimentalTemplate.RegimentType, regimentalTemplate.IsArtillery, regimentalTemplate.RegimentCompanyCount);

            // Create companies
            for (int i = 0; i < regimentalTemplate.RegimentCompanyCount; i++) {

                // Handle squads
                List<Squad> squadss = new List<Squad>();

                // Add all
                if (regimentalTemplate.CountedRegimentalUnitTemplates is not null) {
                    foreach (var counted in regimentalTemplate.CountedRegimentalUnitTemplates) {
                        for (int j = 0; j < counted.Count; j++) {
                            squadss.Add(CreateSquad(counted));
                        }
                    }
                }

                // Set remaining
                int remaining = (regimentalTemplate.RegimentUnitSize / regimentalTemplate.RegimentCompanyCount) - squadss.Count;

                // Get weighted list
                var weighted = new WeightedList<ArmyRegimentalUnitTemplate>(regimentalTemplate.WeightedRegimentalUnitTemplates, x => x.Weight);

                // Loop through and pick
                for (int j = 0; j < remaining; j++) {
                    squadss.Add(CreateSquad(weighted.Pick(BattlegroundsInstance.RNG)));
                }
                /*
                
                This code will print the distribution of squads, use for testing purposes!

                // Get counts in pairs
                var bps = squadss.Select(x => x.SBP.Name).Distinct().OrderBy(x => x).Select(x => (x, squadss.Count(y => y.SBP.Name == x)));

                // Log counts
                Trace.WriteLine($"Generated regiment: {string.Join(", ", bps.Select(x => $"{x.x}[{x.Item2}]"))}", $"{nameof(Army)}::{nameof(GenerateRegiment)}");
                */

                // Create company
                regiment.CreateCompany(i, squadss);

            }

            return regiment;

        }

        private static Squad CreateSquad(ArmyRegimentalUnitTemplate unitTemplate) {
            UnitBuilder ub = new UnitBuilder();
            ub.SetModGUID(BattlegroundsInstance.BattleGroundsTuningMod.Guid);
            ub.SetBlueprint(unitTemplate.BlueprintName);
            return ub.Build(0);
        }

        public void DeployDivision(uint divisionID, List<string> deployLocations, CampaignMap map) {
            if (this.Divisions.FirstOrDefault(x => x.DivisionUid == divisionID) is Division div) {
                
                // Create formation from division
                Formation form = new Formation();
                form.FromDivision(div);
                
                // Determine method to spawn (just spawn or split to fit)
                if (deployLocations.Count == 1) {
                    map.SpawnFormationAt(deployLocations[0], form);
                } else {
                    if (form.CanSplit) { // Make sure we can split
                        Formation[] split = form.Split(deployLocations.Count);
                        for (int i = 0; i < deployLocations.Count; i++) {
                            map.SpawnFormationAt(deployLocations[i], split[i]);
                        }
                    } else {
                        Trace.WriteLine($"Army '{this.ArmyName.LocaleID}' contains deploy division with ID {div.Name.LocaleID}, as there are too few regiments to distribute.", $"{nameof(Army)}::{nameof(DeployDivision)}");
                    }
                }

                // Log that we're deploying some unit
                Trace.WriteLine($"Army '{this.ArmyName.LocaleID}' deploying {div.Name.LocaleID} at {string.Join(", ", deployLocations)}", $"{nameof(Army)}::{nameof(DeployDivision)}");

            } else {
                Trace.WriteLine($"Army '{this.ArmyName.LocaleID}' contains no division with ID {divisionID}", $"{nameof(Army)}::{nameof(DeployDivision)}");
            }            
        }

        public string ToJsonReference() => this.ArmyName.LocaleID;

    }

}
