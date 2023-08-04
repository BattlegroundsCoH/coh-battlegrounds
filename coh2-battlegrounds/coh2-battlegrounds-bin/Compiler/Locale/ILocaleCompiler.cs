namespace Battlegrounds.Compiler.Locale;

/// <summary>
/// Interface for a locale compiler.
/// </summary>
public interface ILocaleCompiler {

    /// <summary>
    /// Translate the given locale contents into a proper UCS file with. 
    /// </summary>
    /// <param name="contents">The contents of the original UCS file.</param>
    /// <param name="abspath">The absolute path to store the result at.</param>
    /// <param name="customNames">Custom names to bake into the locale file.</param>
    void TranslateLocale(byte[] contents, string abspath, string[] customNames);

}
