using System.IO;

using Battlegrounds.Models.Companies;

namespace Battlegrounds.Facades.API;

public interface IBattlegroundsServerAPI {
    
    ValueTask<bool> DeleteCompanyAsync(string companyId);
    
    Task<Company?> GetCompanyAsync(string companyId, string companyUserId);
    
    ValueTask<bool> UploadCompanyAsync(string companyId, string faction, Stream serializedCompanyStream);

}
