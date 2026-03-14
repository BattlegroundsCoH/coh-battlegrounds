namespace Battlegrounds.Services;

/// <summary>
/// Defines methods for checking, downloading, and installing software updates asynchronously.
/// </summary>
/// <remarks>Implementations of this interface provide mechanisms to detect available updates and apply them.
/// Methods are asynchronous and may involve network operations or require elevated permissions, depending on the update
/// source and environment.</remarks>
public interface IUpdateService {

    /// <summary>
    /// Checks asynchronously whether a newer version of the application is available.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if an update is
    /// available; otherwise, <see langword="false"/>.</returns>
    Task<bool> CheckForUpdatesAsync();

    /// <summary>
    /// Downloads the latest available update and installs it asynchronously.
    /// </summary>
    /// <remarks>The update process may require elevated permissions depending on the system configuration.
    /// The method completes when the update has been successfully installed or if an error occurs during the
    /// process.</remarks>
    /// <returns>A task that represents the asynchronous download and installation operation.</returns>
    Task DownloadAndInstallUpdate();

}
