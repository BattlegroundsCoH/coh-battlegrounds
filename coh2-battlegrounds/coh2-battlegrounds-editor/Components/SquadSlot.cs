using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;

using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;
using Battlegrounds.Modding.Content.Companies;
using Battlegrounds.Resources;

namespace Battlegrounds.Editor.Components;

/// <summary>
/// 
/// </summary>
/// <param name="sender"></param>
/// <param name="slotViewModel"></param>
public delegate void SquadSlotViewModelEvent(object sender, SquadSlot slotViewModel);

/// <summary>
/// 
/// </summary>
public sealed class SquadSlot : INotifyPropertyChanged {

    /// <summary>
    /// 
    /// </summary>
    public static readonly ImageSource VetRankAchieved
            = new BitmapImage(new Uri($"pack://application:,,,/Resources/ingame/vet/vstar_yes.png"));

    /// <summary>
    /// 
    /// </summary>
    public static readonly ImageSource VetRankNotAchieved
        = new BitmapImage(new Uri($"pack://application:,,,/Resources/ingame/vet/vstar_no.png"));

    /// <summary>
    /// 
    /// </summary>
    public static readonly ImageSource RallyIcon = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/app/rally_icon.png"));

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 
    /// </summary>
    public string SquadName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string SquadPortrait { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string SquadSymbol { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public CostExtension SquadCost
        => this.CompanyType.GetUnitCost(
            this.BuilderInstance.Blueprint, this.BuilderInstance.Upgrades, this.BuilderInstance.Rank, this.BuilderInstance.Role, this.BuilderInstance.Transport);

    /// <summary>
    /// 
    /// </summary>
    public bool SquadIsTransported { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public ImageSource? SquadTransportIcon { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public UnitBuilder BuilderInstance { get; }

    /// <summary>
    /// 
    /// </summary>
    public ImageSource Rank1 { get; private set; } = VetRankNotAchieved;

    /// <summary>
    /// 
    /// </summary>
    public ImageSource Rank2 { get; private set; } = VetRankNotAchieved;

    /// <summary>
    /// 
    /// </summary>
    public ImageSource Rank3 { get; private set; } = VetRankNotAchieved;

    /// <summary>
    /// 
    /// </summary>
    public ImageSource Rank4 { get; private set; } = VetRankNotAchieved;

    /// <summary>
    /// 
    /// </summary>
    public ImageSource Rank5 { get; private set; } = VetRankNotAchieved;

    /// <summary>
    /// 
    /// </summary>
    public ImageSource? SquadPhase => this.BuilderInstance.Phase switch {
        DeploymentPhase.PhaseInitial => RallyIcon,
        _ => null
    };

    /// <summary>
    /// 
    /// </summary>
    public Brush PhaseBackground => this.BuilderInstance.Role switch {
        DeploymentRole.DirectCommand => (SolidColorBrush)Application.Current.FindResource("BackgroundLightBlueBrush"),
        DeploymentRole.SupportRole => (SolidColorBrush)Application.Current.FindResource("BackgroundPurpleBrush"),
        DeploymentRole.ReserveRole => (SolidColorBrush)Application.Current.FindResource("BackgroundDarkishGreenBrush"),
        _ => (SolidColorBrush)Application.Current.FindResource("BackgroundLightBlueBrush"),
    };

    /// <summary>
    /// 
    /// </summary>
    public Brush PhaseBackgroundHover => this.BuilderInstance.Role switch {
        DeploymentRole.SupportRole => (SolidColorBrush)Application.Current.FindResource("BackgroundLightPurpleBrush"),
        DeploymentRole.ReserveRole => (SolidColorBrush)Application.Current.FindResource("BackgroundGreenBrush"),
        _ => (SolidColorBrush)Application.Current.FindResource("BackgroundLightGrayBrush"),
    };

    /// <summary>
    /// 
    /// </summary>
    public SquadSlotViewModelEvent Click { get; }

    /// <summary>
    /// 
    /// </summary>
    public SquadSlotViewModelEvent RemoveClick { get; }

    /// <summary>
    /// 
    /// </summary>
    public FactionCompanyType CompanyType { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="companyType"></param>
    /// <param name="onClick"></param>
    /// <param name="onRemove"></param>
    public SquadSlot(UnitBuilder builder, FactionCompanyType companyType, SquadSlotViewModelEvent onClick, SquadSlotViewModelEvent onRemove) {

        // Set squad instance
        this.BuilderInstance = builder;

        // Set company type
        this.CompanyType = companyType;

        // Set events
        this.Click = onClick;
        this.RemoveClick = onRemove;

        // Set data known not to change
        this.SquadPortrait = this.BuilderInstance.Blueprint.UI.Portrait;
        this.SquadSymbol = this.BuilderInstance.Blueprint.UI.Symbol;

        // Refresh data
        this.RefreshData();

    }

    /// <summary>
    /// 
    /// </summary>
    [MemberNotNull(nameof(SquadName))]
    public void RefreshData() {

        // Set basic info
        this.SquadName = GameLocale.GetString(this.BuilderInstance.Blueprint.UI.ScreenName);

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
        if (this.SquadIsTransported && ResourceHandler.HasIcon("symbol_icons", transportBp!.UI.Symbol)) {
            this.SquadTransportIcon = ResourceHandler.GetIcon("symbol_icons", transportBp!.UI.Symbol);
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

}
