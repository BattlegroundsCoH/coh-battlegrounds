using Battlegrounds.Models.Blueprints;

namespace Battlegrounds.Models.Companies;

public sealed class Squad {

    public sealed record SlotItem(int Count, UpgradeBlueprint? UpgradeBlueprint, SlotItemBlueprint? SlotItemBlueprint); // UpgradeBlueprint for CoH3, SlotItemBlueprint for CoH2 because reasons...
    public sealed record TransportSquad(SquadBlueprint TransportBlueprint, bool DropOffOnly);

    private readonly string _name = string.Empty;
    private readonly HashSet<SlotItem> _slotItems = [];
    private readonly HashSet<UpgradeBlueprint> _upgrades = [];

    private TransportSquad? _transport = null;

    public int Id { get; } = 0;

    public string Name => _name;

    public bool HasCustomName => !string.IsNullOrEmpty(_name);

    public float Experience { get; } = 0f;

    public int Rank => Blueprint.Veterancy.GetRank(Experience);

    public required SquadBlueprint Blueprint { get; init; } = null!;

    public IReadOnlyList<SlotItem> SlotItems => _slotItems.ToList().AsReadOnly();

    public IReadOnlyList<UpgradeBlueprint> Upgrades => _upgrades.ToList().AsReadOnly();

    public TransportSquad? Transport => _transport;

    public bool HasTransport => _transport is not null;

}
