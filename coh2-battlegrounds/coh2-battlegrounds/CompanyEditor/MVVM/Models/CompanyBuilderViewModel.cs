using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Battlegrounds;
using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;
using Battlegrounds.Modding;
using Battlegrounds.Modding.Content.Companies;

using BattlegroundsApp.LocalData;
using BattlegroundsApp.Modals;
using BattlegroundsApp.Modals.Dialogs.MVVM.Models;
using BattlegroundsApp.MVVM;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.CompanyEditor.MVVM.Models;

public delegate void CompanyBuilderViewModelEvent(object sender, CompanyBuilderViewModel companyBuilderViewModel, object? args = null);

public record CompanyBuilderButton(ICommand Click, LocaleKey? Text, LocaleKey? Tooltip);

public record CompanyBuilderButton2(ICommand Click, LocaleKey? Tooltip, Func<Visibility> VisibleFetcher) {
    public Visibility Visibility {
        get => this.VisibleFetcher();
    }
}

public class CompanyBuilderViewModel : ViewModelBase {

    public override bool KeepAlive => false;

    public CompanyBuilderButton Save { get; }

    public CompanyBuilderButton Reset { get; }

    public CompanyBuilderButton2 Back { get; }

    public override bool SingleInstanceOnly => false; // This will allow us to override

    public bool HasChanges => this.Builder.IsChanged;

    private readonly ModPackage m_activeModPackage;
    private readonly CompanyBuilder? m_builder;

    private readonly List<SquadBlueprint> m_availableSquads;
    private readonly List<AbilityBlueprint> m_abilities;

    public ObservableCollection<AvailableItemViewModel> AvailableItems { get; set; }

    public List<AvailableItemViewModel> AvailableInfantrySquads { get; }
    public List<AvailableItemViewModel> AvailableSupportSquads { get; }
    public List<AvailableItemViewModel> AvailableVehicleSquads { get; }
    public List<AvailableItemViewModel> AvailableLeaderSquads { get; }
    public List<AvailableItemViewModel> AvailableAbilities { get; }

    public ObservableCollection<SquadSlotViewModel> CompanyInfantrySquads { get; set; }
    public ObservableCollection<SquadSlotViewModel> CompanySupportSquads { get; set; }
    public ObservableCollection<SquadSlotViewModel> CompanyVehicleSquads { get; set; }
    public ObservableCollection<SquadSlotViewModel> CompanyLeaderSquads { get; set; }

    public ObservableCollection<AbilitySlotViewModel> CompanyAbilities { get; set; }
    public ObservableCollection<AbilitySlotViewModel> CompanyUnitAbilities { get; set; }
    public ObservableCollection<EquipmentSlotViewModel> CompanyEquipment { get; set; }

    public CompanyBuilder Builder => this.m_builder ?? throw new Exception("Expected a valid instance of a company builder but found none (Invalid call tree!)");

    public CompanyStatistics Statistics { get; }
    public string CompanyName { get; }
    public Faction CompanyFaction { get; }
    public string CompanyGUID { get; }
    public string CompanyType { get; }
    public string CompanyTypeIcon { get; }

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

    public CapacityValue UnitCapacity { get; }

    public CapacityValue AbilityCapacity { get; }

    public CapacityValue StorageCapacity { get; }

    public CapacityValue InfantryCapacity { get; }

    public CapacityValue SupportCapacity { get; }

    public CapacityValue VehicleCapacity { get; }

    public CapacityValue LeaderCapacity { get; }

    public IViewModel? ReturnTo { get; set; }

    public int SaveStatus { get; set; } = -1;

    public bool IsCompanyReplacementEnabled {
        get => this.m_builder?.AutoReinforce ?? false;
        set => this.m_builder?.SetAutoReinforce(value);
    }

