using Battlegrounds.Models.Blueprints.Extensions;

namespace Battlegrounds.Models.Blueprints;

public sealed class EntityBlueprint(string id, HashSet<BlueprintExtension> extensions) : Blueprint(id, extensions) {
 
    public EntityBlueprint() : this(string.Empty, []) {
        // Default constructor for deserialization or empty instances
    }

}
