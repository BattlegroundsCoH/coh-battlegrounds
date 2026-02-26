using Battlegrounds.Models.Lobbies;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.ViewModels;

/// <summary>
/// View model for the post-match results overlay, displayed at the end of a game.
/// </summary>
public sealed class MatchOverViewModel {

    private readonly MatchOverData _data;

    /// <summary>Gets whether the local player won the match.</summary>
    public bool IsVictory => _data.IsVictory;

    /// <summary>Gets the scenario (map) that was played.</summary>
    public string Scenario => _data.Scenario;

    /// <summary>Gets the total match duration.</summary>
    public TimeSpan MatchDuration => _data.MatchDuration;

    /// <summary>Gets whether the match concluded naturally, as opposed to being abandoned.</summary>
    public bool Concluded => _data.Concluded;

    /// <summary>Gets whether any bad events were recorded, indicating potentially incomplete data.</summary>
    public bool HasBadEvents => _data.HasBadEvents;

    /// <summary>Gets the per-squad performance summaries for the local player.</summary>
    public IReadOnlyList<SquadMatchSummary> SquadSummaries => _data.SquadSummaries;

    /// <summary>Gets the command that dismisses this overlay and returns to the lobby view.</summary>
    public IRelayCommand CloseCommand { get; }

    public MatchOverViewModel(MatchOverData data, Action onClose) {
        _data = data;
        CloseCommand = new RelayCommand(onClose);
    }

}
