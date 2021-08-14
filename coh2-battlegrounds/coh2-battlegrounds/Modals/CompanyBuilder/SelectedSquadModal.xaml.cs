using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

using BattlegroundsApp.Controls;
using BattlegroundsApp.Controls.CompanyBuilderControls;
using BattlegroundsApp.Dialogs.YesNo;
using BattlegroundsApp.Popups;
using BattlegroundsApp.Resources;

/*
 * 
 * TODO:
 *  Slot Items
 *  Crew Slot Items
 * 
 */

namespace BattlegroundsApp.Modals.CompanyBuilder {

    /// <summary>
    /// Interaction logic for SelectedSquadModal.xaml
    /// </summary>
    public partial class SelectedSquadModal : Modal, INotifyPropertyChanged {

        public static readonly ImageSource VetRankAchieved
            = new BitmapImage(new Uri($"pack://application:,,,/Resources/ingame/vet/vstar_yes.png"));

        public static readonly ImageSource VetRankNotAchieved
            = new BitmapImage(new Uri($"pack://application:,,,/Resources/ingame/vet/vstar_no.png"));

        private const double VETEXPMAXWIDTH = 136.0;

        private string[] m_availableUpgrades;

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

        public ImageSource Rank1 { get; set; } = VetRankNotAchieved;

        public ImageSource Rank2 { get; set; } = VetRankNotAchieved;

        public ImageSource Rank3 { get; set; } = VetRankNotAchieved;

        public ImageSource Rank4 { get; set; } = VetRankNotAchieved;

        public ImageSource Rank5 { get; set; } = VetRankNotAchieved;

        public Visibility EditNamePossible => this.RankLevel >= 4 ? Visibility.Visible : Visibility.Collapsed;

        public ImageSource CrewIcon { get; set; }

        public ImageSource CrewVeterancy { get; set; }

        public string CrewName { get; set; }

        public Visibility TransportBlueprintSelector { get; set; } = Visibility.Collapsed;

        public Visibility SelectDeploymentMethodVisible { get; }

        public Visibility CrewVisible { get; }

        public Visibility SlotItemVisible { get; }

        public int SlotItemCount { get; }

        public int SlotItemCapacity { get; }

        public int UpgradesCount { get; set; }

        public int UpgradesCapacity { get; }

        public int AbilitySpan => this.UpgradesCapacity is 0 ? 2 : 1;

        public Visibility UpgradeVisibility => this.UpgradesCapacity is 0 ? Visibility.Collapsed : Visibility.Visible;

        public string SelectedSupportBlueprint { get; set; }

        public CostItemPopup SellectedSupportBlueprintTooltip { get; set; }

        public SquadSlotLarge SquadSlot { get; } // We keep a ref to this so we can instantly update it

        public event Action<SquadSlotLarge> OnCrewRemove;

        public event Action<SquadSlotLarge> OnCrewEject;

        private ModPackage m_package;

