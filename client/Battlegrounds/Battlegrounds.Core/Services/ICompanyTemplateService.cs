using Battlegrounds.Core.Companies.Templates;

namespace Battlegrounds.Core.Services;

public interface ICompanyTemplateService {

    ICompanyTemplate? GetCompanyTemplate(string templateId);

}
