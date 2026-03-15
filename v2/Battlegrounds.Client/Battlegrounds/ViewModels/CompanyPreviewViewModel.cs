using System.ComponentModel;

using Battlegrounds.Models.Companies;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.ViewModels;

/// <summary>
/// View model for the company preview overlay, displayed when inspecting another player's company in the lobby.
/// </summary>
public sealed class CompanyPreviewViewModel(Company company, string gameId, Action onClose) : INotifyPropertyChanged {

    public event PropertyChangedEventHandler? PropertyChanged;

    public string CompanyName => company.Name;

    public string Faction => company.Faction;

    public string GameId => gameId;

    public int SquadCount => company.Squads.Count;

    public IReadOnlyList<Squad> StartingUnits { get; } = company.Squads.Where(s => s.Phase == SquadPhase.StartingPhase).ToList();

    public IReadOnlyList<Squad> SkirmishUnits { get; } = company.Squads.Where(s => s.Phase == SquadPhase.SkirmishPhase).ToList();

    public IReadOnlyList<Squad> BattleUnits { get; } = company.Squads.Where(s => s.Phase == SquadPhase.BattlePhase).ToList();

    public IReadOnlyList<Squad> ReservesUnits { get; } = company.Squads.Where(s => s.Phase == SquadPhase.ReservesPhase).ToList();

    public IRelayCommand CloseCommand { get; } = new RelayCommand(onClose);

}