        public SelectedSquadModal(SquadSlotLarge squadSlot, ModPackage modPackage) {
            this.DataContext = this;
            this.SquadSlot = squadSlot;
            this.m_package = modPackage;

            // Get description
            this.LongDesc = GameLocale.GetString(squadSlot.SquadInstance.SBP.UI.LongDescription);
            this.ShortDesc = GameLocale.GetString(squadSlot.SquadInstance.SBP.UI.ShortDescription);

            // Get combat time
            this.CombatTime = squadSlot.SquadInstance.CombatTime == TimeSpan.Zero ? "Has yet to see combat" : squadSlot.SquadInstance.CombatTime.ToString(@"hh\:mm:\ss");

            // Get experience
            this.ExperienceLevel = squadSlot.SquadInstance.VeterancyProgress;
            this.MaxExperienceLevel = squadSlot.SquadInstance.SBP.Veterancy.MaxExperience;

            // Get rank
            this.RankLevel = squadSlot.SquadInstance.VeterancyRank;
            this.MaxRankLevel = squadSlot.SquadInstance.SBP.Veterancy.MaxRank;

            ImageSource[] rankIcoRefs = { this.Rank1, this.Rank2, this.Rank3, this.Rank4, this.Rank5 };

            // Update rank icons
            for (byte i = 0; i < this.RankLevel; i++) {
                rankIcoRefs[i] = VetRankAchieved;
            }

            // Get item
            this.SlotItemCount = squadSlot.SquadInstance.SlotItems.Count;
            this.SlotItemCapacity = squadSlot.SquadInstance.SBP.PickupCapacity;
            this.SlotItemVisible = (squadSlot.SquadInstance.SBP.CanPickupItems && this.SlotItemCount > 0) ? Visibility.Visible : Visibility.Collapsed;

            // Get upgrades
            this.m_availableUpgrades = this.SquadSlot.SquadInstance.SBP.GetUpgrades(true, false);
            this.UpgradesCapacity = this.SquadSlot.SquadInstance.SBP.UpgradeCapacity;
            if (this.UpgradesCapacity is -1) {
                this.UpgradesCapacity = this.m_availableUpgrades.Length;
            }

            // Init component
            this.InitializeComponent();

            // Refresh cost
            this.RefreshCost();

            // Determine if squad can be deployed through transport.
            bool canTow = this.SquadSlot.SquadInstance.SBP.Types.IsAntiTank || this.SquadSlot.SquadInstance.SBP.Types.IsHeavyArtillery;
            bool isTeamWeapon = this.SquadSlot.SquadInstance.SBP.IsTeamWeapon;
            bool canSetDeploymentMethod = (this.SquadSlot.SquadInstance.SBP.Types.IsInfantry && !isTeamWeapon) || canTow;

            // Set default visibility
            this.CrewVisible = Visibility.Collapsed;
            this.SelectDeploymentMethodVisible = Visibility.Collapsed;

            // Determine if we can set deployment method, if possible, get proper blueprints
            if (canSetDeploymentMethod) {

                // Update visibility
                this.CrewVisible = Visibility.Collapsed;
                this.SelectDeploymentMethodVisible = Visibility.Visible;

                // Refresh deployment methods
                this.RefreshDeplymentNethods();

                // Refresh Transport Blueprint
                this.RefreshTransportBlueprints(canTow);

                // Refresh deployment transport if any selected
                if (this.SquadSlot.SquadInstance.SupportBlueprint is not null) {
                    this.ShowSelectedTransport(this.SquadSlot.SquadInstance.SupportBlueprint as SquadBlueprint);
                }

            } else if (this.SquadSlot.SquadInstance.Crew is Squad crew) {

                // Update visibility
                this.CrewVisible = Visibility.Visible;
                this.SelectDeploymentMethodVisible = Visibility.Collapsed;

                // Refresh crew
                this.RefreshCrew(crew);

            }

            // Notify visibility changes
            this.NotifyPropertyChanged(nameof(this.CrewVisible));
            this.NotifyPropertyChanged(nameof(this.SelectDeploymentMethodVisible));

            // Refresh ability list
            this.RefreshAbilities();

            // Refresh upgrade list
            this.RefreshUpgrades();

            // Refresh slot items
            this.RefreshAvailableSlotItems();
            this.RefreshSlotitems();

            // Refresh deployment phase
            this.RefreshDeploymentPhase();

        }

        private void OnDeploymentPhaseClicked(object sender, MouseButtonEventArgs e) {
            Icon obj = sender as Icon;
            SquadBlueprint sbp = this.SquadSlot.SquadInstance.SupportBlueprint as SquadBlueprint;
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
            this.RefreshDeploymentPhase();
        }

        private void RefreshDeploymentPhase() {

            // Set phase states
            this.PhaseIcon0.IconState = this.SquadSlot.SquadInstance.DeploymentPhase is DeploymentPhase.PhaseInitial ? IconState.Active : IconState.Available;
            this.PhaseIcon1.IconState = this.SquadSlot.SquadInstance.DeploymentPhase is DeploymentPhase.PhaseA ? IconState.Active : IconState.Available;
            this.PhaseIcon2.IconState = this.SquadSlot.SquadInstance.DeploymentPhase is DeploymentPhase.PhaseB ? IconState.Active : IconState.Available;
            this.PhaseIcon3.IconState = this.SquadSlot.SquadInstance.DeploymentPhase is DeploymentPhase.PhaseC ? IconState.Active : IconState.Available;

        }

        private void RefreshCost() =>
            this.CostField.Cost = this.SquadSlot.SquadInstance.GetCost(); // Get cost and update values

