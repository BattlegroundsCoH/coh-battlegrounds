using System;
using System.ComponentModel;
using System.Windows.Media;
using Battlegrounds.Game.Blueprints;
using Battlegrounds.Game.Blueprints.Extensions;
using Battlegrounds.Locale;
using Battlegrounds.Resources;

namespace Battlegrounds.Editor.Components;

/// <summary>
/// 
/// </summary>
/// <param name="sender"></param>
/// <param name="availableSquadViewModel"></param>
/// <param name="args"></param>
public delegate void AvailableItemViewModelEvent(object sender, AvailableItem availableSquadViewModel, object? args = null);

/// <summary>
/// 
/// </summary>
/// <param name="blueprint"></param>
/// <returns></returns>
public delegate bool IsBlueprintAvailableHandler(Blueprint blueprint);

/// <summary>
/// 
/// </summary>
public class AvailableItem : INotifyPropertyChanged {

    /// <summary>
    /// 
    /// </summary>
    public string ItemName { get; }

    /// <summary>
    /// 
    /// </summary>
    public ImageSource? ItemSymbol { get; }

    /// <summary>
    /// 
    /// </summary>
    public CostExtension ItemCost { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Blueprint Blueprint { get; }

    /// <summary>
    /// 
    /// </summary>
    public AvailableItemViewModelEvent AddClick { get; }

    /// <summary>
    /// 
    /// </summary>
    public AvailableItemViewModelEvent Move { get; }

    /// <summary>
    /// 
    /// </summary>
    public IsBlueprintAvailableHandler AddEval { get; }

    /// <summary>
    /// 
    /// </summary>
    public bool CanAdd => this.AddEval(this.Blueprint);

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bp"></param>
    /// <param name="onAdd"></param>
    /// <param name="onMove"></param>
    /// <param name="addable"></param>
    /// <exception cref="ArgumentException"></exception>
    public AvailableItem(Blueprint bp, AvailableItemViewModelEvent onAdd, AvailableItemViewModelEvent onMove, IsBlueprintAvailableHandler addable) {

        // Switch on type
        if (bp is SquadBlueprint sbp) {

            this.ItemName = GameLocale.GetString(sbp.UI.ScreenName);
            this.ItemSymbol = ResourceHandler.GetIcon("symbol_icons", sbp.UI.Symbol);
            this.Blueprint = sbp;
            this.ItemCost = sbp.Cost;

        } else if (bp is AbilityBlueprint abp) {

            this.ItemName = GameLocale.GetString(abp.UI.ScreenName);
            this.ItemSymbol = ResourceHandler.GetIcon("symbol_icons", abp.UI.Symbol);
            this.Blueprint = abp;
            this.ItemCost = abp.Cost;

        } else {

            throw new ArgumentException($"Invalid blueprint type '{bp.GetType().Name}'.", nameof(bp));

        }

        // Set events
        this.AddClick = onAdd;
        this.Move = onMove;
        this.AddEval = addable;

    }

    /// <summary>
    /// 
    /// </summary>
    public void Refresh() {
        this.PropertyChanged?.Invoke(this.Move, new(nameof(CanAdd)));
    }

}
