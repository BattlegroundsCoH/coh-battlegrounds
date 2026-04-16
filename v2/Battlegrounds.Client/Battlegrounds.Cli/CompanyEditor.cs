using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Companies;

namespace Battlegrounds.Cli;

/// <summary>
/// Mutable wrapper around a <see cref="Company"/> that tracks changes and can produce an updated immutable
/// <see cref="Company"/> via <see cref="BuildCompany"/>.
/// </summary>
internal sealed class CompanyEditor {

    private readonly List<Squad> _startingUnits = [];
    private readonly List<Squad> _skirmishPhaseUnits = [];
    private readonly List<Squad> _battlePhaseUnits = [];
    private readonly List<Squad> _reservesPhaseUnits = [];
    private readonly List<CapturedItem> _capturedItems = [];

    private readonly string _companyId;
    private readonly string _name;
    private readonly string _faction;
    private readonly string _gameId;
    private readonly string _doctrineId;
    private readonly uint _doctrineVersion;
    private readonly DateTime _createdAt;
    private readonly string _createdBy;
    private readonly uint _originalVersion;

    public string LastSavedBy { get; set; } = string.Empty;

    public bool HasUnsavedChanges { get; private set; } = false;

    /// <summary>
    /// Initialises the editor from an existing company.
    /// </summary>
    public CompanyEditor(Company company) {
        _companyId = company.Id;
        _name = company.Name;
        _faction = company.Faction;
        _gameId = company.GameId;
        _doctrineId = company.DoctrineId;
        _doctrineVersion = company.DoctrineVersion;
        _createdAt = company.CreatedAt;
        _createdBy = company.CreatedBy;
        _originalVersion = company.Version;
        LastSavedBy = company.UpdatedBy;

        foreach (var squad in company.Squads) {
            GetPhaseList(squad.Phase).Add(squad);
        }
        _capturedItems.AddRange(company.CapturedItems);
    }

    // -------------------------------------------------------------------------
    // Read-only views
    // -------------------------------------------------------------------------

    public string CompanyName => _name;
    public string Faction => _faction;
    public string GameId => _gameId;

    public IReadOnlyList<Squad> Squads =>
        [.. _startingUnits, .. _skirmishPhaseUnits, .. _battlePhaseUnits, .. _reservesPhaseUnits];

    public IReadOnlyList<CapturedItem> CapturedItems => _capturedItems.AsReadOnly();

    // -------------------------------------------------------------------------
    // Squad operations
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adds a new squad with the given blueprint and phase.
    /// </summary>
    public Squad AddSquad(SquadBlueprint blueprint, SquadPhase phase, Squad.TransportSquad? transport = null) {
        var squad = new Squad {
            Id = GetNextSquadId(),
            Blueprint = blueprint,
            Phase = phase,
            Transport = transport,
            AddedToCompanyAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
        };
        GetPhaseList(phase).Add(squad);
        HasUnsavedChanges = true;
        return squad;
    }

