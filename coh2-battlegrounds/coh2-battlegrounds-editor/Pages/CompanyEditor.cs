﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

using Battlegrounds.DataLocal;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Misc.Values;
using Battlegrounds.Modding.Content.Companies;
using Battlegrounds.Modding;
using Battlegrounds.UI.Modals.Prompts;
using Battlegrounds.UI.Modals;
using Battlegrounds.UI;
using Battlegrounds.Editor.Components;
using Battlegrounds.Editor.Modals;
using Battlegrounds.Functional;
using Battlegrounds.Game.Blueprints;
using Battlegrounds.Locale;
using Battlegrounds.Logging;
using Battlegrounds.Game;
using Battlegrounds.Game.Blueprints.Extensions;

namespace Battlegrounds.Editor.Pages;

using static Battlegrounds.UI.AppContext;

/// <summary>
/// 
/// </summary>
/// <param name="sender"></param>
/// <param name="companyBuilderViewModel"></param>
/// <param name="args"></param>
public delegate void CompanyBuilderViewModelEvent(object sender, CompanyEditor companyBuilderViewModel, object? args = null);

/// <summary>
/// 
/// </summary>
/// <param name="Click"></param>
/// <param name="Text"></param>
/// <param name="Tooltip"></param>
public record CompanyBuilderButton(ICommand Click, LocaleKey? Text, LocaleKey? Tooltip);

/// <summary>
/// 
/// </summary>
/// <param name="Click"></param>
/// <param name="Tooltip"></param>
/// <param name="VisibleFetcher"></param>
public record CompanyBuilderButton2(ICommand Click, LocaleKey? Tooltip, Func<Visibility> VisibleFetcher) {
    public Visibility Visibility {
        get => this.VisibleFetcher();
    }
}

/// <summary>
/// Class responsible for handling the editing companies.
/// </summary>
public sealed class CompanyEditor : ViewModelBase, IReturnable {

    private static readonly Logger logger = Logger.CreateLogger();

    public override bool KeepAlive => false;

    public CompanyBuilderButton Save { get; }

    public CompanyBuilderButton Reset { get; }

    public CompanyBuilderButton2 Back { get; }

    public override bool SingleInstanceOnly => false; // This will allow us to override

    public bool HasChanges => this.Builder.IsChanged;

    private readonly IModPackage m_activeModPackage;
    private readonly CompanyBuilder? m_builder;

    private readonly List<SquadBlueprint> m_availableSquads;
    private readonly List<AbilityBlueprint> m_abilities;

    public ObservableCollection<AvailableItem> AvailableItems { get; set; }

    public List<AvailableItem> AvailableInfantrySquads { get; }
    public List<AvailableItem> AvailableSupportSquads { get; }
    public List<AvailableItem> AvailableVehicleSquads { get; }
    public List<AvailableItem> AvailableLeaderSquads { get; }
    public List<AvailableItem> AvailableAbilities { get; }

    public ObservableCollection<SquadSlot> CompanyInfantrySquads { get; set; }
    public ObservableCollection<SquadSlot> CompanySupportSquads { get; set; }
    public ObservableCollection<SquadSlot> CompanyVehicleSquads { get; set; }
    public ObservableCollection<SquadSlot> CompanyLeaderSquads { get; set; }

    public ObservableCollection<AbilitySlot> CompanyAbilities { get; set; }
    public ObservableCollection<AbilitySlot> CompanyUnitAbilities { get; set; }
    public ObservableCollection<EquipmentSlot> CompanyEquipment { get; set; }

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

    public Visibility CommanyCommandVisibility => this.SelectedMainTab == 0 ? Visibility.Visible : Visibility.Collapsed;

    public CompanyBuilderViewModelEvent Drop => this.OnItemDrop;
    public CompanyBuilderViewModelEvent Change => this.OnTabChange;

    public CapacityValue UnitCapacity { get; }

    public CapacityValue AbilityCapacity { get; }

    public CapacityValue StorageCapacity { get; }

    public CapacityValue InfantryCapacity { get; }

    public CapacityValue SupportCapacity { get; }

