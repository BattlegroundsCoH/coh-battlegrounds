using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;

using BattlegroundsApp.Resources;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.CompanyEditor.MVVM.Models;

public class EquipmentSlotViewModel {

    private readonly Predicate<EquipmentSlotViewModel> m_canEquipPredicate;

    public CompanyItem Item { get; }
    
    public string Portrait { get; }

    public string Title { get; }

    public string Army { get; }

    public ICommand EquipClick { get; }

    public bool CanEquip => this.m_canEquipPredicate(this);

    public EquipmentSlotViewModel(CompanyItem item, Action<EquipmentSlotViewModel> onEquip, Predicate<EquipmentSlotViewModel> canEquip) {
        
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
        this.EquipClick = new RelayCommand(() => onEquip(this));

        // Set can equip
        this.m_canEquipPredicate = canEquip;

    }

}
