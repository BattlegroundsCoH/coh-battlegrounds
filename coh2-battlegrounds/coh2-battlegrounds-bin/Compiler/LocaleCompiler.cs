using System.IO;
using System.Text;

namespace Battlegrounds.Compiler;

public class LocaleCompiler {

    public LocaleCompiler() {

    }

    public void TranslateLocale(byte[] localeContent, string localeTargetPath) {

        // Read to content
        string[] lines = Encoding.UTF8.GetString(localeContent).Split("\r\n"); // Split on CRLF line ending

        // Open fs
        var fs = File.OpenWrite(localeTargetPath);
        var writer = new StreamWriter(fs, Encoding.Unicode);

        // Write
        for (int i = 0; i < lines.Length; i++) {
            writer.WriteLine(lines[i]);
        }

        // TODO: Write localised specific stuff (like unique unit names)

        // Close writer
        writer.Close();

    }

}
