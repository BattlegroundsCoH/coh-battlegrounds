using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

        [DefaultValue(0)]
        public int SlotPickupCapacity { get; }

        [DefaultValue(false)]
        public bool CanPickupItems { get; }

        [DefaultValue(0)]
        public int UpgradeCapacity { get; }

        [DefaultValue(null)]
        public string[] Upgrades { get; }

        [DefaultValue(null)]
        public string[] AppliedUpgrades { get; }

        public SBP(XmlDocument xmlDocument, string guid, string name, List<EBP> entities) {

            // Set the name
            this.Name = name;

            // Set mod GUID
            this.ModGUID = string.IsNullOrEmpty(guid) ? null : guid;

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
                List<EBP> tmpEbpCollect = new();
                foreach (XmlElement loadout_data in nodes) {
                    int num = int.Parse(loadout_data.FindSubnode("float", "num").GetAttribute("value"));
                    string entity = loadout_data.FindSubnode("instance_reference", "type").GetAttribute("value");
                    EBP ebp = entities.FirstOrDefault(x => entity.EndsWith(x.Name));
                    tmpEbpCollect.Add(ebp);
                    Cost[] dups = new Cost[num];
                    Array.Fill(dups, ebp?.Cost ?? new Cost(0,0,0,0));
                    costList.AddRange(dups);
                    squadLoadoutData.Add(new(ebp?.Name ?? Path.GetFileNameWithoutExtension(entity), num));
                }
                this.SquadCost = new(costList.ToArray());
                if (this.SquadCost.IsNull) {
                    this.SquadCost = null;
                }
                this.Entities = squadLoadoutData.ToArray();
                var temp = squadLoadout.GetValue("//float [@name='squad_female_chance']");
                this.FemaleChance = Program.GetFloat(squadLoadout.GetValue("//float [@name='squad_female_chance']")) / 10.0f;
                this.HasCrew = tmpEbpCollect.Any(x => x?.Drivers?.Length > 0);
            }

            // Load squad abilities
            if (xmlDocument.SelectSingleNode(@"//template_reference[@name='squadexts'] [@value='sbpextensions\squad_ability_ext']") is XmlElement squadAbilities) {
                var nodes = squadAbilities.SelectSubnodes("instance_reference", "ability");
                List<string> abps = new();
                foreach (XmlNode ability in nodes) {
                    abps.Add(Path.GetFileNameWithoutExtension(ability.Attributes["value"].Value));
                }
                if (abps.Count > 0) {
                    this.Abilities = abps.ToArray();
                }
            }

            // Load squad veterancy
            if (xmlDocument.SelectSingleNode(@"//template_reference[@name='squadexts'] [@value='sbpextensions\squad_veterancy_ext']") is XmlElement squadVet) {
                var ranks = squadVet.SelectNodes("//group[@name='veterancy_rank']");
                var ranks_data = new List<VeterancyLevel>();
                foreach (XmlElement rank in ranks) {
                    ranks_data.Add(new(
                        rank.FindSubnode("locstring", "screen_name").GetAttribute("value"),
                        (float)double.Parse(rank.FindSubnode("float", "experience_value").GetAttribute("value"))
                        ));
                }
                this.Veterancy = ranks_data.ToArray();
            }

            // Load pickup data
            if (xmlDocument.SelectSingleNode(@"//template_reference[@name='squadexts'] [@value='sbpextensions\squad_item_slot_ext']") is XmlElement squadItmes) {
                this.CanPickupItems = squadItmes.FindSubnode("bool", "can_pick_up").GetAttribute("value") == bool.TrueString;
                this.SlotPickupCapacity = int.Parse(squadItmes.FindSubnode("float", "num_slots").GetAttribute("value"));
            }

            // Determine if syncweapon (has team_weapon extension)
            this.IsSyncWeapon = xmlDocument.SelectSingleNode(@"//template_reference[@name='squadexts'] [@value='sbpextensions\squad_team_weapon_ext']") is not null;

            // Load squad upgrades
            if (xmlDocument.SelectSingleNode(@"//template_reference[@name='squadexts'] [@value='sbpextensions\squad_upgrade_ext']") is XmlElement squadUpgrades) {
                var nodes = squadUpgrades.SelectSubnodes("instance_reference", "upgrade");
                List<string> ubps = new();
                foreach (XmlNode upgrade in nodes) {
                    ubps.Add(Path.GetFileNameWithoutExtension(upgrade.Attributes["value"].Value));
                }
                if (ubps.Count > 0) {
                    this.Upgrades = ubps.ToArray();
                }
                this.UpgradeCapacity = (int)float.Parse(squadUpgrades.FindSubnode("float", "number_of_slots").GetAttribute("value"));
            }

            // Load squad pre-applied upgrades
            if (xmlDocument.SelectSingleNode(@"//template_reference[@name='squadexts'] [@value='sbpextensions\squad_upgrade_apply_ext']") is XmlElement squadAppliedUpgrades) {
                var nodes = squadAppliedUpgrades.SelectSubnodes("instance_reference", "upgrade");
                List<string> ubps = new();
                foreach (XmlNode upgrade in nodes) {
                    ubps.Add(Path.GetFileNameWithoutExtension(upgrade.Attributes["value"].Value));
                }
                if (ubps.Count > 0) {
                    this.AppliedUpgrades = ubps.ToArray();
                }
            }

        }

    }

}
