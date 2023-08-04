using System.Collections.Generic;

using Battlegrounds.Game.DataSource;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database.Management.Common;

/// <summary>
/// 
/// </summary>
public abstract class CommonLocale : IModLocale {

    /// <summary>
    /// 
    /// </summary>
    protected readonly Dictionary<ModGuid, UcsFile> modLocales;

    /// <summary>
    /// 
    /// </summary>
    public CommonLocale() {
        modLocales = new();
    }

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
    public abstract string GetString(uint localeKey);

    /// <inheritdoc/>
    public void RegisterUcsFile(ModGuid guid, UcsFile file) => modLocales[guid] = file;

}
