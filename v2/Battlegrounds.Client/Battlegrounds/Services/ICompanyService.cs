using Battlegrounds.Models;
using Battlegrounds.Models.Companies;

namespace Battlegrounds.Services;

public interface ICompanyService {
    
    Task<Company?> GetCompanyAsync(string companyId);

    Task<IEnumerable<Company>> GetLocalPlayerCompaniesForFaction(string faction);

    Task<IEnumerable<Company>> GetPlayerCompaniesRemoteAsync(User user);

}
