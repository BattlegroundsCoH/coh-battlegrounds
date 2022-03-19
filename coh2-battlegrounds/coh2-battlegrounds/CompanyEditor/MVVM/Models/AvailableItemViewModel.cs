using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Extensions;
using BattlegroundsApp.MVVM;
using BattlegroundsApp.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BattlegroundsApp.CompanyEditor.MVVM.Models;

public delegate void AvailableItemViewModelEvent(object sender, AvailableItemViewModel availableSquadViewModel, object args = null);

public class AvailableItemViewModel : IViewModel {

    public string ItemName { get; }

    public ImageSource ItemSymbol { get; }

    public CostExtension ItemCost { get; set; }

    public Blueprint Blueprint { get; }

    public AvailableItemViewModelEvent AddClick { get; }

    public AvailableItemViewModelEvent Move { get; }

    public bool CanAdd { get; set; }

    public bool SingleInstanceOnly => false; // This will allow us to override

    public AvailableItemViewModel(Blueprint bp, AvailableItemViewModelEvent onAdd, AvailableItemViewModelEvent onMove) {

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

    }

    public bool UnloadViewModel() => true;

}