    /// <summary>
    /// Removes the squad with the given ID. Returns true if found and removed.
    /// </summary>
    public bool RemoveSquad(int squadId) {
        foreach (var list in AllPhaseLists()) {
            var target = list.Find(s => s.Id == squadId);
            if (target is not null) {
                list.Remove(target);
                // Clear captured-item references pointing to this squad
                for (int i = 0; i < _capturedItems.Count; i++) {
                    if (_capturedItems[i].CapturedBySquadId == squadId) {
                        _capturedItems[i] = new CapturedItem {
                            Id = _capturedItems[i].Id,
                            ItemBlueprint = _capturedItems[i].ItemBlueprint,
                            CapturedBySquadId = -1,
                            CapturedAt = _capturedItems[i].CapturedAt,
                        };
                    }
                }
                HasUnsavedChanges = true;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Updates mutable fields of an existing squad. Pass null for parameters that should not change.
    /// </summary>
    public bool UpdateSquad(int squadId, string? name = null, float? experience = null,
        SquadPhase? phase = null, List<UpgradeBlueprint>? upgrades = null) {
        foreach (var list in AllPhaseLists()) {
            int index = list.FindIndex(s => s.Id == squadId);
            if (index < 0)
                continue;

            Squad old = list[index];
            SquadPhase newPhase = phase ?? old.Phase;

            Squad updated = new Squad {
                Id = old.Id,
                Name = name ?? old.Name,
                Experience = experience ?? old.Experience,
                Phase = newPhase,
                Blueprint = old.Blueprint,
                Transport = old.Transport,
                Passenger = old.Passenger,
                CapturedWeapon = old.CapturedWeapon,
                SlotItems = old.SlotItems.ToList(),
                Upgrades = upgrades ?? old.Upgrades.ToList(),
                AddedToCompanyAt = old.AddedToCompanyAt,
                LastUpdatedAt = DateTime.UtcNow,
                MatchCounts = old.MatchCounts,
                TotalInfantryKills = old.TotalInfantryKills,
                TotalVehicleKills = old.TotalVehicleKills,
            };

            list.RemoveAt(index);
            GetPhaseList(newPhase).Add(updated);

            HasUnsavedChanges = true;
            return true;
        }
        return false;
    }

    // -------------------------------------------------------------------------
    // Captured-item operations
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adds a new captured item with the given entity blueprint.
    /// </summary>
    public CapturedItem AddCapturedItem(EntityBlueprint blueprint, int capturedBySquadId = -1) {
        var item = new CapturedItem {
            Id = GetNextItemId(),
            ItemBlueprint = blueprint,
            CapturedBySquadId = capturedBySquadId,
            CapturedAt = DateTime.UtcNow,
        };
        _capturedItems.Add(item);
        HasUnsavedChanges = true;
        return item;
    }

    /// <summary>
    /// Removes the captured item with the given ID. Returns true if found.
    /// </summary>
    public bool RemoveCapturedItem(int itemId) {
        int index = _capturedItems.FindIndex(c => c.Id == itemId);
        if (index < 0)
            return false;
        _capturedItems.RemoveAt(index);
        HasUnsavedChanges = true;
        return true;
    }

    // -------------------------------------------------------------------------
    // Build
    // -------------------------------------------------------------------------

    /// <summary>
    /// Constructs an updated immutable <see cref="Company"/> from the current editor state.
    /// </summary>
    public Company BuildCompany(string updatedBy) {
        HasUnsavedChanges = false;
        return new Company {
            Id = _companyId,
            Name = _name,
            Faction = _faction,
            GameId = _gameId,
            DoctrineId = _doctrineId,
            DoctrineVersion = _doctrineVersion,
            CreatedAt = _createdAt,
            CreatedBy = _createdBy,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = updatedBy,
            Version = _originalVersion + 1,
            Squads = [.. _startingUnits, .. _skirmishPhaseUnits, .. _battlePhaseUnits, .. _reservesPhaseUnits],
            CapturedItems = [.. _capturedItems],
        };
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private int GetNextSquadId() {
        int max = Squads.Count == 0 ? 0 : Squads.Max(s => s.Id);
        return max + 1;
    }

    private int GetNextItemId() {
        int max = _capturedItems.Count == 0 ? 0 : _capturedItems.Max(c => c.Id);
        return max + 1;
    }

    private List<Squad> GetPhaseList(SquadPhase phase) => phase switch {
        SquadPhase.StartingPhase => _startingUnits,
        SquadPhase.SkirmishPhase => _skirmishPhaseUnits,
        SquadPhase.BattlePhase => _battlePhaseUnits,
        _ => _reservesPhaseUnits,
    };

    private IEnumerable<List<Squad>> AllPhaseLists() =>
        [_startingUnits, _skirmishPhaseUnits, _battlePhaseUnits, _reservesPhaseUnits];

}
