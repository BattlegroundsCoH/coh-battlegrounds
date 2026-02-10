using System.IO;

using Battlegrounds.Models.Companies;
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
    
    ValueTask<bool> DeleteCompanyAsync(string companyId);
    
    Task<Company?> GetCompanyAsync(string companyId, string companyUserId);
    
    Task<IEnumerable<BrowserLobby>> GetLobbiesAsync();

    Task<bool> IsServerAvailableAsync();

    ValueTask<bool> ReportMatchResults(MatchResult result);

    ValueTask<bool> UploadCompanyAsync(string companyId, string faction, Stream serializedCompanyStream);
    
    /// <summary>
    /// Uploads a game mode from the specified location to the server asynchronously.
    /// </summary>
    /// <remarks>Throws an exception if the specified location is invalid or if an error occurs during the
    /// upload process.</remarks>
    /// <param name="lobbyId">The unique identifier of the lobby associated with the game mode.</param>
    /// <param name="gamemodeLocation">The file path or URL of the game mode to upload. This parameter must be a valid, non-null string.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the upload
    /// succeeds; otherwise, <see langword="false"/>.</returns>
    Task<bool> UploadGamemodeAsync(string lobbyId, string gamemodeLocation);

}
