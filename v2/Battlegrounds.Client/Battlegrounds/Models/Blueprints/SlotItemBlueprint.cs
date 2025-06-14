using Battlegrounds.Models.Blueprints.Extensions;

namespace Battlegrounds.Models.Blueprints;

public sealed class SlotItemBlueprint(string id, HashSet<BlueprintExtension> extensions) : Blueprint(id, extensions) {

    public SlotItemBlueprint() : this(string.Empty, []) {
        // Default constructor for deserialization or empty instances
    }

}
