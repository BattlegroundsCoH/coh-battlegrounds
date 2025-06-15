using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

using Battlegrounds.Logging;

namespace Battlegrounds.Compiler.Essence;

/// <summary>
/// Class representing the Essence Editor that can be used to archive files.
/// </summary>
public sealed class EssenceEditor : IArchiver {

    private static readonly Logger logger = Logger.CreateLogger();

    /// <inheritdoc/>
    public bool Extract(string arcfile, string outpath) => throw new NotSupportedException("Extracting data using the Essence Editor is not allowed.");

    /// <inheritdoc/>
    public bool Extract(string arcfile, string outpath, TextWriter output) => throw new NotSupportedException("Extracting data using the Essence Editor is not allowed.");

    /// <inheritdoc/>
    public bool Archive(string archdef, string relativepath, string output)
        => throw new NotImplementedException();

    private string GetArchiverFilepath() {
        string path = BattlegroundsContext.GetRelativePath(BattlegroundsPaths.COH3_FOLDER, "EssenceEditor.exe");
        if (File.Exists(path)) {
            logger.Info($"Using editor @ {path}");
            return path;
        } else {
            logger.Info($"Essence Editor not found @ {path}");
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
                WorkingDirectory = Path.GetDirectoryName(GetArchiverFilepath())
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
                logger.Warning($"Editor has finished with error code = {eCode}");
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
