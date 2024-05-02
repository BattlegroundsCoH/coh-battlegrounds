namespace Battlegrounds.Core.Companies;

public interface IDeploymentPhase {

    int Priority { get; }

    IReadOnlySet<ushort> Squads { get; }

}