    private CompanyBuilderViewModel(ModGuid guid) {

        // Create save
        this.Save = new(new RelayCommand(this.SaveButton), null, null);

        // Create reset
        this.Reset = new(new RelayCommand(this.ResetButton), null, null);

        // Create back
        this.Back = new(new RelayCommand(this.BackButton), null, () => this.ReturnTo is null ? Visibility.Collapsed : Visibility.Visible);

        // Define basic values
        this.CompanyFaction = Faction.Soviet;
        this.CompanyGUID = ModGuid.BaseGame;
        this.CompanyName = string.Empty;
        this.CompanyType = string.Empty;
        this.CompanyTypeIcon = string.Empty;
        this.Statistics = new();

        // Define locales
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
        this.CompanyLeaderSquads = new();
        this.CompanyAbilities = new();
        this.CompanyUnitAbilities = new();
        this.CompanyEquipment = new();
        this.AvailableItems = new();

        // Define list
        this.AvailableInfantrySquads = new();
        this.AvailableSupportSquads = new();
        this.AvailableVehicleSquads = new();
        this.AvailableLeaderSquads = new();
        this.AvailableAbilities = new();
        this.m_availableSquads = new();
        this.m_abilities = new();

        // Set default tabs
        this.SelectedMainTab = 0;
        this.SelectedUnitTabItem = 0;
        this.SelectedAbilityTabItem = 0;

        // Set item visibility
        this.AvailableItemsVisibility = Visibility.Visible;

        // Set capacities
        this.UnitCapacity = new CapacityValue(0, Company.MAX_SIZE, () => this.Builder?.Size ?? 0);
        this.AbilityCapacity = new CapacityValue(0, Company.MAX_ABILITY, () => this.Builder?.AbilityCount ?? 0);
        this.StorageCapacity = new CapacityValue(0, 0);
        this.InfantryCapacity = new CapacityValue(0, 0, () => this.Builder?.InfantryCount ?? 0);
        this.SupportCapacity = new CapacityValue(0, 0, () => this.Builder?.SupportCount ?? 0);
        this.VehicleCapacity = new CapacityValue(0, 0, () => this.Builder?.VehicleCount ?? 0);
        this.LeaderCapacity = new CapacityValue(0, 0, () => this.Builder?.LeaderCount ?? 0);

        // Set fields
        this.m_activeModPackage = ModManager.GetPackageFromGuid(guid) ?? throw new Exception("Attempt to create company builder vm without a valid mod package");

    }

    public CompanyBuilderViewModel(Company company) : this(company.TuningGUID) {

        // Set company information
        this.m_builder = CompanyBuilder.EditCompany(company);
        this.Statistics = company.Statistics;
        this.CompanyName = company.Name;
        this.CompanyFaction = company.Army;
        this.CompanyGUID = company.TuningGUID;
        this.CompanyType = BattlegroundsInstance.Localize.GetString(company.Type.Id);
        this.CompanyTypeIcon = company.Type.UIData.Icon;

        // Load database and display
        this.LoadFactionDatabase();
        this.ShowCompany();

        // Update capacity values
        this.SetCapacityValues();

    }

    public CompanyBuilderViewModel(string companyName, Faction faction, FactionCompanyType type, ModGuid modGuid) : this(modGuid) {

        // Set properties
        this.m_builder = CompanyBuilder.NewCompany(companyName, type, CompanyAvailabilityType.MultiplayerOnly, faction, modGuid);
        this.CompanyName = companyName;
        this.CompanyFaction = faction;
        this.CompanyGUID = modGuid;
        this.CompanyType = BattlegroundsInstance.Localize.GetString(type.Id);
        this.CompanyTypeIcon = type.UIData.Icon;

        // Load database and display
        this.LoadFactionDatabase();
        this.ShowCompany();

        this.AvailableInfantrySquads.ForEach(x => this.AvailableItems.Add(x));

        // Update capacity values
        this.SetCapacityValues();

    }

    private void SetCapacityValues() {

        // Update unit capacity values
        this.InfantryCapacity.Capacity = this.Builder.CompanyType.MaxInfantry;
        this.SupportCapacity.Capacity = this.Builder.CompanyType.MaxTeamWeapons;
        this.VehicleCapacity.Capacity = this.Builder.CompanyType.MaxVehicles;
        this.LeaderCapacity.Capacity = this.Builder.CompanyType.MaxLeaders;

        // Set unit cap
        int unitCap = this.InfantryCapacity.Capacity + this.SupportCapacity.Capacity + this.VehicleCapacity.Capacity + this.LeaderCapacity.Capacity;
        this.UnitCapacity.Capacity = Math.Min(Company.MAX_SIZE, unitCap);

        // Update ability capacity value
        this.AbilityCapacity.Capacity = Math.Min(Company.MAX_ABILITY, this.Builder.CompanyType.MaxAbilities);

    }

