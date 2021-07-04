using System.Collections.Generic;
using System.IO;

using Battlegrounds.Game.DataSource;
using Battlegrounds.Modding;

namespace BattlegroundsApp.Resources {

    /// <summary>
    /// Static utility class for localizing ingame strings.
    /// </summary>
    public static class GameLocale {

        private static UcsFile LocaleFile;
        private static Dictionary<ModGuid, UcsFile> ModLocales;

        static GameLocale() {
            
            // Determine language to use
            string language = Battlegrounds.BattlegroundsInstance.Localize.Language.ToString();
            if (language == "Default") {
                language = "English";
            }
            
            // Load VCoh Locale
            string localePath = Path.Combine(Battlegrounds.Pathfinder.GetOrFindCoHPath(), $"CoH2\\Locale\\{language}\\RelicCoH2.{language}.ucs");
            if (File.Exists(localePath)) {
                LocaleFile = UcsFile.LoadFromFile(localePath);
            }

            // Create dictionary for mod locales
            ModLocales = new Dictionary<ModGuid, UcsFile>();

        }

        public static bool LoadModLocale(ModGuid modguid, string fullLocalePath) {
            if (ModLocales.ContainsKey(modguid)) {
                return true;
            }
            if (File.Exists(fullLocalePath)) {
                ModLocales.Add(modguid, UcsFile.LoadFromFile(fullLocalePath));
                return true;
            } else {
                return false;
            }
        }

        public static string GetString(uint key) => LocaleFile[key];

        public static string GetString(string locStr) {
            if (locStr.StartsWith("$")) {
                locStr = locStr[1..];
                int j = locStr.IndexOf(':');
                if (j > 0) {
                    if (uint.TryParse(locStr[j..], out uint modLocID)) {
                        ModGuid guid = ModGuid.FromGuid(locStr[0..j]);
                        if (guid.IsValid) {
                            if (ModLocales.TryGetValue(guid, out UcsFile modUCS)) {
                                return modUCS[modLocID];
                            } else {
                                return $"Unregistered GUID '{locStr[0..j]}'";
                            }
                        } else {
                            return $"Invalid GUID '{locStr}' (ModID = '{locStr[0..j]}')";
                        }
                    } else {
                        return locStr;
                    }
                } else {
                    return GetString(locStr);
                }
            } else {
                if (uint.TryParse(locStr, out uint locID)) {
                    return GetString(locID);
                } else {
                    return locStr;
                }
            }
        }

    }

}
