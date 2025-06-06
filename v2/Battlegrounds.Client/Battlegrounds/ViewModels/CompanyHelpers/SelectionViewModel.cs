using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Companies;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.ViewModels.CompanyHelpers;

public sealed class SelectionViewModel : INotifyPropertyChanged {

    private readonly SquadBlueprint _squadBlueprint;
    private readonly Squad? _squad;

    private string _customName = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    [MemberNotNullWhen(true, nameof(_squad))]
    public bool IsSquad => _squad is not null; // Used to determine if the selection is a company squad or just a blueprint.

    public SquadBlueprint Blueprint => _squadBlueprint; // The squad blueprint, which is always available.

    public string CustomName {
        get => _customName;
        set {
            if (value == _customName) return;
            _customName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CustomName)));
        }
    }

    public bool CanGiveCustomName => IsSquad && _squad.Rank == _squad.Blueprint.Veterancy.MaxRank;

    public float Experience => _squad?.Experience ?? 0f; // Experience is only available if this is a squad, otherwise it's 0.

    public int Rank => _squad?.Rank ?? 0; // Rank is only available if this is a squad, otherwise it's 0.

    public IRelayCommand<SquadPhase>? AddToCompanyCommand { get; }

    public IRelayCommand? RetireSquadCommand { get; }

    public SelectionViewModel(SquadBlueprint squadBlueprint, Action<SquadPhase, SquadBlueprint> addToCompany) {
        _squadBlueprint = squadBlueprint ?? throw new ArgumentNullException(nameof(squadBlueprint));
        CustomName = string.Empty;        
        AddToCompanyCommand = new RelayCommand<SquadPhase>(x => addToCompany(x, squadBlueprint));
        NotifyAll();
    }

    public SelectionViewModel(Squad squad, Action<Squad> RetireSquad) {
        _squad = squad;
        _squadBlueprint = squad.Blueprint;
        CustomName = squad.HasCustomName ? squad.Name : string.Empty;
        RetireSquadCommand = new RelayCommand(() => RetireSquad(squad));
        NotifyAll();
    }

    private void NotifyAll() {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSquad)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Blueprint)));
    }

}
