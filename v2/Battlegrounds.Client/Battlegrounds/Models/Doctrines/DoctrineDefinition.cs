using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Companies;

namespace Battlegrounds.Models.Doctrines;

/// <summary>
/// Represents a doctrine definition that specifies unit type limits, phase restrictions, blueprint inclusions, and cost
/// modifiers with hierarchical inheritance support.
/// </summary>
/// <remarks>Properties automatically merge with parent doctrine values when accessed. Local values override
/// parent values during merge operations.</remarks>
public sealed class DoctrineDefinition {

    private readonly Dictionary<string, int> _typeLimits = [];
    private readonly BlueprintReference<SquadBlueprint>? _crewBlueprint;
    private readonly PhaseLimitsDefinition? _phaseLimits;
    private readonly BlueprintInclusionDefinition? _includeInclusion;
    private readonly CostModifiersDefinition? _costModifiers;

    public string Id { get; init; } = string.Empty;

    public int Version { get; init; } = 1;

    public string Hash { get; init; } = string.Empty;

    public string Faction { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public bool IsVisible { get; init; } = true;

    public Lazy<DoctrineDefinition>? Parent { get; init; }

    public Dictionary<string, int> TypeLimits {
        get {
            if (Parent is null) {
                return _typeLimits;
            }
            var allLimits = new Dictionary<string, int>(Parent.Value.TypeLimits);
            foreach (var kvp in _typeLimits) {
                allLimits[kvp.Key] = kvp.Value;
            }
            return allLimits;
        }
        init => _typeLimits = value;
    }

    public BlueprintReference<SquadBlueprint> CrewBlueprint {
        get => _crewBlueprint is null ? Parent?.Value.CrewBlueprint ?? throw new InvalidOperationException("Crew blueprint is not defined in this doctrine or any parent doctrine.") : _crewBlueprint;
        init => _crewBlueprint = value;
    }

    public PhaseLimitsDefinition PhaseLimits {
        get => _phaseLimits is null ? Parent?.Value.PhaseLimits ?? new() : _phaseLimits;
        init => _phaseLimits = value;
    }

    public BlueprintInclusionDefinition Blueprints {
        get => Parent is null ? _includeInclusion ?? new() : Parent.Value.Blueprints.MergeWith(_includeInclusion ?? new());
        init => _includeInclusion = value;
    }

    public List<SquadBlueprint> ExclusiveSquadBlueprints {
        get {
            if (Parent is null)
                return [];
            var exclusiveSquads = (_includeInclusion?.Squads ?? []).ToHashSet();
            exclusiveSquads.ExceptWith(Parent.Value.Blueprints.Squads);
            return [.. exclusiveSquads.Select(x => x.Blueprint!)];
        }
    }

    /// <summary>
    /// Gets or initializes the cost modifiers.
    /// </summary>
    /// <remarks>When a parent exists, the returned value is the parent's cost modifiers merged with the local
    /// cost modifiers. When no parent exists, returns the local cost modifiers or a new instance if not set.</remarks>
    public CostModifiersDefinition CostModifiers {
        get => Parent is null ? _costModifiers ?? new() : Parent.Value.CostModifiers.MergeWith(_costModifiers ?? new());
        init => _costModifiers = value;
    }

    /// <summary>
    /// The technology tree definition for this doctrine.
    /// </summary>
    public DoctrineTechTreeDefinition? TechTree { get; init; }

    public bool IsValid(Company company) => throw new NotImplementedException();

    /// <summary>
    /// Defines limit values for different game phases including initial, skirmish, battle, and reserves.
    /// </summary>
    public sealed class PhaseLimitsDefinition {
        public int Initial { get; init; } = 0;
        public int Skirmish { get; init; } = 0;
        public int Battle { get; init; } = 0;
        public int Reserves { get; init; } = 0;
    }

    /// <summary>
    /// Defines blueprints to be included in a collection, containing references to squads and upgrades.
    /// </summary>
    public sealed class BlueprintInclusionDefinition {

        /// <summary>
        /// Collection of blueprint references to squads.
        /// </summary>
        public List<BlueprintReference<SquadBlueprint>> Squads { get; init; } = [];

        /// <summary>
        /// Gets the collection of upgrade blueprint references.
        /// </summary>
        public List<BlueprintReference<UpgradeBlueprint>> Upgrades { get; init; } = [];

        /// <summary>
        /// Merges this instance with another, combining their Squads and Upgrades.
        /// </summary>
        /// <param name="other">The instance to merge with.</param>
        /// <returns>A new instance containing the combined Squads and Upgrades from both instances.</returns>
        public BlueprintInclusionDefinition MergeWith(BlueprintInclusionDefinition other) {
            return new BlueprintInclusionDefinition {
                Squads = [.. this.Squads, .. other.Squads],
                Upgrades = [.. this.Upgrades, .. other.Upgrades]
            };
        }

    }

    /// <summary>
    /// Defines cost modifiers for squads and upgrades.
    /// </summary>
    public sealed class CostModifiersDefinition {

        /// <summary>
        /// Gets the squad cost modifiers.
        /// </summary>
        public Dictionary<string, CostModifier> Squads { get; init; } = [];

        /// <summary>
        /// Gets the upgrade cost modifiers.
        /// </summary>
        public Dictionary<string, CostModifier> Upgrades { get; init; } = [];

        /// <summary>
        /// Merges with another definition to create a new combined definition.
        /// </summary>
        /// <remarks>When duplicate keys exist, values from the current instance take
        /// precedence.</remarks>
        /// <param name="other">The definition to merge with.</param>
        /// <returns>A new <see cref="CostModifiersDefinition"/> containing all squads and upgrades from both definitions.</returns>
        public CostModifiersDefinition MergeWith(CostModifiersDefinition other) {
            var merged = new CostModifiersDefinition();
            List<KeyValuePair<string, CostModifier>> allSquads = [.. other.Squads, .. this.Squads];
            foreach (var kvp in allSquads) {
                merged.Squads[kvp.Key] = kvp.Value;
            }
            List<KeyValuePair<string, CostModifier>> allUpgrades = [.. other.Upgrades, .. this.Upgrades];
            foreach (var kvp in allUpgrades) {
                merged.Upgrades[kvp.Key] = kvp.Value;
            }
            return merged;
        }

        /// <summary>
        /// Represents cost modifiers for resource types.
        /// </summary>
        public sealed class CostModifier {

            /// <summary>
            /// Gets or sets the manpower multiplier.
            /// </summary>
            public float Manpower { get; set; } = 1.0f;

            /// <summary>
            /// Gets or sets the munitions multiplier.
            /// </summary>
            public float Munitions { get; set; } = 1.0f;

            /// <summary>
            /// Gets or sets the fuel multiplier.
            /// </summary>
            public float Fuel { get; set; } = 1.0f;

        }

    }

}
