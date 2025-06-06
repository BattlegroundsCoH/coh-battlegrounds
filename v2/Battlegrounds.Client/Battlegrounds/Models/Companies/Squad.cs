using Battlegrounds.Models.Blueprints;

namespace Battlegrounds.Models.Companies;

public enum SquadPhase : byte {
    ReservesPhase = 0,
    SkirmishPhase = 1,
    BattlePhase = 2,
    StartingPhase = 3,
}

public sealed class Squad {

    public sealed record SlotItem(int Count, UpgradeBlueprint? UpgradeBlueprint, SlotItemBlueprint? SlotItemBlueprint); // UpgradeBlueprint for CoH3, SlotItemBlueprint for CoH2 because reasons...
    public sealed record TransportSquad(SquadBlueprint TransportBlueprint, bool DropOffOnly);

    private readonly string _name = string.Empty;
    private readonly HashSet<SlotItem> _slotItems = [];
    private readonly HashSet<UpgradeBlueprint> _upgrades = [];

    private TransportSquad? _transport = null;

    public int Id { get; init; } = 0;

    public string Name {
        get => _name;
        init => _name = value ?? string.Empty;
    }

    public bool HasCustomName => !string.IsNullOrEmpty(_name);

    public float Experience { get; init; } = 0f;

    public int Rank => Blueprint.Veterancy.GetRank(Experience);

    public SquadPhase Phase { get; init; } = SquadPhase.ReservesPhase;

    public required SquadBlueprint Blueprint { get; init; } = null!;

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

    public TransportSquad? Transport {
        get => _transport;
        init => _transport = value;
    }

    public bool HasTransport => _transport is not null;

}
