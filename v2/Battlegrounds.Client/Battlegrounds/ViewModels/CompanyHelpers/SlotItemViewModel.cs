using Battlegrounds.Models.Blueprints;

namespace Battlegrounds.ViewModels.CompanyHelpers;

public sealed record SlotItemViewModel(Blueprint Blueprint, int Count) {
    public bool IsUpgrade => Blueprint is UpgradeBlueprint; // Determines if the blueprint is an upgrade or a slot item.
    public bool IsSlotItem => Blueprint is SlotItemBlueprint; // Determines if the blueprint is a slot item or an upgrade.
}
