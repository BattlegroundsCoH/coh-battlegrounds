using System.IO;

namespace Battlegrounds.Compiler.Locale.CoH3;

/// <summary>
/// Class representing a locale compiler for Company of Heroes 3
/// </summary>
public sealed class CoH3LocaleCompiler : ILocaleCompiler {
    
    /// <inheritdoc/>
    public void TranslateLocale(byte[] contents, string abspath, string[] customNames) {
        File.WriteAllBytes(abspath, contents);
    }

}
