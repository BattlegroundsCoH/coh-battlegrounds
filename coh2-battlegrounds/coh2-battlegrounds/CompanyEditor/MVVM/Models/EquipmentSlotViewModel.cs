using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;

using BattlegroundsApp.Resources;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.CompanyEditor.MVVM.Models;

public class EquipmentSlotViewModel {

    private readonly CompanyBuilderViewModel m_builderViewModel;

    public CompanyItem Item { get; }
    
    public string Portrait { get; }

    public string Title { get; }

    public string Army { get; }

    public ICommand EquipClick { get; }

    public bool CanEquip => this.IsEquippable();

    public EquipmentSlotViewModel(CompanyItem item, CompanyBuilderViewModel companyBuilder) {
        
        // Set Item
        this.Item = item;

        // Grab portrait and title
        (this.Portrait, this.Title) = item.Item switch {
            IUIBlueprint ui => (ui.UI.Portrait, GameLocale.GetString(ui.UI.ScreenName)),
            _ => (string.Empty, string.Empty)
        };

        // Set army (if any)
        this.Army = item.Item switch {
            EntityBlueprint ebp => ebp.Faction?.Name,
            SquadBlueprint sbp => sbp.Army?.Name,
            _ => ""
        } ?? string.Empty;

        // Define equip click command
        this.EquipClick = new RelayCommand(this.OnEquipmentClicked);

        // Set builder ref
        this.m_builderViewModel = companyBuilder;

    }

    public bool IsEquippable() {
        if (this.Item.Item is SquadBlueprint sbp) {
            return sbp.Category switch {
                SquadCategory.Infantry => !this.m_builderViewModel.InfantryCapacity.IsAtCapacity,
                SquadCategory.Support => !this.m_builderViewModel.SupportCapacity.IsAtCapacity,
                SquadCategory.Vehicle => !this.m_builderViewModel.VehicleCapacity.IsAtCapacity,
                _ => false // TODO: Add support for command units
            };
        } else if (this.Item.Item is EntityBlueprint) {
            return !this.m_builderViewModel.SupportCapacity.IsAtCapacity; // Based on team weapons *always* being ebps
        }
        return false;
    }

    private void OnEquipmentClicked() {

        // Determine how
        if (this.Item.Item is SquadBlueprint sbp) { // Is vehicle...

            // Get driver squad
            var driverSbp = sbp.GetCrewBlueprint(this.m_builderViewModel.CompanyFaction);
            if (driverSbp is null) {
                // TODO: Exception
                return;
            }

            // Add action
            this.m_builderViewModel.Builder.CrewCompanyItem(this.Item.ItemId, driverSbp);

        } else if (this.Item.Item is EntityBlueprint ebp) {

            // Get crew
            var crewSbp = this.m_builderViewModel.Builder.CompanyType.GetWeaponsCrew();

            // Add action
            this.m_builderViewModel.Builder.CrewCompanyItem(this.Item.ItemId, crewSbp);

        }

    }

}
