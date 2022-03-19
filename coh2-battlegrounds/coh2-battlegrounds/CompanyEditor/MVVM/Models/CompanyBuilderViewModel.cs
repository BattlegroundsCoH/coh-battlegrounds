using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;
using Battlegrounds.Modding;
using BattlegroundsApp.Controls.CompanyBuilderControls;
using BattlegroundsApp.LocalData;
using BattlegroundsApp.Modals.CompanyBuilder;
using BattlegroundsApp.MVVM;
using BattlegroundsApp.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BattlegroundsApp.CompanyEditor.MVVM.Models;

public delegate void CompanyBuilderViewModelEvent(object sender, CompanyBuilderViewModel companyBuilderViewModel, object args = null);

public class CompanyBuilderButton {
    public ICommand Click { get; init; }
    public LocaleKey Text { get; init; }
    public LocaleKey Tooltip { get; init; }
}

public class CompanyBuilderViewModel : IViewModel {

    public CompanyBuilderButton Save { get; }

    public CompanyBuilderButton Reset { get; }

    public CompanyBuilderButton Back { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
        if (PropertyChanged is not null) {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public bool SingleInstanceOnly => false; // This will allow us to override

    private int m_companySize;
    private int m_companyAbilityCount;
    private ulong m_initialChecksum;
    private readonly ModPackage m_activeModPackage;

    private List<SquadBlueprint> m_availableSquads;
    private List<SquadBlueprint> m_availableCrews;
    private List<AbilityBlueprint> m_abilities;

    public ObservableCollection<AvailableItemViewModel> AvailableItems { get; set; }

    public List<AvailableItemViewModel> m_availableInfantrySquads;
    public List<AvailableItemViewModel> m_availableSupportSquads;
    public List<AvailableItemViewModel> m_availableVehicleSquads;

    public List<AvailableItemViewModel> m_availableAbilities;

    public ObservableCollection<SquadSlotViewModel> CompanyInfantrySquads { get; set; }
    public ObservableCollection<SquadSlotViewModel> CompanySupportSquads { get; set; }
    public ObservableCollection<SquadSlotViewModel> CompanyVehicleSquads { get; set; }

    public ObservableCollection<AbilitySlotViewModel> CompanyAbilities { get; set; }
    public ObservableCollection<AbilitySlotViewModel> CompanyUnitAbilities { get; set; }
    public ObservableCollection<EquipmentSlot> CompanyEquipment { get; set; }

    public CompanyBuilder Builder { get; }
    public bool CanAddUnits => this.Builder.CanAddUnit;
    public bool CanAddAbilities => this.Builder.CanAddAbility;

    public CompanyStatistics Statistics { get; }
    public string CompanyName { get; }
    public int CompanySize { get => this.m_companySize; set { this.m_companySize = value; this.NotifyPropertyChanged(); } }
    public int CompanyAbilityCount { get => this.m_companyAbilityCount; set { this.m_companyAbilityCount = value; this.NotifyPropertyChanged(); } }
    public string CompanyUnitHeaderItem => $"Units ({this.CompanySize}/{Company.MAX_SIZE})";
    public string CompanyAbilityHeaderItem => $"Abilities ({this.CompanyAbilityCount}/{Company.MAX_ABILITY})";
    public Faction CompanyFaction { get; }
    public string CompanyGUID { get; }
    public string CompanyType { get; }

    public LocaleKey InfantryHeaderItem { get; }
    public LocaleKey SupportHeaderItem { get; }
    public LocaleKey VehiclesHeaderItem { get; }
    public LocaleKey CommanderAbilitiesHeaderItem { get; }
    public LocaleKey UnitAbilitiesHeaderItem { get; }
    public LocaleKey CompanyMatchHistoryLabelContent { get; }
    public LocaleKey CompanyVictoriesLabelContent { get; }
    public LocaleKey CompanyDefeatsLabelContent { get; }
    public LocaleKey CompanyTotalLabelContent { get; }
    public LocaleKey CompanyExperienceLabelContent { get; }
    public LocaleKey CompanyInfantryLossesLabelContent { get; }
    public LocaleKey CompanyVehicleLossesLabelContent { get; }
    public LocaleKey CompanyTotalLossesLabelContent { get; }
    public LocaleKey CompanyRatingLabelContent { get; }
    public LocaleKey CompanyNoUnitDataLabelContent { get; }

    public int SelectedMainTab { get; set; }
    public int SelectedUnitTabItem { get; set; }
    public int SelectedAbilityTabItem { get; set; }

    public Visibility AvailableItemsVisibility { get; set; }

    public CompanyBuilderViewModelEvent Drop => this.OnItemDrop;
    public CompanyBuilderViewModelEvent Change => this.OnTabChange;

    public CompanyBuilderViewModel() {

        // Create save
        this.Save = new() {
            Click = new RelayCommand(this.SaveButton),
        };

        // Create reset
        this.Reset = new() {
            Click = new RelayCommand(this.ResetButton),
        };

        // Create back
        this.Back = new() {
            Click = new RelayCommand(this.BackButton),
            Text = new("")
        };

        // Define locales
        this.InfantryHeaderItem = new LocaleKey("CompanyBuilder_Infantry");
        this.SupportHeaderItem = new LocaleKey("CompanyBuilder_Support");
        this.VehiclesHeaderItem = new LocaleKey("CompanyBuilder_Vehicles");
        this.CommanderAbilitiesHeaderItem = new LocaleKey("CompanyBuilder_CommanderAbilities");
        this.UnitAbilitiesHeaderItem = new LocaleKey("CompanyBuilder_UnitAbilities");
        this.CompanyMatchHistoryLabelContent = new LocaleKey("CompanyBuilder_CompanyMatchHistory");
        this.CompanyVictoriesLabelContent = new LocaleKey("CompanyBuilder_CompanyVictories");
        this.CompanyDefeatsLabelContent = new LocaleKey("CompanyBuilder_CompanyDefeats");
        this.CompanyTotalLabelContent = new LocaleKey("CompanyBuilder_CompanyTotal");
        this.CompanyExperienceLabelContent = new LocaleKey("CompanyBuilder_CompanyExperience");
        this.CompanyInfantryLossesLabelContent = new LocaleKey("CompanyBuilder_CompanyInfantryLosses");
        this.CompanyVehicleLossesLabelContent = new LocaleKey("CompanyBuilder_CompanyVehicleLosses");
        this.CompanyTotalLossesLabelContent = new LocaleKey("CompanyBuilder_CompanyTotalLosses");
        this.CompanyRatingLabelContent = new LocaleKey("CompanyBuilder_CompanyRating");
        this.CompanyNoUnitDataLabelContent = new LocaleKey("CompanyBuilder_NoUnitData");

        // Define observables
        this.CompanyInfantrySquads = new();
        this.CompanySupportSquads = new();
        this.CompanyVehicleSquads = new();
        this.CompanyAbilities = new();
        this.CompanyUnitAbilities = new();
        this.CompanyEquipment = new();
        this.AvailableItems = new();

        // Define list
        this.m_availableInfantrySquads = new();
        this.m_availableSupportSquads = new();
        this.m_availableVehicleSquads = new();
        this.m_availableAbilities = new();

        // Set default tabs
        this.SelectedMainTab = 0;
        this.SelectedUnitTabItem = 0;
        this.SelectedAbilityTabItem = 0;

        this.AvailableItemsVisibility = Visibility.Visible;

    }

    public CompanyBuilderViewModel(Company company) : this() {
        
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

    public CompanyBuilderViewModel(string companyName, Faction faction, CompanyType type, ModGuid modGuid) : this() {

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

        this.m_availableInfantrySquads.ForEach(x => this.AvailableItems.Add(x));

    }

    public void SaveButton() {

        // Commit changes
        var company = this.Builder.Commit().Result;

        // Save
        PlayerCompanies.SaveCompany(company);

    }

    public void ResetButton() {

        // TODO: Show modal warning

    }

    public void BackButton() {

        if (this.Builder.IsChanged) {

            // Await response

        }

    }

    private void FillAvailableItemSlot<TBlueprint>(IEnumerable<TBlueprint> source, List<AvailableItemViewModel> target, bool canAdd) where TBlueprint : Blueprint {

        foreach (var element in source) {
            
            var slot = new AvailableItemViewModel(element, this.OnItemAddClicked, this.OnItemMove) {
                CanAdd = canAdd
            };

            target.Add(slot);
 
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
            Application.Current.Dispatcher.Invoke(() => {

                this.FillAvailableItemSlot(this.m_availableSquads.FindAll(s => s.Types.IsInfantry == true),
                                           this.m_availableInfantrySquads, this.CanAddUnits);

                this.FillAvailableItemSlot(this.m_availableSquads.FindAll(s => s.IsTeamWeapon == true),
                                           this.m_availableSupportSquads, this.CanAddUnits);

                this.FillAvailableItemSlot(this.m_availableSquads.FindAll(s => s.Types.IsVehicle == true || s.Types.IsArmour == true || s.Types.IsHeavyArmour == true),
                                           this.m_availableVehicleSquads, this.CanAddUnits);

                this.FillAvailableItemSlot(this.m_abilities, this.m_availableAbilities, this.CanAddAbilities);

                this.UpdateAvailableItems();

            });

        });

    }

    private void ShowCompany() {

        // Clear collections
        this.CompanyInfantrySquads.Clear();
        this.CompanySupportSquads.Clear();
        this.CompanyVehicleSquads.Clear();
        this.CompanyAbilities.Clear();
        this.CompanyUnitAbilities.Clear();
        this.CompanyEquipment.Clear();

        // Add all units
        this.Builder.EachUnit(this.AddUnitToDisplay, x => (int)x.DeploymentPhase);

        // Add all abilities : TODO
        this.Builder.EachAbility(this.AddAbilityToDisplay);

        // Add all items : TODO
        this.Builder.EachItem(this.AddEquipmentToDisplay);

    }

    private void AddUnitToDisplay(Squad squad) {

        // Create display
        SquadSlotViewModel unitSlot = new(squad, this.OnUnitClicked, this.OnUnitRemoveClicked);

        // Add to collection based on category
        this.GetUnitCollection(squad).Add(unitSlot);

    }

    private ObservableCollection<SquadSlotViewModel> GetUnitCollection(Squad squad) => squad.GetCategory(true) switch {
        "infantry" => this.CompanyInfantrySquads,
        "team_weapon" => this.CompanySupportSquads,
        "vehicle" => this.CompanyVehicleSquads,
        _ => throw new InvalidEnumArgumentException()
    };

    private void AddAbilityToDisplay(Ability ability, bool isUnitAbility) {

        // Create display
        AbilitySlotViewModel abilitySlot = new(ability, this.OnAbilityClicked, this.OnAbilityRemoveClicked);

        // If is unit ability, then update
        if (isUnitAbility) {
            var factionData = this.m_activeModPackage.FactionSettings[this.CompanyFaction];
            var unitData = factionData.UnitAbilities.FirstOrDefault(x => x.Abilities.Any(y => y.Blueprint == ability.ABP.Name));
            abilitySlot.UpdateUnitData(unitData);
            this.CompanyUnitAbilities.Add(abilitySlot);
        } else {
            this.CompanyAbilities.Add(abilitySlot);
        }

    }

    private void AddEquipmentToDisplay(Blueprint blueprint) {

        // Create display
        EquipmentSlot equipmentSlot = new(blueprint);
        equipmentSlot.OnRemove += this.OnEquipmentRemoveClicked;
        equipmentSlot.OnEquipped += this.EquipItem;

        this.CompanyEquipment.Add(equipmentSlot);

    }

    private void OnUnitClicked(object sender, SquadSlotViewModel squadViewModel) {

        // Grab squad
        var squad = squadViewModel.SquadInstance;

        // Create options view model
        var model = new SquadOptionsViewModel(squad);

        // Display modal
        App.ViewManager.GetRightsideModalControl()?.ShowModal(model);

    }

    private void OnUnitRemoveClicked(object sender, SquadSlotViewModel squadSlot) {

        // Grab squad
        var squad = squadSlot.SquadInstance;

        // Remove from company
        this.Builder.RemoveUnit(squad.SquadID);

        // Remove view model
        this.GetUnitCollection(squad).Remove(squadSlot);

    }

    private void OnAbilityClicked(object sender, AbilitySlotViewModel abilityViewModel) {

    }

    private void OnAbilityRemoveClicked(object sender, AbilitySlotViewModel abilityViewModel) {

    }

    private void OnItemAddClicked(object sender, AvailableItemViewModel itemBlueprint, object arg) {

        if (itemBlueprint.Blueprint is SquadBlueprint sbp) {

            // Create squad
            var unitBuilder = new UnitBuilder().SetBlueprint(sbp);

            // Add to company
            var squad = this.Builder.AddAndCommitUnit(unitBuilder);

            // Add to display
            this.AddUnitToDisplay(squad);

        } else if (itemBlueprint.Blueprint is AbilityBlueprint abp) {

            // Get faction ability
            var fabp = this.m_activeModPackage.FactionSettings[this.CompanyFaction].Abilities.FirstOrDefault(x => x.Blueprint == abp.Name);
            if (fabp is null) {
                return;
            }

            // Get special ability
            var special = this.Builder.AddAndCommitAbility(abp, fabp);

            // Add to display
            this.AddAbilityToDisplay(special, false);

        }
        
    }

    private void OnItemMove(object sender, AvailableItemViewModel itemSlot, object arg) {

        if (arg is MouseEventArgs mEvent) {

            if (mEvent.LeftButton is MouseButtonState.Pressed) {

                DataObject obj = new();
                obj.SetData("Source", this);

                if (itemSlot.Blueprint is SquadBlueprint sbp) {
                    obj.SetData("Squad", sbp);
                } else if (itemSlot.Blueprint is AbilityBlueprint abp) {
                    obj.SetData("Ability", abp);
                }

                _ = DragDrop.DoDragDrop(sender as DependencyObject, obj, DragDropEffects.Move);

            }

        }

    }

    private void OnItemDrop(object sender, CompanyBuilderViewModel squadSlot, object arg) {

        if (arg is DragEventArgs dEvent) {

            if (this.CompanySize is not Company.MAX_SIZE && dEvent.Data.GetData("Squad") is SquadBlueprint sbp) {

                // Create squad
                var unitBuilder = new UnitBuilder().SetBlueprint(sbp);

                // Add to company
                var squad = this.Builder.AddAndCommitUnit(unitBuilder);

                // Add to display
                this.AddUnitToDisplay(squad);

                // Mark handled
                dEvent.Effects = DragDropEffects.Move;
                dEvent.Handled = true;

            } else if (dEvent.Data.GetData("Ability") is AbilityBlueprint abp) {

                // Get faction ability
                var fabp = this.m_activeModPackage.FactionSettings[this.CompanyFaction].Abilities.FirstOrDefault(x => x.Blueprint == abp.Name);
                if (fabp is null) {
                    return;
                }

                // Get special ability
                var special = this.Builder.AddAndCommitAbility(abp, fabp);

                // Add to display
                this.AddAbilityToDisplay(special, false);

                // Mark handled
                dEvent.Effects = DragDropEffects.Move;
                dEvent.Handled = true;

                // Get the item slot and disable it
                if (dEvent.Data.GetData("Source") is AvailableItemViewModel itemSlot) {
                    itemSlot.CanAdd = false;
                }

            }

        }

    }

    private void UpdateAvailableItems() {

        this.AvailableItems.Clear();

        if (this.SelectedMainTab == 0) {
            this.AvailableItemsVisibility = Visibility.Visible;
            switch (this.SelectedUnitTabItem) {
                case 0:
                    this.m_availableInfantrySquads.ForEach(x => this.AvailableItems.Add(x));
                    break;
                case 1:
                    this.m_availableSupportSquads.ForEach(x => this.AvailableItems.Add(x));
                    break;
                case 2:
                    this.m_availableVehicleSquads.ForEach(x => this.AvailableItems.Add(x));
                    break;
                default:
                    break;
            }
        } else if (this.SelectedMainTab == 1) {
            this.AvailableItemsVisibility = Visibility.Visible;
            switch (this.SelectedAbilityTabItem) {
                case 0:
                    this.AvailableItemsVisibility = Visibility.Visible;
                    this.m_availableAbilities.ForEach(x => this.AvailableItems.Add(x));
                    break;
                case 1:
                    this.AvailableItemsVisibility = Visibility.Hidden;
                    break;
                default:
                    break;
            }
        } else if (this.SelectedMainTab == 2) {
            this.AvailableItemsVisibility = Visibility.Visible;
            // TODO
        } else if (this.SelectedMainTab == 3) {
            this.AvailableItemsVisibility = Visibility.Hidden;
        }

    }

    private void OnTabChange(object sender, CompanyBuilderViewModel squadSlot, object arg) {

        if (arg is SelectionChangedEventArgs sEvent) {

            if (sEvent.Source is TabControl) {

                this.UpdateAvailableItems();

            }

        }

    }

    private void OnEquipmentRemoveClicked(EquipmentSlot equipmentSlot) { 
    
    }

    private void EquipItem(EquipmentSlot equipmentSlot, SquadBlueprint sbp) { 
    
    }

    private void RemoveCrewAndAddBlueprintToEquipment(SquadSlotLarge squadSlot) {

    }

    private void EjectCrewAndAddBlueprintToPoolAndToEquipment(SquadSlotLarge squadSlot) {
    
    }

    public bool UnloadViewModel() => true;

}
