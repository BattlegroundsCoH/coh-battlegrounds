using Battlegrounds.Models.Blueprints;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.ViewModels.CompanyHelpers;

public sealed record UpgradeViewModel(UpgradeBlueprint Blueprint, bool IsUpgraded, bool CanUpgrade, IRelayCommand<UpgradeBlueprint>? ApplyUpgradeCommand) {
    public bool CanUpdate {
        get {
            if (IsUpgraded)
                return true;
            return CanUpgrade;
        }
    }
}
