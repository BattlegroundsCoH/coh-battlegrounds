using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using BattlegroundsApp.Controls.CompanyBuilderControls;
using BattlegroundsApp.Dialogs.YesNo;
using BattlegroundsApp.LocalData;

namespace BattlegroundsApp.Views {
    /// <summary>
    /// Interaction logic for CompanyBuilderView.xaml
    /// </summary>

    public partial class CompanyBuilderView : ViewState, INotifyPropertyChanged {

        public struct SquadCategory {

            public string Type { get; init; }
            public string[] ExcludeTypes { get; init; }

            public bool IsValid(SquadBlueprint squadBlueprint) => squadBlueprint.Types.Contains(this.Type) && ExcludeTypes.All(x => !squadBlueprint.Types.Contains(x));

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
            if (PropertyChanged is not null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string CompanyName { get; }
        private int _companySize;
        public int CompanySize { get { return this._companySize; } set { this._companySize = value; NotifyPropertyChanged(); } }
        public int CompanyMaxSize { get { return Company.MAX_SIZE; } }
        public Faction CompanyFaction { get; }
        public string CompanyGUID { get; }
        public string CompanyType { get; }

        private List<SquadBlueprint> SquadList => BlueprintManager.GetCollection<SquadBlueprint>().FilterByMod(CompanyGUID).Filter(x => x.Army == CompanyFaction.ToString()).ToList();

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
            CompanySize = company.Units.Length;
            CompanyFaction = company.Army;
            CompanyGUID = company.TuningGUID;
            CompanyType = company.Type.ToString();
            FillAvailableUnits();
            ShowCompany();
        }

        // TODO: CHANGE HOW YOU GET THE GUID! -- FOR THE FUTURE TO SUPPORT OTHER MODS (ITS FINE FOR NOW)
        public CompanyBuilderView(string companyName, Faction faction, CompanyType type) : this() {
            builder = new CompanyBuilder().NewCompany(faction).ChangeName(companyName).ChangeType(type).ChangeTuningMod(BattlegroundsInstance.BattleGroundsTuningMod.Guid);
            CompanyName = companyName;
            CompanySize = 0;
            CompanyFaction = faction;
            CompanyGUID = BattlegroundsInstance.BattleGroundsTuningMod.Guid;
            CompanyType = type.ToString();
            FillAvailableUnits();
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

        private void FillAvailableUnits() {

            foreach(SquadBlueprint squad in SquadList) {
                SquadSlotSmall unitSlot = new SquadSlotSmall(squad);
                this.AvailableUnitsStack.Children.Add(unitSlot);
            }

        }

        private void ShowCompany() {

            this.InfantryWrap.Children.Clear();
            this.SupportWrap.Children.Clear();
            this.VehicleWrap.Children.Clear();
            //this.AbilityList.Children.Clear();

            // TODO: Make UnitSlot addition more generic
            builder.EachUnit(x => {

                if (x.GetCategory(true) == "infantry") {
                    UnitSlot unitSlot = new UnitSlot(this, UnitSlotType.Infantry);
                    unitSlot.SetUnit(x);
                    this.InfantryWrap.Children.Add(unitSlot);

                } else if (x.GetCategory(true) == "team_weapon") {
                    UnitSlot unitSlot = new UnitSlot(this, UnitSlotType.Support);
                    unitSlot.SetUnit(x);
                    this.SupportWrap.Children.Add(unitSlot);

                }
                else if (x.GetCategory(true) == "vehicle") {
                    UnitSlot unitSlot = new UnitSlot(this, UnitSlotType.Vehicle);
                    unitSlot.SetUnit(x);
                    this.VehicleWrap.Children.Add(unitSlot);
                }

                // TODO: Add abilities to list

            });
        }

        public void AddUnitToCompany(UnitBuilder unitBuilder) {
            builder.AddUnit(unitBuilder);
            builder.Commit();
            CompanySize++;
            ShowCompany();
        }

        public void ReplaceUnitInCompany(UnitBuilder unitBuilder) {
            unitBuilder.Apply();
            ShowCompany();
        }

        public void RemoveUnitFromCompany(uint unitID) {
            builder.RemoveUnit(unitID);
            CompanySize--;
            ShowCompany();
        }

        private void OnDrop(object sender, DragEventArgs e) {

            if (CompanySize != CompanyMaxSize) {
                SquadBlueprint squadBlueprint = e.Data.GetData("Squad") as SquadBlueprint;

                UnitBuilder unitBuilder = new UnitBuilder();
                unitBuilder.SetBlueprint(squadBlueprint);

                var squad = builder.AddAndCommitUnit(unitBuilder);
                CompanySize++;

                ShowCompany();

                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }

    }
}
