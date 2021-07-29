using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using BattlegroundsApp.Controls.CompanyBuilderControls;
using BattlegroundsApp.Resources;

namespace BattlegroundsApp.Modals.CompanyBuilder {

    /// <summary>
    /// Interaction logic for SelectedSquadModal.xaml
    /// </summary>
    public partial class SelectedSquadModal : Modal {

        private const double VETEXPMAXWIDTH = 136.0;

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

        public Visibility ManpowerCostVisible => this.ManpowerCost is 0 ? Visibility.Collapsed : Visibility.Visible;

        public Visibility MunitionCostVisible => this.MunitionCost is 0 ? Visibility.Collapsed : Visibility.Visible;

        public Visibility FuelCostVisible => this.FuelCost is 0 ? Visibility.Collapsed : Visibility.Visible;

        public int ManpowerCost { get; set; }

        public int MunitionCost { get; set; }

        public int FuelCost { get; set; }

        public SquadSlotLarge SquadSlot { get; } // We keep a ref to this so we can instantly update it

        public SelectedSquadModal(SquadSlotLarge squadSlot) {
            this.DataContext = this;
            this.SquadSlot = squadSlot;

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
            var cost = this.SquadSlot.SquadInstance.GetCost();
            this.ManpowerCost = (int)cost.Manpower;
            this.MunitionCost = (int)cost.Munitions;
            this.FuelCost = (int)cost.Fuel;
        }

    }

}