    public void SaveButton() {

        try {

            // Commit changes
            var company = this.Builder.Commit().Result;

            // Save
            PlayerCompanies.SaveCompany(company);

            // Set status
            this.SaveStatus = 1;

        } catch (Exception e) {

            // Log error
            Trace.WriteLine(e, nameof(CompanyBrowserViewModel));

            // Set not saved
            this.SaveStatus = 0;

        }

    }

    public void ResetButton() {

        // TODO: Show modal warning

    }

    public void BackButton() {

        // Bail if returnto is not valid (but this somehow was clicked)
        if (this.ReturnTo is null) {
            return;
        }

        // Check if any changes were applied
        if (this.Builder.IsChanged) {
            if (App.ViewManager.GetRightsideModalControl() is not ModalControl mc) {
                App.ViewManager.UpdateDisplay(AppDisplayTarget.Right, new(this.ReturnTo));
                return;
            }
            YesNoDialogViewModel.ShowModal(mc, (_, res) => {
                if (res is not ModalDialogResult.Cancel) {

                    // Then goback
                    App.ViewManager.UpdateDisplay(AppDisplayTarget.Right, new(this.ReturnTo));

                }
            }, "Unsaved Changes", "You have unsaved changes that will be lost if you leave the company builder. Are you sure you want to leave?");
        } else {

            // Then goback
            App.ViewManager.UpdateDisplay(AppDisplayTarget.Right, new(this.ReturnTo));

        }

    }

    private void FillAvailableItemSlot<TBlueprint>(IEnumerable<TBlueprint> source, List<AvailableItemViewModel> target) where TBlueprint : Blueprint {

        foreach (var element in source) {

            var slot = new AvailableItemViewModel(element, this.OnItemAddClicked, this.OnItemMove, this.CanAddBlueprint);

            target.Add(slot);
 
        }

    }

    private void LoadFactionDatabase() {

        Task.Run(() => {

            // Grab type
            var type = this.Builder.CompanyType;

            // Grab crew squads (invisible squads we're going to keep hidden)
            var hidden = type.FactionData?.GetHiddenSquads() ?? Array.Empty<string>();

            // Get available squads
            BlueprintManager.GetCollection<SquadBlueprint>()
                .FilterByMod(this.CompanyGUID)
                .Filter(x => x.Army == this.CompanyFaction)
                .Filter(x => !x.Types.IsVehicleCrew)
                .Filter(x => !type.Exclude.Contains(x.Name))
                .Filter(x => !hidden.Contains(x.Name))
                .ForEach(this.m_availableSquads.Add);

            // Get faction data
            var faction = this.m_activeModPackage.FactionSettings[this.CompanyFaction];

            // Get available abilities
            BlueprintManager.GetCollection<AbilityBlueprint>()
                .FilterByMod(this.CompanyGUID)
                .Filter(x => faction.Abilities.Select(y => y.Blueprint).Contains(x.Name))
                .ForEach(this.m_abilities.Add);

            // Populate lists
            Application.Current.Dispatcher.Invoke(() => {

                this.FillAvailableItemSlot(this.m_availableSquads.FindAll(s => s.Types.IsInfantry == true && !s.Types.IsCommandUnit),
                                           this.AvailableInfantrySquads);

                this.FillAvailableItemSlot(this.m_availableSquads.FindAll(s => s.IsTeamWeapon == true && !s.Types.IsCommandUnit),
                                           this.AvailableSupportSquads);

                this.FillAvailableItemSlot(this.m_availableSquads.FindAll(s => (s.Types.IsVehicle == true || s.Types.IsArmour == true || s.Types.IsHeavyArmour == true) && !s.Types.IsCommandUnit),
                                           this.AvailableVehicleSquads);

                this.FillAvailableItemSlot(this.m_availableSquads.FindAll(s => s.Types.IsCommandUnit),
                                           this.AvailableLeaderSquads);

                this.FillAvailableItemSlot(this.m_abilities, this.AvailableAbilities);

                this.UpdateAvailableItems();

            });

        });

    }

