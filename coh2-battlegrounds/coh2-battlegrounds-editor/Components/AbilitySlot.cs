﻿using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;
using Battlegrounds.Modding.Content;

namespace Battlegrounds.Editor.Components;

public delegate void AbilitySlotViewModelEvent(object sender, AbilitySlot slotViewModel);

/// <summary>
/// 
/// </summary>
public class AbilitySlot {

    /// <summary>
    /// 
    /// </summary>
    public string AbilityName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string AbilityIcon { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string AbilitySymbol { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public CostExtension AbilityCost { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Ability AbilityInstance { get; }

    /// <summary>
    /// 
    /// </summary>
    public AbilitySlotViewModelEvent Click { get; }

    /// <summary>
    /// 
    /// </summary>
    public AbilitySlotViewModelEvent RemoveClick { get; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsRemovable { get; set; } = true;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ability"></param>
    /// <param name="onClick"></param>
    /// <param name="onRemove"></param>
    public AbilitySlot(Ability ability, AbilitySlotViewModelEvent onClick, AbilitySlotViewModelEvent onRemove) {

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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unitData"></param>
    public void UpdateUnitData(FactionData.UnitAbility unitData) {

        // Get blueprint
        var sbp = BlueprintManager.FromBlueprintName<SquadBlueprint>(unitData.Blueprint);
        if (sbp is null) {
            return;
        }

        // Update remove button
        this.IsRemovable = false;

        // Update symbol
        this.AbilitySymbol = sbp.UI.Symbol;

    }

}