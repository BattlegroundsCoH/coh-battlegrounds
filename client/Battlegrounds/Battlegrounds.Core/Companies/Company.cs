using Battlegrounds.Core.Companies.Templates;
using Battlegrounds.Core.Games.Factions;

namespace Battlegrounds.Core.Companies;

public sealed class Company(Guid guid, string name, IFaction faction, ICompanyTemplate template, IList<ISquad> squads, IList<IEquipment> equipment, IList<IDeploymentPhase> phases) : ICompany {

    public Guid Id => guid;
    
    public string Name => name;

    public IFaction Faction => faction;

    public ICompanyTemplate Template => template;

    public IReadOnlyList<ISquad> Squads => (IReadOnlyList<ISquad>)squads;

    public IReadOnlyList<IEquipment> Equipment => (IReadOnlyList<IEquipment>)equipment;

    public IReadOnlyList<IDeploymentPhase> DeploymentPhases => (IReadOnlyList<IDeploymentPhase>)phases;


}
