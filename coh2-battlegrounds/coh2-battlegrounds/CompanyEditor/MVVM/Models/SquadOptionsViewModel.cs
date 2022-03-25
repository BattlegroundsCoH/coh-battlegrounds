using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;

using BattlegroundsApp.MVVM;
using BattlegroundsApp.Resources;

namespace BattlegroundsApp.CompanyEditor.MVVM.Models;

public class SquadOptionsViewModel {

    public record AbilityButton(AbilityBlueprint Abp) {
        public ImageSource Icon => App.ResourceHandler.GetIcon("ability_icons", Abp.UI.Icon);
    }

    public record UpgradeButton(UpgradeBlueprint Ubp, bool IsApplied, EventCommand Clicked) {
        public ImageSource Icon => App.ResourceHandler.GetIcon("upgrade_icons", Ubp.UI.Icon);
    }

    public record PhaseButton(DeploymentPhase Phase, Func<bool> IsActive, EventCommand Clicked) : INotifyPropertyChanged {
        public bool IsActivePhase => this.IsActive();
        public bool IsPickable { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public ImageSource Icon => this.Phase switch {
            DeploymentPhase.PhaseInitial => App.ResourceHandler.GetIcon("phase_icons", "Icons_research_german_battle_phase_01"),
            DeploymentPhase.PhaseA => App.ResourceHandler.GetIcon("phase_icons", "Icons_research_german_battle_phase_02"),
            DeploymentPhase.PhaseB => App.ResourceHandler.GetIcon("phase_icons", "Icons_research_german_battle_phase_03"),
            DeploymentPhase.PhaseC => App.ResourceHandler.GetIcon("phase_icons", "Icons_research_german_battle_phase_04"),
            _ => throw new InvalidEnumArgumentException()
        };
        public void Update(bool pickable) {
            this.IsPickable = pickable;
            this.PropertyChanged?.Invoke(this, new(nameof(IsPickable)));
            this.PropertyChanged?.Invoke(this, new(nameof(IsActivePhase)));
        }
    }

    public record DeployButton(DeploymentMethod Method, Func<bool> IsActive) {
        public ImageSource Icon => this.Method switch {
            DeploymentMethod.None => App.ResourceHandler.GetIcon("deploy_icons", "Icons_bg_deploy_none"),
            DeploymentMethod.DeployAndExit => App.ResourceHandler.GetIcon("deploy_icons", "Icons_bg_deploy_drop_exit"),
            DeploymentMethod.DeployAndStay => App.ResourceHandler.GetIcon("deploy_icons", "Icons_bg_deploy_drop_stay"),
            _ => throw new InvalidEnumArgumentException()
        };
    }

    public UnitBuilder BuilderInstance { get; }

    public CompanyBuilder CompanyBuilder { get; }

    public SquadSlotViewModel TriggerModel { get; }

    public string UnitName => GameLocale.GetString(this.BuilderInstance.GetName());

    public string UnitDesc => GameLocale.GetString(this.BuilderInstance.Blueprint.UI.LongDescription);

    public ObservableCollection<object> Veterancy => new ObservableCollection<object>( Enumerable.Range(0, this.BuilderInstance.Rank).Select(x => (object)x) );

    public ObservableCollection<AbilityButton> Abilities { get; }

    public Visibility ShowAbilities => this.Abilities.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

    public ObservableCollection<UpgradeButton> Upgrades { get; }

    public Visibility ShowUpgrades => this.Upgrades.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

    public ObservableCollection<PhaseButton> Phases { get; }

    public ObservableCollection<DeployButton> DeploySettings { get; }

