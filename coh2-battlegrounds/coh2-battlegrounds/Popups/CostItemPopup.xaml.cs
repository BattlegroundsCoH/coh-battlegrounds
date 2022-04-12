using System.Windows;
using System.Windows.Controls;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.Gameplay;

using BattlegroundsApp.Resources;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Popups {

    /// <summary>
    /// Interaction logic for CostItemPopup.xaml
    /// </summary>
    public partial class CostItemPopup : UserControl {

        public string DisplayName { get; }

        public string DisplayDescription { get; }

        public Visibility ManpowerCostVisible => this.ManpowerCost is 0 ? Visibility.Collapsed : Visibility.Visible;

        public Visibility MunitionCostVisible => this.MunitionCost is 0 ? Visibility.Collapsed : Visibility.Visible;

        public Visibility FuelCostVisible => this.FuelCost is 0 ? Visibility.Collapsed : Visibility.Visible;

        public int ManpowerCost { get; }

        public int MunitionCost { get; }

        public int FuelCost { get; }

        public CostItemPopup(string name, string desc, float manpower, float munitions, float fuel) {

            // Set datacontext
            this.DataContext = this;

            // Set data
            this.DisplayName = GameLocale.GetString(name);
            this.DisplayDescription = GameLocale.GetString(desc);
            this.ManpowerCost = (int)manpower;
            this.MunitionCost = (int)munitions;
            this.FuelCost = (int)fuel;

            // Init component
            this.InitializeComponent();

        }

        public CostItemPopup(UIExtension ui, CostExtension cost) : this(ui.ScreenName, ui.LongDescription, cost.Manpower, cost.Munitions, cost.Fuel) { }

        public CostItemPopup(AbilityBlueprint abp, Squad squad) : this(abp.UI, abp.Cost) {
            RequirementVisualiser.Visualize(this.ReqViz, abp.Requirements, squad);
        }

        public CostItemPopup(UpgradeBlueprint ubp, Squad squad) : this(ubp.UI, ubp.Cost) {
            RequirementVisualiser.Visualize(this.ReqViz, ubp.Requirements, squad);
        }

    }

}
