using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml;

namespace CoH2XML2JSON {

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RequirementType {
        not_supported,
        required_none,
        required_all_in_list,
        required_any_in_list,
        required_critical,
        required_entity_upgrade,
        required_health,
        required_in_cover,
        required_in_state,
        required_in_territory,
        required_interactive_stage,
        required_near_entity,
        required_not,
        required_number_of_squad_members,
        required_ownership,
        required_player_upgrade,
        required_race,
        required_requirement,
        required_resource,
        required_slot_item,
        required_slot_item_size,
        required_squad_upgrade,
        required_squad_veterancy,
        required_team_upgrade,
        required_unit_type
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RequirementReason {
        unknown,
        display,
        usage,
        usage_and_display
    }

    public class Requirement {

        public RequirementType RequirementType { get; }

        public RequirementReason RequirementReason { get; }

        public string UIText { get; }

        public Dictionary<string, object> RequirementProperties { get; }

        private static readonly (string, string)[] name_skip = { ("enum", "reason"), ("locstring", "ui_name") };

        public Requirement(XmlElement xmlElement) {

            // Alloc properties
            this.RequirementProperties = new();

            // Get requirement type
            string reqtype = Path.GetFileNameWithoutExtension(xmlElement.GetAttribute("value"));
            this.RequirementType = GetRequirementType(reqtype);

            // Get requirement reason
            this.RequirementReason = xmlElement.FindSubnode("enum", "reason").GetAttribute("value").ToLowerInvariant() switch {
                "usage" => RequirementReason.usage,
                "display" => RequirementReason.display,
                "usage_and_display" => RequirementReason.usage_and_display,
                _ => RequirementReason.unknown
            };

            // Get UI Display text
            this.UIText = UI.GetStr(xmlElement, "locstring", "ui_name");

            // If not support, bail here
            if (this.RequirementType is RequirementType.not_supported) {
                return;
            }

            // Loop over properties
            foreach (XmlElement sub in xmlElement) {
                (string, string) subType = (sub.Name, sub.GetAttribute("name"));
                if (!name_skip.Contains(subType)) {                    
                    this.RequirementProperties[subType.Item2] = GetObj(sub, subType.Item1, subType.Item2);
                }
            }

        }

        private static object GetObj(XmlElement e, string type, string name) => type switch {
            "enum" or "string" => e.GetAttribute("value"),
            "instance_reference" => Path.GetFileNameWithoutExtension(e.GetAttribute("value")),
            "locstring" => UI.GetStr(e),
            "bool" => e.GetAttribute("value") == bool.TrueString,
            "float" => GetFloat(e.GetAttribute("value")),
            "list" => name is "requirement_table" or "requirements" ? GetRequirements(e) : GetList(e),
            "group" => GetGroup(e),
            _ => null
        };

        private static float GetFloat(string val) {
            if (float.TryParse(val, out float fl)) {
                return fl;
            } else {
                return float.NaN;
            }
        }

        private static List<object> GetList(XmlElement ls) {
            List<object> objs = new();
            foreach (XmlElement sub in ls) {
                objs.Add(GetObj(sub, sub.Name, sub.GetAttribute("name")));
            }
            return objs;
        }

        private static Dictionary<string, object> GetGroup(XmlElement ls) {
            Dictionary<string, object> objs = new();
            foreach (XmlElement sub in ls) {
                objs[sub.GetAttribute("name")] = GetObj(sub, sub.Name, sub.GetAttribute("name"));
            }
            return objs;
        }

        private static RequirementType GetRequirementType(string typeStr) {
            if (Enum.TryParse(typeStr, out RequirementType t)) {
                return t;
            }
            return RequirementType.not_supported;
        }

        public static Requirement[] GetRequirements(XmlElement xmlExtElement) {
            if (xmlExtElement is null) {
                return null;
            }
            var requirementNodes = xmlExtElement.SelectSubnodes("template_reference", "required", false);
            if (requirementNodes.Count > 0) {
                List<Requirement> reqs = new();
                foreach (var element in requirementNodes) {
                    reqs.Add(new(element));
                }
                return reqs.ToArray();
            }
            return null;
        }

    }

}
