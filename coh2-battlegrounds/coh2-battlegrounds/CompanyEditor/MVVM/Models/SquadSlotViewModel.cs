using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.Gameplay;
using BattlegroundsApp.MVVM;
using BattlegroundsApp.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

    public Squad SquadInstance { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    public ImageSource Rank1 { get; set; } = VetRankNotAchieved;

    public ImageSource Rank2 { get; set; } = VetRankNotAchieved;

    public ImageSource Rank3 { get; set; } = VetRankNotAchieved;

    public ImageSource Rank4 { get; set; } = VetRankNotAchieved;

    public ImageSource Rank5 { get; set; } = VetRankNotAchieved;

    public bool SingleInstanceOnly => false; // This will allow us to override

    public SquadSlotViewModelEvent Click { get; }

    public SquadSlotViewModelEvent RemoveClick { get; }

    public SquadSlotViewModel(Squad squad, SquadSlotViewModelEvent onClick, SquadSlotViewModelEvent onRemove) {

        // Set squad instance
        this.SquadInstance = squad;

        // Set events
        this.Click = onClick;
        this.RemoveClick = onRemove;

        // Set data known not to change
        this.SquadPortrait = this.SquadInstance.SBP.UI.Portrait;
        this.SquadSymbol = this.SquadInstance.SBP.UI.Symbol;

        // Get rank
        var rankLevel =  this.SquadInstance.VeterancyRank;

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

        this.SquadName = GameLocale.GetString(this.SquadInstance.SBP.UI.ScreenName);
        this.SquadCost = this.SquadInstance.GetCost();

        // Get veterancy
        if (this.SquadInstance.VeterancyRank > 0) {
            this.SquadVeterancy = new BitmapImage(new Uri($"pack://application:,,,/Resources/ingame/vet/vstar{this.SquadInstance.VeterancyRank}.png"));
        }

        // Set transport
        this.SquadIsTransported = this.SquadInstance.SupportBlueprint is not null;
        if (this.SquadIsTransported && App.ResourceHandler.HasIcon("symbol_icons", (this.SquadInstance.SupportBlueprint as SquadBlueprint).UI.Symbol)) {
            this.SquadTransportIcon = App.ResourceHandler.GetIcon("symbol_icons", (this.SquadInstance.SupportBlueprint as SquadBlueprint).UI.Symbol);
        }

    }

    public bool UnloadViewModel() => true;

}
