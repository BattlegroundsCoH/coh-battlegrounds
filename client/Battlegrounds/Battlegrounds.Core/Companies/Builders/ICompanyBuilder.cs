namespace Battlegrounds.Core.Companies.Builders;

public interface ICompanyBuilder {

    ICompanyBuilder AddSquad(ISquad squad);
    ICompanyBuilder AddSquad(ISquadBuilder squadBuilder);

    ICompany Build();

}
