using Battlegrounds.Core.Companies.Templates;
using Battlegrounds.Core.Factions;

namespace Battlegrounds.Core.Companies;

public interface ICompany {

    Guid Id { get; }

    string Name { get; }

    IFaction Faction { get; }

    ICompanyTemplate Template { get; }

    IReadOnlyList<ISquad> Squads { get; }

    IReadOnlyList<IEquipment> Equipment { get; }

    IReadOnlyList<IDeploymentPhase> DeploymentPhases { get; }

}
