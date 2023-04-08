using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.IO;

using Battlegrounds.Game.DataSource;

using Battlegrounds.Modding;
using Battlegrounds.Functional;

namespace Battlegrounds.Locale;

/// <summary>
/// Static utility class for localizing ingame strings.
/// </summary>
public static class GameLocale {

    private readonly static UcsFile? LocaleFile;
    private readonly static Dictionary<ModGuid, UcsFile> ModLocales;

    [MemberNotNullWhen(true, nameof(LocaleFile))]
    public static bool HasLoadedLocale => LocaleFile is not null;

    static GameLocale() {

        // Determine language to use
        string language = Battlegrounds.BattlegroundsContext.Localize.Language.ToString();
        if (language == "Default") {
            language = "English";
        }

        // Load VCoh Locale
        string localePath = Path.Combine(Battlegrounds.Pathfinder.GetOrFindCoHPath(), $"CoH2\\Locale\\{language}\\RelicCoH2.{language}.ucs");
        if (File.Exists(localePath)) {
            LocaleFile = UcsFile.LoadFromFile(localePath);
        } else {
            Trace.WriteLine($"Failed to load locale file '{localePath}'", nameof(GameLocale));
        }

        // Create dictionary for mod locales
        ModLocales = new Dictionary<ModGuid, UcsFile>();

        // Load Mod Locales
        BattlegroundsContext.ModManager.EachPackage(x => {
            x.LocaleFiles.ForEach(y => {
                if (y.GetLocale(x.ID, language) is UcsFile ucs) {
                    ModLocales.Add(y.ModType switch {
                        ModType.Asset => x.AssetGUID,
                        ModType.Gamemode => x.GamemodeGUID,
                        ModType.Tuning => x.TuningGUID,
                        _ => throw new NotImplementedException()
                    }, ucs);
                }
            });
        });

    }

    public static string GetString(uint key) => HasLoadedLocale ? LocaleFile[key] : $"${key} No Range";

    public static string GetString(string locStr) {
        if (locStr.StartsWith("$")) {
            locStr = locStr[1..];
            int j = locStr.IndexOf(':');
            if (j > 0) {
                string numeric = locStr[(j + 1)..];
                if (uint.TryParse(numeric, out uint modLocID)) {
                    ModGuid guid = ModGuid.FromGuid(locStr[0..j]);
                    if (guid.IsValid) {
                        return ModLocales.TryGetValue(guid, out UcsFile? modUCS) && modUCS is not null ? modUCS[modLocID] : $"Unregistered GUID '{locStr[0..j]}'";
                    }
                } else {
                    return locStr;
                }
            } else {
                return GetString(locStr);
            }
            return $"Invalid GUID '{locStr}' (ModID = '{locStr[0..j]}')";
        } else {
            return uint.TryParse(locStr, out uint locID) ? GetString(locID) : locStr;
        }
    }

}