    public CapacityValue VehicleCapacity { get; }

    public CapacityValue LeaderCapacity { get; }

    public CapacityValue CommandUnitCapacity { get; }

    public CapacityValue SupportUnitCapacity { get; }

    public CapacityValue ReserveUnitCapacity { get; }

    public string CommandUnitLocKey => this.Builder.CompanyType.GetMaxInRole(DeploymentRole.DirectCommand) == BattlegroundsDefine.COMPANY_ROLE_MAX ?
        "CompanyBuilder_RoleCommand_01" : "CompanyBuilder_RoleCommand_02";

    public string SupportUnitLocKey => this.Builder.CompanyType.GetMaxInRole(DeploymentRole.SupportRole) == BattlegroundsDefine.COMPANY_ROLE_MAX ?
        "CompanyBuilder_RoleSupport_01" : "CompanyBuilder_RoleSupport_02";

    public string ReserveUnitLocKey => this.Builder.CompanyType.GetMaxInRole(DeploymentRole.ReserveRole) == BattlegroundsDefine.COMPANY_ROLE_MAX ?
        "CompanyBuilder_RoleReserve_01" : "CompanyBuilder_RoleReserve_02";

    public IViewModel? ReturnTo { get; set; }

    public int SaveStatus { get; set; } = -1;

    public bool IsCompanyReplacementEnabled {
        get => this.m_builder?.AutoReinforce ?? false;
        set => this.m_builder?.SetAutoReinforce(value);
    }

    private CompanyEditor(ModGuid guid, GameCase game) {

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
        this.CommandUnitCapacity = new(0, 0, () => this.Builder.CountUnitsInRole(DeploymentRole.DirectCommand));
        this.SupportUnitCapacity = new(0, 0, () => this.Builder.CountUnitsInRole(DeploymentRole.SupportRole));
        this.ReserveUnitCapacity = new(0, 0, () => this.Builder.CountUnitsInRole(DeploymentRole.ReserveRole));

        // Set fields
        this.m_activeModPackage = BattlegroundsContext.ModManager.GetPackageFromGuid(guid, game) ?? throw new Exception("Attempt to create company builder vm without a valid mod package");

    }

    public CompanyEditor(Company company) : this(company.TuningGUID, company.Game) {

        // Set company information
        this.m_builder = CompanyBuilder.EditCompany(company);
        this.Statistics = company.Statistics;
        this.CompanyName = company.Name;
        this.CompanyFaction = company.Army;
        this.CompanyGUID = company.TuningGUID;
        this.CompanyType = BattlegroundsContext.Localize.GetString(company.Type.Id);
        this.CompanyTypeIcon = company.Type.UIData.Icon;

        // Load database and display
        this.LoadFactionDatabase();
        this.ShowCompany();

        // Update capacity values
        this.SetCapacityValues();

    }

    public CompanyEditor(string companyName, Faction faction, FactionCompanyType type, ModGuid modGuid) : this(modGuid, faction.RequiredDLC.Game) {

        // Set properties
        this.m_builder = CompanyBuilder.NewCompany(companyName, type, CompanyAvailabilityType.MultiplayerOnly, faction, modGuid);
        this.CompanyName = companyName;
        this.CompanyFaction = faction;
        this.CompanyGUID = modGuid;
        this.CompanyType = BattlegroundsContext.Localize.GetString(type.Id);
        this.CompanyTypeIcon = type.UIData.Icon;

        // Load database and display
        this.LoadFactionDatabase();
        this.ShowCompany();

        this.AvailableInfantrySquads.ForEach(this.AvailableItems.Add);

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
            Companies.SaveCompany(company);

            // Set status
            this.SaveStatus = 1;

        } catch (Exception e) {

            // Log error
            logger.Error(e);

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

        // Get view manager
        var viewManager = GetViewManager();

        // Check if any changes were applied
        if (this.Builder.IsChanged) {
            if (viewManager.GetRightsideModalControl() is not ModalControl mc) {
                viewManager.UpdateDisplay(AppDisplayTarget.Right, new(this.ReturnTo));
                return;
            }
            YesNoPrompt.Show(mc, (_, res) => {
                if (res is not ModalDialogResult.Cancel) {

                    // Then goback
                    viewManager.UpdateDisplay(AppDisplayTarget.Right, new(this.ReturnTo));

                }
            }, "Unsaved Changes", "You have unsaved changes that will be lost if you leave the company builder. Are you sure you want to leave?");
        } else {

            // Then goback
            viewManager.UpdateDisplay(AppDisplayTarget.Right, new(this.ReturnTo));

        }

    }

