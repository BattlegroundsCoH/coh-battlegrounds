using Battlegrounds.Data;

namespace Battlegrounds.Misc.Locale;

/// <summary>
/// Interface for objects that can be converted into a localised string argument list.
/// </summary>
public interface ILocaleArgumentsObject : IObjectChanged {

    /// <summary>
    /// Get object as locale arguments.
    /// </summary>
    /// <returns>Array of arguments for localised string formatting.</returns>
    object[] ToArgs();

}
