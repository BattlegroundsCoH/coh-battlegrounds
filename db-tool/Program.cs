using System;
using System.Xml;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace CoH2XML2JSON
{
    class Program
    {

        static string dirPath;
        static string instancesPath;
        static Dictionary<string, (float, float, float, float)> entityCost = new Dictionary<string, (float, float, float, float)>();

        private static void CreateSbpsDatabase()
        {
            string fileName = dirPath + @"\sbps_database.json";

            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                if (!Directory.Exists(instancesPath + @"\sbps"))
                {
                    Console.WriteLine("ERROR: \"sbps\" folder not found!");
                }
                else
                {
                    int filesToProcces = Directory.GetFiles(instancesPath + @"\sbps", "*.xml", SearchOption.AllDirectories).Length;
                    int proccessedFiles = 0;

                    using (FileStream fs = File.OpenWrite(fileName))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.WriteLine("[");
                            foreach (string path in Directory.GetFiles(instancesPath + @"\sbps", "*xml", SearchOption.AllDirectories))
                            {
                                XmlDocument document = new XmlDocument();
                                document.Load(path);

                                string jsdbType = "Battlegrounds.Game.Database.SquadBlueprint";

                                XmlElement e_pbgid = document["instance"]["uniqueid"];
                                string pbgid = e_pbgid.GetAttribute("value");

                                string name = path.Substring(path.LastIndexOf(@"\") + 1);
                                name = name.Remove(name.Length - 4);

                                string localeName = "";
                                string localeDescription = "";
                                string icon = "";
                                string symbol = "";

                                if (document.SelectSingleNode(@"//template_reference[@name='squadexts'] [@value='sbpextensions\squad_ui_ext']") != null)
                                {
                                    XmlElement e_localeName = document.SelectSingleNode("//locstring[@name='screen_name']") as XmlElement;
                                    localeName = e_localeName.GetAttribute("value");

                                    XmlElement e_localeDescription = document.SelectSingleNode("//locstring[@name='help_text']") as XmlElement;
                                    localeDescription = e_localeDescription.GetAttribute("value");

                                    XmlElement e_icon = document.SelectSingleNode("//icon[@name='icon_name']") as XmlElement;
                                    icon = e_icon.GetAttribute("value");

                                    XmlElement e_symbol = document.SelectSingleNode("//icon[@name='symbol_icon_name']") as XmlElement;
                                    symbol = e_symbol.GetAttribute("value");
                                }

                                string army = path.Substring(path.IndexOf("races") + 6, path.Length - path.IndexOf("races") - 6).Split("\\")[0];
                                if (army == "soviets")
                                {
                                    army = army.Replace("soviets", "soviet");
                                } else if (army == "brits")
                                {
                                    army = army.Replace("brits", "british");
                                }

                                string costJsdbType = "Battlegrounds.Game.Gameplay.Cost";

                                List<string> entities = new List<string>();
                                List<float> numOfEntities = new List<float>();
                                XmlNodeList e_entities = document.SelectNodes("//instance_reference[@name='type']");
                                XmlNodeList e_numOfentities = document.SelectNodes("//float[@name='num']");
                                foreach (XmlNode entity in e_entities)
                                {
                                    string entityName = entity.Attributes["value"].Value;
                                    entities.Add(entityName.Substring(entityName.LastIndexOf(@"\") + 1));
                                }
                                foreach (XmlNode num in e_numOfentities)
                                {
                                    numOfEntities.Add(SafeParse(num.Attributes["value"].Value));
                                }

                                var squad = entities.Zip(numOfEntities, (k, v) => ( k, v ));

                                string costManpower = "0";
                                string costMunition = "0";
                                string costFuel = "0";
                                string costFieldTime = "0";

                                foreach (var s in squad)
                                {
                                    costManpower = ((entityCost[s.Item1].Item1) * s.Item2).ToString();
                                    costMunition = ((entityCost[s.Item1].Item2) * s.Item2).ToString();
                                    costFuel = ((entityCost[s.Item1].Item3) * s.Item2).ToString();
                                    if ((Math.Ceiling(((entityCost[s.Item1].Item4) * s.Item2) * 0.06)) < 15)
                                    {
                                        costFieldTime = "15";
                                    } else
                                    {
                                        costFieldTime = (Math.Ceiling(((entityCost[s.Item1].Item4) * s.Item2) * 0.06)).ToString();
                                    }
                                }

                                dynamic sbps = new JObject();
                                sbps.jsdbtype = jsdbType;
                                sbps.PBGID = pbgid;
                                sbps.Name = name;
                                sbps.LocaleName = localeName;
                                sbps.LocaleDescription = localeDescription;
                                sbps.Army = army;
                                sbps.Icon = icon;
                                sbps.Symbol = symbol;
                                sbps.Cost = new JObject();
                                sbps.Cost.jsdbtype = costJsdbType;
                                sbps.Cost.Manpower = costManpower;
                                sbps.Cost.Munition = costMunition;
                                sbps.Cost.Fuel = costFuel;
                                sbps.Cost.FieldTime = costFieldTime;

                                string sbpsJson = JsonConvert.SerializeObject(sbps, Newtonsoft.Json.Formatting.Indented);

                                proccessedFiles++;

                                if (proccessedFiles < filesToProcces)
                                {
                                    sw.WriteLine(sbpsJson + ",");
                                } else
                                {
                                    sw.WriteLine(sbpsJson);
                                }

                            }
                            sw.WriteLine("]");
                        }
                    }

                    Console.WriteLine("Sbps database created with " + proccessedFiles.ToString() + "/" + filesToProcces.ToString() + " items added!");
                }
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
        }

        private static void CreateEbpsDatabase()
        {
            string fileName = dirPath + @"\ebps_database.json";

            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                if (!Directory.Exists(instancesPath + @"\ebps"))
                {
                    Console.WriteLine("ERROR: \"ebps\" folder not found!");
                }
                else
                {
                    int filesToProcces = Directory.GetFiles(instancesPath + @"\ebps\races", "*.xml", SearchOption.AllDirectories).Length;
                    int proccessedFiles = 0;

                    using (FileStream fs = File.Create(fileName))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.WriteLine("[");
                            foreach (string path in Directory.GetFiles(instancesPath + @"\ebps\races", "*xml", SearchOption.AllDirectories))
                            {
                                XmlDocument document = new XmlDocument();
                                document.Load(path);

                                string jsdbType = "Battlegrounds.Game.Database.EntityBlueprint";

                                XmlElement e_pbgid = document["instance"]["uniqueid"];
                                string pbgid = e_pbgid.GetAttribute("value");

                                string name = path.Substring(path.LastIndexOf(@"\") + 1);
                                name = name.Remove(name.Length - 4);

                                string localeName = "";
                                string localeDescription = "";
                                string icon = "";
                                string symbol = "";

                                if (document.SelectSingleNode(@"//template_reference[@name='exts'] [@value='ebpextensions\ui_ext']") != null)
                                {
                                    XmlElement e_localeName = document.SelectSingleNode("//locstring[@name='screen_name']") as XmlElement;
                                    localeName = e_localeName.GetAttribute("value");

                                    XmlElement e_localeDescription = document.SelectSingleNode("//locstring[@name='help_text']") as XmlElement;
                                    localeDescription = e_localeDescription.GetAttribute("value");

                                    XmlElement e_icon = document.SelectSingleNode("//icon[@name='icon_name']") as XmlElement;
                                    icon = e_icon.GetAttribute("value");

                                    XmlElement e_symbol = document.SelectSingleNode("//icon[@name='symbol_icon_name']") as XmlElement;
                                    symbol = e_symbol.GetAttribute("value");
                                }

                                string army = path.Substring(path.IndexOf("races") + 6, path.Length - path.IndexOf("races") - 6).Split("\\")[0];
                                if (army == "soviets")
                                {
                                    army = army.Replace("soviets", "soviet");
                                }
                                else if (army == "brits")
                                {
                                    army = army.Replace("brits", "british");
                                }

                                string costJsdbType = "Battlegrounds.Game.Gameplay.Cost";

                                string costManpower = "0";
                                string costMunition = "0";
                                string costFuel = "0";
                                string costTime = "0";

                                if (document.SelectSingleNode(@"//template_reference[@name='exts'] [@value='ebpextensions\cost_ext']") != null)
                                {
                                    XmlElement e_manpower = document.SelectSingleNode("//float[@name='manpower']") as XmlElement;
                                    costManpower = e_manpower.GetAttribute("value");

                                    XmlElement e_munition = document.SelectSingleNode("//float[@name='munition']") as XmlElement;
                                    costMunition = e_munition.GetAttribute("value");

                                    XmlElement e_fuel = document.SelectSingleNode("//float[@name='fuel']") as XmlElement;
                                    costFuel = e_fuel.GetAttribute("value");

                                    XmlElement e_time = document.SelectSingleNode("//float[@name='time_seconds']") as XmlElement;
                                    costTime = e_time.GetAttribute("value");
                                }

                                entityCost.Add(name, (SafeParse(costManpower), SafeParse(costMunition), SafeParse(costFuel), SafeParse(costTime)));

                                dynamic ebps = new JObject();
                                ebps.jsdbtype = jsdbType;
                                ebps.PBGID = pbgid;
                                ebps.Name = name;
                                ebps.LocaleName = localeName;
                                ebps.LocaleDescription = localeDescription;
                                ebps.Army = army;
                                ebps.Icon = icon;
                                ebps.Symbol = symbol;
                                ebps.Cost = new JObject();
                                ebps.Cost.jsdbtype = costJsdbType;
                                ebps.Cost.Manpower = costManpower;
                                ebps.Cost.Munition = costMunition;
                                ebps.Cost.Fuel = costFuel;
                                ebps.Cost.BuildTime = costTime;

                                string ebpsJson = JsonConvert.SerializeObject(ebps, Newtonsoft.Json.Formatting.Indented);

                                proccessedFiles++;

                                if (proccessedFiles < filesToProcces)
                                {
                                    sw.WriteLine(ebpsJson + ",");
                                }
                                else
                                {
                                    sw.WriteLine(ebpsJson);
                                }

                            }
                            sw.WriteLine("]");
                        }
                    }

                    Console.WriteLine("Ebps database created with " + proccessedFiles.ToString() + "/" + filesToProcces.ToString() + " items added!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
        }

        private static void CreateCriticalDatabase()
        {
            string fileName = dirPath + @"\critical_database.json";

            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                if (!Directory.Exists(instancesPath + @"\critical"))
                {
                    Console.WriteLine("ERROR: \"critical\" folder not found!");
                }
                else
                {
                    int filesToProcces = Directory.GetFiles(instancesPath + @"\critical", "*.xml", SearchOption.AllDirectories).Length;
                    int proccessedFiles = 0;

                    using (FileStream fs = File.Create(fileName))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.WriteLine("[");
                            foreach (string path in Directory.GetFiles(instancesPath + @"\critical", "*xml", SearchOption.AllDirectories))
                            {
                                XmlDocument document = new XmlDocument();
                                document.Load(path);

                                string jsdbType = "Battlegrounds.Game.Database.CriticalBlueprint";

                                XmlElement e_pbgid = document["instance"]["uniqueid"];
                                string pbgid = e_pbgid.GetAttribute("value");

                                string name = path.Substring(path.LastIndexOf(@"\") + 1);
                                name = name.Remove(name.Length - 4);

                                string localeName = "";
                                string localeDescription = "";
                                string icon = "";

                                if (document.SelectSingleNode(@"//template_reference[@name='ui_info'] [@value='tables\ui_info']") != null)
                                {
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
                                critical.Name = name;
                                critical.LocaleName = localeName;
                                critical.LocaleDescription = localeDescription;
                                critical.Icon = icon;

                                string criticalJson = JsonConvert.SerializeObject(critical, Newtonsoft.Json.Formatting.Indented);

                                proccessedFiles++;

                                if (proccessedFiles < filesToProcces)
                                {
                                    sw.WriteLine(criticalJson + ",");
                                }
                                else
                                {
                                    sw.WriteLine(criticalJson);
                                }

                            }
                            sw.WriteLine("]");
                        }
                    }

                    Console.WriteLine("Critical database created with " + proccessedFiles.ToString() + "/" + filesToProcces.ToString() + " items added!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
        }

        private static void CreateSlotItemDatabase()
        {
            string fileName = dirPath + @"\slot_item_database.json";

            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                if (!Directory.Exists(instancesPath + @"\slot_item"))
                {
                    Console.WriteLine("ERROR: \"slot_item\" folder not found!");
                }
                else
                {
                    int filesToProcces = Directory.GetFiles(instancesPath + @"\slot_item", "*.xml", SearchOption.AllDirectories).Length;
                    int proccessedFiles = 0;

                    using (FileStream fs = File.Create(fileName))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.WriteLine("[");
                            foreach (string path in Directory.GetFiles(instancesPath + @"\slot_item", "*xml", SearchOption.AllDirectories))
                            {
                                XmlDocument document = new XmlDocument();
                                document.Load(path);

                                string jsdbType = "Battlegrounds.Game.Database.SlotItemBlueprint";

                                XmlElement e_pbgid = document["instance"]["uniqueid"];
                                string pbgid = e_pbgid.GetAttribute("value");

                                string name = path.Substring(path.LastIndexOf(@"\") + 1);
                                name = name.Remove(name.Length - 4);

                                string localeName = "";
                                string localeDescription = "";
                                string icon = "";

                                if (document.SelectSingleNode(@"//group[@name='ui_info']") != null)
                                {
                                    XmlElement e_localeName = document.SelectSingleNode("//locstring[@name='screen_name']") as XmlElement;
                                    localeName = e_localeName.GetAttribute("value");

                                    XmlElement e_localeDescription = document.SelectSingleNode("//locstring[@name='help_text']") as XmlElement;
                                    localeDescription = e_localeDescription.GetAttribute("value");

                                    XmlElement e_icon = document.SelectSingleNode("//icon[@name='icon_name']") as XmlElement;
                                    icon = e_icon.GetAttribute("value");
                                }

                                dynamic slotItem = new JObject();
                                slotItem.jsdbtype = jsdbType;
                                slotItem.PBGID = pbgid;
                                slotItem.Name = name;
                                slotItem.LocaleName = localeName;
                                slotItem.LocaleDescription = localeDescription;
                                slotItem.Icon = icon;

                                string slotItemJson = JsonConvert.SerializeObject(slotItem, Newtonsoft.Json.Formatting.Indented);

                                proccessedFiles++;

                                if (proccessedFiles < filesToProcces)
                                {
                                    sw.WriteLine(slotItemJson + ",");
                                }
                                else
                                {
                                    sw.WriteLine(slotItemJson);
                                }

                            }
                            sw.WriteLine("]");
                        }
                    }

                    Console.WriteLine("Slot item database created with " + proccessedFiles.ToString() + "/" + filesToProcces.ToString() + " items added!");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
        }

        private static void CreateUpgradeDatabase()
        {
            string fileName = dirPath + @"\upgrade_database.json";

            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                if (!Directory.Exists(instancesPath + @"\upgrade"))
                {
                    Console.WriteLine("ERROR: \"upgrade\" folder not found!");
                }
                else
                {
                    int filesToProcces = Directory.GetFiles(instancesPath + @"\upgrade", "*.xml", SearchOption.AllDirectories).Length;
                    int proccessedFiles = 0;

                    using (FileStream fs = File.Create(fileName))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.WriteLine("[");
                            foreach (string path in Directory.GetFiles(instancesPath + @"\upgrade", "*xml", SearchOption.AllDirectories))
                            {
                                XmlDocument document = new XmlDocument();
                                document.Load(path);

                                string jsdbType = "Battlegrounds.Game.Database.UpgradeBlueprint";

                                XmlElement e_pbgid = document["instance"]["uniqueid"];
                                string pbgid = e_pbgid.GetAttribute("value");

                                string name = path.Substring(path.LastIndexOf(@"\") + 1);
                                name = name.Remove(name.Length - 4);

                                string localeName = "";
                                string localeDescription = "";
                                string icon = "";

                                if (document.SelectSingleNode(@"//group[@name='ui_info']") != null)
                                {
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

                                if (document.SelectSingleNode(@"//group[@name='time_cost']") != null)
                                {
                                    XmlElement e_manpower = document.SelectSingleNode("//float[@name='manpower']") as XmlElement;
                                    costManpower = e_manpower.GetAttribute("value");

                                    XmlElement e_munition = document.SelectSingleNode("//float[@name='munition']") as XmlElement;
                                    costMunition = e_munition.GetAttribute("value");

                                    XmlElement e_fuel = document.SelectSingleNode("//float[@name='fuel']") as XmlElement;
                                    costFuel = e_fuel.GetAttribute("value");
                                }

                                List<string> slotItems = new List<string>();
                                XmlNodeList e_slotItems = document.SelectNodes("//instance_reference[@name='slot_item']");
                                foreach (XmlNode slotItem in e_slotItems)
                                {
                                    slotItems.Add(slotItem.Attributes["value"].Value);
                                }
                                IEnumerable<string> uniqueSlotItems = slotItems.Distinct<string>();

                                dynamic upgrade = new JObject();
                                upgrade.jsdbtype = jsdbType;
                                upgrade.PBGID = pbgid;
                                upgrade.Name = name;
                                upgrade.LocaleName = localeName;
                                upgrade.LocaleDescription = localeDescription;
                                upgrade.Icon = icon;
                                upgrade.Cost = new JObject();
                                upgrade.Cost.jsdbtype = costJsdbType;
                                upgrade.Cost.Manpower = costManpower;
                                upgrade.Cost.Munition = costMunition;
                                upgrade.Cost.Fuel = costFuel;
                                upgrade.SlotItem = new JArray(uniqueSlotItems);

                                string upgradeJson = JsonConvert.SerializeObject(upgrade, Newtonsoft.Json.Formatting.Indented);

                                proccessedFiles++;

                                if (proccessedFiles < filesToProcces)
                                {
                                    sw.WriteLine(upgradeJson + ",");
                                }
                                else
                                {
                                    sw.WriteLine(upgradeJson);
                                }

                            }
                            sw.WriteLine("]");
                        }
                    }

                    Console.WriteLine("Upgrade database created with " + proccessedFiles.ToString() + "/" + filesToProcces.ToString() + " items added!");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
        }

        private static void CreateAbilityDatabase()
        {
            string fileName = dirPath + @"\abilities_database.json";

            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                if (!Directory.Exists(instancesPath + @"\abilities"))
                {
                    Console.WriteLine("ERROR: \"abilities\" folder not found!");
                }
                else
                {
                    int filesToProcces = Directory.GetFiles(instancesPath + @"\abilities", "*.xml", SearchOption.AllDirectories).Length;
                    int proccessedFiles = 0;

                    using (FileStream fs = File.Create(fileName))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.WriteLine("[");
                            foreach (string path in Directory.GetFiles(instancesPath + @"\abilities", "*xml", SearchOption.AllDirectories))
                            {
                                XmlDocument document = new XmlDocument();
                                document.Load(path);

                                string jsdbType = "Battlegrounds.Game.Database.AbilityBlueprint";

                                XmlElement e_pbgid = document["instance"]["uniqueid"];
                                string pbgid = e_pbgid.GetAttribute("value");

                                string name = path.Substring(path.LastIndexOf(@"\") + 1);
                                name = name.Remove(name.Length - 4);

                                string localeName = "";
                                string localeDescription = "";
                                string icon = "";

                                if (document.SelectSingleNode(@"//group[@name='ui_info']") != null)
                                {
                                    XmlElement e_localeName = document.SelectSingleNode("//locstring[@name='screen_name']") as XmlElement;
                                    localeName = e_localeName.GetAttribute("value");

                                    XmlElement e_localeDescription = document.SelectSingleNode("//locstring[@name='help_text']") as XmlElement;
                                    localeDescription = e_localeDescription.GetAttribute("value");

                                    XmlElement e_icon = document.SelectSingleNode("//icon[@name='icon_name']") as XmlElement;
                                    icon = e_icon.GetAttribute("value");
                                }

                                string army = path.Substring(path.IndexOf("abilities") + 10, path.Length - path.IndexOf("abilities") - 10).Split("\\")[0];
                                if (army == "soviets")
                                {
                                    army = army.Replace("soviets", "soviet");
                                }
                                else if (army == "brits")
                                {
                                    army = army.Replace("brits", "british");
                                }

                                string costJsdbType = "Battlegrounds.Game.Gameplay.Cost";

                                string costManpower = "0";
                                string costMunition = "0";
                                string costFuel = "0";

                                if (document.SelectSingleNode(@"//group[@name='cost']") != null)
                                {
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

                                if (proccessedFiles < filesToProcces)
                                {
                                    sw.WriteLine(abilityJson + ",");
                                }
                                else
                                {
                                    sw.WriteLine(abilityJson);
                                }

                            }
                            sw.WriteLine("]");
                        }
                    }

                    Console.WriteLine("Abilities database created with " + proccessedFiles.ToString() + "/" + filesToProcces.ToString() + " items added!");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
        }

        public static float SafeParse(string value)
        {
            if (float.TryParse(value, out float valueFloat))
            {
                return valueFloat;
            } else
            {
                return 0.0f;
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Set path where you want the files to be created to: ");
            dirPath = Console.ReadLine();

            while (!Directory.Exists(dirPath))
            {
                Console.WriteLine("Invalid path! Try again: ");
                dirPath = Console.ReadLine();
            }

            Console.WriteLine("Set path to your \"instances\" folder: ");
            instancesPath = Console.ReadLine();

            while (!Directory.Exists(instancesPath) && !instancesPath.EndsWith(@"\instances"))
            {
                Console.WriteLine("Invalid path! Try again: ");
                instancesPath = Console.ReadLine();
            }

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
