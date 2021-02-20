using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using BattlegroundsApp.Controls.CompanyBuilderControls;
using BattlegroundsApp.Dialogs.YesNo;
using BattlegroundsApp.LocalData;

namespace BattlegroundsApp.Views {
    /// <summary>
    /// Interaction logic for CompanyBuilderView.xaml
    /// </summary>
    public partial class CompanyBuilderView : ViewState {

        public string CompanyName { get; }
        public string CompanySize { get; }
        public string CompanyMaxSize { get { return Company.MAX_SIZE.ToString(); } }
        public string CompanySizeText => $"Company Size: {this.CompanySize}/{this.CompanyMaxSize}";

        private CompanyBuilder builder;
        public CompanyBuilder Builder { get { return this.builder; } }

        public override void StateOnFocus() { }
        public override void StateOnLostFocus() { }

        public CompanyBuilderView() {
            InitializeComponent();
        }

        public CompanyBuilderView(Company company) : this() {
            builder = new CompanyBuilder().DesignCompany(company);
            CompanyName = company.Name;
            CompanySize = company.Units.Length.ToString();
            ShowCompany();
        }

        public CompanyBuilderView(string companyName, Faction faction, CompanyType type) : this() {
            builder = new CompanyBuilder().NewCompany(faction).ChangeName(companyName).ChangeType(type);
            CompanyName = companyName;
            CompanySize = "0";
            ShowCompany();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            var comapny = builder.Commit().Result;
            PlayerCompanies.SaveCompany(comapny);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) {

            var result = YesNoDialogViewModel.ShowYesNoDialog("Back", "Are you sure? All unsaved changes will be lost.");

            if (result == YesNoDialogResult.Confirm) {
                this.StateChangeRequest(new CompanyView());
            }
        }

        private void UnitSlot_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        private void ShowCompany() {

            this.InfantryList.Children.Clear();
            this.SupportList.Children.Clear();
            this.VehicleList.Children.Clear();
            this.AbilityList.Children.Clear();

            builder.EachUnit(x => {

                UnitSlot unitSlot = new UnitSlot(this);
                unitSlot.SetUnit(x);

                if (x.GetCategory(true) == "infantry") {
                    this.InfantryList.Children.Add(unitSlot);

                } else if (x.GetCategory(true) == "team_weapon") {
                    this.SupportList.Children.Add(unitSlot);

                } else if (x.GetCategory(true) == "vehicle") {
                    this.VehicleList.Children.Add(unitSlot);
                } 

                // TODO: Add abilities to list

            });

            UnitSlot AddSlotInfantry = new UnitSlot(this);
            AddSlotInfantry.SetUnit(null);

            UnitSlot AddSlotSupport = new UnitSlot(this);
            AddSlotSupport.SetUnit(null);

            UnitSlot AddSlotVehicle = new UnitSlot(this);
            AddSlotVehicle.SetUnit(null);

            UnitSlot AddSlotAbility = new UnitSlot(this);
            AddSlotAbility.SetUnit(null);

            this.InfantryList.Children.Add(AddSlotInfantry);
            this.SupportList.Children.Add(AddSlotSupport);
            this.VehicleList.Children.Add(AddSlotVehicle);
            this.AbilityList.Children.Add(AddSlotAbility);
        }

        public void AddUnitToCompany(Squad squad) {
            UnitBuilder unitBuilder = new UnitBuilder(squad);
            builder.AddUnit(unitBuilder);
            builder.Commit();
            ShowCompany();
        }
    }
}
