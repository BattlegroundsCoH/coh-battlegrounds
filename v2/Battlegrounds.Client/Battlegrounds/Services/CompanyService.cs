using System.IO;

using Battlegrounds.Models;
using Battlegrounds.Models.Companies;
using Battlegrounds.Serializers;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Services;

public sealed class CompanyService(
    IUserService userService,
    ICompanyDeserializer companyDeserializer,
    ICompanySerializer companySerializer,
    ILogger<CompanyService> logger,
    Configuration configuration) : ICompanyService {

    private readonly ILogger<CompanyService> _logger = logger;
    private readonly IUserService _userService = userService;
    private readonly ICompanyDeserializer _companyDeserializer = companyDeserializer;
    private readonly ICompanySerializer _companySerializer = companySerializer;
    private readonly Configuration _configuration = configuration;

    // This is the local cache of companies, which is used to avoid unnecessary remote calls.
    private readonly HashSet<Company> _localCompanyCache = [];
    private readonly HashSet<Company> _localCompanies = []; // This is the list of companies that are loaded from the local file system.

    public ValueTask<bool> DeleteCompany(string companyId) {
        throw new NotImplementedException();
    }

    public Task<Company?> DownloadRemoteCompanyAsync(string companyId, string? userId = null, bool storeLocally = false) {
        throw new NotImplementedException();
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
            var userId = await ResolveUserId(null); // Resolve the user ID synchronously for simplicity
            serializedCompanyStream.Seek(0, SeekOrigin.Begin); // Reset the stream position to the beginning
            success = await SyncCompanyWithRemoteInternal(userId, serializedCompanyStream); // Call the internal method to handle the actual synchronization
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
        string userId = await ResolveUserId(null); // Resolve the user ID asynchronously
        return await SyncCompanyWithRemoteInternal(userId, serializedCompanyStream); // Call the internal method to handle the actual synchronization
    }

    private ValueTask<bool> SyncCompanyWithRemoteInternal(string userId, Stream serializedCompanyStream) {
               // This method should handle the actual synchronization with the remote store.
        // It will likely involve making an HTTP request to the remote API with the serialized company data.
        throw new NotImplementedException();
    }

    private async ValueTask<string> ResolveUserId(string? userId) {
        if (!string.IsNullOrEmpty(userId)) {
            return userId;
        }
        var localUser = await _userService.GetLocalUserAsync() ?? throw new InvalidOperationException("No local user found. Please log in first.");
        return localUser.UserId;
    }






    /*public Task<bool> DeleteCompanyFromLocalCache(string companyId) {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteCompanyFromRemoteStore(string companyId) {
        throw new NotImplementedException();
    }

    public Task<Company?> DownloadRemoteCompany(string companyId) {
        throw new NotImplementedException();
    }

    public async Task<Company?> GetCompanyAsync(string companyId) {
        var localVersion = _localCompanyCache.FirstOrDefault(c => c.Id == companyId);
        if (localVersion is not null) {
            return localVersion;
        }
        return await DownloadRemoteCompany(companyId);
    }

    public async Task<IEnumerable<Company>> GetLocalCompanies() {
        if (_localCompanyCache.Count is 0) {
            if (!await SyncCompaniesWithRemote()) {
                throw new InvalidOperationException("Failed to sync companies with remote store.");
            }
        }
        return _localCompanyCache.AsEnumerable();
    }

    public async Task<IEnumerable<Company>> GetLocalPlayerCompaniesForFaction(string faction) {
        if (_syncStatus is SyncStatus.NotSyncedRemotely) {
            var remoteCompanies = await GetPlayerCompaniesRemoteAsync(await _userService.GetLocalUserAsync());
            _localCompanyCache.Clear();
            _localCompanyCache.AddRange(remoteCompanies);
            _isLocalCompanyCacheDirty = false;
        }
        return from company in _localCompanyCache where company.Faction == faction select company;
    }

    public Task<IEnumerable<Company>> GetPlayerCompaniesRemoteAsync(User user) {
        var fakeCompanies = new List<Company> {
            new Company { 
                Id = Guid.CreateVersion7().ToString(),
                Name = "Desert Rats",
                Faction = "british_africa",
                GameId = CoH3.GameId,
                Squads = [
                    new Squad {
                        Id = 1,
                        Blueprint = _blueprintService.GetBlueprint<CoH3, SquadBlueprint>("tommy_uk")
                    },
                    new Squad {
                        Id = 2,
                        Blueprint = _blueprintService.GetBlueprint<CoH3, SquadBlueprint>("tommy_uk")
                    },
                    new Squad {
                        Id = 3,
                        Blueprint = _blueprintService.GetBlueprint<CoH3, SquadBlueprint>("tommy_uk")
                    },
                    new Squad {
                        Id = 4,
                        Blueprint = _blueprintService.GetBlueprint<CoH3, SquadBlueprint>("tommy_uk"),
                        Experience = 2750.0f,
                    },
                    new Squad {
                        Id = 5,
                        Blueprint = _blueprintService.GetBlueprint<CoH3, SquadBlueprint>("tommy_uk"),
                    },
                    new Squad {
                        Id = 17,
                        Blueprint = _blueprintService.GetBlueprint<CoH3, SquadBlueprint>("tommy_uk"),
                        Experience = 5400.0f,
                        Transport = new Squad.TransportSquad(_blueprintService.GetBlueprint<CoH3, SquadBlueprint>("halftrack_m3_uk"), DropOffOnly: true),
                    },
                    new Squad {
                        Id = 19,
                        Blueprint = _blueprintService.GetBlueprint<CoH3, SquadBlueprint>("tommy_uk"),
                        Experience = 5400.0f,
                        Upgrades = [_blueprintService.GetBlueprint<CoH3, UpgradeBlueprint>("lmg_bren_tommy_uk")],
                    },
                    new Squad {
                        Id = 28,
                        Blueprint = _blueprintService.GetBlueprint<CoH3, SquadBlueprint>("matilda_uk"),
                    },
                ]
            },
            new Company {
                Id = Guid.CreateVersion7().ToString(),
                Name = "Afrika Korps",
                Faction = "afrika_korps",
                GameId = CoH3.GameId,
                Squads = [
                    new Squad {
                        Id = 1,
                        Blueprint = _blueprintService.GetBlueprint<CoH3, SquadBlueprint>("panzergrenadier_ak"),
                    },
                    new Squad {
                        Id = 2,
                        Blueprint = _blueprintService.GetBlueprint<CoH3, SquadBlueprint>("panzergrenadier_ak"),
                    },
                    new Squad {
                        Id = 3,
                        Blueprint = _blueprintService.GetBlueprint<CoH3, SquadBlueprint>("panzergrenadier_ak"),
                    },
                    new Squad {
                        Id = 4,
                        Experience = 3000.0f,
                        Blueprint = _blueprintService.GetBlueprint<CoH3, SquadBlueprint>("panzergrenadier_ak"),
                    },
                    new Squad {
                        Id = 5,
                        Blueprint = _blueprintService.GetBlueprint<CoH3, SquadBlueprint>("panzergrenadier_ak"),
                    },
                    new Squad {
                        Id = 6,
                        Blueprint = _blueprintService.GetBlueprint<CoH3, SquadBlueprint>("panzergrenadier_ak"),
                        Experience = 6000.0f,
                        Transport = new Squad.TransportSquad(_blueprintService.GetBlueprint<CoH3, SquadBlueprint>("halftrack_250_ak"), DropOffOnly: true),
                    },
                    new Squad {
                        Id = 7,
                        Blueprint = _blueprintService.GetBlueprint<CoH3, SquadBlueprint>("panzergrenadier_ak"),
                        Experience = 6000.0f,
                        Upgrades = [_blueprintService.GetBlueprint<CoH3, UpgradeBlueprint>("lmg_panzergrenaider_ak")],
                    },
                    new Squad {
                        Id = 8,
                        Blueprint = _blueprintService.GetBlueprint<CoH3, SquadBlueprint>("panzer_iii_ak"),
                    },
                ]
            }
        };
        return Task.FromResult(fakeCompanies.AsEnumerable());
    }

    public int LoadCompaniesFromLocalCache() {
        string[] files = Directory.GetFiles(configuration.CompaniesPath, "*.bgc", SearchOption.TopDirectoryOnly);
        _localCompanyCache.Clear();
        int loaded = 0;
        for (int i = 0; i < files.Length; i++) { 
            
        }
    }

    public Task<bool> SyncCompaniesToRemote() {
        throw new NotImplementedException();
    }

    public Task<bool> SyncCompaniesWithRemote() { // This method should fetch the latest companies from the remote store and update the local cache.
        throw new NotImplementedException();
    }

    public Task<bool> UploadCompanyToRemoteStore(Company company) {
        throw new NotImplementedException();
    }*/

}
