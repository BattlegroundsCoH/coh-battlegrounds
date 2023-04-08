using System;
using System.Diagnostics;

using Battlegrounds.Functional;
using Battlegrounds.Locale;
using Microsoft.Win32;

namespace Battlegrounds.UI.Application.Pages;

public sealed class Settings : ViewModelBase {

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

    public Settings() {

        // Create buttons
        this.LanguageButton = new(this.SetLanguage);
        this.BrowseCoHButton = new(this.BrowseCoH);
        this.BrowseSteamButton = new(this.BrowseSteam);
        this.RefreshSteamButton = new(this.RefreshSteam);
        this.SaveButton = new(this.SaveSettings);

        // Set paths
        this.SteamPath = BattlegroundsContext.GetRelativePath(BattlegroundsPaths.STEAM_FOLDER);
        this.CoHPath = BattlegroundsContext.GetRelativePath(BattlegroundsPaths.COH_FOLDER);

        // Set steam data
        var steamData = BattlegroundsContext.Steam;
        this.SteamUserContent = steamData.HasUser ?
            BattlegroundsContext.Localize.GetString("SettingsView_SteamContentFound", steamData.User.Name) :
            BattlegroundsContext.Localize.GetString("SettingsView_SteamContentNotFound");

        // Set settings
        this.AutoUpdate = BattlegroundsContext.OtherOptions.GetCastValueOrDefault(BattlegroundsContext.OPT_AUTOUPDATE, false);
        this.AutoCollectData = BattlegroundsContext.OtherOptions.GetCastValueOrDefault(BattlegroundsContext.OPT_AUTODATA, false);
        this.AutoReportScarErrors = BattlegroundsContext.OtherOptions.GetCastValueOrDefault(BattlegroundsContext.OPT_AUTOSCAR, false);
        this.AutoCollectWorkshop = BattlegroundsContext.OtherOptions.GetCastValueOrDefault(BattlegroundsContext.OPT_AUTOWORKSHOP, true);
        this.ZoomSetting = BattlegroundsContext.OtherOptions.GetCastValueOrDefault(BattlegroundsContext.OPT_ZOOM, 0.0);

        // Set language
        this.m_selectedLang = BattlegroundsContext.Localize.Language;

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
        var steamData = BattlegroundsContext.Steam;

        // Update
        this.SteamUserContent = BattlegroundsContext.Localize.GetString("SettingsView_SteamContentNotFound");

        // Get user
        if (steamData.GetSteamUser()) {

            // Update
            this.SteamUserContent = BattlegroundsContext.Localize.GetString("SettingsView_SteamContentFound", steamData.User.Name);

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
        BattlegroundsContext.OtherOptions[BattlegroundsContext.OPT_ZOOM] = this.ZoomSetting;
        BattlegroundsContext.OtherOptions[BattlegroundsContext.OPT_AUTODATA] = this.AutoCollectData;
        BattlegroundsContext.OtherOptions[BattlegroundsContext.OPT_AUTOUPDATE] = this.AutoUpdate;
        BattlegroundsContext.OtherOptions[BattlegroundsContext.OPT_AUTOSCAR] = this.AutoReportScarErrors;
        BattlegroundsContext.OtherOptions[BattlegroundsContext.OPT_AUTOWORKSHOP] = this.AutoCollectWorkshop;

        // Save paths
        BattlegroundsContext.SaveInstancePath(BattlegroundsPaths.STEAM_FOLDER, this.SteamPath);
        BattlegroundsContext.SaveInstancePath(BattlegroundsPaths.COH_FOLDER, this.CoHPath);

        // Check restart
        bool requiresRestart = BattlegroundsContext.Localize.Language != this.m_selectedLang;

        // Update language
        BattlegroundsContext.ChangeLanguage(this.m_selectedLang);

        // Do save now
        BattlegroundsContext.SaveInstance();

        // Restart if required
        if (requiresRestart) {

            // Close log
            BattlegroundsContext.Log?.SaveAndClose(0);

            // Create new process
            Process.Start("coh2-battlegrounds.exe");

            // Close this
            Environment.Exit(0);

        }

    }

}
