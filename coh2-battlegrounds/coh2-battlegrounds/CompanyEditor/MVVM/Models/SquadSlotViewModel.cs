using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;

using BattlegroundsApp.MVVM;
using BattlegroundsApp.Resources;

namespace BattlegroundsApp.CompanyEditor.MVVM.Models;

public delegate void SquadSlotViewModelEvent(object sender, SquadSlotViewModel slotViewModel);

public class SquadSlotViewModel : IViewModel, INotifyPropertyChanged {

    public static readonly ImageSource VetRankAchieved
            = new BitmapImage(new Uri($"pack://application:,,,/Resources/ingame/vet/vstar_yes.png"));

    public static readonly ImageSource VetRankNotAchieved
        = new BitmapImage(new Uri($"pack://application:,,,/Resources/ingame/vet/vstar_no.png"));

    public event PropertyChangedEventHandler? PropertyChanged;

    public string SquadName { get; set; }

    public string SquadPortrait { get; set; }

    public string SquadSymbol { get; set; }

    public CostExtension SquadCost { get; set; }

    public bool SquadIsTransported { get; set; }

    public ImageSource? SquadTransportIcon { get; set; }

    public UnitBuilder BuilderInstance { get; }

    public ImageSource Rank1 { get; private set; } = VetRankNotAchieved;

    public ImageSource Rank2 { get; private set; } = VetRankNotAchieved;

    public ImageSource Rank3 { get; private set; } = VetRankNotAchieved;

    public ImageSource Rank4 { get; private set; } = VetRankNotAchieved;

    public ImageSource Rank5 { get; private set; } = VetRankNotAchieved;

    public string SquadPhase => this.BuilderInstance.Phase switch {
        DeploymentPhase.PhaseInitial => "I",
        DeploymentPhase.PhaseA => "A",
        DeploymentPhase.PhaseB => "B",
        DeploymentPhase.PhaseC => "C",
        _ => string.Empty
    };

    public Brush PhaseBackground => this.BuilderInstance.Phase switch {
        DeploymentPhase.PhaseInitial => (SolidColorBrush)App.Current.FindResource("BackgroundBlueBrush"),
        DeploymentPhase.PhaseB => (SolidColorBrush)App.Current.FindResource("BackgroundPurpleBrush"),
        DeploymentPhase.PhaseC => (SolidColorBrush)App.Current.FindResource("BackgroundDarkishGreenBrush"),
        _ => (SolidColorBrush)App.Current.FindResource("BackgroundLightBlueBrush"),
    };

    public Brush PhaseBackgroundHover => this.BuilderInstance.Phase switch {
        DeploymentPhase.PhaseB => (SolidColorBrush)App.Current.FindResource("BackgroundLightPurpleBrush"),
        DeploymentPhase.PhaseC => (SolidColorBrush)App.Current.FindResource("BackgroundGreenBrush"),
        _ => (SolidColorBrush)App.Current.FindResource("BackgroundLightGrayBrush"),
    };

    public bool SingleInstanceOnly => false; // This will allow us to override

    public bool KeepAlive => false;

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

        // Refresh data
        this.RefreshData();

    }

    [MemberNotNull(nameof(SquadName), nameof(SquadCost))]
    public void RefreshData() {

        // Set basic info
        this.SquadName = GameLocale.GetString(this.BuilderInstance.Blueprint.UI.ScreenName);
        this.SquadCost = this.BuilderInstance.GetCost();

        // Get rank
        var rankLevel = this.BuilderInstance.Rank;

        // Update rank icons
        this.Rank1 = rankLevel >= 1 ? VetRankAchieved : VetRankNotAchieved;
        this.Rank2 = rankLevel >= 2 ? VetRankAchieved : VetRankNotAchieved;
        this.Rank3 = rankLevel >= 3 ? VetRankAchieved : VetRankNotAchieved;
        this.Rank4 = rankLevel >= 4 ? VetRankAchieved : VetRankNotAchieved;
        this.Rank5 = rankLevel == 5 ? VetRankAchieved : VetRankNotAchieved;

        // Set transport
        var transportBp = this.BuilderInstance.Transport;
        this.SquadIsTransported = transportBp is not null;
        if (this.SquadIsTransported && App.ResourceHandler.HasIcon("symbol_icons", transportBp!.UI.Symbol)) {
            this.SquadTransportIcon = App.ResourceHandler.GetIcon("symbol_icons", transportBp!.UI.Symbol);
        }

        // Refresh
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Rank1)));
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Rank2)));
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Rank3)));
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Rank4)));
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Rank5)));
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.SquadCost)));
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.SquadName)));
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.SquadPhase)));
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.SquadIsTransported)));

    }

    public void UnloadViewModel(OnModelClosed closeCallback, bool destroy) => closeCallback(false);

    public void Swapback() {

    }

}
