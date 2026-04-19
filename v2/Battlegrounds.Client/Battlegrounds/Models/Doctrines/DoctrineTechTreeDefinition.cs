using Battlegrounds.Models.Blueprints;

namespace Battlegrounds.Models.Doctrines;

public sealed class DoctrineTechTreeDefinition {

    public List<TechItem> Items { get; init; } = [];

    public List<TechGraphEdge> Graph { get; init; } = [];

    public sealed class TechGraphEdge {
        public string From { get; init; } = string.Empty;
        public List<string> To { get; init; } = []; // Then leads to a set of nodes, where all can be chosen (non-exclusive)
        public List<string>? Choice { get; init; } = []; // Then leads to a set of nodes, where only one can be chosen (exclusive)
    }

    public sealed class TechItem {
        
        public string Id { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public int RequiredExperience { get; init; } = 0;

        public List<TechItemModifier> Modifiers { get; init; } = [];

    }

    public abstract class TechItemModifier;

    public sealed class TechItemGrantAbilityModifier : TechItemModifier {
        public BlueprintReference<AbilityBlueprint> Ability { get; init; } = BlueprintReference<AbilityBlueprint>.None;
    }

    public sealed class TechItemModifyDoctrineTypeLimitModifier : TechItemModifier {
        public string TypeId { get; init; } = string.Empty;
        public int NewLimit { get; init; }
    }

    public sealed class TechItemGrantBuildingModifier : TechItemModifier {
        public BlueprintReference<EntityBlueprint> Building { get; init; } = BlueprintReference<EntityBlueprint>.None;
    }

}
