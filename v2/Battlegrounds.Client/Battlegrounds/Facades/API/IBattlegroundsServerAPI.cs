using System.IO;

using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Gamemodes;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Replays;

namespace Battlegrounds.Facades.API;

/// <summary>
/// Defines the contract for interacting with the Battlegrounds server API, providing methods for managing companies,
/// lobbies, and game modes.
/// </summary>
/// <remarks>This interface includes methods for deleting companies, retrieving company information, checking
/// server availability, reporting match results, and uploading game modes. Implementations of this interface should
/// ensure proper error handling and validation of inputs.</remarks>
public interface IBattlegroundsServerAPI {
    
    /// <summary>
    /// Asynchronously deletes the company with the specified identifier.
    /// </summary>
    /// <param name="companyId">The unique identifier of the company to delete. Cannot be null or empty.</param>
    /// <returns>A value task that represents the asynchronous operation. The result is <see langword="true"/> if the company was
    /// successfully deleted; otherwise, <see langword="false"/>.</returns>
    ValueTask<bool> DeleteCompanyAsync(string companyId);
    
    /// <summary>
    /// Asynchronously retrieves the company information associated with the specified company and user identifiers.
    /// </summary>
    /// <param name="companyId">The unique identifier of the company to retrieve.</param>
    /// <param name="companyUserId">The identifier of the user within the company context. Used to determine access or scope for the company data.</param>
    /// <param name="progressUpdate">An optional delegate that receives progress updates during the download operation. Can be null if progress
    /// updates are not required.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the company information if found;
    /// otherwise, null.</returns>
    Task<Company?> GetCompanyAsync(string companyId, string companyUserId, DownloadProgressUpdateDelegate? progressUpdate = null);
    
    /// <summary>
    /// Asynchronously retrieves company information for the specified company and user.
    /// </summary>
    /// <param name="companyId">The unique identifier of the company whose information is to be retrieved. Cannot be null or empty.</param>
    /// <param name="companyUserId">The unique identifier of the user associated with the company. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="CompanyInfo"/> object
    /// with the company details if found; otherwise, <see langword="null"/>.</returns>
    Task<CompanyInfo?> GetCompanyInfoAsync(string companyId, string companyUserId);

    /// <summary>
    /// Retrieves the company information associated with the specified user asynchronously.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose company information is to be retrieved. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="UserCompanyInfo"/>
    /// object with the user's company information, or <see langword="null"/> if no company information is found for the
    /// specified user.</returns>
    Task<UserCompanyInfo?> GetUserCompanyInfoAsync(string userId);

    /// <summary>
    /// Asynchronously retrieves a collection of available browser lobbies. 
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see
    /// cref="BrowserLobby"/> objects representing the available lobbies. The collection is empty if no lobbies are
    /// found.</returns>
    Task<IEnumerable<BrowserLobby>> GetLobbiesAsync();

    /// <summary>
    /// Asynchronously determines whether the server is currently available for requests.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the server is
    /// available; otherwise, <see langword="false"/>.</returns>
    Task<bool> IsServerAvailableAsync();

    /// <summary>
    /// Submits the specified match results to the server asynchronously.
    /// </summary>
    /// <param name="result">The match result data to be reported. Must contain all required information for the match outcome.</param>
    /// <param name="progressUpdate">An optional delegate that receives progress updates during the upload operation. If null, progress updates are
    /// not reported.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the match
    /// results were successfully reported; otherwise, <see langword="false"/>.</returns>
    ValueTask<bool> ReportMatchResults(MatchResult result, UploadProgressUpdateDelegate? progressUpdate = null);

    /// <summary>
    /// Asynchronously uploads a company profile to the server using the provided serialized data stream.
    /// </summary>
    /// <remarks>The method does not close or dispose the provided stream. Callers are responsible for
    /// managing the stream's lifetime. The upload may be performed in multiple stages, and progress updates are
    /// reported if a delegate is provided.</remarks>
    /// <param name="companyId">The unique identifier of the company to upload. Cannot be null or empty.</param>
    /// <param name="faction">The faction to which the company belongs. Cannot be null or empty.</param>
    /// <param name="version">The version number of the company data being uploaded. This should be incremented with each update to ensure proper versioning and concurrency control.</param>
    /// <param name="serializedCompanyStream">A stream containing the serialized company data to upload. The stream must be readable and positioned at the
    /// start of the data.</param>
    /// <param name="progressUpdate">An optional delegate that receives progress updates during the upload operation. If null, progress updates are
    /// not reported.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the upload
    /// succeeds; otherwise, <see langword="false"/>.</returns>
    ValueTask<bool> UploadCompanyAsync(string companyId, string faction, uint version, Stream serializedCompanyStream, UploadProgressUpdateDelegate? progressUpdate = null);
    
    /// <summary>
    /// Uploads a game mode from the specified location to the server asynchronously.
    /// </summary>
    /// <remarks>Throws an exception if the specified location is invalid or if an error occurs during the
    /// upload process.</remarks>
    /// <param name="lobbyId">The unique identifier of the lobby associated with the game mode.</param>
    /// <param name="gamemodeLocation">The file path or URL of the game mode to upload. This parameter must be a valid, non-null string.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the upload
    /// succeeds; otherwise, <see langword="false"/>.</returns>
    Task<bool> UploadGamemodeAsync(string lobbyId, string gamemodeLocation, UploadProgressUpdateDelegate? progressUpdate = null);

    /// <summary>
    /// Downloads the game mode associated with the specified lobby asynchronously and saves it to the given destination
    /// path.
    /// </summary>
    /// <remarks>This method performs the download asynchronously. Exceptions may be thrown if the download
    /// fails due to network issues or invalid parameters.</remarks>
    /// <param name="lobbyId">The unique identifier of the lobby from which the game mode will be downloaded. Cannot be null or empty.</param>
    /// <param name="destinationPath">The file system path where the downloaded game mode will be saved. Must be a valid, writable path.</param>
    /// <param name="progressUpdate">An optional delegate that receives periodic updates on the download progress as a percentage. If provided, it
    /// will be invoked during the download operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the download is
    /// successful; otherwise, <see langword="false"/>.</returns>
    Task<bool> DownloadGamemodeAsync(string lobbyId, string destinationPath, DownloadProgressUpdateDelegate? progressUpdate = null);
    
    /// <summary>
    /// Asynchronously retrieves the most recent match result for the specified lobby.
    /// </summary>
    /// <param name="lobbyId">The unique identifier of the lobby for which to retrieve the latest match result. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the latest match result for the
    /// specified lobby, or null if no match has been played.</returns>
    Task<MatchResult?> GetLatestMatchResult(string lobbyId);
    
    /// <summary>
    /// Asynchronously retrieves the metadata for the most recent win condition source.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="LatestWinconditionDTO"/> instance with metadata for the latest win condition source.</returns>
    Task<LatestWinconditionDTO> GetLatestWinconditionSourceMetadata();
    
    /// <summary>
    /// Downloads the latest wincondition source file for the specified tag and saves it to the provided output path.
    /// </summary>
    /// <param name="tag">The tag identifying the version of the wincondition source to download. Cannot be null or empty.</param>
    /// <param name="outWinconditionPath">The file system path where the downloaded wincondition source will be saved. Must be a valid writable path.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the download
    /// succeeds; otherwise, <see langword="false"/>.</returns>
    Task<bool> DownloadLatestWinconditionSource(string tag, string outWinconditionPath);

}
