using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;
using Battlegrounds.Modding;
using BattlegroundsApp.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BattlegroundsApp.MVVM.Models;

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

    public bool SingleInstanceOnly => true;

    private int m_companySize;
    private int m_companyAbilityCount;
    private ulong m_initialChecksum;
    private readonly ModPackage m_activeModPackage;

    public CompanyBuilder Builder { get; }

    public CompanyStatistics Statistics { get; }

    public string CompanyName { get; }

    public int CompanySize { get => this.m_companySize; set { this.m_companySize = value; this.NotifyPropertyChanged(); } }

    public int CompanyAbilityCount { get => this.m_companyAbilityCount; set { this.m_companyAbilityCount = value; this.NotifyPropertyChanged(); } }

    public string CompanyUnitHeaderItem => $"Units ({this.CompanySize}/{Company.MAX_SIZE})";

    public string CompanyAbilityHeaderItem => $"Abilities ({this.CompanyAbilityCount}/{Company.MAX_ABILITY})";

    public Faction CompanyFaction { get; }

    public string CompanyGUID { get; }

    public string CompanyType { get; }

    public LocaleKey CompanyUnitsHeaderItem { get; }

    public LocaleKey CompanyAbilitiesHeaderItem { get; }

    public LocaleKey CompanyInventoryHeaderItem { get; }

    public LocaleKey CompanyStatsHeaderItem { get; }

    public LocaleKey InfantryHeaderItem { get; }

    public LocaleKey SupportHeaderItem { get; }

    public LocaleKey VehiclesHeaderItem { get; }

    public LocaleKey CommanderAbilitiesHeaderItem { get; }

    public LocaleKey UnitAbilitiesHeaderItem { get; }

    public LocaleKey CapturedItemsLabelContent { get; }

    public LocaleKey CapturedEquipmentLabelContent { get; }

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
        this.CompanyUnitsHeaderItem = new LocaleKey("CompanyBuilder_Units");
        this.CompanyAbilitiesHeaderItem = new LocaleKey("CompanyBuilder_Abilities");
        this.CompanyInventoryHeaderItem = new LocaleKey("CompanyBuilder_Inventory");
        this.CompanyStatsHeaderItem = new LocaleKey("CompanyBuilder_Stats");
        this.InfantryHeaderItem = new LocaleKey("CompanyBuilder_Infantry");
        this.SupportHeaderItem = new LocaleKey("CompanyBuilder_Support");
        this.VehiclesHeaderItem = new LocaleKey("CompanyBuilder_Vehicles");
        this.CommanderAbilitiesHeaderItem = new LocaleKey("CompanyBuilder_CommanderAbilities");
        this.UnitAbilitiesHeaderItem = new LocaleKey("CompanyBuilder_UnitAbilities");
        this.CapturedItemsLabelContent = new LocaleKey("CompanyBuilder_CapturedItems");
        this.CapturedEquipmentLabelContent = new LocaleKey("CompanyBuilder_CapturedEquipment");
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
    
    }

    private void ShowCompany() { 
    
    }

}
