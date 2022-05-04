using Battlegrounds;
using Battlegrounds.Locale;
using Battlegrounds.Functional;

using BattlegroundsApp.Utilities;
using Microsoft.Win32;

namespace BattlegroundsApp.MVVM.Models;

public class SettingsViewModel : ViewModelBase {

    private LocaleLanguage m_selectedLang;

    public override bool SingleInstanceOnly => true;

    public override bool KeepAlive => false;

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

    public string SteamUserContent { get; set; }

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
        this.SteamUserContent = steamData.HasUser ? 
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

    private void BrowseSteam() {

        // Create OFD
        OpenFileDialog ofd = new() {
            Title = "Select Steam Executable",
            CheckPathExists = true,
            CheckFileExists = true,
            Filter = "Steam Executable (Steam.exe)|Steam.exe",
            Multiselect = false
        };

        // Show it
        if (ofd.ShowDialog() is true && Pathfinder.VerifySteamPath(ofd.FileName)) {

            // Grab select file
            this.SteamPath = ofd.FileName;
            this.Notify(nameof(SteamPath));

            // Refresh steam
            this.RefreshSteam();

        }

    }

    private void RefreshSteam() {

        // Get steam instance
        var steamData = BattlegroundsInstance.Steam;

        // Update
        this.SteamUserContent = BattlegroundsInstance.Localize.GetString("SettingsView_SteamContentNotFound");

        // Get user
        if (steamData.GetSteamUser()) {

            // Update
            this.SteamUserContent = BattlegroundsInstance.Localize.GetString("SettingsView_SteamContentFound", steamData.User.Name);

        }

        // Notify
        this.Notify(nameof(this.SteamUserContent));

    }

    private void BrowseCoH() {

        // Create OFD
        OpenFileDialog ofd = new() {
            Title = "Select Company of Heroes 2 Executable",
            CheckPathExists = true,
            CheckFileExists = true,
            Filter = "Company of Heroes 2 Executable (RelicCoH2.exe)|RelicCoH2.exe",
            Multiselect = false
        };

        // Show it
        if (ofd.ShowDialog() is true && Pathfinder.VerifyCoHPath(ofd.FileName)) {

            // Grab select file
            this.CoHPath = ofd.FileName;
            this.Notify(nameof(CoHPath));

        }

    }

    private void SaveSettings() {

        // Save all that
        BattlegroundsInstance.OtherOptions[BattlegroundsInstance.OPT_ZOOM] = this.ZoomSetting;
        BattlegroundsInstance.OtherOptions[BattlegroundsInstance.OPT_AUTODATA] = this.AutoCollectData;
        BattlegroundsInstance.OtherOptions[BattlegroundsInstance.OPT_AUTOUPDATE] = this.AutoUpdate;
        BattlegroundsInstance.OtherOptions[BattlegroundsInstance.OPT_AUTOSCAR] = this.AutoReportScarErrors;
        BattlegroundsInstance.OtherOptions[BattlegroundsInstance.OPT_AUTOWORKSHOP] = this.AutoCollectWorkshop;

        // Save paths
        BattlegroundsInstance.SaveInstancePath(BattlegroundsPaths.STEAM_FOLDER, this.SteamPath);
        BattlegroundsInstance.SaveInstancePath(BattlegroundsPaths.COH_FOLDER, this.CoHPath);

        // Check restart
        bool requiresRestart = BattlegroundsInstance.Localize.Language != this.m_selectedLang;

        // Update language
        BattlegroundsInstance.ChangeLanguage(this.m_selectedLang);

        // Do save now
        BattlegroundsInstance.SaveInstance();

        // Restart if required
        if (requiresRestart) {

            // Restart
            App.Restart();

        }

    }

}
