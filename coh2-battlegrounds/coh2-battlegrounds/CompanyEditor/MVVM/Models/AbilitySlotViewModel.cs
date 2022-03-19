using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;
using BattlegroundsApp.MVVM;
using BattlegroundsApp.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BattlegroundsApp.CompanyEditor.MVVM.Models;

public delegate void AbilitySlotViewModelEvent(object sender, AbilitySlotViewModel slotViewModel);

public class AbilitySlotViewModel : IViewModel {

    public string AbilityName { get; set; }

    public string AbilityIcon { get; set; }

    public string AbilitySymbol { get; set; }

    public CostExtension AbilityCost { get; set; }

    public Ability AbilityInstance { get; }

    public AbilitySlotViewModelEvent Click { get; }

    public AbilitySlotViewModelEvent RemoveClick { get; }

    public Visibility RemoveButtonVisibility { get; set; }

    public bool SingleInstanceOnly => false; // This will allow us to override

    public AbilitySlotViewModel(Ability ability, AbilitySlotViewModelEvent onClick, AbilitySlotViewModelEvent onRemove) {

        // Set ability instance
        this.AbilityInstance = ability;

        // Set events
        this.Click = onClick;
        this.RemoveClick = onRemove;

        // Set data
        this.AbilityName = GameLocale.GetString(this.AbilityInstance.ABP.UI.ScreenName);
        this.AbilityCost = this.AbilityInstance.ABP.Cost;

        this.AbilityIcon = this.AbilityInstance.ABP.UI.Icon;
        this.AbilitySymbol = this.AbilityInstance.ABP.UI.Symbol;

    }

    public void UpdateUnitData(ModPackage.FactionData.UnitAbility unitData) {

        // Get blueprint
        var sbp = BlueprintManager.FromBlueprintName<SquadBlueprint>(unitData.Blueprint);
        if (sbp is null) {
            return;
        }

        // Update remove button
        // TODO

        // Update symbol
        this.AbilitySymbol = sbp.UI.Symbol;

    }

    public bool UnloadViewModel() => true;

}
