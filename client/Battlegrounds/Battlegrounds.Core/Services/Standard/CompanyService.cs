using Battlegrounds.Core.Companies;
using Battlegrounds.Core.Companies.Serializing;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Core.Services.Standard;

public class CompanyService(ILogger<CompanyService> logger, ICompanySerializer companySerializer) : ICompanyService {

    private readonly ILogger<CompanyService> _logger = logger;
    private readonly ICompanySerializer _companySerializer = companySerializer;

    public Task<ICompany?> DownloadSyncedCompany(string companyId) {
        throw new NotImplementedException();
    }

    public IList<ICompany> GetCompanies(string alliance) => [];

    public IList<ICompany> GetCompanies() {
        throw new NotImplementedException();
    }

    public ICompany GetCompany(string companyId) {
        throw new NotImplementedException();
    }

    public Task<ICompany> LoadCompanyFromFile(Stream stream) {
        throw new NotImplementedException();
    }

    public void RegisterCompany(ICompany company) {
        throw new NotImplementedException();
    }

    public Task<bool> SaveCompany(ICompany company) {
        throw new NotImplementedException();
    }

    public Task<bool> SynchronizeCompany(ICompany company) {
        throw new NotImplementedException();
    }

}
