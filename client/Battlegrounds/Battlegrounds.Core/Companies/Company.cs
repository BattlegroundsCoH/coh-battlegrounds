using Battlegrounds.Core.Companies.Templates;
using Battlegrounds.Core.Factions;

namespace Battlegrounds.Core.Companies;

public sealed class Company(Guid guid, string name, IFaction faction, ICompanyTemplate template) : ICompany {

    private readonly List<CompanySquad> _squads = [];

    public Guid Id => guid;

    public string Name => name;

    public IFaction Faction => faction;

    public ICompanyTemplate Template => template;

    public IReadOnlyList<ISquad> Squads => _squads;

    public IReadOnlyList<IEquipment> Equipment => throw new NotImplementedException();

    public IReadOnlyList<IDeploymentPhase> DeploymentPhases => throw new NotImplementedException();


}
