using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

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

        private const double VETEXPMAXWIDTH = 136.0;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public string Icon => this.SquadSlot.SquadIcon;

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

        }

        private void OnDeploymentPhaseClicked(object sender, MouseButtonEventArgs e) {
            var obj = sender as Image;
            switch (obj.Tag as string) {
                case "0":
                    break;
                case "1":
                    break;
                case "2":
                    break;
                case "3":
                    break;
                default:
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
        }

        private void RefreshTransportBlueprints() {

            // Get blueprints
            var blueprints = this.m_package.FactionSettings[this.SquadSlot.SquadInstance.SBP.Army].Transports
                .Select(x => BlueprintManager.FromBlueprintName<SquadBlueprint>(x))
                .Select(x => new IconComboBoxItem(new BitmapImage(new Uri($"pack://application:,,,/Resources/ingame/unit_icons/{x.UI.Icon}.png")), GameLocale.GetString(x.UI.ScreenName), x))
                .ToList();

            // Add to combobox
            this.TransportBlueprintCombobox.SetItemSource(blueprints);

        }

        private void TransportBlueprintCombobox_SelectionChanged(object sender, IconComboBoxItem newItem) {
            SquadBlueprint sbp = newItem.Source as SquadBlueprint;
            this.SelectedSupportBlueprint = GameLocale.GetString(sbp.UI.ScreenName);
            this.SquadSlot.SquadInstance.SetDeploymentMethod(sbp, this.SquadSlot.SquadInstance.DeploymentMethod, this.SquadSlot.SquadInstance.DeploymentPhase);
            this.NotifyPropertyChanged(nameof(this.SelectedSupportBlueprint));
        }

    }

}
