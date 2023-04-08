using System.Collections.Generic;

using Battlegrounds.Game.DataSource;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database.Management.CoH2;

/// <summary>
/// 
/// </summary>
public class CoH2Locale : IModLocale {

    /// <summary>
    /// 
    /// </summary>
    public static readonly UcsFile Locale = GameCases.GetLocale(GameCase.CompanyOfHeroes2);

    private readonly Dictionary<ModGuid, UcsFile> modLocales;

    /// <summary>
    /// 
    /// </summary>
    public CoH2Locale() {
        this.modLocales = new();
    }

    /// <inheritdoc/>
    public void RegisterUcsFile(ModGuid guid, UcsFile file) => modLocales[guid] = file;

    /// <inheritdoc/>
    public string GetString(string locStr) {
        if (locStr.StartsWith("$")) {
            locStr = locStr[1..];
            int j = locStr.IndexOf(':');
            if (j > 0) {
                string numeric = locStr[(j + 1)..];
                if (uint.TryParse(numeric, out uint modLocID)) {
                    ModGuid guid = ModGuid.FromGuid(locStr[0..j]);
                    if (guid.IsValid) {
                        return modLocales.TryGetValue(guid, out UcsFile? modUCS) && modUCS is not null ? modUCS[modLocID] : $"Unregistered GUID '{locStr[0..j]}'";
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

    /// <inheritdoc/>
    public string GetString(uint localeKey) 
        => Locale[localeKey];

}
