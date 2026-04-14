using System.ComponentModel;

using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Blueprints.Extensions;
using Battlegrounds.Models.Companies;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.ViewModels.CompanyHelpers;

public sealed class ItemSelectionViewModel : INotifyPropertyChanged {

    public sealed record SquadAssignable(Squad Squad, ItemSelectionViewModel ViewModel);

    private readonly CompanyEditorViewModel _parentViewModel;
    private readonly CapturedItem _capturedItem;
    private readonly CostExtension? _costExtension;

    public event PropertyChangedEventHandler? PropertyChanged;

    public IRelayCommand DestroyItemCommand { get; init; }

    public IRelayCommand<SquadPhase> AddToCompanyCommand { get; init; }

    public Blueprint Blueprint => _capturedItem.ItemBlueprint ?? throw new InvalidOperationException("Captured item does not have an associated blueprint.");

    public bool HasCostInfo => _costExtension is not null;

    public CostExtension? Cost => _costExtension;

    public bool IsTeamWeapon => _capturedItem.IsTeamWeapon;

    public ICollection<SquadAssignable> ItemAssignableUnits { get; } = [];

    public CapturedItem Item => _capturedItem;

    public ItemSelectionViewModel(CompanyEditorViewModel parentViewModel, CapturedItem capturedItem) {

        this._parentViewModel = parentViewModel;
        this._capturedItem = capturedItem;

        this.DestroyItemCommand = new RelayCommand(this.DestroyItem);
        this.AddToCompanyCommand = new RelayCommand<SquadPhase>(this.AddToCompany);

        if (capturedItem.IsTeamWeapon) {
            var teamWeaponExtension = capturedItem.ItemBlueprint!.TeamWeapon;
            _costExtension = teamWeaponExtension.RecrewSquadBlueprint.Blueprint?.Cost;
        } else {
            ItemAssignableUnits = [.. _parentViewModel.StartingUnits
                .Union(_parentViewModel.SkirmishPhaseUnits)
                .Union(_parentViewModel.BattlePhaseUnits)
                .Union(_parentViewModel.ReservesPhaseUnits)
                .Where(x => x.Blueprint.Category is SquadCategory.Infantry && x.Upgrades.Count is 0 && x.Squad.SlotItems.Count is 0)
                .Select(x => new SquadAssignable(x.Squad, this))];
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItemAssignableUnits)));

    }

    private void DestroyItem() => _parentViewModel.DestroyItem(_capturedItem);

    private void AddToCompany(SquadPhase phase) => _parentViewModel.AddCapturedSquadToCompany(phase, _capturedItem);

}
