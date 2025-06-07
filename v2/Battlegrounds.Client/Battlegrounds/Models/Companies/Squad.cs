using System.Diagnostics.CodeAnalysis;

using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Blueprints.Extensions;

namespace Battlegrounds.Models.Companies;

/// <summary>
/// Represents the various phases of a squad's engagement in a battle.
/// </summary>
/// <remarks>The phases define the progression of a squad's involvement, starting from reserves and moving through
/// skirmish, battle, and starting phases. This enumeration can be used to track or control the current state of a squad
/// during gameplay.</remarks>
public enum SquadPhase : byte {

    /// <summary>
    /// Represents the default phase and is the last phase in which a squad can be deployed. (Deployable after 10 minutes)
    /// </summary>
    ReservesPhase = 0,

    /// <summary>
    /// Represents the initial phase where (early game - first 5 minutes)
    /// </summary>
    SkirmishPhase = 1,

    /// <summary>
    /// Represents the main phase of battle (mid-game - deployable after 5 minutes)
    /// </summary>
    BattlePhase = 2,

    /// <summary>
    /// Squad is deployed immediately at the start of the math without waiting for manual deployment.
    /// </summary>
    StartingPhase = 3,

}

/// <summary>
/// Class representing a squad within a company, which can be deployed in battles.
/// </summary>
public sealed class Squad {

    public sealed record SlotItem(int Count, UpgradeBlueprint? UpgradeBlueprint, SlotItemBlueprint? SlotItemBlueprint); // UpgradeBlueprint for CoH3, SlotItemBlueprint for CoH2 because reasons...
    public sealed record TransportSquad(SquadBlueprint TransportBlueprint, bool DropOffOnly);

    private readonly string _name = string.Empty;
    private readonly HashSet<SlotItem> _slotItems = [];
    private readonly HashSet<UpgradeBlueprint> _upgrades = [];

    private TransportSquad? _transport = null;

    /// <summary>
    /// Gets or initializes the unique identifier of this squad.
    /// </summary>
    public int Id { get; init; } = 0;

    /// <summary>
    /// Get or initializes the custom name of this squad.
    /// </summary>
    public string Name {
        get => _name;
        init => _name = value ?? string.Empty;
    }

    /// <summary>
    /// Gets a value indicating whether a custom name has been assigned.
    /// </summary>
    public bool HasCustomName => !string.IsNullOrEmpty(_name);

    /// <summary>
    /// Gets or initializes the experience of this squad.
    /// </summary>
    public float Experience { get; init; } = 0f;

    /// <summary>
    /// Gets the current rank of this squad based on its experience and the veterancy extension defined in the blueprint.
    /// </summary>
    public int Rank => Blueprint.TryGetExtension<VeterancyExtension>(out var veterancy) ? veterancy.GetRank(Experience) : 0;

    /// <summary>
    /// Gets or initializes the deployment phase of this squad within the company.
    /// </summary>
    public SquadPhase Phase { get; init; } = SquadPhase.ReservesPhase;

    /// <summary>
    /// Gets or initializes the date and time when this squad was added to the company.
    /// </summary>
    public DateTime AddedToCompanyAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or initializes the date and time when this squad was last updated.
    /// </summary>
    public DateTime LastUpdatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or initializes the total number of infantry kills this squad has achieved.
    /// </summary>
    public int TotalInfantryKills { get; init; } = 0;

    /// <summary>
    /// Gets or initializes the total number of vehicle kills this squad has achieved.
    /// </summary>
    public int TotalVehicleKills { get; init; } = 0;

    /// <summary>
    /// Gets the total number of kills this squad has achieved, both infantry and vehicles.
    /// </summary>
    public int TotalKills => TotalInfantryKills + TotalVehicleKills;

    /// <summary>
    /// Gets or initializes the number of matches this squad has participated in.
    /// </summary>
    public int MatchCounts { get; init; } = 0;

    /// <summary>
    /// Gets or initializes the blueprint that defines the characteristics and abilities of this squad.
    /// </summary>
    public required SquadBlueprint Blueprint { get; init; } = null!;

    /// <summary>
    /// Gets the collection of slot items associated with this instance.
    /// </summary>
    public IReadOnlyList<SlotItem> SlotItems {
        get => _slotItems.ToList().AsReadOnly();
        init {
            _slotItems.Clear();
            if (value is not null) {
                foreach (var item in value) {
                    if (item is not null) {
                        _slotItems.Add(item);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets the collection of upgrade blueprints associated with the current instance.
    /// </summary>
    /// <remarks>The property is initialized with a collection of valid upgrade blueprints. Null values within
    /// the  provided collection are ignored during initialization.</remarks>
    public IReadOnlyList<UpgradeBlueprint> Upgrades {
        get => _upgrades.ToList().AsReadOnly();
        init {
            _upgrades.Clear();
            if (value is not null) {
                foreach (var item in value) {
                    if (item is not null) {
                        _upgrades.Add(item);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets or initializes the transport squad associated with this instance.
    /// </summary>
    public TransportSquad? Transport {
        get => _transport;
        init => _transport = value;
    }

    /// <summary>
    /// Gets a value indicating whether a transport instance is available.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Transport))]
    public bool HasTransport => _transport is not null;

    public override bool Equals(object? obj) {
        if (obj is Squad other) {
            return Id == other.Id;
        }
        return false;
    }

    public override int GetHashCode() {
        return Id.GetHashCode();
    }

    public override string ToString() {
        return $"({Blueprint.Id}) - Phase: {Phase}, Rank: {Rank}, Experience: {Experience:F2}";
    }

}
