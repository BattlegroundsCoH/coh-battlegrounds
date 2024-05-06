using Battlegrounds.Core.Companies;

namespace Battlegrounds.Core.Services;

public interface ICompanyService {

    ICompany? GetCompany(string companyId);

    ICompany? GetCompany(Guid companyId);

    IList<ICompany> GetCompanies(string alliance);

    IList<ICompany> GetCompanies();

    Task<ICompany?> DownloadSyncedCompany(string companyId);

    Task<bool> SynchronizeCompany(ICompany company);

    Task<bool> SaveCompany(ICompany company, Stream outputStream);

    Task<ICompany?> LoadCompanyFromStream(Stream inputStream);

    void RegisterCompany(ICompany company);

}
