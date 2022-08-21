using System;
using System.ComponentModel;
using System.Windows.Media;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Extensions;

using BattlegroundsApp.MVVM;
using BattlegroundsApp.Resources;

namespace BattlegroundsApp.CompanyEditor.MVVM.Models;

public delegate void AvailableItemViewModelEvent(object sender, AvailableItemViewModel availableSquadViewModel, object? args = null);

public delegate bool IsBlueprintAvailableHandler(Blueprint blueprint);

public class AvailableItemViewModel : INotifyPropertyChanged {

    public string ItemName { get; }

    public ImageSource? ItemSymbol { get; }

    public CostExtension ItemCost { get; set; }

    public Blueprint Blueprint { get; }

    public AvailableItemViewModelEvent AddClick { get; }

    public AvailableItemViewModelEvent Move { get; }

    public IsBlueprintAvailableHandler AddEval { get; }

    public bool CanAdd => this.AddEval(this.Blueprint);

    public bool SingleInstanceOnly => false; // This will allow us to override

    public event PropertyChangedEventHandler? PropertyChanged;

    public AvailableItemViewModel(Blueprint bp, AvailableItemViewModelEvent onAdd, AvailableItemViewModelEvent onMove, IsBlueprintAvailableHandler addable) {

        // Switch on type
        if (bp is SquadBlueprint sbp) {

            this.ItemName = GameLocale.GetString(sbp.UI.ScreenName);
            this.ItemSymbol = App.ResourceHandler.GetIcon("symbol_icons", sbp.UI.Symbol);
            this.Blueprint = sbp;
            this.ItemCost = sbp.Cost;

        } else if (bp is AbilityBlueprint abp) {

            this.ItemName = GameLocale.GetString(abp.UI.ScreenName);
            this.ItemSymbol = App.ResourceHandler.GetIcon("symbol_icons", abp.UI.Symbol);
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

    public void Refresh() {
        this.PropertyChanged?.Invoke(this.Move, new(nameof(CanAdd)));
    }

}
