namespace Battlegrounds.Core.Games.Blueprints;

public sealed class SquadBlueprint(PropertyBagGroupId id, string referenceId) : Blueprint(id, referenceId) {
    
    public sealed class Builder : BlueprintBaseBuilder<SquadBlueprint> {
        public override SquadBlueprint Build() => new SquadBlueprint(PropertyBagGroupId, ReferenceId);
    }

}
