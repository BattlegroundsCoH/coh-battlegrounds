using Battlegrounds.Models.Companies;

namespace Battlegrounds.Services;

/// <summary>
/// Represents the result of an attempt to save a company.
/// </summary>
/// <remarks>This enumeration is used to indicate the outcome of a company save operation. The result can be one
/// of the following values: <list type="bullet"> <item> <term><see cref="Success"/></term> <description>The operation
/// completed successfully.</description> </item> <item> <term><see cref="FailedSave"/></term> <description>The
/// operation failed during the save process.</description> </item> <item> <term><see cref="FailedSync"/></term>
/// <description>The operation failed during synchronization but a local save was made.</description> </item> </list></remarks>
public enum SaveCompanyResult {

    /// <summary>
    /// Represents the success status of an operation.
    /// </summary>
    /// <remarks>This enumeration can be used to indicate whether an operation completed successfully or
    /// encountered an error.</remarks>
    Success,

    /// <summary>
    /// Represents the result of a failed save operation.
    /// </summary>
    /// <remarks>This enumeration value is typically used to indicate that a save operation did not complete
    /// successfully. It can be used in error handling or logging scenarios to identify the failure.</remarks>
    FailedSave,

    /// <summary>
    /// Represents the result of a failed synchronization operation.
    /// </summary>
    /// <remarks>This type is typically used to encapsulate information about synchronization failures, such
    /// as error details or retry recommendations.</remarks>
    FailedSync,

}

/// <summary>
/// Defines a service for managing company data, including loading, retrieving, saving, synchronizing, and deleting
/// companies.
/// </summary>
/// <remarks>This interface provides methods for interacting with company data stored locally or remotely. It
/// supports asynchronous operations for efficient handling of potentially long-running tasks, such as downloading or
/// synchronizing data. Implementations of this interface may include caching mechanisms to optimize performance and
/// reduce redundant calls to external systems.</remarks>
public interface ICompanyService {

    /// <summary>
    /// Loads all companies from the local file system into the local cache.
    /// </summary>
    /// <returns>The amount of companies that were loaded</returns>
    ValueTask<int> LoadPlayerCompaniesAsync();

    /// <summary>
    /// Gets a company by its ID
    /// </summary>
    /// <param name="companyId">The ID of the company to get</param>
    /// <param name="userId">Optional user ID to specify which user to get the company for, if none is specified, local user is used</param>
    /// <param name="localOnly">Flag if only companies in the local cache should be checked</param>
    /// <returns></returns>
    Task<Company?> GetCompanyAsync(string companyId, string? userId = null, bool localOnly = false);

    /// <summary>
    /// Retrieves all companies that are stored locally.
    /// </summary>
    /// <returns>An enumerable list of locally stored companies</returns>
    Task<IEnumerable<Company>> GetLocalCompaniesAsync();

    /// <summary>
    /// Asynchronously retrieves a cached collection of local companies.
    /// </summary>
    /// <remarks>This method returns the cached data for local companies, which may be used to avoid repeated
    /// calls to external services or databases.  The returned collection represents the current state of the cache and
    /// may not reflect real-time updates.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see
    /// cref="IEnumerable{Company}"/>  representing the cached local companies. If the cache is empty, the result will
    /// be an empty collection.</returns>
    Task<IEnumerable<Company>> GetLocalCompanyCacheAsync();

    /// <summary>
    /// Downloads a company from the remote store and updates the local cache.
    /// </summary>
    /// <param name="companyId">The ID of the company to download</param>
    /// <param name="userId">Optional user ID to specify which user to download from, if none is specified, local user is used</param>
    /// <param name="storeLocally">Flag to determine if the remote company should be stored locally</param>
    /// <returns>The downloaded company</returns>
    Task<Company?> DownloadRemoteCompanyAsync(string companyId, string? userId = null, bool storeLocally = false);

    /// <summary>
    /// Synchronizes the specified company with the remote system.
    /// </summary>
    /// <remarks>This method performs an asynchronous operation to update the remote system with the latest
    /// data for the specified company. Ensure that the <paramref name="company"/> object contains valid and complete
    /// data before calling this method.</remarks>
    /// <param name="company">The company to be synchronized. Cannot be null.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.  The result is <see
    /// langword="true"/> if the synchronization succeeds; otherwise, <see langword="false"/>.</returns>
    ValueTask<bool> SyncCompanyWithRemote(Company company);

    /// <summary>
    /// Deletes the company with the specified identifier.
    /// </summary>
    /// <remarks>This method performs an asynchronous operation to delete a company. Ensure that the company
    /// identifier  provided is valid and corresponds to an existing company. If the company does not exist, the method 
    /// will return <see langword="false"/> without throwing an exception.</remarks>
    /// <param name="companyId">The unique identifier of the company to delete. Cannot be null or empty.</param>
    /// <param name="syncWithRemote">Flag to determine if the deletion should be synchronized with the remote server.
    /// <see langword="true"/> to synchronize with the remote server; otherwise, <see langword="false"/>.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.  The result is <see
    /// langword="true"/> if the company was successfully deleted; otherwise, <see langword="false"/>.</returns>
    ValueTask<bool> DeleteCompany(string companyId, bool syncWithRemote = true);

    /// <summary>
    /// Saves the specified company to the local database and optionally synchronizes it with a remote server.
    /// </summary>
    /// <remarks>If <paramref name="syncWithRemote"/> is <see langword="true"/>, the method attempts to
    /// synchronize the company  with the remote server after saving it locally. The synchronization process may
    /// introduce additional latency.</remarks>
    /// <param name="company">The company to be saved. Cannot be null.</param>
    /// <param name="syncWithRemote">A value indicating whether the company should be synchronized with the remote server. <see langword="true"/> to
    /// synchronize with the remote server; otherwise, <see langword="false"/>.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the asynchronous operation.  The result is <see
    /// langword="true"/> if the company was successfully saved; otherwise, <see langword="false"/>.</returns>
    ValueTask<SaveCompanyResult> SaveCompany(Company company, bool syncWithRemote = true);

}
