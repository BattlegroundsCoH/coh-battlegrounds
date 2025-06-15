using System.Diagnostics;
using System.IO;

using Battlegrounds.Models.Matches;

namespace Battlegrounds.Models.Playing;

public sealed class CoH3AppInstance(Game game) : GameAppInstance {

    private Process? _process;

    public override Game Game => game;

    public override async Task<bool> Launch(params string[] args) {
        
        if (_process is not null) {
            return false;
        }

        string executablePath = game.AppExecutableFullPath;
        string arguments = string.Join(" ", args);
        string workingDirectory = Path.GetDirectoryName(executablePath) ?? string.Empty;
        if (!string.IsNullOrEmpty(workingDirectory)) {
            // TODO: Error handling
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
            return false;
        }

        bool launched = await Task.Run(() => _process.Start());
        if (!launched) {
            _process.Dispose();
            _process = null;
            return false;
        }

        while (!_process.HasExited && _process.MainWindowHandle == IntPtr.Zero) {
            await Task.Delay(50);
        }

        if (_process.HasExited) {
            _process = null;
            return false;
        }

        return true;
        
    }

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
            return new MatchResult {
                Failed = true,
                ErrorMessage = "Replay file not found."
            };
        }

        // TODO: Check for scar errors in warnings log

        return new MatchResult {
            Failed = false,
            ReplayFilePath = replayFilePath
        };

    }

}
