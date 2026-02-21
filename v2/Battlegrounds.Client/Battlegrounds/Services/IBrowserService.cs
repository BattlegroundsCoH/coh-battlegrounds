namespace Battlegrounds.Services;

/// <summary>
/// Provides functionality to open a URL in the default web browser.
/// </summary>
public interface IBrowserService {

    /// <summary>
    /// Opens the specified URL in the default web browser.
    /// </summary>
    /// <remarks>This method launches the default web browser to navigate to the specified URL.  Ensure that
    /// the URL is properly formatted and accessible.</remarks>
    /// <param name="url">The URL to open. Must be a valid, well-formed URI.</param>
    void OpenUrl(string url);

}
