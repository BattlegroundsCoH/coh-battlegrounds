using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Battlegrounds.Game;

/// <summary>
/// Static class for launching Company of Heroes 2 with the proper set of arguments.
/// </summary>
public static class CoH2Launcher {

    /// <summary>
    /// Const ID for marking process as having run to completion
    /// </summary>
    public const int PROCESS_OK = 0;

    /// <summary>
    /// Const ID for marking process not found
    /// </summary>
    public const int PROCESS_NOT_FOUND = 1;

    /// <summary>
    /// The steam app ID for Company of Heroes 2.
    /// </summary>
    public const string GameAppID = "231430";

    /// <summary>
    /// Launch Company of Heroes 2 through Steam
    /// </summary>
    public static bool Launch() {

        // Create argument(s) - more can be given later if needed
        StringBuilder commandline = new StringBuilder();
        commandline.Append($"-applaunch {GameAppID} ");

        // Start the process
        ProcessStartInfo startInfo = new ProcessStartInfo {
            FileName = Pathfinder.GetOrFindSteamPath(),
            Arguments = commandline.ToString()
        };

        // Try start it
        try {
            Process.Start(startInfo);
        } catch {

            // if err, return false
            return false;

        }

        // Return true -> is started
        return true;

    }

    /// <summary>
    /// Watch the RelicCoH2.exe process
    /// </summary>
    /// <returns>Integer value representing the result of the operation (0 = OK, 1 = Not found)</returns>
    public static int WatchProcess() {

        // The attempts counter
        int attemps = 0;

        // The processes
        Process? proc;

        // Log start time
        var start = DateTime.Now;
        double sec;

        // While we havent found it and there are still attempts to make
        do {

            // Try get proc
            proc = GetCoH2Process();

            // Wait 5ms
            Thread.Sleep(5);

            // Increase attempts so it's not an endless loop
            attemps++;
            
            // Update amount of seconds
            sec = (DateTime.Now - start).TotalSeconds;

        } while (sec < 40 && proc is null);

        // None found?
        if (proc is null) {
            Trace.WriteLine($"Failed to detect a running instance of RelicCoH2.exe after {sec:0.00}s", nameof(CoH2Launcher));
            return PROCESS_NOT_FOUND;
        }

        // Log found
        Trace.WriteLine($"Successfully detected a running instance of RelicCoH2.exe after {sec:0.00}s", nameof(CoH2Launcher));

        // Wait for exit
        proc.WaitForExit();

        // Return OK
        return PROCESS_OK;

    }

    private static Process? GetCoH2Process() {

        // Try and find it
        var processes = Process.GetProcessesByName("RelicCoH2");

        // If found - break
        if (processes.Length > 0) {
            return processes[0];
        }

        // None found -> return null
        return null;

    }

    /// <summary>
    /// Get if Company of Heroes 2 is running.
    /// </summary>
    /// <returns>If the game is running then <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public static bool IsRunning() => GetCoH2Process() is not null;

}
