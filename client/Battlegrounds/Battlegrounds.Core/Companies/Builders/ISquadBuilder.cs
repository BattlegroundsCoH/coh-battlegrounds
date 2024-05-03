using Battlegrounds.Core.Games.Blueprints;

namespace Battlegrounds.Core.Companies.Builders;

public interface ISquadBuilder {

    ICompanyBuilder AddToCompany();

    ICompanyBuilder AddToCompany(int count);

    ISquad Build();

    ISquad Build(ushort squadIndex);

}
