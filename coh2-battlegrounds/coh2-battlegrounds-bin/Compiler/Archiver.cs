using System.IO;
using System.Diagnostics;
using System.Threading;

namespace Battlegrounds.Compiler;

public static class Archiver {

    public static bool Archive(string archdef, string relativepath, string output)
        => RunArchiver($" -c \"{archdef}\" -a \"{output}\" -v -r \"{relativepath}\\\"", null);

    public static bool Extract(string arcfile, string outpath)
        => RunArchiver($" -a \"{arcfile}\" -e \"{outpath}\" -v ", null);

    public static bool Extract(string arcfile, string outpath, TextWriter output)
        => RunArchiver($" -a \"{arcfile}\" -e \"{outpath}\" -v ", output);

    private static string GetArchiverFilepath() {
        string path = Path.Combine(Pathfinder.GetOrFindCoHPath(), "Archive.exe");
        if (File.Exists(path)) {
            Trace.WriteLine($"Using archiver @ {path}", nameof(Archiver));
            return path;
        } else {
            Trace.WriteLine($"Acrhive file not found @ {path}", nameof(Archiver));
            return string.Empty;
        }
    }

    private static bool RunArchiver(string args, TextWriter? outputStream) {

        Process archiveProcess = new Process {
            StartInfo = new ProcessStartInfo() {
                FileName = GetArchiverFilepath(),
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardInput = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false,
            },
            EnableRaisingEvents = true,
        };

        if (outputStream is null) {
            archiveProcess.OutputDataReceived += ArchiveProcess_OutputDataReceived;
        } else {
            archiveProcess.OutputDataReceived += (sender, e) => {
                if (e.Data != null && e.Data != string.Empty && e.Data != " ") {
                    outputStream.WriteLine(e.Data);
                }
            };
        }

        try {

            if (!archiveProcess.Start()) {
                archiveProcess.Dispose();
                return false;
            } else {
                archiveProcess.BeginOutputReadLine();
            }

            Thread.Sleep(1000);

            do {
                Thread.Sleep(100);
            } while (!archiveProcess.HasExited);

            if (archiveProcess.ExitCode != 0) {
                int eCode = archiveProcess.ExitCode;
                Trace.WriteLine($"Archiver has finished with error code = {eCode}");
                archiveProcess.Dispose();
                return false;
            }

        } catch {
            archiveProcess.Dispose();
            return false;
        }

        archiveProcess.Dispose();

        return true;

    }

    private static void ArchiveProcess_OutputDataReceived(object sender, DataReceivedEventArgs e) {
        if (e.Data != null && e.Data != string.Empty && e.Data != " ")
            Trace.WriteLine(e.Data);
    }

}
