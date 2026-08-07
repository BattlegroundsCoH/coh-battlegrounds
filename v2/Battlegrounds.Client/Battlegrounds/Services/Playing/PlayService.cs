using System.IO;
using System.IO.Compression;
using System.Text.Json;

using Battlegrounds.Facades.API;
using Battlegrounds.Models;
using Battlegrounds.Models.Gamemodes;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services.Infrastructure;
using Battlegrounds.Services.Playing.Common;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Services.Playing;

/// <summary>
/// Provides services for building and launching game modes and applications for supported games.
/// </summary>
/// <remarks>This service currently supports operations for Company of Heroes 3. Attempting to use unsupported
/// games will result in a failure. The service is not thread-safe.</remarks>
/// <param name="coh3Archiver">The archiver service used to create and manage mod archives for Company of Heroes 3.</param>
/// <param name="configuration">The configuration settings that control game launch options and behavior.</param>
/// <param name="app">The application instance used to launch game applications.</param>
/// <param name="battlegroundsServerAPI">The API client used to interact with the Battlegrounds server for matchmaking and lobby management.</param>
/// <param name="logger">The logger used to log information and errors during game mode building and launching operations.</param>
public sealed class PlayService(CoH3ArchiverService coh3Archiver, Configuration configuration, BattlegroundsApp app, IBattlegroundsServerAPI battlegroundsServerAPI, ILogger<PlayService> logger) : AbstractPlayService(coh3Archiver) {

    private readonly Configuration _configuration = configuration;
    private readonly BattlegroundsApp _battlegroundsApp = app;
    private readonly IBattlegroundsServerAPI _battlegroundsServerAPI = battlegroundsServerAPI;
    private readonly ILogger<PlayService> _logger = logger;

    public override async Task<LaunchGameAppResult> LaunchGameApp(Game game)
        => game switch {
            CoH3 coh3 => await LaunchCoH3GameApp(coh3),
            _ => (new LaunchGameAppResult() {
                Failed = true,
                ErrorMessage = "Game not supported."
            })
        };

    private async Task<LaunchGameAppResult> LaunchCoH3GameApp(CoH3 coh3) {

        _logger.LogInformation("Launching Company of Heroes 3 game app.");
        GameAppInstance appInstance = new CoH3AppInstance(coh3);
        if (!await appInstance.Launch([])) {
            _logger.LogError("Failed to launch Company of Heroes 3 game app.");
            return new LaunchGameAppResult() {
                Failed = true,
                ErrorMessage = "Failed to launch game app."
            };
        }
        
        _logger.LogInformation("Successfully launched Company of Heroes 3 game app.");
        return new LaunchGameAppResult() {
            Failed = false,
            ErrorMessage = string.Empty,
            GameInstance = appInstance
        };

    }

    public override async Task EnsureModSourceIsAvailable() {

        if (!_battlegroundsApp.IsFirstRun && !_configuration.AutoSyncWinconditionSourceFiles) {
            _logger.LogInformation("Auto-sync of win condition source files is disabled in configuration. Skipping sync.");
            return;
        }

        var storedWinconditionSourceMetadataPath = Path.Combine(_configuration.DocumentsPath, "wc_metadata.json");
        LatestWinconditionDTO storedMetadata;
        if (File.Exists(storedWinconditionSourceMetadataPath)) {
            using var storedMetadataStream = File.OpenRead(storedWinconditionSourceMetadataPath);
            storedMetadata = await JsonSerializer.DeserializeAsync<LatestWinconditionDTO>(storedMetadataStream) ?? throw new Exception("Deserialized metadata was null.");
        } else {
            _logger.LogInformation("No stored win condition source metadata found. Will attempt to fetch latest metadata from server.");
            storedMetadata = new LatestWinconditionDTO(string.Empty, string.Empty, 0, DateTime.MinValue);
        }

        var latestAvailableSource = await _battlegroundsServerAPI.GetLatestWinconditionSourceMetadata();
        if (latestAvailableSource is null) {
            _logger.LogError("Failed to fetch latest win condition source metadata from server.");
            // TODO: Push error to user interface?
            return;
        }

        if (latestAvailableSource.Equals(storedMetadata)) {
            _logger.LogInformation("Stored win condition source is up to date.");
            return;
        }

        _logger.LogInformation("Stored win condition source metadata is outdated. Will attempt to fetch latest metadata from server.");

        var outWinconditionPath = Path.Combine(_configuration.DocumentsPath, "wcs");
        if (Directory.Exists(outWinconditionPath)) {
            Directory.Delete(outWinconditionPath, true);
        }
        Directory.CreateDirectory(outWinconditionPath);

        var outWinconditionFilePath = Path.Combine(outWinconditionPath, "winconditions.zip");
        if (!await _battlegroundsServerAPI.DownloadLatestWinconditionSource(latestAvailableSource.Tag, outWinconditionFilePath)) {
            _logger.LogError("Failed to download latest win condition source from server.");
            return;
        }

        // Extract the downloaded zip file
        using var zipFile = ZipFile.OpenRead(outWinconditionFilePath);
        await zipFile.ExtractToDirectoryAsync(outWinconditionPath, true);

        zipFile.Dispose();
        File.Delete(outWinconditionFilePath);

        // Update configuration
#if DEBUG
        if (_battlegroundsApp.IsFirstRun || string.IsNullOrEmpty(_configuration.CoH3.ModProjectPath)) {
            _configuration.CoH3.ModProjectPath = outWinconditionFilePath.Replace("winconditions.zip", "bg_wincondition.coh3mod");
            _logger.LogDebug("DEBUG MODE: Overriding win condition mod project path with local debug path.");
            _battlegroundsApp.SaveConfiguration();
        } else {
            _logger.LogDebug("DEBUG MODE: Preserving existing win condition mod project path in configuration.");
            _logger.LogDebug("DEBUG MODE: Current win condition mod project path: {ModProjectPath}", _configuration.CoH3.ModProjectPath);
            _logger.LogDebug("DEBUG MODE: New win condition mod project path from latest source: {NewModProjectPath}", outWinconditionFilePath.Replace("winconditions.zip", "bg_wincondition.coh3mod"));
        }
#else
        _configuration.CoH3.ModProjectPath = outWinconditionFilePath.Replace("winconditions.zip", "bg_wincondition.coh3mod");
        _battlegroundsApp.SaveConfiguration();
#endif

        // Update stored metadata
        var newMetadataJson = JsonSerializer.Serialize(latestAvailableSource);
        await File.WriteAllTextAsync(storedWinconditionSourceMetadataPath, newMetadataJson);


    }

}
