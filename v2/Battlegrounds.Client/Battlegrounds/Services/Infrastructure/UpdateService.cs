using Battlegrounds.Models;

using Microsoft.Extensions.Logging;

using Velopack;
using Velopack.Sources;

namespace Battlegrounds.Services.Infrastructure;

/// <summary>
/// Provides functionality to check for, download, and install application updates using a GitHub repository as the
/// update source.
/// </summary>
/// <remarks>This service is intended to be used as a singleton and manages update detection and installation for
/// the application. Update operations are logged for diagnostic purposes. Thread safety is ensured by the service's
/// design, but update checks and installations should not be performed concurrently.</remarks>
/// <param name="configuration">The application configuration containing settings required to access the GitHub repository for updates.</param>
/// <param name="logger">The logger used to record informational and error messages during update operations.</param>
public sealed class UpdateService(Configuration configuration, ILogger<UpdateService> logger) : IUpdateService {

    private readonly ILogger<UpdateService> _logger = logger;
    private readonly UpdateManager _mgr = new UpdateManager(new GithubSource(configuration.GithubRepository, null, false));

    private bool? _hasDetectedUpdates;
    private UpdateInfo? _updateInfo;

    public async Task<bool> CheckForUpdatesAsync() {

        if (_hasDetectedUpdates.HasValue) {
            return _hasDetectedUpdates.Value;
        }

#if DEBUG
        return false;
#else
        try {

            if (!mgr.IsInstalled) {
                _hasDetectedUpdates = false;
                _logger.LogInformation("No installation found, skipping update check");
                return false;
            }

            var update = await mgr.CheckForUpdatesAsync();
            if (update is null) {
                _hasDetectedUpdates = false;
                _logger.LogInformation("No updates found");
                return false;
            }

            _logger.LogInformation("Update found: {Version}", update.TargetFullRelease.Version);
            _updateInfo = update;
            _hasDetectedUpdates = true;
            return true;

        } catch (Exception ex) { 
            _logger.LogError(ex, "Failed to check for updates");
            return false;
        }
#endif

    }

    public async Task DownloadAndInstallUpdate() {

        if (!_hasDetectedUpdates.HasValue || !_hasDetectedUpdates.Value || _updateInfo is null) {
            return;
        }

        try {

            _logger.LogInformation("Downloading update {Version}", _updateInfo.TargetFullRelease.Version);

            await _mgr.DownloadUpdatesAsync(_updateInfo);

            _logger.LogInformation("Installing update {Version}", _updateInfo.TargetFullRelease.Version);

            _mgr.ApplyUpdatesAndRestart(_updateInfo);

        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to download and install update");
        }

    }

}
