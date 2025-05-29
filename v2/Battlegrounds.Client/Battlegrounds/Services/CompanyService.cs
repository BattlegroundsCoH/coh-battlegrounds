using Battlegrounds.Models;
using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

public sealed class CompanyService(IUserService userService, IBlueprintService blueprintService, IGameService gameService) : ICompanyService {

    private readonly IBlueprintService _blueprintService = blueprintService;
    private readonly IGameService _gameService = gameService;
    private readonly IUserService _userService = userService;
    private readonly List<Company> _localCompanyCache = [];
    private bool _isLocalCompanyCacheDirty = true;

    public Task<Company?> GetCompanyAsync(string companyId) {
        var localVersion = _localCompanyCache.FirstOrDefault(c => c.Id == companyId);
        if (localVersion is not null) {
            return Task.FromResult<Company?>(localVersion);
        }
        // TODO: Implement remote company retrieval
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Company>> GetLocalPlayerCompaniesForFaction(string faction) {
        if (_isLocalCompanyCacheDirty) {
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

}
