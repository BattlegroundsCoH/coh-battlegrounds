using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds;
using Battlegrounds.Locale;
using Battlegrounds.Functional;

using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.MVVM.Models;

public class SettingsViewModel : IViewModel {

    private LocaleLanguage m_selectedLang;

    public bool SingleInstanceOnly => true;

    public bool KeepAlive => false;

    public RelayCommand<int> LanguageButton { get; }

    public RelayCommand BrowseSteamButton { get; }

    public RelayCommand RefreshSteamButton { get; }

    public RelayCommand BrowseCoHButton { get; }

    public RelayCommand SaveButton { get; }

    public bool AutoCollectData { get; set; }

    public bool AutoCollectWorkshop { get; set; }

    public bool AutoUpdate { get; set; }

    public bool AutoReportScarErrors { get; set; }

    public double ZoomSetting { get; set; }

    public string SteamPath { get; set; }

    public string CoHPath { get; set; }

    public string SteaUserContent { get; set; }

    public SettingsViewModel() {
        
        // Create buttons
        this.LanguageButton = new(this.SetLanguage);
        this.BrowseCoHButton = new(this.BrowseCoH);
        this.BrowseSteamButton = new(this.BrowseSteam);
        this.RefreshSteamButton = new(this.RefreshSteam);
        this.SaveButton = new(this.SaveSettings);

        // Set paths
        this.SteamPath = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.STEAM_FOLDER);
        this.CoHPath = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.COH_FOLDER);

        // Set steam data
        var steamData = BattlegroundsInstance.Steam;
        this.SteaUserContent = steamData.HasUser ? 
            BattlegroundsInstance.Localize.GetString("SettingsView_SteamContentFound", steamData.User.Name) :
            BattlegroundsInstance.Localize.GetString("SettingsView_SteamContentNotFound");

        // Set settings
        this.AutoUpdate = BattlegroundsInstance.OtherOptions.GetCastValueOrDefault(BattlegroundsInstance.OPT_AUTOUPDATE, false);
        this.AutoCollectData = BattlegroundsInstance.OtherOptions.GetCastValueOrDefault(BattlegroundsInstance.OPT_AUTODATA, false);
        this.AutoReportScarErrors = BattlegroundsInstance.OtherOptions.GetCastValueOrDefault(BattlegroundsInstance.OPT_AUTOSCAR, false);
        this.AutoCollectWorkshop = BattlegroundsInstance.OtherOptions.GetCastValueOrDefault(BattlegroundsInstance.OPT_AUTOWORKSHOP, true);
        this.ZoomSetting = BattlegroundsInstance.OtherOptions.GetCastValueOrDefault(BattlegroundsInstance.OPT_ZOOM, 0.0);

        // Set language
        this.m_selectedLang = BattlegroundsInstance.Localize.Language;

    }

    private void SetLanguage(int langId)
        => this.m_selectedLang = (LocaleLanguage)langId;

    private void BrowseSteam() { }

    private void RefreshSteam() { }

    private void BrowseCoH() {

    }

    private void SaveSettings() {

        // Save all that
        BattlegroundsInstance.OtherOptions[BattlegroundsInstance.OPT_ZOOM] = this.ZoomSetting;
        BattlegroundsInstance.OtherOptions[BattlegroundsInstance.OPT_AUTODATA] = this.AutoCollectData;
        BattlegroundsInstance.OtherOptions[BattlegroundsInstance.OPT_AUTOUPDATE] = this.AutoUpdate;
        BattlegroundsInstance.OtherOptions[BattlegroundsInstance.OPT_AUTOSCAR] = this.AutoReportScarErrors;
        BattlegroundsInstance.OtherOptions[BattlegroundsInstance.OPT_AUTOWORKSHOP] = this.AutoCollectWorkshop;

        // Check restart
        bool requiresRestart = BattlegroundsInstance.Localize.Language != this.m_selectedLang;

        // Update language
        BattlegroundsInstance.ChangeLanguage(this.m_selectedLang);

        // Do save now
        BattlegroundsInstance.SaveInstance();

        // Restart if required
        if (requiresRestart) {
            // TODO: ...
        }

    }

    public void Swapback() {
    }

    public void UnloadViewModel(OnModelClosed onClosed, bool requestDestroyed) {
        onClosed(false);
    }

}
