using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

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

            public bool IsValid(SquadBlueprint squadBlueprint) => squadBlueprint.Types.Contains(this.Type) && this.ExcludeTypes.All(x => !squadBlueprint.Types.Contains(x));

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
            if (PropertyChanged is not null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private int _companySize;

        public string CompanyName { get; }

        public int CompanySize { get => this._companySize; set { this._companySize = value; this.NotifyPropertyChanged(); } }

        public Faction CompanyFaction { get; }

        public string CompanyGUID { get; }

        public string CompanyType { get; }

        private List<SquadBlueprint> SquadList
            => BlueprintManager.GetCollection<SquadBlueprint>().FilterByMod(this.CompanyGUID).Filter(x => x.Army == this.CompanyFaction.ToString()).ToList();

        public static SquadCategory[] Category => new[] {
            new SquadCategory {
                Type = "infantry",
                ExcludeTypes = new [] { "team_weapon" }
            },
            new SquadCategory {
                Type = "team_weapon",
                ExcludeTypes = Array.Empty<string>()
            },
            new SquadCategory {
                Type = "vehicle",
                ExcludeTypes = Array.Empty<string>()
            }
        };

        public CompanyBuilder Builder { get; }

        public override void StateOnFocus() { }
        public override void StateOnLostFocus() { }

        public CompanyBuilderView() {
            this.InitializeComponent();
        }

        public CompanyBuilderView(Company company) : this() {
            this.Builder = new CompanyBuilder().DesignCompany(company);
            this.CompanyName = company.Name;
            this.CompanySize = company.Units.Length;
            this.CompanyFaction = company.Army;
            this.CompanyGUID = company.TuningGUID;
            this.CompanyType = company.Type.ToString();
            this.FillAvailableUnits();
            this.ShowCompany();
        }

        // TODO: CHANGE HOW YOU GET THE GUID! -- FOR THE FUTURE TO SUPPORT OTHER MODS (ITS FINE FOR NOW)
        public CompanyBuilderView(string companyName, Faction faction, CompanyType type) : this() {
            this.Builder = new CompanyBuilder().NewCompany(faction).ChangeName(companyName).ChangeType(type).ChangeTuningMod(BattlegroundsInstance.BattleGroundsTuningMod.Guid);
            this.CompanyName = companyName;
            this.CompanySize = 0;
            this.CompanyFaction = faction;
            this.CompanyGUID = BattlegroundsInstance.BattleGroundsTuningMod.Guid;
            this.CompanyType = type.ToString();
            this.FillAvailableUnits();
            this.ShowCompany();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) 
            => PlayerCompanies.SaveCompany(this.Builder.Commit().Result);

        private void BackButton_Click(object sender, RoutedEventArgs e) {

            YesNoDialogResult result = YesNoDialogViewModel.ShowYesNoDialog("Back", "Are you sure? All unsaved changes will be lost.");

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
            this.Builder.EachUnit(x => {

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
            this.Builder.AddUnit(unitBuilder).Commit();
            this.CompanySize++;
            this.ShowCompany();
        }

        public void ReplaceUnitInCompany(UnitBuilder unitBuilder) {
            unitBuilder.Apply();
            this.ShowCompany();
        }

        public void RemoveUnitFromCompany(uint unitID) {
            this.Builder.RemoveUnit(unitID);
            this.CompanySize--;
            this.ShowCompany();
        }

        private void OnDrop(object sender, DragEventArgs e) {

            if (this.CompanySize is not Company.MAX_SIZE) {

                SquadBlueprint squadBlueprint = e.Data.GetData("Squad") as SquadBlueprint;

                UnitBuilder unitBuilder = new UnitBuilder().SetBlueprint(squadBlueprint);
                Squad squad = this.Builder.AddAndCommitUnit(unitBuilder);

                this.CompanySize++;

                this.ShowCompany();

                e.Effects = DragDropEffects.Move;
                e.Handled = true;

            }
        }

    }
}
