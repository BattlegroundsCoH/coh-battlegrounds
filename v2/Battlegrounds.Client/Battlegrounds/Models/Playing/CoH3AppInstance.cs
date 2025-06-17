using System.Diagnostics;
using System.IO;

using Battlegrounds.Models.Matches;

using Serilog;

namespace Battlegrounds.Models.Playing;

public sealed class CoH3AppInstance(Game game) : GameAppInstance {

    private readonly ILogger _logger = Log.ForContext<CoH3AppInstance>();
    private Process? _process;

    public override Game Game => game;

    public override async Task<bool> Launch(params string[] args) {
        
        if (_process is not null) {
            _logger.Error("Game process is already running. Cannot launch again.");
            return false;
        }

        _logger.Information("Launching game: {GameName} with args: {Args}", game.GameName, string.Join(" ", args));

        string executablePath = game.AppExecutableFullPath;
        string arguments = string.Join(" ", args);
        string workingDirectory = Path.GetDirectoryName(executablePath) ?? string.Empty;
        if (string.IsNullOrEmpty(workingDirectory)) {
            _logger.Error("Working directory is empty, cannot launch game.");
            return false;
        }

        ProcessStartInfo startInfo = new() {
            FileName = executablePath,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = false
        };

        _process = Process.Start(startInfo);
        if (_process is null) {
            _logger.Error("Failed to start game process. Executable: {Executable}, Arguments: {Arguments}", executablePath, arguments);
            return false;
        }

        bool launched = await Task.Run(() => _process.Start());
        if (!launched) {
            _process.Dispose();
            _process = null;
            _logger.Error("Failed to launch game process. Executable: {Executable}, Arguments: {Arguments}", executablePath, arguments);
            return false;
        }

        while (!_process.HasExited && _process.MainWindowHandle == IntPtr.Zero) {
            await Task.Delay(200);
        }

        if (_process.HasExited) {
            _logger.Warning("Game process exited before the main window was created. Executable: {Executable}, Arguments: {Arguments}", executablePath, arguments);
            _process = null;
            return await TryWaitForReplacementProcess();
        }

        return true;
        
    }

    private async Task<bool> TryWaitForReplacementProcess() {

        _logger.Information("Waiting for replacement process to start...");

        int waitTime = 10000; // 10 seconds
        int elapsedTime = 0;
        while (elapsedTime < waitTime) {
            await Task.Delay(250); // Check 1/4th of a second
            elapsedTime += 250;
            Process[] processes = Process.GetProcesses();
            for (int i = 0; i < processes.Length; i++) {
                if (processes[i].ProcessName is "RelicCoH3" or "RelicCoH3.exe" or "Anvil") {
                    _process = processes[i];
                    _logger.Information("Found replacement process: {ProcessId} ({ProcessName}) after {Seconds}s", _process.Id, _process.ProcessName, (elapsedTime / 1000.0));
                    return true;
                }
            }
        }

        return false;

    }

    private const string SCAR_ERROR_INDICATOR = "GameWorld::OnFatalScarError";

    public override async Task<MatchResult> WaitForMatch() {

        if (_process is null) {
            throw new InvalidOperationException("Game process is not running.");
        }

        await _process.WaitForExitAsync();

        int exitCode = _process.ExitCode;
        if (exitCode != 0) {
            // TODO: Check if bugsplat is running and report that
            return new MatchResult {
                Failed = true,
                ErrorMessage = $"Game exited with code {exitCode}."
            };
        }

        string[] replayFiles = Directory.GetFiles(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "my games",
                "Company of Heroes 3",
                "playback"
            ),
            "*.rec",
            SearchOption.TopDirectoryOnly
        );

        string replayFilePath = replayFiles.Length > 0 ? replayFiles[0] : string.Empty;
        for (int i = 1; i < replayFiles.Length; i++) {
            if (File.GetLastWriteTime(replayFiles[i]) > File.GetLastWriteTime(replayFilePath)) {
                replayFilePath = replayFiles[i];
            }
        }

        if (string.IsNullOrEmpty(replayFilePath)) {
            _logger.Warning("No replay file found");
            return new MatchResult {
                Failed = true,
                ErrorMessage = "Replay file not found."
            };
        }

        _logger.Information("Found replay file: {ReplayFileLocation}", replayFilePath);

        string warnings = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "my games", "Company of Heroes 3", "warnings.log");
        if (!File.Exists(warnings)) {
            _logger.Warning("Warnings log file not found: {WarningsLogPath}", warnings);
        } else {
            string contents = await File.ReadAllTextAsync(warnings);
            if (contents.Contains(SCAR_ERROR_INDICATOR)) {
                _logger.Error("Found SCAR error in warnings log: {WarningsLogPath}", warnings);
                return new MatchResult {
                    Failed = true,
                    ScarError = true,
                    ErrorMessage = "SCAR error detected in warnings log."
                };
            } else {
                _logger.Information("No SCAR errors found in warnings log.");
            }
        }

        // TODO: Check for scar errors in warnings log

        return new MatchResult {
            Failed = false,
            ReplayFilePath = replayFilePath
        };

    }

}
