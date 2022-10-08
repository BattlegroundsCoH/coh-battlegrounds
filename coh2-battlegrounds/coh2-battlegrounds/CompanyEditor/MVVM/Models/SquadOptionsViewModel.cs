using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using Battlegrounds;
using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;
using Battlegrounds.Misc.Values;
using Battlegrounds.Resources;
using Battlegrounds.UI;

using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.CompanyEditor.MVVM.Models;

public class SquadOptionsViewModel : INotifyPropertyChanged {

    public record AbilityButton(AbilityBlueprint Abp) {
        public ImageSource? Icon => ResourceHandler.GetIcon("ability_icons", this.Abp.UI.Icon);
        public string Title => GameLocale.GetString(this.Abp.UI.ScreenName);
        public string Desc => GameLocale.GetString(this.Abp.UI.LongDescription);
        public CostExtension Cost => this.Abp.Cost;
    }

    public record UpgradeButton(UpgradeBlueprint Ubp, Func<bool> CheckApplied, Func<bool> CheckAvailable, EventCommand Clicked) : INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;
        public bool IsApplied => this.CheckApplied();
        public bool IsAvailable => this.CheckAvailable();
        public ImageSource? Icon => ResourceHandler.GetIcon("upgrade_icons", this.Ubp.UI.Icon);
        public string Title => GameLocale.GetString(this.Ubp.UI.ScreenName);
        public string Desc => GameLocale.GetString(this.Ubp.UI.LongDescription);
        public CostExtension Cost => this.Ubp.Cost;
        public void Update() {
            this.PropertyChanged?.Invoke(this, new(nameof(IsAvailable)));
            this.PropertyChanged?.Invoke(this, new(nameof(IsApplied)));
        }
    }

    public record DeployButton(DeploymentMethod Method, Func<bool> IsActive, EventCommand Clicked) : INotifyPropertyChanged {
        public bool IsActiveMethod => this.IsActive();
        public event PropertyChangedEventHandler? PropertyChanged;
        public ImageSource? Icon => this.Method switch {
            DeploymentMethod.None => ResourceHandler.GetIcon("deploy_icons", "Icons_bg_deploy_none"),
            DeploymentMethod.DeployAndExit => ResourceHandler.GetIcon("deploy_icons", "Icons_bg_deploy_drop_exit"),
            DeploymentMethod.DeployAndStay => ResourceHandler.GetIcon("deploy_icons", "Icons_bg_deploy_drop_stay"),
            _ => throw new InvalidEnumArgumentException()
        };
        public void Update() {
            this.PropertyChanged?.Invoke(this, new(nameof(IsActiveMethod)));
        }
    }

    public record DeployUnitButton(SquadBlueprint Blueprint, Func<bool> IsActive, EventCommand Clicked) : INotifyPropertyChanged {
        public bool IsActiveMethod => this.IsActive();
        public event PropertyChangedEventHandler? PropertyChanged;
        public ImageSource? Icon => ResourceHandler.GetIcon("unit_icons", this.Blueprint.UI.Icon);
        public string Title => GameLocale.GetString(this.Blueprint.UI.ScreenName);
        public string Desc => GameLocale.GetString(this.Blueprint.UI.LongDescription);
        public CostExtension Cost => this.Blueprint.Cost;
        public void Update() {
            this.PropertyChanged?.Invoke(this, new(nameof(IsActiveMethod)));
        }
    }

    private static string RankToStar(int i, int rank) => i < rank
        ? "pack://application:,,,/coh2-battlegrounds;component/Resources/ingame/vet/vstar_yes.png"
        : "pack://application:,,,/coh2-battlegrounds;component/Resources/ingame/vet/vstar_no.png";

    public UnitBuilder BuilderInstance { get; }

    public UnitBuilder? CrewBuilderInstance => this.BuilderInstance.CrewBuilder;

    public CompanyBuilder CompanyBuilder { get; }

    public SquadSlotViewModel TriggerModel { get; }

    public string UnitName => GameLocale.GetString(this.BuilderInstance.GetName());

    public string UnitRawName => GameLocale.GetString(this.BuilderInstance.Blueprint.UI.ScreenName);

    public string UnitDesc => GameLocale.GetString(this.BuilderInstance.Blueprint.UI.LongDescription);

    public string UnitHelpText => GameLocale.GetString(this.BuilderInstance.Blueprint.UI.ShortDescription);

    public ImageSource? UnitPortrait => ResourceHandler.GetIcon("portraits", this.BuilderInstance.Blueprint.UI.Portrait);

    public string CrewTitle => this.CrewBuilderInstance is not null ? GameLocale.GetString(this.CrewBuilderInstance.Blueprint.UI.ScreenName) : "";

    public string CrewDesc => this.CrewBuilderInstance is not null ? GameLocale.GetString(this.CrewBuilderInstance.Blueprint.UI.LongDescription) : "";

    public string CrewHelperDesc => this.CrewBuilderInstance is not null ? GameLocale.GetString(this.CrewBuilderInstance.Blueprint.UI.ShortDescription) : "";

    public ImageSource? CrewPortrait => this.CrewBuilderInstance is not null ? ResourceHandler.GetIcon("portraits", this.CrewBuilderInstance.Blueprint.UI.Portrait) : null;

    public string DeployMethodTitle => this.BuilderInstance.DeployMethod switch {
        DeploymentMethod.None => BattlegroundsInstance.Localize.GetString("CompanySquadView_Deploy_None"),
        DeploymentMethod.DeployAndExit => BattlegroundsInstance.Localize.GetString("CompanySquadView_Deploy_Exit"),
        DeploymentMethod.DeployAndStay => BattlegroundsInstance.Localize.GetString("CompanySquadView_Deploy_Stay"),
        _ => throw new InvalidEnumArgumentException()
    };

    public string DeployMethodDesc => this.BuilderInstance.DeployMethod switch {
        DeploymentMethod.None => BattlegroundsInstance.Localize.GetString("CompanySquadView_Deploy_None_Desc"),
        DeploymentMethod.DeployAndExit => BattlegroundsInstance.Localize.GetString("CompanySquadView_Deploy_Exit_Desc"),
        DeploymentMethod.DeployAndStay => BattlegroundsInstance.Localize.GetString("CompanySquadView_Deploy_Stay_Desc"),
        _ => throw new InvalidEnumArgumentException()
    };

    public Visibility DeployUnitVisible => this.BuilderInstance.DeployMethod is DeploymentMethod.None ? Visibility.Collapsed : Visibility.Visible;

    public string UnitSymbol => this.BuilderInstance.Blueprint.UI.Symbol;

    public ObservableCollection<string> Veterancy => new ObservableCollection<string>(Enumerable.Range(0, 5).Select(x => RankToStar(x, this.BuilderInstance.Rank)));

    public ObservableCollection<string> CrewVeterancy => new ObservableCollection<string>(Enumerable.Range(0, 5).Select(x => RankToStar(x, this.CrewBuilderInstance?.Rank ?? 0)));

    public ObservableCollection<AbilityButton> Abilities { get; }

    public Visibility ShowAbilities => this.Abilities.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

    public ObservableCollection<UpgradeButton> Upgrades { get; }

    public Visibility ShowUpgrades => this.Upgrades.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ShowCrew => this.BuilderInstance.CrewBuilder is not null ? Visibility.Visible : Visibility.Collapsed;

    public ObservableCollection<DeployButton> DeploySettings { get; }

    public ObservableCollection<DeployUnitButton> DeployUnits { get; }

    public TimeSpan CombatTime => this.BuilderInstance.CombatTime;

    public RelayCommand SaveExitCommand { get; }

    public CostExtension Cost => this.TriggerModel.SquadCost;

    public Visibility NameButtonVisibility => UnitBuilder.AllowCustomName(this.BuilderInstance) ? Visibility.Visible : Visibility.Collapsed;

    public ProgressValue Experience => new ProgressValue(this.BuilderInstance.Experience, this.BuilderInstance.Blueprint.Veterancy.MaxExperience);

    public CapacityValue UpgradeCapacity { get; }

    public int LowerSpan => this.BuilderInstance.Blueprint.Category is SquadCategory.Vehicle ? 2 : 1;

    public Visibility DeployMethodsVisible => this.LowerSpan == 1 ? Visibility.Visible : Visibility.Collapsed;

    public EventCommand<MouseEventArgs> RallyClick { get; }

    public bool CanSetStarting => this.CompanyBuilder.IsPhaseAvailable(DeploymentPhase.PhaseInitial) && this.BuilderInstance.Role is DeploymentRole.DirectCommand;

    public bool IsStarting => this.BuilderInstance.Phase is DeploymentPhase.PhaseInitial;

    public bool CanSetStartingVisual => this.IsStarting || this.CanSetStarting;

    public SquadOptionsViewModel(SquadSlotViewModel triggerer, CompanyBuilder companyBuilder) {
        
        // Store trigger and instance refs
        this.TriggerModel = triggerer;
        this.BuilderInstance = triggerer.BuilderInstance;
        this.CompanyBuilder = companyBuilder;

        // Create relay and event commands
        this.SaveExitCommand = new(this.CloseModal);
        this.RallyClick = new EventCommand<MouseEventArgs>(this.RallyCommand);

        // Collect all abilities
        var abilities = this.BuilderInstance.Abilities.Filter(x => x.UI.Icon is not "").Filter(x => ResourceHandler.HasIcon("ability_icons", x.UI.Icon));
        this.Abilities = new ObservableCollection<AbilityButton>(abilities.Map(x => new AbilityButton(x)));

        // Create upgrade cap
        this.UpgradeCapacity = new(this.BuilderInstance.Blueprint.UpgradeCapacity, () => this.BuilderInstance.Upgrades.Length);

        // Collect all upgrades        
        this.Upgrades = new();

        // Collect all upgrades
        this.BuilderInstance.Blueprint.Upgrades
            .Map(BlueprintManager.FromBlueprintName<UpgradeBlueprint>)
            .Filter(x => x.UI.Icon is not "" && ResourceHandler.HasIcon("upgrade_icons", x.UI.Icon))
            .Map(x => new UpgradeButton(x, () => this.BuilderInstance.Upgrades.Contains(x), () => !this.UpgradeCapacity.IsAtCapacity, new EventCommand<MouseEventArgs>(this.UpgradeCommand)))
            .ForEach(this.Upgrades.Add);

        // Create event command
        var dcmd = new EventCommand<MouseEventArgs>(this.DeployCommand);

        // Create deploy buttons
        this.DeploySettings = new() {
            new DeployButton(DeploymentMethod.None, () => this.BuilderInstance.DeployMethod == DeploymentMethod.None, dcmd),
            new DeployButton(DeploymentMethod.DeployAndExit, () => this.BuilderInstance.DeployMethod == DeploymentMethod.DeployAndExit, dcmd),
            new DeployButton(DeploymentMethod.DeployAndStay, () => this.BuilderInstance.DeployMethod == DeploymentMethod.DeployAndStay, dcmd),
        };

        // If heavy arty remove the none option
        if (this.BuilderInstance.Blueprint.Types.IsHeavyArtillery && !this.BuilderInstance.Blueprint.Types.IsAntiTank)
            this.DeploySettings.RemoveAt(0);

        // Create deploy unit buttons
        this.DeployUnits = new();
        this.BuilderInstance.GetTransportUnits(this.CompanyBuilder.CompanyType)
            .Map(x => new DeployUnitButton(x, () => this.BuilderInstance.Transport == x, new EventCommand<MouseEventArgs>(this.DeployUnitCommand)))
            .ForEach(this.DeployUnits.Add);

    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void UpgradeCommand(object sender, MouseEventArgs args) {

        // Bail if null
        if (sender is not FrameworkElement frameworkElement)
            return;

        // Grab model
        if (frameworkElement.DataContext is not UpgradeButton model) {
            return;
        }

        // Grab bp
        var bp = model.Ubp;

        // Decide what to do
        if (model.IsApplied) {
            this.BuilderInstance.RemoveUpgrade(bp);
        } else if (!this.UpgradeCapacity.IsAtCapacity) {
            this.BuilderInstance.AddUpgrade(bp);
        }

        // Refresh
        this.UpgradeCapacity.Update(this);

        // Loop over
        foreach (var upg in this.Upgrades) {
            upg.Update();
        }

        // Update cost
        this.PropertyChanged?.Invoke(this, new(nameof(Cost)));

        // Mark as handled
        args.Handled = true;

    }

    private void DeployCommand(object sender, MouseEventArgs args) {

        // Bail if null
        if (sender is not FrameworkElement frameworkElement)
            return;

        // Grab model
        if (frameworkElement.DataContext is not DeployButton model) {
            return;
        }

        // Set method
        this.BuilderInstance.SetDeploymentMethod(model.Method);
        if (model.Method is DeploymentMethod.None) {
            this.BuilderInstance.SetTransportBlueprint(string.Empty);
        } else if (this.BuilderInstance.Transport is null && model.Method is not DeploymentMethod.None) {
            if (this.DeployUnits.FirstOrDefault() is DeployUnitButton bttn) {
                this.BuilderInstance.SetTransportBlueprint(bttn.Blueprint);
                foreach (var bp in this.DeployUnits) {
                    bp.Update();
                }
            }
        } 

        // Refresh all settings
        foreach (var method in this.DeploySettings) {
            method.Update();
        }

        // Refresh all units
        foreach (var unit in this.DeployUnits) {
            unit.Update();
        }

        // Update cost
        this.PropertyChanged?.Invoke(this, new(nameof(Cost)));

        // Update UI
        this.PropertyChanged?.Invoke(this, new(nameof(DeployMethodTitle)));
        this.PropertyChanged?.Invoke(this, new(nameof(DeployMethodDesc)));
        this.PropertyChanged?.Invoke(this, new(nameof(DeployUnitVisible)));

        // Mark as handled
        args.Handled = true;

    }

    private void DeployUnitCommand(object sender, MouseEventArgs args) {

        // Bail if null
        if (sender is not FrameworkElement frameworkElement)
            return;

        // Grab model
        if (frameworkElement.DataContext is not DeployUnitButton model) {
            return;
        }

        // Set transport
        this.BuilderInstance.SetTransportBlueprint(model.Blueprint);

        // Refresh all
        foreach (var bp in this.DeployUnits) {
            bp.Update();
        }

        // Update cost
        this.PropertyChanged?.Invoke(this, new(nameof(Cost)));

        // Mark as handled
        args.Handled = true;

    }

    /// <summary>
    /// 
    /// </summary>
    public void OnClose() {

        // Make the model refresh its visual representation
        this.TriggerModel.RefreshData();

    }

    /// <summary>
    /// 
    /// </summary>
    public void CloseModal() {
        this.OnClose();
        App.ViewManager.GetRightsideModalControl()?.CloseModal();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    public void SetCustomName(string text)
        => this.BuilderInstance.SetCustomName(text);

    internal void RefreshName() {
        this.PropertyChanged?.Invoke(this, new(nameof(this.UnitName)));
    }

    private void RallyCommand(object sender, MouseEventArgs args) {

        // Bail if null
        if (sender is not FrameworkElement frameworkElement)
            return;

        // Update
        if (this.BuilderInstance.Phase is not DeploymentPhase.PhaseStandard) {
            this.BuilderInstance.SetDeploymentPhase(DeploymentPhase.PhaseStandard);
        } else if (this.CanSetStarting) {
            this.BuilderInstance.SetDeploymentPhase(DeploymentPhase.PhaseInitial);
        }

        // Update starting flag
        this.PropertyChanged?.Invoke(this, new(nameof(CanSetStarting)));
        this.PropertyChanged?.Invoke(this, new(nameof(IsStarting)));

        // Mark as handled
        args.Handled = true;

    }

}
