namespace Battlegrounds.Core.Companies.Templates;

public sealed class CompanyTemplate(string id, string name, int maxInfantry, int maxSupport, int maxArmour, int maxUnits) : ICompanyTemplate {

    public string Id => id;

    public string Name => name;

    public int MaxInfantry => maxInfantry;

    public int MaxSupport => maxSupport;

    public int MaxArmour => maxArmour;

    public int MaxUnits => maxUnits;

    public bool Validate(ICompany company) {
        throw new NotImplementedException();
    }

    public override string ToString() => id;

}
