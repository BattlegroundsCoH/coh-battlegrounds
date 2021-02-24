using System;
using System.Collections.Generic;
using System.Diagnostics;

using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Json;
using Battlegrounds.Lua;

namespace Battlegrounds.Campaigns.Organisation {
    
    /// <summary>
    /// 
    /// </summary>
    public class Army : IJsonObject {

        private record ArmyRegimentalUnitTemplate(string BlueprintName, Range VetRange, double Weight);
        private record ArmyRegimentalTemplate(string RegimentType, int RegimentUnitSize, int RegimentCompanyCount, ArmyRegimentalUnitTemplate[] RegimentalUnitTemplates, bool IsArtillery = false);

        private Dictionary<string, List<ArmyRegimentalTemplate>> m_regimentTemplates;

        /// <summary>
        /// Get or initialize the army group name.
        /// </summary>
        public string Name { get; init; }

        public string Description { get; init; }

        public string ArmyName { get; set; }

        public Division[] Divisions { get; init; }

        public Faction Faction { get; init; }

        public CampaignArmyTeam Team { get; }

        public Army(CampaignArmyTeam team) {
            this.Team = team;
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
            ArmyRegimentalUnitTemplate[] unitTemplates = new ArmyRegimentalUnitTemplate[units.Size];

            // Create units
            units.Pairs((k, v) => {
                
                int i = (int)((k as LuaNumber) - 1);
                var vt = v as LuaTable;
                
                var range = (vt["rank_range"] as LuaTable).IfTrue(x => x is not null)
                .ThenDo(x => new Range((int)(x[1] as LuaNumber), (int)(x[2] as LuaNumber)))
                .OrDefaultTo(() => new Range(0,0));

                unitTemplates[i] = new ArmyRegimentalUnitTemplate(vt["sbp"].Str(), range, vt["weight"] as LuaNumber);

            });

            // Add template
            this.m_regimentTemplates[templName].Add(new ArmyRegimentalTemplate(templType, regSize, comCount, unitTemplates, isArty));

        }

        public void NewDivision(string divisionName, string template, string deploy, LuaTable regiments) {



        }

        public string ToJsonReference() => throw new NotSupportedException();

    }

}
