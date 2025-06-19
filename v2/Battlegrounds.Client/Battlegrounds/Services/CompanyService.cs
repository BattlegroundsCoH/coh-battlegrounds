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
        
    private readonly HashSet<Company> _localCompanyCache = []; // This is the local cache of companies, which is used to avoid unnecessary remote calls.
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

    public async ValueTask<SaveCompanyResult> SaveCompany(Company company, bool syncWithRemote = true) {
        if (company is null) {
            throw new ArgumentNullException(nameof(company), "Company cannot be null.");
        }
        using var serializedCompanyStream = new MemoryStream();
        _companySerializer.SerializeCompany(serializedCompanyStream, company);
        serializedCompanyStream.Seek(0, SeekOrigin.Begin); // Reset the stream position to the beginning

        string companyFilePath = Path.Combine(_configuration.CompaniesPath, $"{company.Id}.bgc");
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
        using var serializedCompanyStream = new MemoryStream();
        _companySerializer.SerializeCompany(serializedCompanyStream, company);
        serializedCompanyStream.Seek(0, SeekOrigin.Begin); // Reset the stream position to the beginning
        return await SyncCompanyWithRemoteInternal(company, serializedCompanyStream); // Call the internal method to handle the actual synchronization
    }

    private ValueTask<bool> SyncCompanyWithRemoteInternal(Company company, Stream serializedCompanyStream) {
        return _serverAPI.UploadCompanyAsync(company.Id, $"{company.GameId}_{company.Faction}", serializedCompanyStream); // Upload the serialized company to the remote store
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
            UpdatedAt = DateTime.Now, // Update the timestamp to now
            Squads = squads,
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

}
