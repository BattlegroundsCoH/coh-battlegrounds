using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using Battlegrounds.Functional;
using Battlegrounds.Game;
using Battlegrounds.Logging;

namespace Battlegrounds.Compiler;

/// <summary>
/// Class representing an interface for interacting with the Battlegrounds archiver
/// </summary>
public sealed class BattlegroundsArchiver : IArchiver {

    private static readonly Logger logger = Logger.CreateLogger();

    private readonly string toolDirectory;
    private readonly GameCase game;

    /// <summary>
    /// Initialise a new <see cref="BattlegroundsArchiver"/> instance.
    /// </summary>
    /// <param name="toolDirectory">The path to the actual tool to execute through the Battlegrounds arhciver</param>
    /// <param name="game">The archiver game mode to target</param>
    public BattlegroundsArchiver(string toolDirectory, GameCase game) {
        this.toolDirectory = toolDirectory;
        this.game = game;
    }

    /// <inheritdoc/>
    public bool Archive(string archiveDefinitionPath)
        => ArchiveInternal(archiveDefinitionPath, Array.Empty<string>());

    /// <inheritdoc/>
    public bool Archive(string archiveDefinitionPath, Dictionary<string, string> optionalArguments) 
        => ArchiveInternal(archiveDefinitionPath, optionalArguments.MapValues((k,v) => new string[] { k, v }).Flatten());

    private bool ArchiveInternal(string definitionPath, string[] arguments) {

        string[] args = new string[] { "-tool_dir", toolDirectory, "-def", definitionPath }.Concat(arguments)
            .IfTrue(_ => game is GameCase.CompanyOfHeroes3)
            .Then(x => x.Append("-coh3"));

        using Process archiveProcess = new Process {
            StartInfo = new ProcessStartInfo() {
                FileName = "battlegrounds-archiver.exe",
                Arguments = string.Join(' ', args),
                RedirectStandardOutput = true,
                RedirectStandardInput = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false,
            },
            EnableRaisingEvents = true,
        };

        archiveProcess.OutputDataReceived += Process_OutputDataReceived;

        try {

            if (!archiveProcess.Start()) {
                return false;
            }

            archiveProcess.BeginOutputReadLine();

            Thread.Sleep(1000);

            archiveProcess.WaitForExit();

            if (archiveProcess.ExitCode != 0) {
                int eCode = archiveProcess.ExitCode;
                logger.Warning($"Archiver has finished with error code = {eCode}");
                return false;
            }

        } catch {
            return false;
        }

        return true;

    }

    private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e) {
        if (e.Data != null && e.Data != string.Empty && e.Data != " ")
            logger.Info(e.Data);
    }

}
