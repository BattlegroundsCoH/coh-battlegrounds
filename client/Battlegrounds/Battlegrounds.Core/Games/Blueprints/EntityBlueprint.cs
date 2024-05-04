namespace Battlegrounds.Core.Games.Blueprints;

public sealed class EntityBlueprint(PropertyBagGroupId id, string referenceId) : Blueprint(id, referenceId) {

    public class Builder : BlueprintBaseBuilder<EntityBlueprint> {
        public override EntityBlueprint Build() {
            throw new NotImplementedException();
        }
    }

}
