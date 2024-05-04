using Battlegrounds.Core.Companies;
using Battlegrounds.Core.Companies.Templates;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Core.Services.Standard;

public class CompanyService(ILogger<CompanyService> logger) : ICompanyService {

    private readonly ILogger<CompanyService> _logger = logger;

    public ICompany[] GetCompanies(string alliance) => [];

    public ICompany GetCompany(string companyId) {
        throw new NotImplementedException();
    }

    public ICompanyTemplate? GetCompanyTemplate(string templateId) {
        throw new NotImplementedException();
    }

}