        private void OnDeploymentMethodClicked(object sender, MouseButtonEventArgs e) {
            Icon obj = sender as Icon;
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
                    if (obj.IconState is not IconState.Disabled) {
                        this.TransportBlueprintSelector = Visibility.Collapsed;
                        this.SquadSlot.SquadInstance.SetDeploymentMethod(null, DeploymentMethod.Paradrop, this.SquadSlot.SquadInstance.DeploymentPhase);
                    }
                    break;
                case "4": // Glider
                    if (obj.IconState is not IconState.Disabled) {
                        this.TransportBlueprintSelector = Visibility.Collapsed;
                        this.SquadSlot.SquadInstance.SetDeploymentMethod(null, DeploymentMethod.Glider, this.SquadSlot.SquadInstance.DeploymentPhase);
                    }
                    break;
                default:
                    break;
            }
            this.RefreshDeplymentNethods();
            this.NotifyPropertyChanged(nameof(this.TransportBlueprintSelector));
            this.RefreshCost();
        }

        private void RefreshTransportBlueprints(bool isTow) {

            // Determine transport list source
            var factionData = this.m_package.FactionSettings[this.SquadSlot.SquadInstance.SBP.Army];
            string[] source = isTow ? factionData.TowTransports : factionData.Transports;

            // Get blueprints
            List<IconComboBoxItem> blueprints = source
                .Select(x => BlueprintManager.FromBlueprintName<SquadBlueprint>(x))
                .Select(x => new IconComboBoxItem(App.ResourceHandler.GetIcon("unit_icons", x.UI.Icon), GameLocale.GetString(x.UI.ScreenName), x))
                .ToList();

            // Add to combobox
            this.TransportBlueprintCombobox.SetItemSource(blueprints);

            // Update selected blueprint
            this.NotifyPropertyChanged(nameof(this.SelectedSupportBlueprint));

        }

        private void TransportBlueprintCombobox_SelectionChanged(object sender, IconComboBoxSelectedChangedEventArgs newItem) {
            if (this.SquadSlot.SquadInstance.DeploymentMethod is DeploymentMethod.DeployAndExit or DeploymentMethod.DeployAndStay) {
                SquadBlueprint sbp = newItem.Item.Source as SquadBlueprint;
                this.SquadSlot.SquadInstance.SetDeploymentMethod(sbp, this.SquadSlot.SquadInstance.DeploymentMethod, this.SquadSlot.SquadInstance.DeploymentPhase);
                this.ShowSelectedTransport(sbp);
            }
        }

        private void ShowSelectedTransport(SquadBlueprint sbp) {
            this.SelectedSupportBlueprint = GameLocale.GetString(sbp.UI.ScreenName);
            this.SellectedSupportBlueprintTooltip = new CostItemPopup(sbp.UI, sbp.Cost);
            this.NotifyPropertyChanged(nameof(this.SelectedSupportBlueprint));
            this.NotifyPropertyChanged(nameof(this.SellectedSupportBlueprintTooltip));
            this.RefreshCost();
        }

        private void RefreshAbilities() {

            // Get all abilities
            string[] abilities = this.SquadSlot.SquadInstance.SBP.GetAbilities(true);
            var abps = abilities.Select(x => BlueprintManager.FromBlueprintName<AbilityBlueprint>(x))
                .Where(x => !string.IsNullOrEmpty(x.UI.Icon) && App.ResourceHandler.HasIcon("ability_icons", x.UI.Icon));

            // Create observable collection
            this.AbilitiesGrid.Icons = new(abps.Select(x => new IconGridElement() {
                Icon = x.UI.Icon,
                Tag = x,
                Tooltip = new CostItemPopup(x, this.SquadSlot.SquadInstance)
            }));

        }

        private void RefreshAvailableSlotItems() {



        }

        private void RefreshSlotitems() {



        }

        private void RefreshUpgrades() {

            // Update capacities
            this.UpgradesCount = this.SquadSlot.SquadInstance.Upgrades.Count;

            // Check for upgrade count
            if (this.m_availableUpgrades.Length is <= 0) {
                return;
            }

            // Get upgrades
            var ubps = this.m_availableUpgrades.Select(x => BlueprintManager.FromBlueprintName<UpgradeBlueprint>(x))
            .Where(x => !string.IsNullOrEmpty(x.UI.Icon));

            // Create observable collection
            this.UpgradesGrid.Icons = new(ubps.Select(x => new IconGridElement() {
                Icon = x.UI.Icon,
                Tag = x,
                Tooltip = new CostItemPopup(x, this.SquadSlot.SquadInstance),
                State = this.GetUpgradeState(x)
            }));

        }