    public SquadOptionsViewModel(SquadSlotViewModel triggerer, CompanyBuilder companyBuilder) {
        
        // Store trigger and instance refs
        this.TriggerModel = triggerer;
        this.BuilderInstance = triggerer.BuilderInstance;
        this.CompanyBuilder = companyBuilder;

        // Collect all abilities
        var abilities = this.BuilderInstance.Abilities.Filter(x => x.UI.Icon is not "").Filter(x => App.ResourceHandler.HasIcon("ability_icons", x.UI.Icon));
        this.Abilities = new ObservableCollection<AbilityButton>(abilities.Map(x => new AbilityButton(x)));

        // Collect all upgrades        
        this.Upgrades = new();
        this.RefreshUpgrades();

        // Create phase buttons
        this.Phases = new ObservableCollection<PhaseButton>() {
            new PhaseButton(DeploymentPhase.PhaseInitial, () => this.BuilderInstance.Phase == DeploymentPhase.PhaseInitial, new EventCommand<MouseEventArgs>(this.PhaseCommand)) {
                IsPickable = this.CompanyBuilder.IsPhaseAvailable(DeploymentPhase.PhaseInitial)
            },
            new PhaseButton(DeploymentPhase.PhaseA, () => this.BuilderInstance.Phase == DeploymentPhase.PhaseA, new EventCommand<MouseEventArgs>(this.PhaseCommand)){
                IsPickable = this.CompanyBuilder.IsPhaseAvailable(DeploymentPhase.PhaseA)
            },
            new PhaseButton(DeploymentPhase.PhaseB, () => this.BuilderInstance.Phase == DeploymentPhase.PhaseB, new EventCommand<MouseEventArgs>(this.PhaseCommand)){
                IsPickable = this.CompanyBuilder.IsPhaseAvailable(DeploymentPhase.PhaseB)
            },
            new PhaseButton(DeploymentPhase.PhaseC, () => this.BuilderInstance.Phase == DeploymentPhase.PhaseC, new EventCommand<MouseEventArgs>(this.PhaseCommand)){
                IsPickable = this.CompanyBuilder.IsPhaseAvailable(DeploymentPhase.PhaseC)
            }
        };

        // Create deploy buttons
        this.DeploySettings = new() {
            new DeployButton(DeploymentMethod.None, () => this.BuilderInstance.DeployMethod == DeploymentMethod.None),
            new DeployButton(DeploymentMethod.DeployAndExit, () => this.BuilderInstance.DeployMethod == DeploymentMethod.DeployAndExit),
            new DeployButton(DeploymentMethod.DeployAndStay, () => this.BuilderInstance.DeployMethod == DeploymentMethod.DeployAndStay),
        };

    }

    private void UpgradeCommand(object sender, MouseEventArgs args) {

        // Grab model
        if ((sender as FrameworkElement).DataContext is not UpgradeButton model) {
            return;
        }

        // Grab bp
        var bp = model.Ubp;

        // Decide what to do
        if (model.IsApplied) {
            this.BuilderInstance.RemoveUpgrade(bp);
        } else {
            this.BuilderInstance.AddUpgrade(bp);
        }

        // Refresh upgrades
        this.RefreshUpgrades();

    }

    private void RefreshUpgrades() {

        // Clear upgrades
        this.Upgrades.Clear();

        // Collect all upgrades
        var upgrades = this.BuilderInstance.Blueprint.Upgrades
            .Map(x => BlueprintManager.FromBlueprintName<UpgradeBlueprint>(x))
            .Filter(x => x.UI.Icon is not "" && App.ResourceHandler.HasIcon("upgrade_icons", x.UI.Icon))
            .Map(x => new UpgradeButton(x, this.BuilderInstance.Upgrades.Contains(x), new EventCommand<MouseEventArgs>(this.UpgradeCommand)))
            .ForEach(this.Upgrades.Add);

    }

    private void PhaseCommand(object sender, MouseEventArgs args) {

        // Grab model
        if ((sender as FrameworkElement).DataContext is not PhaseButton model) {
            return;
        }

        // Do nothing is not pickable
        if (!model.IsPickable)
            return;

        // Set phase
        this.BuilderInstance.SetDeploymentPhase(model.Phase);

        // Refresh all
        foreach (var phase in this.Phases) {
            phase.Update(this.CompanyBuilder.IsPhaseAvailable(phase.Phase));
        }

    }

}
