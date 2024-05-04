namespace Battlegrounds.Core.Games.Blueprints;

public sealed class UpgradeBlueprint(PropertyBagGroupId id, string referenceId) : Blueprint(id, referenceId) {

    public sealed class Builder : BlueprintBaseBuilder<UpgradeBlueprint> {
        public override UpgradeBlueprint Build() => new UpgradeBlueprint(PropertyBagGroupId, ReferenceId);
    }

}
