using Battlegrounds.Game.DataSource;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database.Management;

/// <summary>
/// Interface representing a mod locale context.
/// </summary>
public interface IModLocale {
    
    /// <summary>
    /// Register a <see cref="UcsFile"/> in the mod context.
    /// </summary>
    /// <param name="guid">The <see cref="ModGuid"/> that identifies keys in the <see cref="UcsFile"/>.</param>
    /// <param name="file">The file to register.</param>
    void RegisterUcsFile(ModGuid guid, UcsFile file);

    /// <summary>
    /// Get a string from the locale key string.
    /// </summary>
    /// <remarks>
    /// Supports raw numeric entries, '$' prefixed strings and <see cref="ModGuid"/> prefixed strings.
    /// </remarks>
    /// <param name="localeKey">The localised string identifier to lookup localised string value for.</param>
    /// <returns>The found localised string or a "no range" string.</returns>
    string GetString(string localeKey);

    /// <summary>
    /// Get a string from the locale key.
    /// </summary>
    /// <param name="localeKey">The locale key to look up.</param>
    /// <returns>The found localised string or a "no range" string.</returns>
    string GetString(uint localeKey);

}
