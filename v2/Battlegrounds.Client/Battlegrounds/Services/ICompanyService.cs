using Battlegrounds.Models.Companies;

namespace Battlegrounds.Services;

public interface ICompanyService {
    
    Task<List<Company>> GetLocalPlayerCompaniesForFaction(string faction);

}