        private IconState GetUpgradeState(UpgradeBlueprint ubp) {
            if (this.SquadSlot.SquadInstance.Upgrades.Contains(ubp)) {
                return IconState.Active;
            }
            return ubp.Requirements.Length is 0 || ubp.Requirements.All(x => x.IsTrue(this.SquadSlot.SquadInstance)) ? IconState.Available : IconState.Disabled;
        }

        private void RefreshCrew(Squad crew) {

            // Get crew icon
            this.CrewIcon = App.ResourceHandler.HasIcon("unit_icons", crew.SBP.UI.Icon)
                ? App.ResourceHandler.GetIcon("unit_icons", crew.SBP.UI.Icon)
                : new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/no_icon.png"));

            // Get crew sbp name
            this.CrewName = GameLocale.GetString(crew.SBP.UI.ScreenName);

            // Get veterancy
            if (crew.VeterancyRank > 0) {
                this.CrewVeterancy = new BitmapImage(new Uri($"pack://application:,,,/Resources/ingame/vet/vstar{crew.VeterancyRank}.png"));
            }

            // Notify property changes
            this.NotifyPropertyChanged(nameof(this.CrewIcon));
            this.NotifyPropertyChanged(nameof(this.CrewName));
            this.NotifyPropertyChanged(nameof(this.CrewVeterancy));

        }

        private void RefreshDeplymentNethods() {

            // Set method states
            this.DeployIcon0.IconState = this.SquadSlot.SquadInstance.DeploymentMethod is DeploymentMethod.None ? IconState.Active : IconState.Available;
            this.DeployIcon1.IconState = this.SquadSlot.SquadInstance.DeploymentMethod is DeploymentMethod.DeployAndExit ? IconState.Active : IconState.Available;
            this.DeployIcon2.IconState = this.SquadSlot.SquadInstance.DeploymentMethod is DeploymentMethod.DeployAndStay ? IconState.Active : IconState.Available;

            // TEMP SOLUTION
            this.DeployIcon3.IconState = IconState.Disabled;
            this.DeployIcon4.IconState = IconState.Disabled;

        }

        protected override void OnModalClosing(ModalCloseEventArgs closeArgs) {

            // Call base
            base.OnModalClosing(closeArgs);

            // Update slot
            this.SquadSlot.RefreshData();

        }

        private void UpgradesGrid_Clicked(object sender, IconGridIconClickedEventArgs args) {

            // Bail if already applied
            if (args.Element.IconState is IconState.Active) {
                return;
            }

            // Get clicked blueprint
            UpgradeBlueprint upb = args.ClickTag as UpgradeBlueprint;

            // Create dialog to verify user wishes to upgrade.
            ModalDialog confirmModal = ModalDialog.CreateModal("Apply Upgrade", $"Are you sure you want to apply upgrade {GameLocale.GetString(upb.UI.ScreenName)}?",
                YesNoDialogResult.Confirm,
                YesNoDialogResult.Cancel,
                (sender, success, value) => {
                    if (success) {

                        // Add upgrade
                        this.SquadSlot.SquadInstance.AddUpgradeIfNotFound(upb);

                        // Update upgrade slot
                        args.Element.IconState = IconState.Active;

                    }
                });

            // Show the modal
            this.LayeredModal.ShowModal(confirmModal);

        }

        private void OnRemoveCrew(object sender, MouseButtonEventArgs e) {

            // Create dialog to verify user wishes to upgrade.
            ModalDialog confirmModal = ModalDialog.CreateModal("Remove Crew", $"Are you sure you want to remove the crew?\nThis will put the vehicle into the equipment view.",
                YesNoDialogResult.Confirm,
                YesNoDialogResult.Cancel,
                (sender, success, value) => {
                    if (success) {
                        this.OnCrewRemove?.Invoke(this.SquadSlot);
                        this.CloseModal(); // Close self
                    }
                });

            // Show the modal
            this.LayeredModal.ShowModal(confirmModal);

        }
    }

}
