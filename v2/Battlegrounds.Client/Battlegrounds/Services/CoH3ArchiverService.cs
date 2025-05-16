using System.Diagnostics;
using System.IO;

using Battlegrounds.Models;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

public sealed class CoH3ArchiverService(IGameService gameService, Configuration configuration) : IArchiverService {

    private sealed class EssenceEditor {
        private Process? _eeProcess;
        private bool _failedToStart = false;
        public bool FailedToStart => _failedToStart;
        public void Launch(string absolutePath, string[] args) { 
            if (_eeProcess is not null) {
                throw new InvalidOperationException("Essence Editor is already running.");
            }
            string workingDirectory = Path.GetDirectoryName(absolutePath) ?? throw new InvalidOperationException("Unable to determine working directory.");
            _eeProcess = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = absolutePath,
                    Arguments = string.Join(" ", args),
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                },
            };
            _failedToStart = !_eeProcess.Start();
        }
        public async Task WaitForExitAsync() {
            if (_eeProcess is null) {
                throw new InvalidOperationException("Essence Editor is not running.");
            }
            await _eeProcess.WaitForExitAsync();
            if (_eeProcess.ExitCode != 0) {
                string errorOutput = await _eeProcess.StandardError.ReadToEndAsync();
                throw new InvalidOperationException($"Essence Editor exited with code {_eeProcess.ExitCode}: {errorOutput}");
            }
            string output = await _eeProcess.StandardOutput.ReadToEndAsync();
            if (!string.IsNullOrEmpty(output)) {
                Console.WriteLine(output);
            }
            _eeProcess.Dispose();
            _eeProcess = null;
        }
    }

    private readonly CoH3 _game = gameService.GetGame<CoH3>();
    private readonly Configuration _configuration = configuration;

    public async Task<bool> CreateModArchiveAsync(string modProjectFilePath) { // TODO: More error handling

        string destination = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Company of Heroes 3", "Mods");
        EssenceEditor ee = new();
        ee.Launch(_game.ArchiverExecutable, [
            "--build_mod",
            modProjectFilePath,
            "--build_path",
            destination,
        ]);

        if (ee.FailedToStart) {
            // TODO: Log error
            return false;
        }

        await ee.WaitForExitAsync();

        return true;

    }

}
