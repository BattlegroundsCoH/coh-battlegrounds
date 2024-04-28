using Battlegrounds.Core.Companies;

namespace Battlegrounds.Core.Services;

public interface ICompanyService {

    ICompany GetCompany(string companyId);

    ICompany[] GetCompanies(string alliance);

}
