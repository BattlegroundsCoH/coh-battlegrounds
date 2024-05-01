namespace Battlegrounds.Core.Games.Blueprints;

public abstract class Blueprint(PropertyBagGroupId id, string referenceId) : IBlueprint {

    public PropertyBagGroupId Pbgid { get; } = id;

    public string ScarReferenceId { get; } = referenceId;

    public override bool Equals(object? obj) => obj switch {
        Blueprint bp => this.Pbgid == bp.Pbgid,
        _ => false
    };

    public override int GetHashCode() {
        HashCode hashCode = new HashCode();
        hashCode.Add(Pbgid);
        hashCode.Add(ScarReferenceId);
        return hashCode.ToHashCode();
    }

    public override string ToString() => ScarReferenceId;

}
