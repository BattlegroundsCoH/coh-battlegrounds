using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.DataCompany;

using BattlegroundsApp.MVVM;
using BattlegroundsApp.Resources;

namespace BattlegroundsApp.CompanyEditor.MVVM.Models;

public delegate void SquadSlotViewModelEvent(object sender, SquadSlotViewModel slotViewModel);

public class SquadSlotViewModel : IViewModel {

    public static readonly ImageSource VetRankAchieved
            = new BitmapImage(new Uri($"pack://application:,,,/Resources/ingame/vet/vstar_yes.png"));

    public static readonly ImageSource VetRankNotAchieved
        = new BitmapImage(new Uri($"pack://application:,,,/Resources/ingame/vet/vstar_no.png"));

    public string SquadName { get; set; }

    public string SquadPortrait { get; set; }

    public string SquadSymbol { get; set; }

    public BitmapImage SquadVeterancy { get; set; }

    public CostExtension SquadCost { get; set; }

    public bool SquadIsTransported { get; set; }

    public ImageSource SquadTransportIcon { get; set; }

    public UnitBuilder BuilderInstance { get; }

    public ImageSource Rank1 { get; } = VetRankNotAchieved;

    public ImageSource Rank2 { get; } = VetRankNotAchieved;

    public ImageSource Rank3 { get; } = VetRankNotAchieved;

    public ImageSource Rank4 { get; } = VetRankNotAchieved;

    public ImageSource Rank5 { get; } = VetRankNotAchieved;

    public bool SingleInstanceOnly => false; // This will allow us to override

    public SquadSlotViewModelEvent Click { get; }

    public SquadSlotViewModelEvent RemoveClick { get; }

    public SquadSlotViewModel(UnitBuilder builder, SquadSlotViewModelEvent onClick, SquadSlotViewModelEvent onRemove) {

        // Set squad instance
        this.BuilderInstance = builder;

        // Set events
        this.Click = onClick;
        this.RemoveClick = onRemove;

        // Set data known not to change
        this.SquadPortrait = this.BuilderInstance.Blueprint.UI.Portrait;
        this.SquadSymbol = this.BuilderInstance.Blueprint.UI.Symbol;

        // Get rank
        var rankLevel =  this.BuilderInstance.Rank;

        // Update rank icons
        this.Rank1 = rankLevel >= 1 ? VetRankAchieved : VetRankNotAchieved;
        this.Rank2 = rankLevel >= 2 ? VetRankAchieved : VetRankNotAchieved;
        this.Rank3 = rankLevel >= 3 ? VetRankAchieved : VetRankNotAchieved;
        this.Rank4 = rankLevel >= 4 ? VetRankAchieved : VetRankNotAchieved;
        this.Rank5 = rankLevel == 5 ? VetRankAchieved : VetRankNotAchieved;

        // Refresh data
        this.RefreshData();
    }

    public void RefreshData() {

        // Set basic info
        this.SquadName = GameLocale.GetString(this.BuilderInstance.Blueprint.UI.ScreenName);
        this.SquadCost = this.BuilderInstance.GetCost();

        // Get veterancy
        if (this.BuilderInstance.Rank > 0) {
            this.SquadVeterancy = new BitmapImage(new Uri($"pack://application:,,,/Resources/ingame/vet/vstar{this.BuilderInstance.Rank}.png"));
        }

        // Set transport
        this.SquadIsTransported = this.BuilderInstance.Transport is not null;
        if (this.SquadIsTransported && App.ResourceHandler.HasIcon("symbol_icons", this.BuilderInstance.Transport.UI.Symbol)) {
            this.SquadTransportIcon = App.ResourceHandler.GetIcon("symbol_icons", this.BuilderInstance.Transport.UI.Symbol);
        }

    }

    public bool UnloadViewModel() => true;

}
