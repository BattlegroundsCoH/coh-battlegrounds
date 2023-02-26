using System;
using System.Diagnostics;
using System.IO;

namespace Battlegrounds.Developer.Commands;

public class CreateInstallerCommand : Command {

    const string msbuild_path = @"C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin";

    public static readonly Argument<string> PLATFORM = new Argument<string>("-p", "Specifies the build platform", "x64");

    public CreateInstallerCommand() : base("create-installer", "Creates a .msi file", PLATFORM) { }

    public override void Execute(CommandArgumentList argumentList) {

        string platform = argumentList.GetValue(PLATFORM);
        if (platform != "x64" && platform != "x86") {
            Console.WriteLine("Invalid platform");
            return;
        }

        string workdir = Path.GetFullPath("..\\..\\..\\..\\");

        // Create publish files 
        ProcessStartInfo createPublishFiles = new ProcessStartInfo() {
            UseShellExecute = false,
            FileName = Path.Combine(workdir, "coh2-battlegrounds-console\\bin\\Debug\\net6.0\\coh2-battlegrounds-console.exe"),
            Arguments = $"mki -p {platform} -dz",
            WorkingDirectory = Path.Combine(workdir, "coh2-battlegrounds-console\\bin\\Debug\\net6.0\\"),
        };

        Process? createPublishFilesProcess = Process.Start(createPublishFiles);
        if (createPublishFilesProcess is null) {
            Console.WriteLine($"Failed to create coh2-battlegrounds-console.exe mki -p {platform} -dz process");
            return;
        }

        createPublishFilesProcess!.WaitForExit();

        if (!CompileProject("coh2-battlegrounds-installer.wixproj", Path.Combine(workdir, "coh2-battlegrounds-installer"), platform)) {
            return;
        }

    }

    private bool CompileProject(string project, string projectDir, string platform) {

        // Firstly we trigger MSBuild.exe on our solution file
        ProcessStartInfo msbuild_bin = new ProcessStartInfo() {
            UseShellExecute = false,
            FileName = $"{msbuild_path}\\MSBuild.exe",
            Arguments = $"{project} -property:Configuration=Release -property:Platform={platform}",
            WorkingDirectory = projectDir,
        };

        // Trigger compile
        Process? binbuilder = Process.Start(msbuild_bin);
        if (binbuilder is null) {
            Console.WriteLine("Failed to create MS build process");
            return false;
        }

        // Grab io
        binbuilder.OutputDataReceived += (s, e) => {
            Console.WriteLine($"msbuild: {e.Data}");
        };
        binbuilder.ErrorDataReceived += (s, e) => {
            Console.WriteLine($"error: {e.Data}");
        };

        // Wait for exit
        binbuilder.WaitForExit();

        // Return OK
        return true;

    }

}
