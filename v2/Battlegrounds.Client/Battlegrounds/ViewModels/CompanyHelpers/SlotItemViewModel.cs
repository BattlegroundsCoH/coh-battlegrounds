using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Companies;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.ViewModels.CompanyHelpers;

public sealed record SlotItemViewModel(Squad.SlotItem Item, IRelayCommand<SlotItemViewModel>? RemoveItemCommand) {
    public Blueprint Blueprint => Item.EntityBlueprint!; // Gets the blueprint associated with the slot item.
}
