using System.IO;

using Battlegrounds.Facades.API;
using Battlegrounds.Models;
using Battlegrounds.Models.Companies;
using Battlegrounds.Serializers;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Services;

public sealed class CompanyService(
    IUserService userService,
    ICompanyDeserializer companyDeserializer,
    ICompanySerializer companySerializer,
    IBattlegroundsServerAPI serverAPI,
    ILogger<CompanyService> logger,
    Configuration configuration) : ICompanyService {

    private readonly ILogger<CompanyService> _logger = logger;
    private readonly IBattlegroundsServerAPI _serverAPI = serverAPI;
    private readonly IUserService _userService = userService;
    private readonly ICompanyDeserializer _companyDeserializer = companyDeserializer;
    private readonly ICompanySerializer _companySerializer = companySerializer;
    private readonly Configuration _configuration = configuration;

    // This is the local cache of companies, which is used to avoid unnecessary remote calls.
    private readonly HashSet<Company> _localCompanyCache = [];
    private readonly HashSet<Company> _localCompanies = []; // This is the list of companies that are loaded from the local file system.

    public async ValueTask<bool> DeleteCompany(string companyId, bool syncWithRemote = true) {
        if (string.IsNullOrEmpty(companyId)) {
            throw new ArgumentException("Company ID cannot be null or empty.", nameof(companyId));
        }
        string companyFilePath = Path.Combine(_configuration.CompaniesPath, $"{companyId}.bgc");
        if (!File.Exists(companyFilePath)) {
            _logger.LogWarning("Company file {CompanyFile} does not exist.", companyFilePath);
            return false; // Return false if the company file does not exist
        }
        try {
            File.Delete(companyFilePath); // Delete the company file from the local file system
            _localCompanyCache.RemoveWhere(c => c.Id == companyId); // Remove from the local cache
            _localCompanies.RemoveWhere(c => c.Id == companyId); // Remove from the local companies list
        } catch (Exception ex) {
            _logger.LogError(ex, "Error deleting company file {CompanyFile}: {ExMessage}", companyFilePath, ex.Message);
            return false; // Return false if deletion failed
        }
        if (syncWithRemote) {
            return await _serverAPI.DeleteCompanyAsync(companyId); // Sync with remote store
        }
        return true; // Return true if deletion was successful and no remote sync is needed
    }

    public async Task<Company?> DownloadRemoteCompanyAsync(string companyId, string? userId = null, bool storeLocally = false) {
        string actualUserId = await ResolveUserId(userId); // Resolve the user ID synchronously for simplicity
        Company? company = await _serverAPI.GetCompanyAsync(companyId, actualUserId); // Download the company from the remote store
        if (company is null) {
            _logger.LogWarning("Company with ID {CompanyId} not found for user {UserId}.", companyId, actualUserId);
            return null;
        }
        if (storeLocally) {
            await SaveCompany(company, syncWithRemote: false); // Save the company locally without syncing with remote
        }
        return company; // Return the downloaded company
    }

    public async Task<Company?> GetCompanyAsync(string companyId, string? userId = null, bool localOnly = false) {
        var localCompany = _localCompanyCache.FirstOrDefault(c => c.Id == companyId);
        if (localOnly || localCompany is not null) {
            return localCompany;
        }
        string actualUserId = await ResolveUserId(userId);
        return await DownloadRemoteCompanyAsync(companyId, actualUserId, storeLocally: false);
    }

    public Task<IEnumerable<Company>> GetLocalCompaniesAsync() => Task.FromResult(_localCompanies.AsEnumerable());

    public Task<IEnumerable<Company>> GetLocalCompanyCacheAsync() => Task.FromResult(_localCompanyCache.AsEnumerable());

    public ValueTask<int> LoadPlayerCompaniesAsync() { // This method loads all companies from the local file system into the local cache. (May be asynced in the future)
        _localCompanies.Clear(); // Clear the local companies list before loading
        int loaded = 0;
        string[] companyFiles = Directory.GetFiles(_configuration.CompaniesPath, "*.bgc", SearchOption.TopDirectoryOnly);
        for (int i = 0; i < companyFiles.Length; i++) {
            string companyFile = companyFiles[i];
            try {
                using var stream = File.OpenRead(companyFile);
                Company company = _companyDeserializer.DeserializeCompany(stream) ?? throw new InvalidDataException($"Failed to deserialize company from file: {companyFile}");
                _localCompanyCache.Add(company);
                _localCompanies.Add(company); // Add to the local companies list as well
                loaded++;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error loading company from file {CompanyFile}: {ExMessage}", companyFile, ex.Message);
            }
        }
        return ValueTask.FromResult(loaded); // Return the number of loaded companies
    }

    public async ValueTask<bool> SaveCompany(Company company, bool syncWithRemote = true) {
        if (company is null) {
            throw new ArgumentNullException(nameof(company), "Company cannot be null.");
        }
        using var serializedCompanyStream = new MemoryStream();
        _companySerializer.SerializeCompany(serializedCompanyStream, company);
        serializedCompanyStream.Seek(0, SeekOrigin.Begin); // Reset the stream position to the beginning

        string companyFilePath = Path.Combine(_configuration.CompaniesPath, $"{company.Id}.bgc");
        try {
            File.WriteAllBytes(companyFilePath, serializedCompanyStream.ToArray()); // Save the serialized company to a file
        } catch (Exception ex) {
            _logger.LogError(ex, "Error saving company to file {CompanyFile}: {ExMessage}", companyFilePath, ex.Message);
            return false; // Return false if saving failed
        }

        bool success = true;
        if (syncWithRemote) {
            serializedCompanyStream.Seek(0, SeekOrigin.Begin); // Reset the stream position to the beginning
            success = await SyncCompanyWithRemoteInternal(company.Id, company.Faction, serializedCompanyStream); // Call the internal method to handle the actual synchronization
        }

        if (success) {
            _localCompanyCache.Add(company); // Add the company to the local cache
            _localCompanies.Add(company); // Add to the local companies list as well
        }
        return success;

    }

    public async ValueTask<bool> SyncCompanyWithRemote(Company company) {
        if (company is null) {
            throw new ArgumentNullException(nameof(company), "Company cannot be null.");
        }
        using var serializedCompanyStream = new MemoryStream();
        _companySerializer.SerializeCompany(serializedCompanyStream, company);
        serializedCompanyStream.Seek(0, SeekOrigin.Begin); // Reset the stream position to the beginning
        return await SyncCompanyWithRemoteInternal(company.Id, company.Faction, serializedCompanyStream); // Call the internal method to handle the actual synchronization
    }

    private ValueTask<bool> SyncCompanyWithRemoteInternal(string companyId, string faction, Stream serializedCompanyStream) {
        return _serverAPI.UploadCompanyAsync(companyId, faction, serializedCompanyStream); // Upload the serialized company to the remote store
    }

    private async ValueTask<string> ResolveUserId(string? userId) {
        if (!string.IsNullOrEmpty(userId)) {
            return userId;
        }
        var localUser = await _userService.GetLocalUserAsync() ?? throw new InvalidOperationException("No local user found. Please log in first.");
        return localUser.UserId;
    }

}