    private void ShowCompany() {

        // Refresh unit overview
        this.RefreshUnitOverview();

        // Clear collections
        this.CompanyAbilities.Clear();
        this.CompanyEquipment.Clear();

        // Add all abilities
        this.Builder.EachAbility(this.AddAbilityToDisplay);

        // Add all items
        this.Builder.EachItem(this.AddEquipmentToDisplay);

    }

    private void RefreshUnitOverview() {

        // Clear equipment
        this.CompanyInfantrySquads.Clear();
        this.CompanySupportSquads.Clear();
        this.CompanyVehicleSquads.Clear();
        this.CompanyUnitAbilities.Clear();

        // Add all units
        this.Builder.EachUnit(this.AddUnitToDisplay, x => (int)x.Phase);

    }

    private void AddUnitToDisplay(UnitBuilder builder) {

        // Create display
        SquadSlotViewModel unitSlot = new(builder, this.Builder.CompanyType, this.OnUnitClicked, this.OnUnitRemoveClicked);
        unitSlot.PropertyChanged += (sender, args) => {
            if (args.PropertyName is nameof(SquadSlotViewModel.SquadPhase)) { // Refresh order 
                var collection = GetUnitCollection(builder);
                var backup = collection.ToArray();
                collection.Clear();
                backup.OrderBy(x => (int)x.BuilderInstance.Phase).ForEach(collection.Add);
            }
        };

        // Add to collection based on category
        this.GetUnitCollection(builder).Add(unitSlot);

        // Notify change
        this.UnitCapacity.Update(this);
        this.GetUnitCapacity(builder).Update(this);

    }

    private ObservableCollection<SquadSlotViewModel> GetUnitCollection(UnitBuilder builder) => builder.Blueprint.Category switch {
        SquadCategory.Infantry => this.CompanyInfantrySquads,
        SquadCategory.Support => this.CompanySupportSquads,
        SquadCategory.Vehicle => this.CompanyVehicleSquads,
        SquadCategory.Leader => this.CompanyLeaderSquads,
        _ => throw new InvalidEnumArgumentException()
    };

