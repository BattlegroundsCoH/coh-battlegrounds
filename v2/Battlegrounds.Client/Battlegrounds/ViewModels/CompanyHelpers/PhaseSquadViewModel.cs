using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Companies;

namespace Battlegrounds.ViewModels.CompanyHelpers;

/// <summary>
/// View model wrapper for a <see cref="Squad"/> displayed in a phase column, including its resolved passenger squad (if any).
/// </summary>
public sealed class PhaseSquadViewModel {

    private readonly Squad _squad;
    private readonly Squad? _passengerSquad;

    public Squad Squad => _squad;

    public SquadBlueprint Blueprint => _squad.Blueprint;

    public int Id => _squad.Id;

    public bool HasCustomName => _squad.HasCustomName;

    public string Name => _squad.Name;

    public IReadOnlyList<UpgradeBlueprint> Upgrades => _squad.Upgrades;

    public Squad.TransportSquad? Transport => _squad.Transport;

    public bool HasTransport => _squad.HasTransport;

    public bool HasPassenger => _passengerSquad is not null;

    public SquadBlueprint? PassengerBlueprint => _passengerSquad?.Blueprint;

    public int? PassengerId => _passengerSquad?.Id;

    public PhaseSquadViewModel(Squad squad, Squad? passengerSquad = null) {
        _squad = squad;
        _passengerSquad = passengerSquad;
    }

}
