using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Battlegrounds.Logging;

namespace Battlegrounds.Compiler.Locale.CoH2;

/// <summary>
/// Class representing a locale compiler specifically for CoH2.
/// </summary>
public sealed class CoH2LocaleCompiler : ILocaleCompiler {

    private static readonly Logger logger = Logger.CreateLogger();

    private readonly Regex m_ucsRegex; // Cuz we never really know how messed up the input files are...

    /// <summary>
    /// Starting Key Index for custom unit names
    /// </summary>
    public const int CUSTOM_NAME_KEYSTART = 38;
    
    /// <summary>
    /// Ending Key Index for custom unit names (Inclusive)
    /// </summary>
    public const int CUSTOM_NAME_KEYEND = 102; // (Inclusive)

    /// <summary>
    /// Create a new <see cref="CoH2LocaleCompiler"/> instance.
    /// </summary>
    public CoH2LocaleCompiler() {
        m_ucsRegex = new(@"(?<id>\d+)\s+(?<content>.*)");
    }

    /// <inheritdoc/>
    public void TranslateLocale(byte[] localeContent, string localeTargetPath, string[] customNames) {

        // Read to content
        string[] lines = Encoding.UTF8.GetString(localeContent).Split("\r\n"); // Split on CRLF line ending

        // Open fs
        using var fs = File.OpenWrite(localeTargetPath);
        using var writer = new StreamWriter(fs, Encoding.Unicode);

        // Write
        for (int i = 0; i < lines.Length; i++) {

            // split string
            var m = m_ucsRegex.Match(lines[i]);
            if (m.Success) {

                // Look at key
                int k = int.Parse(m.Groups["id"].Value);

                // If custom name key, spit it out now
                int j = k - CUSTOM_NAME_KEYSTART;
                if (k is >= CUSTOM_NAME_KEYSTART and <= CUSTOM_NAME_KEYEND && j < customNames.Length) {
                    writer.WriteLine($"{k}\t{customNames[j]}");
                } else {

                    // Just write it
                    writer.WriteLine($"{k}\t{m.Groups["content"].Value}");

                }

            } else {

                // Log
                if (!string.IsNullOrEmpty(lines[i])) {
                    logger.Warning($"Failed to compile UCS file entry '{lines[i]}'");
                }

            }

        }

        // Close writer
        writer.Close();

    }

}