    private CapacityValue GetUnitCapacity(UnitBuilder builder) => builder.Blueprint.Category switch {
        SquadCategory.Infantry => this.InfantryCapacity,
        SquadCategory.Support => this.SupportCapacity,
        SquadCategory.Vehicle => this.VehicleCapacity,
        SquadCategory.Leader => this.LeaderCapacity,
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
            this.AbilityCapacity.Update(this);
        }

    }

    private void AddEquipmentToDisplay(CompanyItem item) {

        // Create display
        var equipmentSlot = new EquipmentSlotViewModel(item, this.OnEquipmentClicked, this.CanEquipItem);

        // Add to equipment list
        this.CompanyEquipment.Add(equipmentSlot);

    }

    private void OnUnitClicked(object sender, SquadSlotViewModel squadViewModel) {

        // Create options view model
        var model = new SquadOptionsViewModel(squadViewModel, this.Builder);

        // Display modal
        App.ViewManager.GetRightsideModalControl()?.ShowModal(model);

    }

    private void OnUnitRemoveClicked(object sender, SquadSlotViewModel squadSlot) {

        // Grab squad
        var unitBuilder = squadSlot.BuilderInstance;

        // Remove from company
        this.Builder.RemoveUnit(unitBuilder);

        // Remove view model
        this.GetUnitCollection(unitBuilder).Remove(squadSlot);
        
        // Refresh capacities
        this.UnitCapacity.Update(this);
        this.GetUnitCapacity(unitBuilder).Update(this);

        // Loop over items and refresh (Expensive call!)
        this.UpdateAvailableItems();

    }

    private void OnAbilityClicked(object sender, AbilitySlotViewModel abilityViewModel) {

    }

    private void OnAbilityRemoveClicked(object sender, AbilitySlotViewModel abilitySlot) {

        // Grab ability
        var ability = abilitySlot.AbilityInstance;

        // Remove from company
        this.Builder.RemoveAbility(ability);

        // Remove view model
        this.CompanyAbilities.Remove(abilitySlot);
        
        // Loop over items and refresh (Expensive call!)
        this.UpdateAvailableItems();

    }

    private void OnItemAddClicked(object sender, AvailableItemViewModel itemBlueprint, object? arg) {

        if (itemBlueprint.Blueprint is SquadBlueprint sbp) {

            // Trigger new unit
            this.NewUnit(sbp);

        } else if (itemBlueprint.Blueprint is AbilityBlueprint abp) {

            // Trigger new ability
            this.NewAbility(abp);
        }
        
    }

    private bool CanEquipItem(EquipmentSlotViewModel equipmentSlot) {
        if (equipmentSlot.Item.Item is SquadBlueprint sbp) {
            return sbp.Category switch {
                SquadCategory.Infantry => !this.InfantryCapacity.IsAtCapacity,
                SquadCategory.Support => !this.SupportCapacity.IsAtCapacity,
                SquadCategory.Vehicle => !this.VehicleCapacity.IsAtCapacity,
                _ => false // TODO: Add support for command units
            };
        } else if (equipmentSlot.Item.Item is EntityBlueprint) {
            return !this.SupportCapacity.IsAtCapacity; // Based on team weapons *always* being ebps
        }
        return false;
    }

    private void OnEquipmentClicked(EquipmentSlotViewModel equipmentSlot) {

        try {

            // Determine how
            if (equipmentSlot.Item.Item is SquadBlueprint sbp) { // Is vehicle...

                // Get driver squad
                var driverSbp = sbp.GetCrewBlueprint(this.CompanyFaction);
                driverSbp ??= this.Builder.CompanyType.FactionData!.GetDriver(sbp.Types);

                // Add action
                this.Builder.CrewCompanyItem(equipmentSlot.Item.ItemId, driverSbp);

            } else if (equipmentSlot.Item.Item is EntityBlueprint ebp) {

                // Get crew
                var crewSbp = this.Builder.CompanyType.GetWeaponsCrew();

                // Add action
                this.Builder.CrewCompanyItem(equipmentSlot.Item.ItemId, crewSbp);

            } else return;

        } catch (InvalidOperationException) {
            // TODO: Show error message with message from caught exception
            return;
        }

        // Remove item
        this.CompanyEquipment.Remove(equipmentSlot);

        // Refresh unit status
        this.RefreshUnitOverview();

    }

    private void OnItemMove(object sender, AvailableItemViewModel itemSlot, object? arg) {

        if (arg is MouseEventArgs mEvent) {

            if (mEvent.LeftButton is MouseButtonState.Pressed) {

                // Bail if can add is not possible
                if (!itemSlot.CanAdd)
                    return;

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

    private void OnItemDrop(object sender, CompanyBuilderViewModel squadSlot, object? arg) {

        if (arg is DragEventArgs dEvent) {

            if (dEvent.Data.GetData("Squad") is SquadBlueprint sbp) {

                // Add unit
                this.NewUnit(sbp);

                // Mark handled
                dEvent.Effects = DragDropEffects.Move;
                dEvent.Handled = true;

            } else if (dEvent.Data.GetData("Ability") is AbilityBlueprint abp) {

                // New ability
                this.NewAbility(abp);

                // Mark handled
                dEvent.Effects = DragDropEffects.Move;
                dEvent.Handled = true;

            }

        }

    }

    private bool CanAddBlueprint(Blueprint bp) {
        if (bp is SquadBlueprint sbp) {
            return !this.UnitCapacity.IsAtCapacity && sbp.Category switch {
                SquadCategory.Infantry => !this.InfantryCapacity.IsAtCapacity,
                SquadCategory.Support => !this.SupportCapacity.IsAtCapacity,
                SquadCategory.Vehicle => !this.VehicleCapacity.IsAtCapacity,
                SquadCategory.Leader => !this.LeaderCapacity.IsAtCapacity,
                _ => false
            };
        } else if (bp is AbilityBlueprint abp) {
            return !this.AbilityCapacity.IsAtCapacity;
        }
        return false;
    }

    private void UpdateAvailableItems() {

        this.AvailableItems.Clear();

        if (this.SelectedMainTab == 0) {
            this.AvailableItemsVisibility = Visibility.Visible;

            switch (this.SelectedUnitTabItem) {
                case 0:
                    this.AvailableInfantrySquads.ForEach(this.AvailableItems.Add);
                    break;
                case 1:
                    this.AvailableSupportSquads.ForEach(this.AvailableItems.Add);
                    break;
                case 2:
                    this.AvailableVehicleSquads.ForEach(this.AvailableItems.Add);
                    break;
                case 3:
                    this.AvailableLeaderSquads.ForEach(this.AvailableItems.Add);
                    break;
                default:
                    break;
            }
        } else if (this.SelectedMainTab == 1) {
            this.AvailableItemsVisibility = Visibility.Visible;
            this.RefreshAbilityDisplay();
            switch (this.SelectedAbilityTabItem) {
                case 0:
                    this.AvailableItemsVisibility = Visibility.Visible;
                    this.AvailableAbilities.ForEach(x => this.AvailableItems.Add(x));
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

        // Loop over items and refresh
        foreach (var item in this.AvailableItems) {
            item.Refresh();
        }

    }

    private void OnTabChange(object sender, CompanyBuilderViewModel squadSlot, object? arg) {

        if (arg is SelectionChangedEventArgs sEvent) {

            if (sEvent.Source is TabControl) {

                this.UpdateAvailableItems();

            }

        }

    }

    private void RefreshAbilityDisplay() {

        this.CompanyAbilities.Clear();
        this.CompanyUnitAbilities.Clear();

        // Add all abilities
        this.Builder.EachAbility(this.AddAbilityToDisplay);

    }

    private void NewUnit(SquadBlueprint sbp) {

        // Get earliest phase
        var earliestPhase = this.Builder.CompanyType.GetEarliestPhase(sbp);

        // Determine the initial phase of the unit
        var basicPhase = this.Builder.IsPhaseAvailable(DeploymentPhase.PhaseInitial) && earliestPhase is DeploymentPhase.PhaseStandard ?
            DeploymentPhase.PhaseInitial : earliestPhase;

        // Get the default phase
        var defaultPhase = this.Builder.GetFirstAvailablePhase(basicPhase);
        if (defaultPhase is DeploymentPhase.PhaseNone) {
            // TODO: Show warning for user
            return;
        }

        // Create squad (in initial phase or in phase A)
        var unitBuilder = UnitBuilder.NewUnit(sbp).SetDeploymentPhase(defaultPhase).SetDeploymentRole(this.Builder.CompanyType.GetUnitRole(sbp));

        // If heavy arty add tow
        if (sbp.Types.IsHeavyArtillery && !sbp.Types.IsAntiTank)
            unitBuilder.SetDeploymentMethod(DeploymentMethod.DeployAndStay).SetTransportBlueprint(this.Builder.CompanyType.GetTowTransports()[0]);

        // Add to company
        this.Builder.AddUnit(unitBuilder);

        // Add to display
        this.AddUnitToDisplay(unitBuilder);

        // Update capacity values
        this.SetCapacityValues();

        // Loop over items and refresh (Expensive call!)
        this.UpdateAvailableItems();

    }

    private void NewAbility(AbilityBlueprint abp) {

        // Get faction ability
        var fabp = this.m_activeModPackage.FactionSettings[this.CompanyFaction].Abilities.FirstOrDefault(x => x.Blueprint == abp.Name);
        if (fabp is null) {
            return;
        }

        // Create ability
        Ability sabp = new(abp, fabp.LockoutBlueprint, Array.Empty<string>(), fabp.AbilityCategory, fabp.MaxUsePerMatch, 0);

        // Add to company
        this.Builder.AddAbility(sabp);

        // Add to display
        this.AddAbilityToDisplay(sabp, false);

        // Loop over items and refresh (Expensive call!)
        this.UpdateAvailableItems();

    }

    public override void UnloadViewModel(OnModelClosed closeCallback, bool destroy) {
        
        // Destroy if requested
        if (destroy) {
            App.ViewManager.DestroyView(this);
        }
        
        // Check if any changes
        if (this.HasChanges) {

            // If modal control not found, bail
            if (App.ViewManager.GetRightsideModalControl() is not ModalControl mc) {
                closeCallback(false);
                return;
            }

            // Ask user to confirm change
            YesNoDialogViewModel.ShowModal(mc, (_, res) => {
                closeCallback(res is ModalDialogResult.Cancel);
            }, "Unsaved Changes", "You have unsaved changes that will be lost if you leave the company builder. Are you sure you want to leave?");
        
        } else {
            
            // Invoke callback now
            closeCallback(false);

        }

    }

}
