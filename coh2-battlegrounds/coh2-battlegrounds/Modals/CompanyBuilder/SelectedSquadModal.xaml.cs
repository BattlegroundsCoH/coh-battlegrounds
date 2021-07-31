using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

using BattlegroundsApp.Controls;
using BattlegroundsApp.Controls.CompanyBuilderControls;
using BattlegroundsApp.Resources;

namespace BattlegroundsApp.Modals.CompanyBuilder {

    /// <summary>
    /// Interaction logic for SelectedSquadModal.xaml
    /// </summary>
    public partial class SelectedSquadModal : Modal, INotifyPropertyChanged {

        public class SelectSquadAbility {
            public AbilityBlueprint ABP { get; }
            public ImageSource Ico { get; }
            public SelectSquadAbility(AbilityBlueprint abp) {
                this.ABP = abp;
                if (App.ResourceHandler.HasIcon("ability_icons", abp.UI.Icon)) {
                    this.Ico = App.ResourceHandler.GetIcon("ability_icons", abp.UI.Icon);
                } else {
                    Trace.WriteLine($"Failed to locate icon name '{abp.UI.Icon}'.", nameof(ResourceHandler));
                }
            }
        }

        private const double VETEXPMAXWIDTH = 136.0;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ImageSource Icon => this.SquadSlot.SquadIcon;

        public string SBPName => this.SquadSlot.SquadName;

        public string LongDesc { get; }

        public string ShortDesc { get; }

        public string CombatTime { get; }

        public float ExperienceLevel { get; }

        public float MaxExperienceLevel { get; }

        public double ExperienceVisualLevel => this.ExperienceLevel / this.MaxExperienceLevel * VETEXPMAXWIDTH;

        public byte RankLevel { get; }

        public int MaxRankLevel { get; }

        public Visibility EditNamePossible => this.RankLevel >= 4 ? Visibility.Visible : Visibility.Collapsed;

        public Visibility TransportBlueprintSelector { get; set; } = Visibility.Collapsed;

        public bool AllowParadrop { get; }

        public bool AllowGlider { get; }

        public Visibility ManpowerCostVisible => this.ManpowerCost is 0 ? Visibility.Collapsed : Visibility.Visible;

        public Visibility MunitionCostVisible => this.MunitionCost is 0 ? Visibility.Collapsed : Visibility.Visible;

        public Visibility FuelCostVisible => this.FuelCost is 0 ? Visibility.Collapsed : Visibility.Visible;

        public int ManpowerCost { get; set; }

        public int MunitionCost { get; set; }

        public int FuelCost { get; set; }

        public string SelectedSupportBlueprint { get; set; }

        public SquadSlotLarge SquadSlot { get; } // We keep a ref to this so we can instantly update it

        private ModPackage m_package;

        public SelectedSquadModal(SquadSlotLarge squadSlot, ModPackage modPackage) {
            this.DataContext = this;
            this.SquadSlot = squadSlot;
            this.m_package = modPackage;

            // Get description
            this.LongDesc = GameLocale.GetString(squadSlot.SquadInstance.SBP.UI.LongDescription);
            this.ShortDesc = GameLocale.GetString(squadSlot.SquadInstance.SBP.UI.ShortDescription);

            // Get combat time
            this.CombatTime = squadSlot.SquadInstance.CombatTime == TimeSpan.Zero ? "Has yet to see combat" : squadSlot.SquadInstance.CombatTime.ToString();

            // Get experience
            this.ExperienceLevel = squadSlot.SquadInstance.VeterancyProgress;
            this.MaxExperienceLevel = squadSlot.SquadInstance.SBP.Veterancy.MaxExperience;

            // Get rank
            this.RankLevel = squadSlot.SquadInstance.VeterancyRank;
            this.MaxRankLevel = squadSlot.SquadInstance.SBP.Veterancy.MaxRank;

            // Refresh cost
            this.RefreshCost();

            // Init component
            this.InitializeComponent();

            // Refresh Transport Blueprint
            this.RefreshTransportBlueprints();

            // Refresh ability list
            this.RefreshAbilities();

            // Refresh slot items
            this.RefreshAvailableSlotItems();
            this.RefreshSlotitems();

        }

        private void OnDeploymentPhaseClicked(object sender, MouseButtonEventArgs e) {
            var obj = sender as Image;
            var sbp = this.SquadSlot.SquadInstance.SupportBlueprint as SquadBlueprint;
            var dmode = this.SquadSlot.SquadInstance.DeploymentMethod;
            switch (obj.Tag as string) {
                case "0":
                    this.SquadSlot.SquadInstance.SetDeploymentMethod(sbp, dmode, DeploymentPhase.PhaseInitial);
                    break;
                case "1":
                    this.SquadSlot.SquadInstance.SetDeploymentMethod(sbp, dmode, DeploymentPhase.PhaseA);
                    break;
                case "2":
                    this.SquadSlot.SquadInstance.SetDeploymentMethod(sbp, dmode, DeploymentPhase.PhaseB);
                    break;
                case "3":
                    this.SquadSlot.SquadInstance.SetDeploymentMethod(sbp, dmode, DeploymentPhase.PhaseC);
                    break;
                default:
                    this.SquadSlot.SquadInstance.SetDeploymentMethod(sbp, dmode, DeploymentPhase.PhaseNone);
                    break;
            }
        }

