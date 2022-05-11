using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Globalization;
using System.Linq;

namespace CoH2XML2JSON {

    public delegate T BlueprintFactory<T>(XmlDocument document, string path, string name) where T : BP;

    public class Program {

        // Define the culture to use when parsing numbers etc.
        public static readonly CultureInfo FormatCulture = CultureInfo.GetCultureInfo("en-US");

        public static float GetFloat(string value) => float.Parse(value, FormatCulture);

        static readonly JsonSerializerOptions serializerOptions = new() { 
            WriteIndented = true, 
            IgnoreReadOnlyFields = false,
            IgnoreReadOnlyProperties = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
        };

        static string dirPath;
        static string instancesPath;
        static string modguid;
        static string modname;
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

        private static List<T> GenericDatabaseGet<T>(string dbname, string lookpath, BlueprintFactory<T> instanceCreator) where T : BP {

            // Get destination
            string fileName = Path.Combine(dirPath, dbname);

            try {

                // If file already exists, delete it.
                if (File.Exists(fileName)) {
                    File.Delete(fileName);
                }

                // Get folder to search and read .xml files from
                string searchDir = Path.Combine(instancesPath, lookpath);

                // Make sure there's a folder to read
                if (!Directory.Exists(searchDir)) {

                    Console.WriteLine($"INFO: \"{lookpath}\" folder not found - the database creaton will be skipped.");

                } else {

                    var files = Directory.GetFiles(searchDir, "*.xml", SearchOption.AllDirectories);
                    List<T> bps = new();

                    foreach (string path in files) {

                        XmlDocument document = new XmlDocument();
                        document.Load(path);

                        string name = path[(path.LastIndexOf(@"\") + 1)..^4];
                        T bp = instanceCreator(document, path, name);
                        string sbpsJson = JsonSerializer.Serialize(bp, serializerOptions);

                        bps.Add(bp);

                    }

                    // Return found values
                    return bps;

                }
            } catch (Exception e) {

                // Log error and wait for user to exit
                Console.WriteLine(e.ToString());
                Console.ReadLine();

            }

            // Return something
            return new();

        }

        public static void GenericDatabaseSave<T>(string dbname, IEnumerable<T> data) where T : BP {

            // Get destination
            string fileName = Path.Combine(dirPath, dbname);

            // Write
            File.WriteAllText(fileName, JsonSerializer.Serialize(data.ToArray(), serializerOptions));
            Console.WriteLine($"Created database: {fileName}");


        }

        public static void GenericDatabase<T>(string dbname, string lookpath, BlueprintFactory<T> instanceCreator) where T : BP {

            // Get
            var bps = GenericDatabaseGet<T>(dbname, lookpath, instanceCreator);

            // Save
            GenericDatabaseSave<T>(dbname, bps);

        }

        public static void GenericDatabase<T>(string dbname, BlueprintFactory<T> instanceCreator, params string[] paths) where T : BP {
            GenericDatabaseSave(dbname, paths.Aggregate((IEnumerable<T>)(new List<T>()), (a, b) => a.Concat(GenericDatabaseGet<T>(dbname, b, instanceCreator))));
        }

        public class LastUse {
            public string OutPath { get; set; } 
            public string InstancePath { get; set; }
            public string ModGuid { get; set; }
            public string ModName { get; set; }
        }

        public static void Main(string[] args) {

            bool doLastIgnoreInput = args.Length == 1 && args[0] == "-do_last";

            Console.WriteLine(string.Join(" ", args));

            LastUse last = null;
            if (File.Exists("last.json")) {
                last = JsonSerializer.Deserialize<LastUse>(File.ReadAllText("last.json"));
                Console.WriteLine("Use settings from last execution?");
                Console.WriteLine("Output Directory: " + last.OutPath);
                Console.WriteLine("Instance Directory: " + last.InstancePath);
                Console.WriteLine("ModGUID: " + last.ModGuid);
                Console.WriteLine();
                if (doLastIgnoreInput) {
                    dirPath = last.OutPath;
                    modguid = last.ModGuid;
                    instancesPath = last.InstancePath;
                    modname = last.ModName;
                } else {
                    Console.Write("(Y/N): ");
                    if (Console.ReadLine().ToLower() is not "y") {
                        last = null;
                    } else {
                        dirPath = last.OutPath;
                        modguid = last.ModGuid;
                        instancesPath = last.InstancePath;
                        modname = last.ModName;
                    }
                }
            }

            if (last is null) {
                Console.Write("Set path where you want the files to be created to: ");
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

                Console.Write("Set path to your \"instances\" folder: ");
                instancesPath = Console.ReadLine();

                while (!Directory.Exists(instancesPath) && !instancesPath.EndsWith(@"\instances")) {
                    Console.WriteLine("Invalid path! Try again: ");
                    instancesPath = Console.ReadLine();
                }

                Console.Write("Mod GUID (Leave empty if not desired/available):");
                modguid = Console.ReadLine().Replace("-", "");
                if (modguid.Length != 32) {
                    modguid = string.Empty;
                }

                Console.Write("Mod Name (Leave empty for vanilla):");
                modname = Console.ReadLine();
                if (string.IsNullOrEmpty(modname)) {
                    modname = "vcoh";
                }

            }

            LastUse lu = new() { InstancePath = instancesPath, ModGuid = modguid, OutPath = dirPath, ModName = modname };
            File.WriteAllText("last.json", JsonSerializer.Serialize(lu));

            GenericDatabase($"{modname}-abp-db.json", "abilities", (doc, path, name) => new ABP(doc, modguid, name) { Army = GetFactionFromPath(path) });
            GenericDatabase($"{modname}-ebp-db.json", (doc, path, name) => {
                var ebp = new EBP(doc, modguid, name) { Army = GetFactionFromPath(path) };
                entities.Add(ebp);
                return ebp;
            }, "ebps\\races", "ebps\\gameplay");
            GenericDatabase($"{modname}-sbp-db.json", "sbps\\races", (doc, path, name) => new SBP(doc, modguid, name, entities) { Army = GetFactionFromPath(path) });
            GenericDatabase($"{modname}-cbp-db.json", "critical", (doc, path, name) => new Critical(doc, modguid, name));
            GenericDatabase($"{modname}-ibp-db.json", "slot_item", (doc, path, name) => new SlotItem(doc, modguid, name) { Army = GetFactionFromPath(path) });
            GenericDatabase($"{modname}-ubp-db.json", "upgrade", (doc, path, name) => new UBP(doc, modguid, name));
            GenericDatabase($"{modname}-wbp-db.json", "weapon", (doc, path, name) => new WBP(doc, modguid, name, path));

            Console.WriteLine();

            if (!doLastIgnoreInput) {
                Console.WriteLine("Created databases - Press any key to exit");
                Console.Read();
            }

        }

    }

}
