using System.Diagnostics;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Services.Infrastructure;

/// <summary>
/// Provides functionality to open URLs in the default web browser.
/// </summary>
/// <remarks>This service logs the attempt to open a URL and handles any errors that occur during the process. It
/// ensures that the provided URL is valid and non-empty before attempting to open it.</remarks>
/// <param name="logger">The service logger instance</param>
public sealed class BrowserService(ILogger<BrowserService> logger) : IBrowserService {

    private readonly ILogger<BrowserService> _logger = logger;

    /// <summary>
    /// Opens the specified URL in the default web browser.
    /// </summary>
    /// <remarks>This method uses the system's default web browser to open the specified URL.  If the
    /// operation fails, an exception is logged and rethrown.</remarks>
    /// <param name="url">The URL to open. This must be a valid, non-empty string.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="url"/> is null, empty, or consists only of whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown if an error occurs while attempting to open the URL.</exception>
    public void OpenUrl(string url) {
        if (string.IsNullOrWhiteSpace(url)) {
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
        }
        try {
            _logger.LogInformation("Opening URL: {Url}", url);
            Process.Start(new ProcessStartInfo {
                FileName = url,
                UseShellExecute = true
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to open URL: {Url}", url);
            throw new InvalidOperationException($"Could not open URL: {url}", ex);
        }
    }

}
