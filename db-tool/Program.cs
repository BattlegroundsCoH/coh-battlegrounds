using System;
using System.Xml;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;

namespace CoH2XML2JSON {
    
    public class Program {

        static readonly JsonSerializerOptions serializerOptions = new() { 
            WriteIndented = true, 
            IgnoreReadOnlyFields = false,
            IgnoreReadOnlyProperties = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
        };

        static string dirPath;
        static string instancesPath;
        static string modguid;
        static Dictionary<string, string> slotItemSymbols = new Dictionary<string, string>();
        static List<EBP> entities = new();

        public static readonly string[] racebps = new string[] {
            "racebps\\soviet",
            "racebps\\aef",
            "racebps\\british",
            "racebps\\german",
            "racebps\\west_german",
        };

        private static string GetFactionFromPath(string path) {
            string army = path.Substring(path.IndexOf("races") + 6, path.Length - path.IndexOf("races") - 6).Split("\\")[0];
            if (army == "soviets") {
                army = "soviet";
            } else if (army == "brits") {
                army = "british";
            }
            return army;
        }

        private static void CreateSbpsDatabase() {

            string fileName = dirPath + @"\sbps_database.json";

            try {
                if (File.Exists(fileName)) {
                    File.Delete(fileName);
                }

                if (!Directory.Exists(instancesPath + @"\sbps")) {
                    Console.WriteLine("ERROR: \"sbps\" folder not found!");
                } else {
                    int filesToProcces = Directory.GetFiles(instancesPath + @"\sbps", "*.xml", SearchOption.AllDirectories).Length;
                    int proccessedFiles = 0;
                    List<SBP> sbps = new();

                    foreach (string path in Directory.GetFiles(instancesPath + @"\sbps", "*xml", SearchOption.AllDirectories)) {

                        Console.WriteLine(Path.GetFileNameWithoutExtension(path));

                        XmlDocument document = new XmlDocument();
                        document.Load(path);

                        string name = path[(path.LastIndexOf(@"\") + 1)..^4];
                        var sbp = new SBP(document, modguid, name, entities) { Army = GetFactionFromPath(path) };
                        string sbpsJson = JsonSerializer.Serialize(sbp, serializerOptions);

                        proccessedFiles++;
                        sbps.Add(sbp);

                    }

                    File.WriteAllText(fileName, JsonSerializer.Serialize(sbps.ToArray(), serializerOptions));

                    Console.WriteLine("Sbps database created with " + proccessedFiles.ToString() + "/" + filesToProcces.ToString() + " items added!");
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
        }

        private static void CreateEbpsDatabase() {
            string fileName = dirPath + @"\ebps_database.json";

            try {
                if (File.Exists(fileName)) {
                    File.Delete(fileName);
                }

                if (!Directory.Exists(instancesPath + @"\ebps")) {
                    Console.WriteLine("ERROR: \"ebps\" folder not found!");
                } else {
                    int filesToProcces = Directory.GetFiles(instancesPath + @"\ebps\races", "*.xml", SearchOption.AllDirectories).Length;
                    int proccessedFiles = 0;

                    foreach (string path in Directory.GetFiles(instancesPath + @"\ebps\races", "*xml", SearchOption.AllDirectories)) {
                        string name = path[(path.LastIndexOf(@"\") + 1)..^4];
                        Console.WriteLine(name);
                        XmlDocument document = new XmlDocument();
                        document.Load(path);
                        try {
                            var ebp = new EBP(document, modguid, name) { Army = GetFactionFromPath(path) };
                            entities.Add(ebp);
                        } catch (Exception) {
                            Console.WriteLine("Failed to parse entity: " + name);
                        }
                        proccessedFiles++;
                    }

                    File.WriteAllText(fileName, JsonSerializer.Serialize(entities.ToArray()));

                    Console.WriteLine("Ebps database created with " + proccessedFiles.ToString() + "/" + filesToProcces.ToString() + " items added!");
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
        }

        private static void CreateCriticalDatabase() {
            string fileName = dirPath + @"\critical_database.json";

            try {
                if (File.Exists(fileName)) {
                    File.Delete(fileName);
                }

                if (!Directory.Exists(instancesPath + @"\critical")) {
                    Console.WriteLine("ERROR: \"critical\" folder not found!");
                } else {
                    int filesToProcces = Directory.GetFiles(instancesPath + @"\critical", "*.xml", SearchOption.AllDirectories).Length;
                    int proccessedFiles = 0;

                    using (FileStream fs = File.Create(fileName)) {
                        using (StreamWriter sw = new StreamWriter(fs)) {
                            sw.WriteLine("[");
                            foreach (string path in Directory.GetFiles(instancesPath + @"\critical", "*xml", SearchOption.AllDirectories)) {
                                XmlDocument document = new XmlDocument();
                                document.Load(path);
                                /*
                                string jsdbType = "Battlegrounds.Game.Database.CriticalBlueprint";

                                XmlElement e_pbgid = document["instance"]["uniqueid"];
                                string pbgid = e_pbgid.GetAttribute("value");

                                string name = path[(path.LastIndexOf(@"\") + 1)..];
                                name = name.Remove(name.Length - 4);

                                string localeName = "";
                                string localeDescription = "";
                                string icon = "";

                                if (document.SelectSingleNode(@"//template_reference[@name='ui_info'] [@value='tables\ui_info']") != null) {
                                    XmlElement e_localeName = document.SelectSingleNode("//locstring[@name='screen_name']") as XmlElement;
                                    localeName = e_localeName.GetAttribute("value");

                                    XmlElement e_localeDescription = document.SelectSingleNode("//locstring[@name='help_text']") as XmlElement;
                                    localeDescription = e_localeDescription.GetAttribute("value");

                                    XmlElement e_icon = document.SelectSingleNode("//icon[@name='icon_name']") as XmlElement;
                                    icon = e_icon.GetAttribute("value");
                                }

                                dynamic critical = new JObject();
                                critical.jsdbtype = jsdbType;
                                critical.PBGID = pbgid;
                                critical.ModGUID = modguid;
                                critical.Name = name;
                                critical.LocaleName = localeName;
                                critical.LocaleDescription = localeDescription;
                                critical.Icon = icon;

                                string criticalJson = JsonConvert.SerializeObject(critical, Newtonsoft.Json.Formatting.Indented);
                                */
                                proccessedFiles++;

                                if (proccessedFiles < filesToProcces) {
                                    //sw.WriteLine(criticalJson + ",");
                                } else {
                                    //sw.WriteLine(criticalJson);
                                }

                            }
                            sw.WriteLine("]");
                        }
                    }

                    Console.WriteLine("Critical database created with " + proccessedFiles.ToString() + "/" + filesToProcces.ToString() + " items added!");
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
        }

        private static void CreateSlotItemDatabase() {
            string fileName = dirPath + @"\slot_item_database.json";

            try {
                if (File.Exists(fileName)) {
                    File.Delete(fileName);
                }

                if (!Directory.Exists(instancesPath + @"\slot_item")) {
                    Console.WriteLine("ERROR: \"slot_item\" folder not found!");
                } else {
                    int filesToProcces = Directory.GetFiles(instancesPath + @"\slot_item", "*.xml", SearchOption.AllDirectories).Length;
                    int proccessedFiles = 0;
                    List<SlotItem> slotItems = new();
                    foreach (string path in Directory.GetFiles(instancesPath + @"\slot_item", "*xml", SearchOption.AllDirectories)) {
                        string name = path[(path.LastIndexOf(@"\") + 1)..^4];
                        Console.WriteLine(name);
                        XmlDocument document = new XmlDocument();
                        document.Load(path);
                        try {
                            var item = new SlotItem(document, modguid, name) { };
                            slotItems.Add(item);
                            string ebpsJson = JsonSerializer.Serialize(item, serializerOptions);
                        } catch (Exception) {
                            Console.WriteLine("Failed to parse entity: " + name);
                        }
                        proccessedFiles++;
                    }
                    File.WriteAllText(fileName, JsonSerializer.Serialize(slotItems.ToArray(), serializerOptions));
                    Console.WriteLine("Slot item database created with " + proccessedFiles.ToString() + "/" + filesToProcces.ToString() + " items added!");
                }

            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
        }

        private static void CreateUpgradeDatabase() {
            string fileName = dirPath + @"\upgrade_database.json";

            try {
                if (File.Exists(fileName)) {
                    File.Delete(fileName);
                }

                if (!Directory.Exists(instancesPath + @"\upgrade")) {
                    Console.WriteLine("ERROR: \"upgrade\" folder not found!");
                } else {
                    int filesToProcces = Directory.GetFiles(instancesPath + @"\upgrade", "*.xml", SearchOption.AllDirectories).Length;
                    int proccessedFiles = 0;

                    using (FileStream fs = File.Create(fileName)) {
                        using (StreamWriter sw = new StreamWriter(fs)) {
                            sw.WriteLine("[");
                            foreach (string path in Directory.GetFiles(instancesPath + @"\upgrade", "*xml", SearchOption.AllDirectories)) {
                                XmlDocument document = new XmlDocument();
                                document.Load(path);
                                /*
                                string jsdbType = "Battlegrounds.Game.Database.UpgradeBlueprint";

                                XmlElement e_pbgid = document["instance"]["uniqueid"];
                                string pbgid = e_pbgid.GetAttribute("value");

                                string name = path[(path.LastIndexOf(@"\") + 1)..];
                                name = name.Remove(name.Length - 4);

                                string localeName = "";
                                string localeDescription = "";
                                string icon = "";

                                if (document.SelectSingleNode(@"//group[@name='ui_info']") != null) {
                                    XmlElement e_localeName = document.SelectSingleNode("//locstring[@name='screen_name']") as XmlElement;
                                    localeName = e_localeName.GetAttribute("value");

                                    XmlElement e_localeDescription = document.SelectSingleNode("//locstring[@name='help_text']") as XmlElement;
                                    localeDescription = e_localeDescription.GetAttribute("value");

                                    XmlElement e_icon = document.SelectSingleNode("//icon[@name='icon_name']") as XmlElement;
                                    icon = e_icon.GetAttribute("value");
                                }

                                string costJsdbType = "Battlegrounds.Game.Gameplay.Cost";

                                string costManpower = "0";
                                string costMunition = "0";
                                string costFuel = "0";

                                if (document.SelectSingleNode(@"//group[@name='time_cost']") != null) {
                                    XmlElement e_manpower = document.SelectSingleNode("//float[@name='manpower']") as XmlElement;
                                    costManpower = e_manpower.GetAttribute("value");

                                    XmlElement e_munition = document.SelectSingleNode("//float[@name='munition']") as XmlElement;
                                    costMunition = e_munition.GetAttribute("value");

                                    XmlElement e_fuel = document.SelectSingleNode("//float[@name='fuel']") as XmlElement;
                                    costFuel = e_fuel.GetAttribute("value");
                                }

                                List<string> slotItems = new List<string>();
                                XmlNodeList e_slotItems = document.SelectNodes("//instance_reference[@name='slot_item']");
                                foreach (XmlNode slotItem in e_slotItems) {
                                    slotItems.Add(Path.GetFileNameWithoutExtension(slotItem.Attributes["value"].Value));
                                }
                                IEnumerable<string> uniqueSlotItems = slotItems.Distinct<string>();

                                dynamic upgrade = new JObject();
                                upgrade.jsdbtype = jsdbType;
                                upgrade.PBGID = pbgid;
                                upgrade.ModGUID = modguid;
                                upgrade.Name = name;
                                upgrade.LocaleName = localeName;
                                upgrade.LocaleDescription = localeDescription;
                                upgrade.Icon = icon;
                                upgrade.Cost = new JObject();
                                upgrade.Cost.jsdbtype = costJsdbType;
                                upgrade.Cost.Manpower = costManpower;
                                upgrade.Cost.Munition = costMunition;
                                upgrade.Cost.Fuel = costFuel;
                                upgrade.SlotItems = new JArray(uniqueSlotItems);

                                if (slotItems.FirstOrDefault() is string item) {
                                    if (slotItemSymbols.TryGetValue(item, out string itemSymbol)) {
                                        upgrade.Symbol = itemSymbol;
                                    }
                                }

                                string upgradeJson = JsonConvert.SerializeObject(upgrade, Newtonsoft.Json.Formatting.Indented);

                                proccessedFiles++;

                                if (proccessedFiles < filesToProcces) {
                                    sw.WriteLine(upgradeJson + ",");
                                } else {
                                    sw.WriteLine(upgradeJson);
                                }
                                */
                            }
                            sw.WriteLine("]");
                        }
                    }

                    Console.WriteLine("Upgrade database created with " + proccessedFiles.ToString() + "/" + filesToProcces.ToString() + " items added!");
                }

            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
        }

        private static void CreateAbilityDatabase() {
            string fileName = dirPath + @"\abilities_database.json";

            try {
                if (File.Exists(fileName)) {
                    File.Delete(fileName);
                }

                if (!Directory.Exists(instancesPath + @"\abilities")) {
                    Console.WriteLine("ERROR: \"abilities\" folder not found!");
                } else {
                    int filesToProcces = Directory.GetFiles(instancesPath + @"\abilities", "*.xml", SearchOption.AllDirectories).Length;
                    int proccessedFiles = 0;

                    using (FileStream fs = File.Create(fileName)) {
                        using (StreamWriter sw = new StreamWriter(fs)) {
                            sw.WriteLine("[");
                            foreach (string path in Directory.GetFiles(instancesPath + @"\abilities", "*xml", SearchOption.AllDirectories)) {
                                XmlDocument document = new XmlDocument();
                                document.Load(path);
                                /*
                                string jsdbType = "Battlegrounds.Game.Database.AbilityBlueprint";

                                XmlElement e_pbgid = document["instance"]["uniqueid"];
                                string pbgid = e_pbgid.GetAttribute("value");

                                string name = path[(path.LastIndexOf(@"\") + 1)..];
                                name = name.Remove(name.Length - 4);

                                string localeName = "";
                                string localeDescription = "";
                                string icon = "";

                                if (document.SelectSingleNode(@"//group[@name='ui_info']") != null) {
                                    XmlElement e_localeName = document.SelectSingleNode("//locstring[@name='screen_name']") as XmlElement;
                                    localeName = e_localeName.GetAttribute("value");

                                    XmlElement e_localeDescription = document.SelectSingleNode("//locstring[@name='help_text']") as XmlElement;
                                    localeDescription = e_localeDescription.GetAttribute("value");

                                    XmlElement e_icon = document.SelectSingleNode("//icon[@name='icon_name']") as XmlElement;
                                    icon = e_icon.GetAttribute("value");
                                }

                                string army = path.Substring(path.IndexOf("abilities") + 10, path.Length - path.IndexOf("abilities") - 10).Split("\\")[0];
                                if (army == "soviets") {
                                    army = army.Replace("soviets", "soviet");
                                } else if (army == "brits") {
                                    army = army.Replace("brits", "british");
                                }

                                string costJsdbType = "Battlegrounds.Game.Gameplay.Cost";

                                string costManpower = "0";
                                string costMunition = "0";
                                string costFuel = "0";

                                if (document.SelectSingleNode(@"//group[@name='cost']") != null) {
                                    XmlElement e_manpower = document.SelectSingleNode("//float[@name='manpower']") as XmlElement;
                                    costManpower = e_manpower.GetAttribute("value");

                                    XmlElement e_munition = document.SelectSingleNode("//float[@name='munition']") as XmlElement;
                                    costMunition = e_munition.GetAttribute("value");

                                    XmlElement e_fuel = document.SelectSingleNode("//float[@name='fuel']") as XmlElement;
                                    costFuel = e_fuel.GetAttribute("value");
                                }

                                dynamic ability = new JObject();
                                ability.jsdbtype = jsdbType;
                                ability.PBGID = pbgid;
                                ability.ModGUID = modguid;
                                ability.Name = name;
                                ability.LocaleName = localeName;
                                ability.LocaleDescription = localeDescription;
                                ability.Army = army;
                                ability.Icon = icon;
                                ability.Cost = new JObject();
                                ability.Cost.jsdbtype = costJsdbType;
                                ability.Cost.Manpower = costManpower;
                                ability.Cost.Munition = costMunition;
                                ability.Cost.Fuel = costFuel;

                                string abilityJson = JsonConvert.SerializeObject(ability, Newtonsoft.Json.Formatting.Indented);

                                proccessedFiles++;

                                if (proccessedFiles < filesToProcces) {
                                    sw.WriteLine(abilityJson + ",");
                                } else {
                                    sw.WriteLine(abilityJson);
                                }
                                */
                            }
                            sw.WriteLine("]");
                        }
                    }

                    Console.WriteLine("Abilities database created with " + proccessedFiles.ToString() + "/" + filesToProcces.ToString() + " items added!");
                }

            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }

        }

        public static void Main(string[] args) {

            Console.WriteLine("Set path where you want the files to be created to: ");
            dirPath = Console.ReadLine();

            while (!Directory.Exists(dirPath)) {
                if (string.IsNullOrEmpty(dirPath)) { // Because I'm lazy - this is a quick method to simply use the directory of the .exe
                    Console.WriteLine($"Using: {Environment.CurrentDirectory}");
                    dirPath = Environment.CurrentDirectory;
                    break;
                }
                Console.WriteLine("Invalid path! Try again: ");
                dirPath = Console.ReadLine();
            }

            Console.WriteLine("Set path to your \"instances\" folder: ");
            instancesPath = Console.ReadLine();

            while (!Directory.Exists(instancesPath) && !instancesPath.EndsWith(@"\instances")) {
                Console.WriteLine("Invalid path! Try again: ");
                instancesPath = Console.ReadLine();
            }

            Console.WriteLine("Mod GUID (Leave empty if not desired/available):");
            modguid = Console.ReadLine().Replace("-", "");
            if (modguid.Length != 32) {
                modguid = string.Empty;
            }

            Console.WriteLine();

            CreateAbilityDatabase();
            CreateEbpsDatabase();
            CreateSbpsDatabase();
            CreateCriticalDatabase();
            CreateSlotItemDatabase();
            CreateUpgradeDatabase();

            Console.WriteLine("Created databases - Press any key to exit");
            Console.Read();

        }

    }

}
