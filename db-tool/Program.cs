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
            int rid = path.IndexOf("races");
            string army = path;
            if (rid != -1) {
                army = path.Substring(rid + 6, path.Length - rid - 6).Split("\\")[0];
            } else {
                for (int i = 0; i < racebps.Length; i++) {
                    string k = racebps[i][8..];
                    if (path.Contains(k)) {
                        return k;
                    }
                }
                army = "NULL";
            }
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

                    File.WriteAllText(fileName, JsonSerializer.Serialize(entities.ToArray(), serializerOptions));

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
                    List<Critical> criticals = new();

                    foreach (string path in Directory.GetFiles(instancesPath + @"\critical", "*xml", SearchOption.AllDirectories)) {
                        string name = path[(path.LastIndexOf(@"\") + 1)..^4];
                        Console.WriteLine(name);
                        XmlDocument document = new XmlDocument();
                        document.Load(path);
                        try {
                            var critical = new Critical(document, modguid, name);
                            criticals.Add(critical);
                        } catch (Exception) {
                            Console.WriteLine("Failed to parse critical: " + name);
                        }
                        proccessedFiles++;
                    }
                    File.WriteAllText(fileName, JsonSerializer.Serialize(criticals.ToArray(), serializerOptions));
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
                    List<UBP> upgrades = new();
                    foreach (string path in Directory.GetFiles(instancesPath + @"\upgrade", "*xml", SearchOption.AllDirectories)) {
                        string name = path[(path.LastIndexOf(@"\") + 1)..^4];
                        Console.WriteLine(name);
                        XmlDocument document = new XmlDocument();
                        document.Load(path);
                        try {
                            var upgrade = new UBP(document, modguid, name) { };
                            upgrades.Add(upgrade);
                        } catch (Exception) {
                            Console.WriteLine("Failed to parse entity: " + name);
                        }
                        proccessedFiles++;
                    }
                    File.WriteAllText(fileName, JsonSerializer.Serialize(upgrades.ToArray(), serializerOptions));
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
                    List<ABP> abps = new();
                    foreach (string path in Directory.GetFiles(instancesPath + @"\abilities", "*xml", SearchOption.AllDirectories)) {
                        string name = path[(path.LastIndexOf(@"\") + 1)..^4];
                        Console.WriteLine(name);
                        XmlDocument document = new XmlDocument();
                        document.Load(path);
                        try {
                            var abp = new ABP(document, modguid, name) { Army = GetFactionFromPath(path) };
                            abps.Add(abp);
                        } catch (Exception) {
                            Console.WriteLine("Failed to parse entity: " + name);
                        }
                        proccessedFiles++;
                    }
                    File.WriteAllText(fileName, JsonSerializer.Serialize(abps.ToArray(), serializerOptions));

                    Console.WriteLine("Abilities database created with " + proccessedFiles.ToString() + "/" + filesToProcces.ToString() + " items added!");
                }

            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }

        }

        private static void CreateWeaponDatabase() {
            string fileName = dirPath + @"\weapons_database.json";

            try {
                if (File.Exists(fileName)) {
                    File.Delete(fileName);
                }

                if (!Directory.Exists(instancesPath + @"\weapon")) {
                    Console.WriteLine("ERROR: \"weapon\" folder not found!");
                } else {
                    int filesToProcces = Directory.GetFiles(instancesPath + @"\weapon", "*.xml", SearchOption.AllDirectories).Length;
                    int proccessedFiles = 0;
                    List<WBP> wbps = new();
                    foreach (string path in Directory.GetFiles(instancesPath + @"\weapon", "*xml", SearchOption.AllDirectories)) {
                        string name = path[(path.LastIndexOf(@"\") + 1)..^4];
                        Console.WriteLine(name);
                        XmlDocument document = new XmlDocument();
                        document.Load(path);
                        try {
                            var wbp = new WBP(document, modguid, name);
                            wbps.Add(wbp);
                        } catch (Exception) {
                            Console.WriteLine("Failed to parse entity: " + name);
                        }
                        proccessedFiles++;
                    }
                    File.WriteAllText(fileName, JsonSerializer.Serialize(wbps.ToArray(), serializerOptions));

                    Console.WriteLine("Weapons database created with " + proccessedFiles.ToString() + "/" + filesToProcces.ToString() + " items added!");
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
            CreateWeaponDatabase();

            Console.WriteLine();
            Console.WriteLine("Created databases - Press any key to exit");
            Console.Read();

        }

    }

}
