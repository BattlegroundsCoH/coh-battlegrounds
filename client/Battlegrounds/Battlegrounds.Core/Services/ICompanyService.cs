using Battlegrounds.Core.Companies;

namespace Battlegrounds.Core.Services;

public interface ICompanyService {

    ICompany GetCompany(string companyId);

    IList<ICompany> GetCompanies(string alliance);

    IList<ICompany> GetCompanies();

    Task<ICompany?> DownloadSyncedCompany(string companyId);

    Task<bool> SynchronizeCompany(ICompany company);

    Task<bool> SaveCompany(ICompany company);

    Task<ICompany> LoadCompanyFromFile(Stream stream);

    void RegisterCompany(ICompany company);

}
