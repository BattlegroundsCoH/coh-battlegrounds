using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace CoH2XML2JSON {

    public class SBP : BP {

        public record Loadout(string EBP, int Count);

        public record VeterancyLevel(string ScreenName, float Experience);

        public override string ModGUID { get; }
        
        public override ulong PBGID { get; }

        public override string Name { get; }

        public string Army { get; init; }

        public UI Display { get; }

        public Cost SquadCost { get; }

        public bool HasCrew { get; }

        public bool IsSyncWeapon { get; }

        public Loadout[] Entities { get; }

        public float FemaleChance { get; }

        public string[] Abilities { get; }

        public VeterancyLevel[] Veterancy { get; }

        public string[] Types { get; }

        public SBP(XmlDocument xmlDocument, string guid, string name, List<EBP> entities) {

            // Set the name
            this.Name = name;

            // Set mod GUID
            this.ModGUID = guid;

            // Load pbgid
            this.PBGID = ulong.Parse(xmlDocument["instance"]["uniqueid"].GetAttribute("value"));

            // Load display
            this.Display = new(xmlDocument.SelectSingleNode(@"//template_reference[@name='squadexts'] [@value='sbpextensions\squad_ui_ext']") as XmlElement);

            // Load squad types
            if (xmlDocument.SelectSingleNode(@"//template_reference[@name='squadexts'] [@value='sbpextensions\squad_type_ext']") is XmlNode squadTypeList) {
                var typeList = squadTypeList.SelectSingleNode(@"//list[@name='squad_type_list']");
                var tmpList = new List<string>();
                foreach (XmlNode type in typeList) {
                    tmpList.Add(type.Attributes["value"].Value);
                }
                this.Types = tmpList.ToArray();
            } else {
                this.Types = Array.Empty<string>();
            }

            // Load squad loadout
            if (xmlDocument.SelectSingleNode(@"//template_reference[@name='squadexts'] [@value='sbpextensions\squad_loadout_ext']") is XmlElement squadLoadout) {
                var nodes = squadLoadout.SelectNodes("//group[@name='loadout_data']");
                List<Cost> costList = new();
                List<Loadout> squadLoadoutData = new();
                foreach (XmlElement loadout_data in nodes) {
                    int num = int.Parse(loadout_data.FindSubnode("float", "num").GetAttribute("value"));
                    string entity = loadout_data.FindSubnode("instance_reference", "type").GetAttribute("value");
                    EBP ebp = entities.FirstOrDefault(x => entity.EndsWith(x.Name));
                    Cost[] dups = new Cost[num];
                    Array.Fill(dups, ebp.Cost);
                    costList.AddRange(dups);
                    squadLoadoutData.Add(new(ebp.Name, num));
                }
                this.SquadCost = new(costList.ToArray());
                this.Entities = squadLoadoutData.ToArray();
                this.FemaleChance = float.Parse(squadLoadout.GetValue("//float [@name='squad_female_chance']")) / 10.0f;
            }

            // Load squad abilities
            if (xmlDocument.SelectSingleNode(@"//template_reference[@name='squadexts'] [@value='sbpextensions\squad_ability_ext']") is XmlElement squadAbilities) {
                var nodes = squadAbilities.SelectNodes("//instance_reference[@name='ability']");
                List<string> abps = new();
                foreach (XmlNode ability in nodes) {
                    abps.Add(ability.Attributes["value"].Value);
                }
                if (abps.Count > 0) {
                    this.Abilities = abps.ToArray();
                }
            }

            // Load squad loadout
            if (xmlDocument.SelectSingleNode(@"//template_reference[@name='squadexts'] [@value='sbpextensions\squad_veterancy_ext']") is XmlElement squadVet) {
                var racedata = squadVet.SelectSingleNode("//group[@name='race_data']") as XmlElement;
                var ranks = racedata.SelectNodes("//group[@name='veterancy_rank']");
                var ranks_data = new List<VeterancyLevel>();
                foreach (XmlElement rank in ranks) {
                    ranks_data.Add(new(
                        rank.FindSubnode("locstring", "screen_name").GetAttribute("value"),
                        float.Parse(rank.FindSubnode("float", "experience_value").GetAttribute("value"))
                        ));
                }
                this.Veterancy = ranks_data.ToArray();
            }

            // Determine if syncweapon (has team_weapon extension)
            this.IsSyncWeapon = xmlDocument.SelectSingleNode(@"//template_reference[@name='squadexts'] [@value='sbpextensions\squad_team_weapon_ext']") is not null;

            // Determine if has crew
            this.HasCrew = entities.Any(x => x.Drivers?.Any(y => y.SBP == this.Name) ?? false);

        }

    }

}
