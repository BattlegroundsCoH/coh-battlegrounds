using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

using BattlegroundsApp.Controls.CompanyBuilderControls;
using BattlegroundsApp.Dialogs.YesNo;
using BattlegroundsApp.LocalData;
using BattlegroundsApp.Modals.CompanyBuilder;

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
        private string m_initialChecksum;
        private ModPackage m_activeModPackage;

        public string CompanyName { get; }

        public int CompanySize { get => this._companySize; set { this._companySize = value; this.NotifyPropertyChanged(); } }

        public string CompanyUnitHeaderItem => $"Units ({this.CompanySize}/{Company.MAX_SIZE})";

        public Faction CompanyFaction { get; }

        public string CompanyGUID { get; }

        public string CompanyType { get; }

        public Visibility HoverDataVisiblity { get; set; } = Visibility.Hidden;

        public Visibility InverseHoverDataVisibility => this.HoverDataVisiblity is Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;

        public ObjectHoverData HoverData { get; set; }

        public CompanyBuilder Builder { get; }

        public CompanyStatistics Statistics { get; }

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

        public override void StateOnFocus() { }
        public override void StateOnLostFocus() { }

        public CompanyBuilderView() {
            this.InitializeComponent();
        }

        public CompanyBuilderView(Company company) : this() {
            this.Builder = new CompanyBuilder().DesignCompany(company);
            this.Statistics = company.Statistics;
            this.CompanyName = company.Name;
            this.CompanySize = company.Units.Length;
            this.CompanyFaction = company.Army;
            this.CompanyGUID = company.TuningGUID;
            this.CompanyType = company.Type.ToString();
            this.FillAvailableUnits();
            this.ShowCompany();
            this.m_initialChecksum = company.Checksum;
            this.m_activeModPackage = ModManager.GetPackageFromGuid(company.TuningGUID);
        }

        public CompanyBuilderView(string companyName, Faction faction, CompanyType type, ModGuid modGuid) : this() {
            this.Builder = new CompanyBuilder().NewCompany(faction).ChangeName(companyName).ChangeType(type).ChangeTuningMod(modGuid);
            this.Statistics = new();
            this.CompanyName = companyName;
            this.CompanySize = 0;
            this.CompanyFaction = faction;
            this.CompanyGUID = modGuid;
            this.CompanyType = type.ToString();
            this.FillAvailableUnits();
            this.ShowCompany();
            this.m_initialChecksum = string.Empty;
            this.m_activeModPackage = ModManager.GetPackageFromGuid(modGuid);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
            => PlayerCompanies.SaveCompany(this.Builder.Commit().Result);

        private void BackButton_Click(object sender, RoutedEventArgs e) {

            if (this.Builder.CalculateChecksum() == this.m_initialChecksum) {
                this.StateChangeRequest(new CompanyView());
                return;
            }

            YesNoDialogResult result = YesNoDialogViewModel.ShowYesNoDialog("Back", "Are you sure? All unsaved changes will be lost.");

            if (result == YesNoDialogResult.Confirm) {
                this.StateChangeRequest(new CompanyView());
            }

        }

        private void UnitSlot_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        private void FillAvailableUnits() {

            foreach (SquadBlueprint squad in this.SquadList) {
                SquadSlotSmall unitSlot = new SquadSlotSmall(squad);
                unitSlot.OnHoverUpdate += this.OnSlotHover;
                this.AvailableUnitsStack.Children.Add(unitSlot);
            }

        }

        private void OnSlotHover(SquadSlotSmall unitSlot, bool enter) {
            if (!enter) {
                this.HoverDataVisiblity = Visibility.Hidden;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.HoverDataVisiblity)));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.InverseHoverDataVisibility)));
                return;
            }
            this.HoverDataVisiblity = Visibility.Visible;
            this.HoverData = unitSlot.HoverData;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.HoverData)));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.HoverDataVisiblity)));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.InverseHoverDataVisibility)));
        }

        private void ShowCompany() {

            this.InfantryWrap.Children.Clear();
            this.SupportWrap.Children.Clear();
            this.VehicleWrap.Children.Clear();
            //this.AbilityList.Children.Clear();

            // TODO: Make UnitSlot addition more generic
            this.Builder.EachUnit(x => {
                SquadSlotLarge unitSlot = new(x);
                unitSlot.OnClick += this.OnSlotClicked;
                if (x.GetCategory(true) is "infantry") {
                    this.InfantryWrap.Children.Add(unitSlot);
                } else if (x.GetCategory(true) is "team_weapon") {
                    this.SupportWrap.Children.Add(unitSlot);
                } else if (x.GetCategory(true) is "vehicle") {
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

        private void OnSlotClicked(SquadSlotLarge squadSlot) {
            SelectedSquadModal squadModal = new(squadSlot, this.m_activeModPackage);
            // Some more stuff here?
            this.ShowModal(squadModal);
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
