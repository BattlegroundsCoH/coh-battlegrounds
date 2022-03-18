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

public delegate void AvailableSquadViewModelEvent(object sender, AvailableSquadViewModel availableSquadViewModel, object args = null);

public class AvailableSquadViewModel : IViewModel {

    public string SquadName { get; }

    public ImageSource SquadSymbol { get; }

    public CostExtension SquadCost { get; set; }

    public Blueprint Blueprint { get; }

    public AvailableSquadViewModelEvent AddClick { get; }

    public AvailableSquadViewModelEvent Move { get; }

    public bool CanAdd { get; set; }

    public bool SingleInstanceOnly => false; // This will allow us to override

    public AvailableSquadViewModel(Blueprint bp, AvailableSquadViewModelEvent onAdd, AvailableSquadViewModelEvent onMove) {

        if (bp is SquadBlueprint sbp) {

            this.SquadName = GameLocale.GetString(sbp.UI.ScreenName);
            this.SquadSymbol = App.ResourceHandler.GetIcon("symbol_icons", sbp.UI.Symbol);
            this.Blueprint = sbp;
            this.SquadCost = sbp.Cost;

        } else {

            throw new ArgumentException($"Invalid blueprint type '{bp.GetType().Name}'.", nameof(bp));

        }

        // Set events
        this.AddClick = onAdd;
        this.Move = onMove;

    }

    public bool UnloadViewModel() => true;

}
