using System.IO;

using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Replays;

namespace Battlegrounds.Facades.API;

public interface IBattlegroundsServerAPI {
    
    ValueTask<bool> DeleteCompanyAsync(string companyId);
    
    Task<Company?> GetCompanyAsync(string companyId, string companyUserId);

    ValueTask<bool> IsServerAvailableAsync();

    ValueTask<bool> ReportMatchResults(MatchResult result);

    ValueTask<bool> UploadCompanyAsync(string companyId, string faction, Stream serializedCompanyStream);

}