    private void FillAvailableItemSlot<TBlueprint>(IEnumerable<TBlueprint> source, List<AvailableItem> target) where TBlueprint : Blueprint {

        foreach (var element in source) {

            var slot = new AvailableItem(element, this.OnItemAddClicked, this.OnItemMove, this.CanAddBlueprint);

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
            var src = this.Builder.BlueprintDatabase.GetCollection<SquadBlueprint>();
            var collection = 
            this.Builder.BlueprintDatabase.GetCollection<SquadBlueprint>()
                //.FilterByMod(this.CompanyGUID) // This line is no longer needed since mods now have their own dedicated DB
                .Filter(x => x.Army == this.CompanyFaction)
                .Filter(x => !x.Types.IsVehicleCrew)
                .Filter(x => !type.Exclude.Contains(x.Name))
                .Filter(x => !hidden.Contains(x.Name))
                .Filter(x => !this.m_activeModPackage.GetCaptureSquads().Contains(x.Name))
                .Filter(x => !UIExtension.IsEmpty(x.UI))
                .ForEach(this.m_availableSquads.Add);

            // Get faction data
            var faction = this.m_activeModPackage.FactionSettings[this.CompanyFaction];

            // Get available abilities
            this.Builder.BlueprintDatabase.GetCollection<AbilityBlueprint>()
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
        SquadSlot unitSlot = new(builder, this.Builder.CompanyType, this.OnUnitClicked, this.OnUnitRemoveClicked);
        unitSlot.PropertyChanged += (sender, args) => {
            if (args.PropertyName is nameof(SquadSlot.SquadPhase)) { // Refresh order 
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
        this.GetUnitRole(builder).Update(this);

    }

    private ObservableCollection<SquadSlot> GetUnitCollection(UnitBuilder builder) => builder.Blueprint.Category switch {
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

    private CapacityValue GetUnitRole(UnitBuilder builder) => builder.Role switch {
        DeploymentRole.DirectCommand => this.CommandUnitCapacity,
        DeploymentRole.SupportRole => this.SupportUnitCapacity,
        DeploymentRole.ReserveRole => this.ReserveUnitCapacity,
        _ => throw new InvalidEnumArgumentException()
    };

    private void AddAbilityToDisplay(Ability ability, bool isUnitAbility) {

        // Create display
        AbilitySlot abilitySlot = new(ability, this.OnAbilityClicked, this.OnAbilityRemoveClicked);

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
        var equipmentSlot = new EquipmentSlot(item, this.OnEquipmentClicked, this.CanEquipItem);

        // Add to equipment list
        this.CompanyEquipment.Add(equipmentSlot);

    }

    private void OnUnitClicked(object sender, SquadSlot squadViewModel) {

        // Create options view model
        var model = new SquadSettings(squadViewModel, this.Builder);

        // Display modal
        GetRightsideModalControl().ShowModal(model);

    }

    private void OnUnitRemoveClicked(object sender, SquadSlot squadSlot) {

        // Grab squad
        var unitBuilder = squadSlot.BuilderInstance;

        // Remove from company
        this.Builder.RemoveUnit(unitBuilder);

        // Remove view model
        this.GetUnitCollection(unitBuilder).Remove(squadSlot);

        // Refresh capacities
        this.UnitCapacity.Update(this);
        this.GetUnitCapacity(unitBuilder).Update(this);
        this.GetUnitRole(unitBuilder).Update(this);

        // Loop over items and refresh (Expensive call!)
        this.UpdateAvailableItems();

    }

    private void OnAbilityClicked(object sender, AbilitySlot abilityViewModel) {

    }

    private void OnAbilityRemoveClicked(object sender, AbilitySlot abilitySlot) {

        // Grab ability
        var ability = abilitySlot.AbilityInstance;

        // Remove from company
        this.Builder.RemoveAbility(ability);

        // Remove view model
        this.CompanyAbilities.Remove(abilitySlot);

        // Loop over items and refresh (Expensive call!)
        this.UpdateAvailableItems();

    }

    private void OnItemAddClicked(object sender, AvailableItem itemBlueprint, object? arg) {

        if (itemBlueprint.Blueprint is SquadBlueprint sbp) {

            // Trigger new unit
            this.NewUnit(sbp);

        } else if (itemBlueprint.Blueprint is AbilityBlueprint abp) {

            // Trigger new ability
            this.NewAbility(abp);
        }

    }

    private bool CanEquipItem(EquipmentSlot equipmentSlot) {
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

    private void OnEquipmentClicked(EquipmentSlot equipmentSlot) {

        try {

            // Determine how
            if (equipmentSlot.Item.Item is SquadBlueprint sbp) { // Is vehicle...

                // Get driver squad
                var driverSbp = m_activeModPackage.GetDataSource().GetBlueprints(Game.GameCase.CompanyOfHeroes2).GetCrewBlueprint(sbp, this.CompanyFaction);
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

    private void OnItemMove(object sender, AvailableItem itemSlot, object? arg) {

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

    private void OnItemDrop(object sender, CompanyEditor _, object? arg) {

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

            // Collect initial data
            var role = this.Builder.CompanyType.GetUnitRole(sbp);
            int roleCount = this.Builder.CountUnitsInRole(role);
            int roleMax = this.Builder.CompanyType.GetMaxInRole(role);

            // Define check flags
            bool companyNotFull = !this.UnitCapacity.IsAtCapacity;
            bool compayRoleCap = roleCount < roleMax;

            // Return if company not full, role cap allows, and category allows
            return companyNotFull && compayRoleCap && sbp.Category switch {
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

        // Clear available items
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
                    this.AvailableAbilities.ForEach(this.AvailableItems.Add);
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

    private void OnTabChange(object sender, CompanyEditor _, object? arg) {

        if (arg is SelectionChangedEventArgs sEvent) {

            if (sEvent.Source is TabControl) {

                this.UpdateAvailableItems();

            }

            // Update company command
            this.Notify(nameof(CommanyCommandVisibility));

        }

    }

    private void RefreshAbilityDisplay() {

        // Clear abilities
        this.CompanyAbilities.Clear();
        this.CompanyUnitAbilities.Clear();

        // Add all abilities
        this.Builder.EachAbility(this.AddAbilityToDisplay);

    }

    private void NewUnit(SquadBlueprint sbp) {

        // Get the unit role
        var role = this.Builder.CompanyType.GetUnitRole(sbp);

        // Determine the initial phase of the unit
        var defaultPhase = this.Builder.IsPhaseAvailable(DeploymentPhase.PhaseInitial) && role is DeploymentRole.DirectCommand ?
            DeploymentPhase.PhaseInitial : DeploymentPhase.PhaseStandard;

        // Create squad (in initial phase or in phase A)
        var unitBuilder = UnitBuilder.NewUnit(sbp).SetDeploymentPhase(defaultPhase).SetDeploymentRole(role);

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

        // Get view
        var vm = GetViewManager(); // TODO: 'UnloadViewModel' should probably return the view manager with it...

        // Destroy if requested
        if (destroy) {
            vm.DestroyView(this);
        }

        // Check if any changes
        if (this.HasChanges) {

            // If modal control not found, bail
            if (GetRightsideModalControl() is not ModalControl mc) {
                closeCallback(false);
                return;
            }

            // Ask user to confirm change
            YesNoPrompt.Show(mc, (_, res) => {
                closeCallback(res is ModalDialogResult.Cancel);
            }, "Unsaved Changes", "You have unsaved changes that will be lost if you leave the company builder. Are you sure you want to leave?");

        } else {

            // Invoke callback now
            closeCallback(false);

        }

    }

}
