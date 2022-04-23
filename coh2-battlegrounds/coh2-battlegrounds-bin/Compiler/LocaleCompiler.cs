using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Battlegrounds.Compiler;

public class LocaleCompiler {

    private readonly Regex m_ucsRegex; // Cuz we never really know how messed up the input files are...

    public const int CUSTOM_NAME_KEYSTART = 38;
    public const int CUSTOM_NAME_KEYEND = 102; // (Inclusive)

    public LocaleCompiler() {
        this.m_ucsRegex = new(@"(?<id>\d+)\s+(?<content>.*)");
    }

    public void TranslateLocale(byte[] localeContent, string localeTargetPath, string[] customNames) {

        // Read to content
        string[] lines = Encoding.UTF8.GetString(localeContent).Split("\r\n"); // Split on CRLF line ending

        // Open fs
        using var fs = File.OpenWrite(localeTargetPath);
        using var writer = new StreamWriter(fs, Encoding.Unicode);

        // Write
        for (int i = 0; i < lines.Length; i++) {

            // split string
            var m = this.m_ucsRegex.Match(lines[i]);
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
                Trace.WriteLine($"Failed to compile UCS file entry '{lines[i]}'", nameof(LocaleCompiler));

            }

        }

        // Close writer
        writer.Close();

    }

}
