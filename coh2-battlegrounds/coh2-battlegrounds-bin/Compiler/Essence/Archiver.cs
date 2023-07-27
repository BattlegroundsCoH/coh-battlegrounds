using System.IO;
using System.Diagnostics;
using System.Threading;
using Battlegrounds.Logging;

namespace Battlegrounds.Compiler.Essence;

/// <summary>
/// Class representing the CoH2/CoH1 archiver for archiving and extracting .sga files.
/// </summary>
public sealed class Archiver : IArchiver {

    private static readonly Logger logger = Logger.CreateLogger();

    /// <inheritdoc/>
    public bool Archive(string archdef, string relativepath, string output)
        => RunArchiver($" -c \"{archdef}\" -a \"{output}\" -v -r \"{relativepath}\\\"", null);

    /// <inheritdoc/>
    public bool Extract(string arcfile, string outpath)
        => RunArchiver($" -a \"{arcfile}\" -e \"{outpath}\" -v ", null);

    /// <inheritdoc/>
    public bool Extract(string arcfile, string outpath, TextWriter output)
        => RunArchiver($" -a \"{arcfile}\" -e \"{outpath}\" -v ", output);

    private string GetArchiverFilepath() {
        string path = BattlegroundsContext.GetRelativePath(BattlegroundsPaths.COH2_FOLDER, "Archive.exe");
        if (File.Exists(path)) {
            logger.Info($"Using archiver @ {path}");
            return path;
        } else {
            logger.Info($"Acrhive file not found @ {path}");
            return string.Empty;
        }
    }

    private bool RunArchiver(string args, TextWriter? outputStream) {

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
                logger.Warning($"Archiver has finished with error code = {eCode}");
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
            logger.Info(e.Data);
    }

}
