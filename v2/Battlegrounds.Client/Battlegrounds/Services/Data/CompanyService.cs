using System.IO;

using Battlegrounds.Facades.API;
using Battlegrounds.Models;
using Battlegrounds.Models.Companies;
using Battlegrounds.Serializers;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Services.Data;

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
        
    private readonly HashSet<Company> _localCompanyCache = []; // This is the local cache of companies, which is used to avoid unnecessary remote calls.
    private readonly HashSet<Company> _localCompanies = []; // This is the list of companies that are loaded from the local file system.

    public int CompanyCount { get; private set;  }

    public async Task<bool> DeleteCompany(string companyId, bool syncWithRemote = true) {
        if (string.IsNullOrEmpty(companyId)) {
            throw new ArgumentException("Company ID cannot be null or empty.", nameof(companyId));
        }

        string companiesPath = await GetOrCreateLocalUserCompanyPath() ?? throw new InvalidOperationException("Failed to get or create local user company path. Cannot delete company.");

        string companyFilePath = Path.Combine(companiesPath, $"{companyId}.bgc");
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

    public async Task<Company?> DownloadRemoteCompanyAsync(string companyId, string? userId = null, bool storeLocally = false, DownloadProgressUpdateDelegate? downloadProgressUpdate = null) {
        string actualUserId = await ResolveUserId(userId); // Resolve the user ID synchronously for simplicity
        Company? company = await _serverAPI.GetCompanyAsync(companyId, actualUserId); // Download the company from the remote store
        if (company is null) {
            _logger.LogWarning("Company with ID {CompanyId} not found for user {UserId}.", companyId, actualUserId);
            return null;
        }
        _localCompanyCache.Add(company); // Add the downloaded company to the local cache
        if (storeLocally) {
            await SaveCompany(company, syncWithRemote: false); // Save the company locally without syncing with remote
        }
        return company; // Return the downloaded company
    }

    public async ValueTask<Company?> GetCompanyAsync(string companyId, string? userId = null, bool localOnly = false, DownloadProgressUpdateDelegate? downloadProgressUpdate = null) {
        var localCompany = _localCompanyCache.FirstOrDefault(c => c.Id == companyId);
        if (localOnly || localCompany is not null) {
            return localCompany;
        }
        return await DownloadRemoteCompanyAsync(companyId, userId, storeLocally: false, downloadProgressUpdate: downloadProgressUpdate);
    }

    public Task<IEnumerable<Company>> GetLocalCompaniesAsync() => Task.FromResult(_localCompanies.AsEnumerable());

    public Task<IEnumerable<Company>> GetLocalCompanyCacheAsync() => Task.FromResult(_localCompanyCache.AsEnumerable());

    private async Task<string?> GetOrCreateLocalUserCompanyPath() {

        var localUser = await _userService.GetLocalUserAsync();
        if (localUser is null) {
            _logger.LogWarning("No local user found. Cannot load companies without a logged-in user.");
            return null;
        }

        string path = Path.Combine(_configuration.CompaniesPath, localUser.UserId);
        if (!Directory.Exists(path)) {
            try {
                Directory.CreateDirectory(path);
                _logger.LogInformation("Created local company directory for user {UserId} at path {Path}.", localUser.UserId, path);
            } catch (Exception ex) {
                _logger.LogError(ex, "Error creating local company directory for user {UserId} at path {Path}: {ExMessage}", localUser.UserId, path, ex.Message);
                return null;
            }
        }

        return path;

    }

    public async Task<int> LoadPlayerCompaniesAsync() { // This method loads all companies from the local file system into the local cache. (May be asynced in the future)

        string? companiesPath = await GetOrCreateLocalUserCompanyPath();
        if (companiesPath is null) {
            _logger.LogError("Failed to get or create local user company path. Cannot load companies.");
            return 0; // Return 0 if the local user company path could not be created
        }

        _localCompanies.Clear(); // Clear the local companies list before loading
        int loaded = 0;

        string[] companyFiles = Directory.GetFiles(companiesPath, "*.bgc", SearchOption.TopDirectoryOnly);
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
        CompanyCount = loaded; // Update the company count after loading
        return loaded; // Return the number of loaded companies
    }

    public async Task<SaveCompanyResult> SaveCompany(Company company, bool syncWithRemote = true) {
        if (company is null) {
            throw new ArgumentNullException(nameof(company), "Company cannot be null.");
        }
        using var serializedCompanyStream = new MemoryStream();
        _companySerializer.SerializeCompany(serializedCompanyStream, company);
        serializedCompanyStream.Seek(0, SeekOrigin.Begin); // Reset the stream position to the beginning

        string companiesPath = await GetOrCreateLocalUserCompanyPath() ?? throw new InvalidOperationException("Failed to get or create local user company path. Cannot save company.");
        string companyFilePath = Path.Combine(companiesPath, $"{company.Id}.bgc");
        try {
            File.WriteAllBytes(companyFilePath, serializedCompanyStream.ToArray()); // Save the serialized company to a file
            UpdateLocalCompanies(company);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error saving company to file {CompanyFile}: {ExMessage}", companyFilePath, ex.Message);
            return SaveCompanyResult.FailedSave;
        }

        UpdateLocalCompanyCache(company);

        bool success = true;
        if (syncWithRemote) {
            serializedCompanyStream.Seek(0, SeekOrigin.Begin); // Reset the stream position to the beginning
            success = await SyncCompanyWithRemoteInternal(company, serializedCompanyStream); // Call the internal method to handle the actual synchronization
        }

        return success ? SaveCompanyResult.Success : SaveCompanyResult.FailedSync;

    }

    private void UpdateLocalCompanyCache(Company company) {
        if (company is null) {
            throw new ArgumentNullException(nameof(company), "Company cannot be null.");
        }
        _localCompanyCache.RemoveWhere(c => c.Id == company.Id); // Remove the old company from the cache
        _localCompanyCache.Add(company); // Add the updated company to the cache
    }

    private void UpdateLocalCompanies(Company company) {
        if (company is null) {
            throw new ArgumentNullException(nameof(company), "Company cannot be null.");
        }
        _localCompanies.RemoveWhere(c => c.Id == company.Id); // Remove the old company from the cache
        _localCompanies.Add(company); // Add the updated company to the cache
    }

    public async ValueTask<bool> SyncCompanyWithRemote(Company company) {
        if (company is null) {
            throw new ArgumentNullException(nameof(company), "Company cannot be null.");
        }

        var info = await _serverAPI.GetCompanyInfoAsync(company.Id, await ResolveUserId(null));
        if (info is not null) {
            if (company.Version == info.Version) {
                _logger.LogInformation("Company {CompanyId} is already up to date with the server. Server version is {ServerVersion} and local version is {LocalVersion}", company.Id, info.Version, company.Version);
                return true; // Local and server versions match (already synchronized)
            } else if (company.Version < info.Version) {
                _logger.LogWarning("Company {CompanyId} has a newer version on the remote server. Server version is {ServerVersion} and local version is {LocalVersion}", company.Id, info.Version, company.Version);
                return true; // TODO: Mark company as potentially in conflict
            }
        }

        using var serializedCompanyStream = new MemoryStream();
        _companySerializer.SerializeCompany(serializedCompanyStream, company);
        serializedCompanyStream.Seek(0, SeekOrigin.Begin); // Reset the stream position to the beginning
        return await SyncCompanyWithRemoteInternal(company, serializedCompanyStream); // Call the internal method to handle the actual synchronization
    }

    private ValueTask<bool> SyncCompanyWithRemoteInternal(Company company, Stream serializedCompanyStream) {
        return _serverAPI.UploadCompanyAsync(company.Id, $"{company.GameId}_{company.Faction}", company.Version, serializedCompanyStream); // Upload the serialized company to the remote store
    }

    private async ValueTask<string> ResolveUserId(string? userId) {
        if (!string.IsNullOrEmpty(userId)) {
            return userId;
        }
        var localUser = await _userService.GetLocalUserAsync() ?? throw new InvalidOperationException("No local user found. Please log in first.");
        return localUser.UserId;
    }

    public async ValueTask<Company?> ApplyEvents(LinkedList<CompanyEventModifier>? localEvents, Company company, bool commitLocally = false) {

        List<Squad> squads = [.. company.Squads];
        var enumerator = localEvents?.GetEnumerator() ?? throw new ArgumentNullException(nameof(localEvents), "Local events cannot be null.");
        while (enumerator.MoveNext()) {
            CompanyEventModifier modifierEvent = enumerator.Current;
            switch (modifierEvent.EventType) {
                case CompanyEventModifier.EVENT_TYPE_IN_MATCH: {
                    int indexOfSquad = squads.FindIndex(s => s.Id == modifierEvent.SquadId);
                    if (indexOfSquad >= 0) {
                        squads[indexOfSquad] = squads[indexOfSquad].Update(matchCounts: squads[indexOfSquad].MatchCounts + 1); // Update the squad in the list
                        _logger.LogInformation("Squad {SquadId} updated in replay event with match count increment.", modifierEvent.SquadId);
                    } else {
                        _logger.LogWarning("Squad {SquadId} not found for in-match event.", modifierEvent.SquadId);
                    }
                    break;
                }
                case CompanyEventModifier.EVENT_TYPE_KILL_SQUAD: {
                    Squad? squad = squads.FirstOrDefault(s => s.Id == modifierEvent.SquadId);
                    if (squad is not null) {
                        squads.Remove(squad);
                        _logger.LogInformation("Squad {SquadId} killed in replay event.", modifierEvent.SquadId);
                    } else {
                        _logger.LogWarning("Squad {SquadId} not found for killing event.", modifierEvent.SquadId);
                    }
                    break;
                }
                case CompanyEventModifier.EVENT_TYPE_EXPERIENCE_GAIN: {
                    int indexOfSquad = squads.FindIndex(s => s.Id == modifierEvent.SquadId);
                    if (indexOfSquad >= 0) {
                        squads[indexOfSquad] = squads[indexOfSquad].Update(experience: modifierEvent.FloatValue); // Update the squad in the list
                        _logger.LogInformation("Squad {SquadId} gained {Experience} experience in replay event.", modifierEvent.SquadId, modifierEvent.FloatValue);
                    } else {
                        _logger.LogWarning("Squad {SquadId} not found for experience gain event.", modifierEvent.SquadId);
                    }
                    break;
                }
                case CompanyEventModifier.EVENT_TYPE_STATISTICS: {
                    int indexOfSquad = squads.FindIndex(s => s.Id == modifierEvent.SquadId);
                    if (indexOfSquad >= 0) {
                        Squad updatedSquad = squads[indexOfSquad].Update(
                            infantryKills: squads[indexOfSquad].TotalInfantryKills + modifierEvent.IntValue1,
                            vehicleKills: squads[indexOfSquad].TotalVehicleKills + modifierEvent.IntValue2
                        );
                        squads[indexOfSquad] = updatedSquad; // Update the squad in the list
                        _logger.LogInformation("Squad {SquadId} statistics updated in replay event.", modifierEvent.SquadId);
                    } else {
                        _logger.LogWarning("Squad {SquadId} not found for statistics update event.", modifierEvent.SquadId);
                    }
                    break;
                }
                case CompanyEventModifier.EVENT_TYPE_PICKUP: {
                    int indexOfSquad = squads.FindIndex(s => s.Id == modifierEvent.SquadId);
                    if (indexOfSquad >= 0) {
                        throw new NotImplementedException("Pickup event handling is not implemented yet."); // Placeholder for pickup event handling
                    } else {
                        _logger.LogWarning("Squad {SquadId} not found for pickup event.", modifierEvent.SquadId);
                    }
                    break;
                }
                default:
                    _logger.LogWarning("Unknown replay event type: {ReplayEventType}", modifierEvent.EventType);
                    break;
            }
        }

        Company updatedCompany = new Company {
            Id = company.Id,
            Name = company.Name,
            Faction = company.Faction,
            GameId = company.GameId,
            CreatedAt = company.CreatedAt,
            CreatedBy = company.CreatedBy,
            UpdatedAt = DateTime.Now, // Update the timestamp to now
            UpdatedBy = "ReplayEventProcessor", // Indicate that the update was made by the replay event processor
            Squads = squads,
            Version = company.Version + 1 // Increment the version number
        };

        if (commitLocally) {
            if (await SaveCompany(updatedCompany, syncWithRemote: false) != SaveCompanyResult.Success) {
                _logger.LogError("Failed to commit changes to the local company file for company {CompanyId}.", company.Id);
                return null; // Return null => indicating that the company was not updated successfully
            }
        }

        _logger.LogInformation("Applied {EventCount} replay events to company {CompanyId}.", localEvents?.Count ?? 0, company.Id);
        return updatedCompany; // Return true if events were successfully applied and company was updated

    }

    public async Task SyncWithServerAsync() {

        if (!_configuration.AutoSyncCompanies) {
            _logger.LogInformation("Auto-syncing of companies with remote server is disabled in configuration. Skipping synchronization.");
            return;
        }

        if (!await _userService.IsUserLoggedIn) {
            _logger.LogWarning("No user is currently logged in. Skipping synchronization with remote server.");
            return;
        }

        _logger.LogInformation("Starting synchronization of local companies with remote server. Local company count: {LocalCompanyCount}", _localCompanyCache.Count);

        if (! await _serverAPI.IsServerAvailableAsync()) {
            _logger.LogWarning("Remote server is not available. Skipping synchronization.");
            return; // Exit early if the server is not available
        }

        foreach (var company in _localCompanyCache.ToList()) {
            bool success = await SyncCompanyWithRemote(company);
            if (success) {
                _logger.LogInformation("Successfully synchronized company {CompanyId} with remote server.", company.Id);
            } else {
                _logger.LogError("Failed to synchronize company {CompanyId} with remote server.", company.Id);
            }
        }
        _logger.LogInformation("Completed synchronization of local companies with remote server.");

        _logger.LogInformation("Checking if server has unsynchronized companies");

        var user = await ResolveUserId(null);
        var userCompanyInfo = await _serverAPI.GetUserCompanyInfoAsync(user);
        if (userCompanyInfo is not null) {

            foreach (var faction in userCompanyInfo.Companies) {
                foreach (var company in faction.Value) {

                    if (!_localCompanies.Any(x => x.Id == company.Id)) {
                        _logger.LogInformation("Detected a company on remote server that is not available locally... fetching company {CompanyId}", company.Id);
                        var remoteCompany = await DownloadRemoteCompanyAsync(company.Id, user, true);
                        if (remoteCompany is not null) {
                            _localCompanies.Add(remoteCompany);
                            _localCompanyCache.Add(remoteCompany);
                        }
                    }

                }
            }

        }

        _logger.LogInformation("Finished syncing companies with server");

    }

}
