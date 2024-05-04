namespace Battlegrounds.Core.Companies;

public class DeploymentPhase(int priority, ISet<ushort> squads) : IDeploymentPhase {

    public int Priority => priority;

    public IReadOnlySet<ushort> Squads => (IReadOnlySet<ushort>)squads;

}
