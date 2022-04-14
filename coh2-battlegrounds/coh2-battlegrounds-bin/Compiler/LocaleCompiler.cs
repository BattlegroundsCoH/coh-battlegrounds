using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Battlegrounds.Compiler;

public class LocaleCompiler {

    private readonly Regex m_ucsRegex; // Cuz we never really know how messed up the input files are...

    public LocaleCompiler() {
        this.m_ucsRegex = new(@"(?<id>\d+)\s+(?<content>.*)");
    }

    public void TranslateLocale(byte[] localeContent, string localeTargetPath) {

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

                writer.WriteLine($"{m.Groups["id"].Value}\t{m.Groups["content"].Value}");

            } else {

            }
        }

        // TODO: Write localised specific stuff (like unique unit names)

        // Close writer
        writer.Close();

    }

}
