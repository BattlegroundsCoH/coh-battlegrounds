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

    /// <summary>
    /// Represents an item assigned to a slot, including its quantity and associated blueprint information.
    /// </summary>
    /// <remarks>Either EntityBlueprint or SlotItemBlueprint is used depending on the game context. Only one
    /// is typically relevant for a given slot item.</remarks>
    /// <param name="CompanyItemId">The number of items represented by this slot item. Must be zero or greater.</param>
    /// <param name="EntityBlueprint">The entity blueprint associated with the slot item, or null if not applicable. Used for Company of Heroes 3.</param>
    /// <param name="SlotItemBlueprint">The slot item blueprint associated with the slot item, or null if not applicable. Used for Company of Heroes 2.</param>
    public sealed record SlotItem(int CompanyItemId, EntityBlueprint? EntityBlueprint, SlotItemBlueprint? SlotItemBlueprint); // EntityBlueprint for CoH3, SlotItemBlueprint for CoH2 because reasons...

    /// <summary>
    /// Represents a transport squad configuration, including its blueprint and whether it is limited to drop-off
    /// operations.
    /// </summary>
    /// <param name="TransportBlueprint">The blueprint that defines the transport squad's composition and capabilities. Cannot be null.</param>
    /// <param name="DropOffOnly">true if the squad is restricted to drop-off operations only; otherwise, false.</param>
    public sealed record TransportSquad(SquadBlueprint TransportBlueprint, bool DropOffOnly);

    /// <summary>
    /// Represents a group of passengers identified by a unique squad identifier.
    /// </summary>
    /// <param name="PassengerSquadId">The unique identifier for the passenger squad.</param>
    public sealed record PassengerSquad(int PassengerSquadId) {
        
        /// <summary>
        /// Retrieves the squad with the specified passenger squad ID from the given company.
        /// </summary>
        /// <param name="company">The company from which to retrieve the squad. Cannot be null.</param>
        /// <returns>The squad that matches the passenger squad ID.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no squad with the specified passenger squad ID exists in the company.</exception>
        public Squad GetSquad(Company company) => company.Squads.FirstOrDefault(x => x.Id == PassengerSquadId) ?? throw new InvalidOperationException($"No squad found with ID {PassengerSquadId}");

    }

    /// <summary>
    /// Represents a weapon that has been captured, including its associated entity blueprint and the blueprint of the
    /// crew operating it.
    /// </summary>
    /// <param name="CompanyItemId">The unique identifier for the company item associated with the captured weapon.</param>
    /// <param name="WeaponEntityBlueprint">The blueprint that defines the captured weapon entity. May be null if no weapon is associated.</param>
    /// <param name="CrewBlueprint">The blueprint that defines the crew assigned to the captured weapon. May be null if no crew is assigned.</param>
    public sealed record CaptureInfo(int CompanyItemId, EntityBlueprint? WeaponEntityBlueprint, SquadBlueprint? CrewBlueprint);

    private readonly string _name = string.Empty;
    private readonly HashSet<SlotItem> _slotItems = [];
    private readonly HashSet<UpgradeBlueprint> _upgrades = [];

    private TransportSquad? _transport = null;
    private PassengerSquad? _pasenger = null;
    private CaptureInfo? _capturedWeapon = null;

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

    /// <summary>
    /// Gets the passenger squad associated with this instance.
    /// </summary>
    public PassengerSquad? Passenger {
        get => _pasenger;
        init => _pasenger = value;
    }

    /// <summary>
    /// Gets a value indicating whether a passenger is associated with this instance.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Passenger))]
    public bool HasPassenger => _pasenger is not null;

    /// <summary>
    /// Gets the information about the weapon that was captured, if any.
    /// </summary>
    public CaptureInfo? CapturedWeapon {
        get => _capturedWeapon;
        init => _capturedWeapon = value;
    }

    /// <summary>
    /// Gets a value indicating whether a weapon has been captured.
    /// </summary>
    /// <remarks>When this property is <see langword="true"/>, the <c>CapturedWeapon</c> property is
    /// guaranteed to be non-null.</remarks>
    [MemberNotNullWhen(true, nameof(CapturedWeapon))]
    public bool IsCapturedWeapon => _capturedWeapon is not null;

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

    public Squad Update(float? experience = null, int? matchCounts = null, int? infantryKills = null, int? vehicleKills = null, List<SlotItem>? slotItems = null, PassengerSquad? passenger = null) {
        return new Squad() {
            Id = this.Id,
            Name = this.Name,
            Experience = experience ?? this.Experience,
            Phase = this.Phase,
            AddedToCompanyAt = this.AddedToCompanyAt,
            LastUpdatedAt = DateTime.UtcNow, // Update the last updated time
            TotalInfantryKills = infantryKills ?? this.TotalInfantryKills,
            TotalVehicleKills = vehicleKills ?? this.TotalVehicleKills,
            MatchCounts = matchCounts ?? this.MatchCounts,
            Blueprint = this.Blueprint,
            SlotItems = slotItems ?? [.. this.SlotItems], // Use existing slot items if not provided
            Upgrades = this.Upgrades, // Keep existing upgrades
            Transport = this.Transport, // Keep existing transport squad if any
            Passenger = passenger ?? this.Passenger, // Keep existing passenger squad if any 
            CapturedWeapon = this.CapturedWeapon,
        };
    }

}
