using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

using BattlegroundsApp.Controls.CompanyBuilderControls;
using BattlegroundsApp.Dialogs.OK;
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

        private int m_companySize;
        private int m_companyAbilityCount;
        private ulong m_initialChecksum;
        private readonly ModPackage m_activeModPackage;

        private List<SquadBlueprint> m_availableSquads;
        private List<SquadBlueprint> m_availableCrews;
        private List<AbilityBlueprint> m_abilities;

        public string CompanyName { get; }

        public int CompanySize { get => this.m_companySize; set { this.m_companySize = value; this.NotifyPropertyChanged(); } }

        public int CompanyAbilityCount { get => this.m_companyAbilityCount; set { this.m_companyAbilityCount = value; this.NotifyPropertyChanged(); } }

        public string CompanyUnitHeaderItem => $"Units ({this.CompanySize}/{Company.MAX_SIZE})";

        public string CompanyAbilityHeaderItem => $"Abilities ({this.CompanyAbilityCount}/{Company.MAX_ABILITY})";

        public Faction CompanyFaction { get; }

        public string CompanyGUID { get; }

        public string CompanyType { get; }

        public Visibility HoverDataVisiblity { get; set; } = Visibility.Hidden;

        public Visibility InverseHoverDataVisibility => this.HoverDataVisiblity is Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;

        public ObjectHoverData HoverData { get; set; }

        public CompanyBuilder Builder { get; }

        public CompanyStatistics Statistics { get; }

        public bool CanAddAbilities => this.Builder.CanAddAbility;

        public bool CanAddUnits => this.Builder.CanAddUnit;

        public string AvailableObjectType { get; set; } = "Available Units";

        public Visibility AvailableUnitsVisible { get; set; } = Visibility.Visible;

        public Visibility AvailableAbilitiesVisible { get; set; } = Visibility.Collapsed;

        public Visibility AvailableCrewsVisible { get; set; } = Visibility.Collapsed;

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

        public CompanyBuilderView() => this.InitializeComponent();

        public CompanyBuilderView(Company company) : this() {

            // Set company information
            this.Builder = new CompanyBuilder().DesignCompany(company);
            this.Statistics = company.Statistics;
            this.CompanyName = company.Name;
            this.CompanySize = company.Units.Length;
            this.CompanyFaction = company.Army;
            this.CompanyGUID = company.TuningGUID;
            this.CompanyType = company.Type.ToString();

            // Set fields
            this.m_initialChecksum = company.Checksum;
            this.m_activeModPackage = ModManager.GetPackageFromGuid(company.TuningGUID);

            // Load database and display
            this.LoadFactionDatabase();
            this.ShowCompany();

        }

        public CompanyBuilderView(string companyName, Faction faction, CompanyType type, ModGuid modGuid) : this() {

            // Set properties
            this.Builder = new CompanyBuilder().NewCompany(faction).ChangeName(companyName).ChangeType(type).ChangeTuningMod(modGuid).Commit();
            this.Statistics = new();
            this.CompanyName = companyName;
            this.CompanySize = 0;
            this.CompanyFaction = faction;
            this.CompanyGUID = modGuid;
            this.CompanyType = type.ToString();

            // Set fields
            this.m_initialChecksum = 0;
            this.m_activeModPackage = ModManager.GetPackageFromGuid(modGuid);

            // Load database and display
            this.LoadFactionDatabase();
            this.ShowCompany();

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            var company = this.Builder.Commit().Result;
            PlayerCompanies.SaveCompany(company); // Side-effect: Will triger a checksum recalculation.
            this.m_initialChecksum = company.Checksum; // Update edit detector checksum.
            _ = OKDialogViewModel.ShowOKDialog("Company Saved", "The company was successfully saved on the local machine");
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e) {

            if (YesNoDialogViewModel.ShowYesNoDialog("Reset Company", "Are you sure? The entiry company will be reset") is YesNoDialogResult.Confirm) {
                var backup = this.Builder.Commit().Result;
                _ = this.Builder.ReleaseCompany().NewCompany(backup.Army).ChangeTuningMod(backup.TuningGUID)
                    .ChangeType(backup.Type).ChangeAvailability(CompanyAvailabilityType.MultiplayerOnly);
            }

        }

        private void BackButton_Click(object sender, RoutedEventArgs e) {

            if (this.Builder.CalculateChecksum() == this.m_initialChecksum) {
                _ = this.StateChangeRequest(new CompanyView());
                return;
            }

            if (YesNoDialogViewModel.ShowYesNoDialog("Back", "Are you sure? All unsaved changes will be lost.") is YesNoDialogResult.Confirm) {
                _ = this.StateChangeRequest(new CompanyView());
            }

        }

        private void OnSlotHover(AvailableItemSlot itemSlot, bool enter) {
            if (!enter) {
                this.HoverDataVisiblity = Visibility.Hidden;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.HoverDataVisiblity)));
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.InverseHoverDataVisibility)));
                return;
            }
            this.HoverDataVisiblity = Visibility.Visible;
            this.HoverData = itemSlot.HoverData;
            this.HoverDataCost.Cost = this.HoverData.Cost;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.HoverData)));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.HoverDataVisiblity)));
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.InverseHoverDataVisibility)));
        }

        private void ShowCompany() {

            // Clear the wraps
            this.InfantryWrap.Children.Clear();
            this.SupportWrap.Children.Clear();
            this.VehicleWrap.Children.Clear();
            this.AbilityWrap.Children.Clear();
            this.UnitAbilityWrap.Children.Clear();
            this.EquipmentWrap.Children.Clear();
            this.ItemWrap.Children.Clear();

            // Add all units
            this.Builder.EachUnit(this.AddUnitToDisplay, x => (int)x.DeploymentPhase);

            // Add all abilities
            this.Builder.EachAbility(this.AddAbilityToDisplay);

            // Add all items
            this.Builder.EachItem(this.AddEquipmentToDisplay);

        }

        public void RemoveUnitFromCompany(Squad squad) {

            // If we can currently not add units, fix it so we can now (since we're removing one)
            if (!this.CanAddUnits) {
                this.SetCanAddUnits(true);
            }

            // Remove unit
            _ = this.Builder.RemoveUnit(squad.SquadID);

            // Update company Size
            this.CompanySize--;
            this.NotifyPropertyChanged(nameof(this.CompanyUnitHeaderItem));

        }

        private void OnSlotClicked(SquadSlotLarge squadSlot) {
            SelectedSquadModal modal = new(squadSlot, this.m_activeModPackage);
            modal.OnCrewRemove += this.RemoveCrewAndAddBlueprintToEquipment;
            modal.OnCrewEject += this.EjectCrewAndAddBlueprintToPoolAndToEquipment;
            this.ShowModal(modal);
        }

        private void OnSlotRemoveClicked(SquadSlotLarge squadSlot) {

            // Determine container and remove from there
            switch (squadSlot.SquadInstance.GetCategory(true)) {
                case "infantry":
                    this.InfantryWrap.Children.Remove(squadSlot);
                    break;
                case "team_weapon":
                    this.SupportWrap.Children.Remove(squadSlot);
                    break;
                case "vehicle":
                    this.VehicleWrap.Children.Remove(squadSlot);
                    break;
                default:
                    break;
            }

            // Remove from ocmpany
            this.RemoveUnitFromCompany(squadSlot.SquadInstance);

        }

        private void OnAbilityRemoveClicked(AbilitySlot abilitySlot) {

            // Remove from display
            this.AbilityWrap.Children.Remove(abilitySlot);

            // Remove from company
            _ = this.Builder.RemoveAbility(abilitySlot.Ability);

            // Refresh
            this.SetCanAddAbilities(this.CanAddAbilities);

            // Update company ability count
            this.CompanyAbilityCount--;
            this.NotifyPropertyChanged(nameof(this.CompanyAbilityHeaderItem));

        }

        private void OnEquipmentRemoveClicked(EquipmentSlot equipmentSlot) {

            // Remove item visually
            if (equipmentSlot.Equipment is SlotItemBlueprint) {
                this.ItemWrap.Children.Remove(equipmentSlot);
            } else {
                this.EquipmentWrap.Children.Remove(equipmentSlot);
            }

            // Remove from company
            _ = this.Builder.RemoveEquipment(equipmentSlot.Equipment);

        }

        private void OnUnitDrop(object sender, DragEventArgs e) {

            if (this.CompanySize is not Company.MAX_SIZE && e.Data.GetData("Squad") is SquadBlueprint sbp) {

                // Get squad and add to company
                var unitBuilder = new UnitBuilder().SetBlueprint(sbp).SetDeploymentPhase(this.GetRecommendedDeploymentPhase());
                var squad = this.Builder.AddAndCommitUnit(unitBuilder);

                // Update company unit count
                this.CompanySize++;
                this.NotifyPropertyChanged(nameof(this.CompanyUnitHeaderItem));

                // Add to display
                this.AddUnitToDisplay(squad);

                // Mark handled
                e.Effects = DragDropEffects.Move;
                e.Handled = true;

                // Check if we can no longer add units
                if (!this.CanAddUnits) {
                    this.SetCanAddUnits(false);
                }

            }

        }

        private void OnAbilityDrop(object sender, DragEventArgs e) {

            // Make sure we got an ability blueprint
            if (e.Data.GetData("Ability") is AbilityBlueprint abp) {

                // Get faction ability
                var fabp = this.m_activeModPackage.FactionSettings[this.CompanyFaction].Abilities.FirstOrDefault(x => x.Blueprint == abp.Name);
                if (fabp is null) {
                    return;
                }

                // Get special ability
                var special = this.Builder.AddAndCommitAbility(abp, fabp.AbilityCategory, fabp.MaxUsePerMatch);

                // Update company ability count
                this.CompanyAbilityCount++;
                this.NotifyPropertyChanged(nameof(this.CompanyAbilityHeaderItem));

                // Add to display
                this.AddAbilityToDisplay(special, false);

                // Mark handled
                e.Effects = DragDropEffects.Move;
                e.Handled = true;

                // Get the item slot and disable it
                if (e.Data.GetData("Source") is AvailableItemSlot itemSlot) {
                    itemSlot.CanAdd = false;
                }

                // Check if we can no longer add units
                if (!this.CanAddAbilities) {
                    this.SetCanAddAbilities(false);
                }

            }

        }

        private DeploymentPhase GetRecommendedDeploymentPhase() {

            // Get deployment phase counts
            Dictionary<DeploymentPhase, int> dict = new() {
                [DeploymentPhase.PhaseInitial] = this.Builder.CountUnitsInPhase(DeploymentPhase.PhaseInitial),
                [DeploymentPhase.PhaseA] = this.Builder.CountUnitsInPhase(DeploymentPhase.PhaseA),
                [DeploymentPhase.PhaseB] = this.Builder.CountUnitsInPhase(DeploymentPhase.PhaseB),
                [DeploymentPhase.PhaseC] = this.Builder.CountUnitsInPhase(DeploymentPhase.PhaseC),
            };

            // Remove initial if already full
            if (dict[DeploymentPhase.PhaseInitial] >= Company.MAX_INITIAL) {
                _ = dict.Remove(DeploymentPhase.PhaseInitial);
            }

            // Calc constant threshold
            const double removePhaseThreshold = (Company.MAX_SIZE - Company.MAX_INITIAL) * (1 / 3.0);

            // Remove all where 1/3 is occupied
            Dictionary<DeploymentPhase, int> phases = dict.Where(x => x.Value <= removePhaseThreshold).ToDictionary();

            // Get the one with the smallest value
            var min = phases.MinPair(x => x.Value);

            // Return result
            return min.Key;

        }

        private void AddUnitToDisplay(Squad squad) {

            // Create display
            SquadSlotLarge unitSlot = new(squad);
            unitSlot.OnClick += this.OnSlotClicked;
            unitSlot.OnRemove += this.OnSlotRemoveClicked;

            // Add to wrap container based on simplified category
            switch (squad.GetCategory(true)) {
                case "infantry":
                    _ = this.InfantryWrap.Children.Add(unitSlot);
                    break;
                case "team_weapon":
                    _ = this.SupportWrap.Children.Add(unitSlot);
                    break;
                case "vehicle":
                    _ = this.VehicleWrap.Children.Add(unitSlot);
                    break;
                default:
                    break;
            }

        }

        private void AddAbilityToDisplay(Ability specialAbility, bool isUnitAbility) {

            // Build container
            AbilitySlot slot = new(specialAbility);
            slot.OnRemove += this.OnAbilityRemoveClicked;

            // If is unit ability, then update
            if (isUnitAbility) {
                var factionData = this.m_activeModPackage.FactionSettings[this.CompanyFaction];
                var unitData = factionData.UnitAbilities.FirstOrDefault(x => x.Abilities.Any(y => y.Blueprint == specialAbility.ABP.Name));
                slot.UpdateUnitData(unitData);
            }

            // Determine source
            _ = (isUnitAbility ? this.UnitAbilityWrap : this.AbilityWrap).Children.Add(slot);

        }

        private void AddEquipmentToDisplay(Blueprint blueprint) {

            // Build container
            EquipmentSlot slot = new(blueprint);
            slot.OnRemove += this.OnEquipmentRemoveClicked;
            slot.OnEquipped += this.EquipItem;

            // Determine source
            _ = (blueprint is SlotItemBlueprint ? this.ItemWrap : this.EquipmentWrap).Children.Add(slot);

        }

        private void SetCanAddUnits(bool canAdd) {
            foreach (object obj in this.AvailableUnitsStack.Children) {
                if (obj is AvailableItemSlot slot) {
                    slot.CanAdd = canAdd;
                }
            }
            foreach (object obj in this.AvailableCrews.Children) {
                if (obj is AvailableItemSlot slot) {
                    slot.CanAdd = canAdd;
                }
            }
        }

        private void SetCanAddAbilities(bool canAdd) {
            foreach (object obj in this.AvailableAbilities.Children) {
                if (obj is AvailableItemSlot slot) {
                    slot.CanAdd = canAdd && !this.Builder.HasAbility(slot.Blueprint.Name);
                }
            }
        }

        private async void FillStack<TBlueprint>(IEnumerable<TBlueprint> source, StackPanel target, bool canAdd) where TBlueprint : Blueprint {
            foreach (var element in source) {
                var slot = await this.Dispatcher.InvokeAsync(() => new AvailableItemSlot(element) {
                    CanAdd = canAdd
                });
                slot.OnHoverUpdate += this.OnSlotHover;
                _ = await this.Dispatcher.InvokeAsync(() => target.Children.Add(slot));
            }
        }

        private void LoadFactionDatabase() {

            _ = Task.Run(() => {

                // Get available squads
                this.m_availableSquads = BlueprintManager.GetCollection<SquadBlueprint>()
                    .FilterByMod(this.CompanyGUID)
                    .Filter(x => x.Army == this.CompanyFaction.ToString())
                    .Filter(x => !x.Types.IsVehicleCrew)
                    .ToList();

                // Get available crews
                this.m_availableCrews = BlueprintManager.GetCollection<SquadBlueprint>()
                    .FilterByMod(this.CompanyGUID)
                    .Filter(x => x.Army == this.CompanyFaction.ToString())
                    .Filter(x => x.Types.IsVehicleCrew)
                    .ToList();

                // Get faction data
                var faction = this.m_activeModPackage.FactionSettings[this.CompanyFaction];

                // Get available abilities
                this.m_abilities = BlueprintManager.GetCollection<AbilityBlueprint>()
                    .FilterByMod(this.CompanyGUID)
                    .Filter(x => faction.Abilities.Select(y => y.Blueprint).Contains(x.Name))
                    .ToList();

                // Populate lists
                this.FillStack(this.m_availableSquads, this.AvailableUnitsStack, this.CanAddUnits);
                this.FillStack(this.m_availableCrews, this.AvailableCrews, this.CanAddUnits);
                this.FillStack(this.m_abilities, this.AvailableAbilities, this.CanAddAbilities);

            });

        }

        private void OnOverviewSelectionChanged(object sender, SelectionChangedEventArgs e) {

            // Get tabcontrol
            TabControl tabControl = sender as TabControl;

            // Switch all of by default
            this.AvailableUnitsVisible = Visibility.Collapsed;
            this.AvailableAbilitiesVisible = Visibility.Collapsed;
            this.AvailableCrewsVisible = Visibility.Collapsed;

            // Switch on selected index
            switch (tabControl.SelectedIndex) {
                case 0:
                    this.AvailableObjectType = "Available Units";
                    this.AvailableUnitsVisible = Visibility.Visible;
                    break;
                case 1:
                    this.AvailableObjectType = "Available Abilities";
                    this.AvailableAbilitiesVisible = Visibility.Visible;
                    break;
                case 2:
                    this.AvailableObjectType = "Available Crews";
                    this.AvailableCrewsVisible = Visibility.Visible;
                    break;
                default: break;
            }

            // Trigger refresh data
            this.NotifyPropertyChanged(nameof(this.AvailableObjectType));
            this.NotifyPropertyChanged(nameof(this.AvailableUnitsVisible));
            this.NotifyPropertyChanged(nameof(this.AvailableAbilitiesVisible));
            this.NotifyPropertyChanged(nameof(this.AvailableCrewsVisible));

        }

        private void RemoveCrewAndAddBlueprintToEquipment(SquadSlotLarge squadSlot) {

            // Get vehicle
            var vehicle = squadSlot.SquadInstance;
            var ebp = vehicle.SBP.GetVehicleBlueprint();

            // Add to equipment
            this.Builder.AddEquipment(ebp);

            // Remove squad
            this.OnSlotRemoveClicked(squadSlot);

            // Add ebp to display
            this.AddEquipmentToDisplay(ebp);

        }

        private void EjectCrewAndAddBlueprintToPoolAndToEquipment(SquadSlotLarge squadSlot) { }

        private void EquipItem(EquipmentSlot equipmentSlot, SquadBlueprint sbp) {

            // Remove equipment.
            this.OnEquipmentRemoveClicked(equipmentSlot);

            // Determine how to apply
            if (equipmentSlot.Equipment is SlotItemBlueprint slotitem) {

                // Throw exception for now
                throw new NotImplementedException();

            } else if (equipmentSlot.Equipment is EntityBlueprint ebp) {

                // Get vehicle sbp
                var vehicleSbp = ebp.GetVehicleSquadBlueprint(this.CompanyFaction);
                var builder = new UnitBuilder().SetModGUID(this.CompanyGUID).SetBlueprint(vehicleSbp)
                    .CreateAndGetCrew(x => x.SetBlueprint(sbp)).SetDeploymentPhase(this.GetRecommendedDeploymentPhase());

                // Construct
                var result = this.Builder.AddAndCommitUnit(builder);

                // Add to display
                this.AddUnitToDisplay(result);

                // Update company unit count
                this.CompanySize++;
                this.NotifyPropertyChanged(nameof(this.CompanyUnitHeaderItem));

            }

        }

    }

}
