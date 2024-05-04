using Battlegrounds.Core.Companies;
using Battlegrounds.Core.Companies.Templates;

namespace Battlegrounds.Core.Services;

public interface ICompanyService {

    ICompany GetCompany(string companyId);

    ICompany[] GetCompanies(string alliance);

    ICompanyTemplate? GetCompanyTemplate(string templateId);

}
