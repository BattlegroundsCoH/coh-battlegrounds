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
using Battlegrounds;
using Battlegrounds.Game.Database;
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

        public struct SquadCategory {

            public string Type { get; init; }
            public string[] ExcludeTypes { get; init; }

            public bool IsValid(SquadBlueprint squadBlueprint) => squadBlueprint.Types.Contains(this.Type) && ExcludeTypes.All(x => !squadBlueprint.Types.Contains(x));

        }

        public string CompanyName { get; }
        public string CompanySize { get; }
        public string CompanyMaxSize { get { return Company.MAX_SIZE.ToString(); } }
        public string CompanySizeText => $"Company Size: {this.CompanySize}/{this.CompanyMaxSize}";
        public Faction CompanyFaction { get; }
        public string CompanyGUID { get; }

        public SquadCategory[] Category => new[] { 
            new SquadCategory {
                Type = "infantry",
                ExcludeTypes = new [] { "team_weapon" }
            }, 
            new SquadCategory { 
                Type = "team_weapon",
                ExcludeTypes = new string[0]
            }, 
            new SquadCategory { 
                Type = "vehicle",
                ExcludeTypes = new string[0]
            } 
        };

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
            CompanyFaction = company.Army;
            CompanyGUID = company.TuningGUID;
            ShowCompany();
        }

        // TODO: CHANGE HOW YOU GET THE GUID!
        public CompanyBuilderView(string companyName, Faction faction, CompanyType type) : this() {
            builder = new CompanyBuilder().NewCompany(faction).ChangeName(companyName).ChangeType(type).ChangeTuningMod(BattlegroundsInstance.BattleGroundsTuningMod.Guid);
            CompanyName = companyName;
            CompanySize = "0";
            CompanyFaction = faction;
            CompanyGUID = BattlegroundsInstance.BattleGroundsTuningMod.Guid;
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

            // TODO: Make UnitSlot addition more generic
            builder.EachUnit(x => {

                if (x.GetCategory(true) == "infantry") {
                    UnitSlot unitSlot = new UnitSlot(this, UnitSlotType.Infantry);
                    unitSlot.SetUnit(x);
                    this.InfantryList.Children.Add(unitSlot);

                } else if (x.GetCategory(true) == "team_weapon") {
                    UnitSlot unitSlot = new UnitSlot(this, UnitSlotType.Support);
                    unitSlot.SetUnit(x);
                    this.SupportList.Children.Add(unitSlot);

                } else if (x.GetCategory(true) == "vehicle") {
                    UnitSlot unitSlot = new UnitSlot(this, UnitSlotType.Vehicle);
                    unitSlot.SetUnit(x);
                    this.VehicleList.Children.Add(unitSlot);
                } 

                // TODO: Add abilities to list

            });

            UnitSlot AddSlotInfantry = new UnitSlot(this, UnitSlotType.Infantry);
            AddSlotInfantry.SetUnit(null);

            UnitSlot AddSlotSupport = new UnitSlot(this, UnitSlotType.Support);
            AddSlotSupport.SetUnit(null);

            UnitSlot AddSlotVehicle = new UnitSlot(this, UnitSlotType.Vehicle);
            AddSlotVehicle.SetUnit(null);

            UnitSlot AddSlotAbility = new UnitSlot(this, UnitSlotType.Ability);
            AddSlotAbility.SetUnit(null);

            this.InfantryList.Children.Add(AddSlotInfantry);
            this.SupportList.Children.Add(AddSlotSupport);
            this.VehicleList.Children.Add(AddSlotVehicle);
            this.AbilityList.Children.Add(AddSlotAbility);
        }

        public void AddUnitToCompany(UnitBuilder unitBuilder) {
            builder.AddUnit(unitBuilder);
            builder.Commit();
            ShowCompany();
        }

        public void ReplaceUnitInCompany(UnitBuilder unitBuilder) {
            unitBuilder.Apply();
            ShowCompany();
        }

        public void RemoveUnitFromCompany(uint unitID) {
            builder.RemoveUnit(unitID);
            ShowCompany();

        }
    }
}