        private void RefreshCost() {

            // Get cost and update values
            var cost = this.SquadSlot.SquadInstance.GetCost();
            this.ManpowerCost = (int)cost.Manpower;
            this.MunitionCost = (int)cost.Munitions;
            this.FuelCost = (int)cost.Fuel;

            // Notify property changes
            this.NotifyPropertyChanged(nameof(this.ManpowerCost));
            this.NotifyPropertyChanged(nameof(this.MunitionCost));
            this.NotifyPropertyChanged(nameof(this.FuelCost));
            this.NotifyPropertyChanged(nameof(this.ManpowerCostVisible));
            this.NotifyPropertyChanged(nameof(this.MunitionCostVisible));
            this.NotifyPropertyChanged(nameof(this.FuelCostVisible));

        }

        private void OnDeploymentMethodClicked(object sender, MouseButtonEventArgs e) {
            Image obj = sender as Image;
            SquadBlueprint sbp = this.TransportBlueprintCombobox.SelectedItem.Source as SquadBlueprint;
            switch (obj.Tag as string) {
                case "0": // None
                    this.TransportBlueprintSelector = Visibility.Collapsed;
                    this.SquadSlot.SquadInstance.SetDeploymentMethod(null, DeploymentMethod.None, this.SquadSlot.SquadInstance.DeploymentPhase);
                    break;
                case "1": // Deploy and Exit
                    this.TransportBlueprintSelector = Visibility.Visible;
                    this.SquadSlot.SquadInstance.SetDeploymentMethod(sbp, DeploymentMethod.DeployAndExit, this.SquadSlot.SquadInstance.DeploymentPhase);
                    break;
                case "2": // Deploy and Stay
                    this.TransportBlueprintSelector = Visibility.Visible;
                    this.SquadSlot.SquadInstance.SetDeploymentMethod(sbp, DeploymentMethod.DeployAndStay, this.SquadSlot.SquadInstance.DeploymentPhase);
                    break;
                case "3": // Paradrop
                    this.TransportBlueprintSelector = Visibility.Collapsed;
                    this.SquadSlot.SquadInstance.SetDeploymentMethod(null, DeploymentMethod.Paradrop, this.SquadSlot.SquadInstance.DeploymentPhase);
                    break;
                case "4": // Glider
                    this.TransportBlueprintSelector = Visibility.Collapsed;
                    this.SquadSlot.SquadInstance.SetDeploymentMethod(null, DeploymentMethod.Glider, this.SquadSlot.SquadInstance.DeploymentPhase);
                    break;
                default:
                    break;
            }
            this.NotifyPropertyChanged(nameof(this.TransportBlueprintSelector));
            this.RefreshCost();
        }

        private void RefreshTransportBlueprints() {

            // Get blueprints
            List<IconComboBoxItem> blueprints = this.m_package.FactionSettings[this.SquadSlot.SquadInstance.SBP.Army].Transports
                .Select(x => BlueprintManager.FromBlueprintName<SquadBlueprint>(x))
                .Select(x => new IconComboBoxItem(App.ResourceHandler.GetIcon("unit_icons", x.UI.Icon), GameLocale.GetString(x.UI.ScreenName), x))
                .ToList();

            // Add to combobox
            this.TransportBlueprintCombobox.SetItemSource(blueprints);

        }

        private void TransportBlueprintCombobox_SelectionChanged(object sender, IconComboBoxItem newItem) {
            if (this.SquadSlot.SquadInstance.DeploymentMethod is DeploymentMethod.DeployAndExit or DeploymentMethod.DeployAndStay) {
                SquadBlueprint sbp = newItem.Source as SquadBlueprint;
                this.SelectedSupportBlueprint = GameLocale.GetString(sbp.UI.ScreenName);
                this.SquadSlot.SquadInstance.SetDeploymentMethod(sbp, this.SquadSlot.SquadInstance.DeploymentMethod, this.SquadSlot.SquadInstance.DeploymentPhase);
                this.NotifyPropertyChanged(nameof(this.SelectedSupportBlueprint));
                this.RefreshCost();
            }
        }

        private void RefreshAbilities() {

            // Get all abilities
            string[] abilities = this.SquadSlot.SquadInstance.SBP.GetAbilities(true);
            var abps = abilities.Select(x => BlueprintManager.FromBlueprintName<AbilityBlueprint>(x))
                .Where(x => !string.IsNullOrEmpty(x.UI.Icon));

            // Map to abp presenter class
            SelectSquadAbility[] presenters = abps.Select(x => new SelectSquadAbility(x)).Where(x => x.Ico is not null).ToArray();

            // Create images and append
            presenters.ForEach(x => {
                Image img = new() {
                    Source = x.Ico,
                    Tag = x,
                    ToolTip = GameLocale.GetString(x.ABP.UI.ScreenName),
                    Width = 48,
                    Height = 48
                };
                this.AbilitiesList.Children.Add(img);
            });

        }

        private void RefreshAvailableSlotItems() {



        }

        private void RefreshSlotitems() {



        }

    }

}
