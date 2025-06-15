using System.Diagnostics;
using System.IO;

using Battlegrounds.Models;
using Battlegrounds.Models.Playing;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Services;

public sealed class CoH3ArchiverService(CoH3 game, Configuration configuration, ILogger<CoH3ArchiverService> logger, ILogger<CoH3ArchiverService.EssenceEditor> eeLogger) : IArchiverService {

    private readonly ILogger<CoH3ArchiverService> _logger = logger;
    private readonly ILogger<EssenceEditor> _eeLogger = eeLogger ?? throw new ArgumentNullException(nameof(eeLogger));
    private readonly CoH3 _game = game ?? throw new ArgumentNullException(nameof(game));
    private readonly Configuration _configuration = configuration;

    public sealed class EssenceEditor(ILogger<EssenceEditor> logger) {
        private Process? _eeProcess;
        private bool _failedToStart = false;
        private readonly ILogger<EssenceEditor> _logger = logger;
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
                _logger.LogInformation("Essence Editor output: \n{Output}", output);
            }
            _eeProcess.Dispose();
            _eeProcess = null;
        }
    }


    public async Task<bool> CreateModArchiveAsync(string modProjectFilePath) { // TODO: More error handling

        _logger.LogInformation("Creating mod archive for {ModProjectFilePath}", modProjectFilePath);

        EssenceEditor ee = new(_eeLogger);
        ee.Launch(_game.ArchiverExecutable, [
            "--build_mod",
            $"\"{modProjectFilePath}\"",
            "--build_path",
            $"\"{_configuration.CoH3.ModBuildPath}\"",
            "--no_burn_window" // Prevents the Essence Editor from showing a window during the build process
        ]);

        if (ee.FailedToStart) {
            _logger.LogError("Failed to start Essence Editor. Please ensure it is installed and the path is correct.");
            return false;
        }

        await ee.WaitForExitAsync();

        // Move the mod archive to the correct location
        string archivePath = Path.Combine(_configuration.CoH3.ModBuildPath, "extension", "bg_wincondition", "bg_wincondition.sga");
        if (!File.Exists(archivePath)) {
            _logger.LogError("Mod archive not found at {ArchivePath}", archivePath);
            return false;
        }

        _logger.LogInformation("Mod archive found at {ArchivePath}", archivePath);

        // Maybe use "subscriptions" instead of "local" in the final path?
        string finalArchiveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "my games", "Company of Heroes 3", "mods", "extension", "local", "bg_wincondition");
        string finalArchivePath = Path.Combine(finalArchiveDirectory, "bg_wincondition.sga");
        try {
            if (!Directory.Exists(finalArchiveDirectory)) {
                Directory.CreateDirectory(finalArchiveDirectory); // Ensure the directory exists
            }
            if (File.Exists(finalArchivePath)) {
                File.Delete(finalArchivePath); // Remove existing archive if it exists
            }
            File.Copy(archivePath, finalArchivePath);
            _logger.LogInformation("Mod archive created successfully at {FinalArchivePath}", finalArchivePath);
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to move mod archive to {FinalArchivePath}", finalArchivePath);
            return false;
        }

        return true;

    }

}
