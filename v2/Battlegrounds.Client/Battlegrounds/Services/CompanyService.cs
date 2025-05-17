
using Battlegrounds.Models.Companies;

namespace Battlegrounds.Services;

public sealed class CompanyService : ICompanyService {

    public Task<List<Company>> GetLocalPlayerCompaniesForFaction(string faction) {
        return Task.FromResult(new List<Company>());
    }

}
