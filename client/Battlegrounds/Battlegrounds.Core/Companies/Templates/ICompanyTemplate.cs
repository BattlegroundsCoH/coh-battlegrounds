namespace Battlegrounds.Core.Companies.Templates;

public interface ICompanyTemplate {

    string Id { get; }

    string Name { get; }

    int MaxInfantry { get; }

    int MaxSupport { get; }

    int MaxArmour { get; }

    int MaxUnits { get; }

    bool Validate(ICompany company);

}
