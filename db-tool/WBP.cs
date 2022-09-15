using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Xml;

namespace CoH2XML2JSON {
    
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WeaponCategory {
        undefined,
        ballistic,
        explosive,
        flame,
        smallarms,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum WeaponSmallArmsType {
        invalid,
        heavymachinegun,
        lightmachinegun,
        submachinegun,
        pistol,
        rifle
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum WeaponBalisticType {
        invalid,
        antitankgun,
        tankgun,
        infantryatgun,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum WeaponExplosiveType {
        invalid,
        grenade,
        artillery,
        mine,
        mortar
    }

    public class WBP : BP {

        public override string ModGUID { get; }

        public override ulong PBGID { get; }

        public override string Name { get; }

        public UI Display { get; }

        [DefaultValue(0.0f)]
        public float Range { get; }

        [DefaultValue(0.0f)]
        public float Damage { get; }

        public int MagazineSize { get; }

        public float FireRate { get; }

        public WeaponCategory Category { get; }

        public WeaponSmallArmsType SmallArmsType { get; }

        public WeaponBalisticType BalisticType { get; }

        public WeaponExplosiveType ExplosiveType { get; }

        public string CallbackType { get; }

        public WBP(XmlDocument xmlDocument, string guid, string name, string filepath) {

            // Set the name
            this.Name = name;

            // Set mod GUID
            this.ModGUID = string.IsNullOrEmpty(guid) ? null : guid;

            // Load pbgid
            this.PBGID = ulong.Parse(xmlDocument["instance"]["uniqueid"].GetAttribute("value"));

            // Get range
            this.Range = Program.GetFloat((xmlDocument.SelectSingleNode("//group[@name='range']") as XmlElement).FindSubnode("float", "max")?.GetAttribute("value") ?? "0");

            // Get damage
            this.Damage = Program.GetFloat((xmlDocument.FirstChild as XmlElement).FindSubnode("group", "damage").FindSubnode("float", "max")?.GetAttribute("value") ?? "0");

            // Get additional reload data
            XmlElement reloadData = xmlDocument.SelectSingleNode("//group[@name='reload']") as XmlElement;
            XmlElement reloadFreqData = reloadData.FindSubnode("group", "frequency");
            XmlElement burstData = xmlDocument.SelectSingleNode("//group[@name='burst']") as XmlElement;
            XmlElement burstRateOfFireData = burstData.FindSubnode("group", "rate_of_fire");

            // Get the min number of shots before reloading
            int reloadAfterShots = (int)float.Parse(reloadFreqData.FindSubnode("float", "min").GetAttribute("value"));
            if (reloadAfterShots < 1) {
                reloadAfterShots = 1;
            }

            // Get if can burst
            bool canBurst = bool.Parse(burstData.FindSubnode("bool", "can_burst").GetAttribute("value"));

            // Get burst data
            float burstMin = Program.GetFloat(burstRateOfFireData.FindSubnode("float", "min").GetAttribute("value"));
            float burstMax = Program.GetFloat(burstRateOfFireData.FindSubnode("float", "max").GetAttribute("value"));
            float rate_of_fire = (burstMin + burstMax) / 2.0f;

            // Set properties
            this.FireRate = canBurst ? rate_of_fire : 1;
            this.MagazineSize = canBurst ? (int)(reloadAfterShots * rate_of_fire) : reloadAfterShots;

            // Set types
            this.Category = IfContains(filepath, x => x switch {
                "small_arms" => WeaponCategory.smallarms,
                "flame_throwers" => WeaponCategory.flame,
                "explosive_weapons" => WeaponCategory.explosive,
                "ballistic_weapon" => WeaponCategory.ballistic,
                _ => WeaponCategory.undefined
            });
            this.SmallArmsType = this.Category is WeaponCategory.smallarms ? IfContains(filepath, x => x switch {
                "rifle" => WeaponSmallArmsType.rifle,
                "pistol" => WeaponSmallArmsType.pistol,
                "sub_machine_gun" => WeaponSmallArmsType.submachinegun,
                "light_machine_gun" => WeaponSmallArmsType.lightmachinegun,
                "heavy_machine_gun" => WeaponSmallArmsType.heavymachinegun,
                _ => WeaponSmallArmsType.invalid
            }) : WeaponSmallArmsType.invalid;
            this.BalisticType = this.Category is WeaponCategory.ballistic ? IfContains(filepath, x => x switch {
                "anti_tank_gun" => WeaponBalisticType.antitankgun,
                "infantry_anti_tank_weapon" => WeaponBalisticType.infantryatgun,
                "tank_gun" => WeaponBalisticType.tankgun,
                _ => WeaponBalisticType.invalid
            }) : WeaponBalisticType.invalid;
            this.ExplosiveType = this.Category is WeaponCategory.explosive ? IfContains(filepath, x => x switch {
                "mine" => WeaponExplosiveType.mine,
                "mortar" => WeaponExplosiveType.mortar,
                "grenade" => WeaponExplosiveType.grenade,
                "light_artillery" or "heavy_artillery" => WeaponExplosiveType.artillery,
                _ => WeaponExplosiveType.invalid
            }) : WeaponExplosiveType.invalid;

            // Get callback type
            XmlElement fireData = xmlDocument.SelectSingleNode("//group[@name='fire']") as XmlElement;
            XmlElement onFireActions = fireData.FindSubnode("list", "on_fire_actions");
            foreach (XmlElement action in onFireActions) {
                if (action.Name == "template_reference" && action.GetAttribute("value") == "action\\scar_function_call") {
                    if (action.FindSubnode("string", "function_name") is XmlElement func && func.GetAttribute("value").StartsWith("ScarEvent_")) {
                        this.CallbackType = func.GetAttribute("value");
                    }
                }
            }

        }

        private static T IfContains<T>(string path, Func<string, T> typeMap) where T : Enum {
            string[] check = path.Split('\\');
            for (int i = 0;  i < check.Length; i++) {
                T val = typeMap(check[i]);
                if (val.CompareTo(default(T)) != 0) {
                    return val;
                }
            }
            return typeMap(path);
        }

    }

}
