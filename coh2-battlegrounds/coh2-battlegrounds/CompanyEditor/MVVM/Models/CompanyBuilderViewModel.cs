using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;
using Battlegrounds.Modding;
using BattlegroundsApp.Controls.CompanyBuilderControls;
using BattlegroundsApp.Modals.CompanyBuilder;
using BattlegroundsApp.MVVM;
using BattlegroundsApp.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BattlegroundsApp.CompanyEditor.MVVM.Models;

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

    public ObservableCollection<SquadSlotLarge> CompanyInfantrySquads { get; set; }
    public ObservableCollection<SquadSlotLarge> CompanySupportSquads { get; set; }
    public ObservableCollection<SquadSlotLarge> CompanyVehicleSquads { get; set; }
    public ObservableCollection<AbilitySlot> CompanyAbilities { get; set; }
    public ObservableCollection<AbilitySlot> CompanyUnitAbilities { get; set; }
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
    public LocaleKey CompanyResetButtonContent { get; }
    public LocaleKey CompanySaveButtonContent { get; }
    public LocaleKey CompanyNoUnitDataLabelContent { get; }

    public CompanyBuilderViewModel() {

        // Create save
        this.Save = new() {
            Click = new RelayCommand(this.SaveButton),
            Text = new("")
        };

        // Create reset
        this.Reset = new() {
            Click = new RelayCommand(this.ResetButton),
            Text = new("")
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
        this.CompanyResetButtonContent = new LocaleKey("CompanyBuilder_Reset");
        this.CompanySaveButtonContent = new LocaleKey("CompanyBuilder_Save");
        this.CompanyNoUnitDataLabelContent = new LocaleKey("CompanyBuilder_NoUnitData");

        this.CompanyInfantrySquads = new();
        this.CompanySupportSquads = new();
        this.CompanyVehicleSquads = new();
        this.CompanyAbilities = new();
        this.CompanyUnitAbilities = new();
        this.CompanyEquipment = new();

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

    }

    public void SaveButton() {

    }

    public void ResetButton() {

    }

    public void BackButton() {

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

            // Populate lists : TODO
            //this.FillStack(this.m_availableSquads, this.AvailableUnitsStack, this.CanAddUnits);
            //this.FillStack(this.m_availableCrews, this.AvailableCrews, this.CanAddUnits);
            //this.FillStack(this.m_abilities, this.AvailableAbilities, this.CanAddAbilities);

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
        SquadSlotLarge unitSlot = new(squad);
        unitSlot.OnClick += this.OnSlotClicked;
        unitSlot.OnRemove += this.OnSlotRemoveClicked;

        // Add to collection based on category
        switch (squad.GetCategory(true)) {
            case "infantry":
                this.CompanyInfantrySquads.Add(unitSlot);
                break;
            case "team_weapon":
                this.CompanySupportSquads.Add(unitSlot);
                break;
            case "vehicle":
                this.CompanyVehicleSquads.Add(unitSlot);
                break;
            default:
                break;
        }

    }

    private void AddAbilityToDisplay(Ability ability, bool isUnitAbility) {

        // Create display
        AbilitySlot abilitySlot = new(ability);
        abilitySlot.OnRemove += this.OnAbilityRemoveClicked;

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

    private void OnSlotClicked(SquadSlotLarge squadSlot) {

    }

    private void OnSlotRemoveClicked(SquadSlotLarge squadSlot) {

    }

    private void OnAbilityRemoveClicked(AbilitySlot abilitySlot) {

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
