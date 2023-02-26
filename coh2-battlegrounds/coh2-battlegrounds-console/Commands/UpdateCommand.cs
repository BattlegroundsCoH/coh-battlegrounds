using Battlegrounds.Verification;

using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Battlegrounds.Developer.Commands;

public class UpdateCommand : Command {

    public static readonly string ChecksumPath = Path.GetFullPath("..\\..\\..\\..\\coh2-battlegrounds\\checksum.txt");

    public static readonly Argument<string> TARGET = new Argument<string>("-target", "Specifies the update target [db, checksum]", "db");

    public static readonly Argument<string> MOD = new Argument<string>("-m", "Specifies the mod to update, applicable when target=db", "vcoh");

    public static readonly Argument<string> TOOL =
        new Argument<string>("-t", "Specifies the full path to the tools folder", @"C:\Program Files (x86)\Steam\steamapps\common\Company of Heroes 2 Tools");

    public static readonly Argument<string> PLATFORM = new Argument<string>("-p", "Specifies build platform", "x64");

    public UpdateCommand() : base("update", "Will update specified elements of the source build.", PLATFORM, TARGET, MOD, TOOL) { }

    private ConsoleColor m_col;

    public override void Execute(CommandArgumentList argumentList) {
        // Flag to set the build platform
        string platform = argumentList.GetValue(PLATFORM);
        if (platform != "x64" && platform != "x86") {
            Console.WriteLine("Invalid platform");
            return;
        }

        this.m_col = Console.ForegroundColor;
        switch (argumentList.GetValue(TARGET)) {
            case "db":
                this.UpdateDatabase(argumentList);
                break;
            case "checksum":
                ComputeChecksum(platform);
                break;
            default:
                Console.WriteLine("Undefined update target.");
                break;
        }
    }

    private void Err(string msg) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(msg);
        Console.ForegroundColor = this.m_col;
    }

    record LastUse(string OutPath, string InstancePath, string ModGuid, string ModName);

    private void UpdateDatabase(CommandArgumentList args) {

        // Grab mod target
        string mod = args.GetValue(MOD);

        // Get path to xaml to json tool
        string xmlreader = Path.GetFullPath("..\\..\\..\\..\\..\\db-tool\\bin\\debug\\net6.0\\CoH2XML2JSON.exe");

        // Verify
        if (!File.Exists(xmlreader)) {
            xmlreader = Path.GetFullPath("..\\..\\..\\..\\..\\db-tool\\bin\\debug\\net5.0\\CoH2XML2JSON.exe");
            if (!File.Exists(xmlreader)) {
                this.Err("Fatal Error: Failed to locate xml2json executable");
                return;
            }
        }

        // Store recent
        string recent = xmlreader.Replace("CoH2XML2JSON.exe", "last.json");

        // Get input dir
        string xmlinput = mod switch {
            "vcoh" => Path.GetFullPath(args.GetValue(TOOL) + @"\assets\data\attributes\instances"),
            "mod_bg" => Path.GetFullPath("..\\..\\..\\..\\..\\coh2-battlegrounds-mod\\tuning_mod\\instances"),
            _ => $"$DIR({mod})"
        };

        // Log and bail
        if (!Directory.Exists(xmlinput)) {
            this.Err($"Fatal Error: Directory {xmlinput} not found for mod {mod}");
            return;
        }

        // Get output dir
        string jsonoutput = mod switch {
            "vcoh" => Path.GetFullPath("..\\..\\..\\..\\coh2-battlegrounds\\bg_common\\data"),
            "mod_bg" => Path.GetFullPath("..\\..\\..\\..\\coh2-battlegrounds\\bg_common\\data"),
            _ => throw new NotSupportedException()
        };

        // Get GUID
        string guid = mod switch {
            "vcoh" => "",
            "mod_bg" => "142b113740474c82a60b0a428bd553d5",
            _ => throw new NotSupportedException()
        };

        // Log
        Console.WriteLine($"XML to Json converter: {xmlreader}");
        Console.WriteLine($"XML input: {xmlinput}");
        Console.WriteLine($"Json Out: {jsonoutput}");

        // Save
        File.WriteAllText(recent, JsonSerializer.Serialize(new LastUse(jsonoutput, xmlinput, guid, mod)));

        // Invoke
        var proc = Process.Start(new ProcessStartInfo(xmlreader, "-do_last") { WorkingDirectory = Path.GetDirectoryName(xmlreader) });
        proc?.WaitForExit();

        Console.WriteLine();
        Console.WriteLine(" -> Task DONE");

    }

    internal static void ComputeChecksum(string platform) {

        // Get path to release build
        string releaseExe = Path.GetFullPath($"..\\..\\..\\..\\coh2-battlegrounds\\bin\\Release\\net6.0-windows\\win-{platform}\\publish\\coh2-battlegrounds.exe");

        // Log
        Console.WriteLine($"Checksum file: {ChecksumPath}");
        Console.WriteLine($"Release Build: {releaseExe}");

        // Make sure release build exists
        if (!File.Exists(releaseExe)) {
            Console.WriteLine("Release executable not found - aborting!");
            return;
        }

        // Compute hash
        Integrity.CheckIntegrity(releaseExe);

        // Log hash
        Console.WriteLine($"Computed integrity hash as: {Integrity.IntegrityHashString}");
        Console.WriteLine($"Saving integrity hash to checksum file.");

        // Save
        File.WriteAllText(ChecksumPath, Integrity.IntegrityHashString);
        File.Copy(ChecksumPath, releaseExe.Replace("coh2-battlegrounds.exe", "checksum.txt"), true);

        // Log
        Console.WriteLine("Saved hash to checksum file(s).");

    }

}
