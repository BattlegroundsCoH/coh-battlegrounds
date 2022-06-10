using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using Battlegrounds;
using Battlegrounds.Compiler;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Match.Play;
using Battlegrounds.Modding;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Networking.Server;

using BattlegroundsApp.Lobby.MVVM.Views;
using BattlegroundsApp.LocalData;

namespace BattlegroundsApp.Lobby.MVVM.Models;

public class LobbyParticipantModel : LobbyModel {

    private bool m_hasDownloadedGamemode = false;

    private ModPackage? m_package;

    public override LobbyMutButton StartMatchButton { get; }

    public override LobbySetting<ScenOp> MapDropdown { get; }

    public override LobbySetting<IGamemode> GamemodeDropdown { get; }

    public override LobbySetting<IGamemodeOption> GamemodeOptionDropdown { get; }

    public override LobbySetting<OnOffOption> WeatherDropdown { get; }

    public override LobbySetting<OnOffOption> SupplySystemDropdown { get; }

    public override LobbySetting<ModPackageOption> ModPackageDropdown { get; }

    public override ModPackage ModPackage => this.m_package ?? throw new Exception("No Mod Package Defined");

    private readonly Func<string, string> LOCSTR_DOWNLOAD = x => BattlegroundsInstance.Localize.GetString("LobbyView_DownloadGamemode", x); 

    public LobbyParticipantModel(ILobbyHandle handle, ILobbyTeam allies, ILobbyTeam axis) : base(handle, allies, axis) {

        // Define start match buttnn
        this.StartMatchButton = new(new(this.CancelMatch), Visibility.Hidden) { Title = LOCSTR_WAIT(), IsEnabled = false, NotificationVisible = Visibility.Hidden };

        // Init dropdowns 
        this.MapDropdown = LobbySetting<ScenOp>.NewValue("LobbyView_SettingScenario", string.Empty);
        this.GamemodeDropdown = LobbySetting<IGamemode>.NewValue("LobbyView_SettingGamemode", string.Empty);
        this.GamemodeOptionDropdown = LobbySetting<IGamemodeOption>.NewValue("LobbyView_SettingOption", handle.Settings[LobbyConstants.SETTING_GAMEMODEOPTION]);
        this.WeatherDropdown = LobbySetting<OnOffOption>.NewValue("LobbyView_SettingWeather", handle.Settings[LobbyConstants.SETTING_WEATHER] is "1" ? "On" : "Off");
        this.SupplySystemDropdown = LobbySetting<OnOffOption>.NewValue("LobbyView_SettingSupply", handle.Settings[LobbyConstants.SETTING_LOGISTICS] is "1" ? "On" : "Off");
        this.ModPackageDropdown = LobbySetting<ModPackageOption>.NewValue("LobbyView_SettingTuning", string.Empty);

        // Add dropdowns
        this.MapSetting.Add(this.MapDropdown);
        this.MapSetting.Add(this.GamemodeDropdown);
        this.GamemodeSettings.Add(this.GamemodeOptionDropdown);
        this.AuxSettings.Add(this.WeatherDropdown);
        this.AuxSettings.Add(this.SupplySystemDropdown);

        // Subscribe to common lobby events
        handle.OnLobbySettingUpdate += this.OnLobbyChange;

        // Subscribe to match specific events
        if (handle is ILobbyMatchNotifier matchNotifier) {
            matchNotifier.OnLobbyNotifyGamemode += this.OnGamemodeReleased;
            matchNotifier.OnLobbyNotifyResults += this.OnResultsReleased;
            matchNotifier.OnLobbyBeginMatch += this.OnMatchBegin;
            matchNotifier.OnLobbyLaunchGame += this.OnLaunchGame;
            matchNotifier.OnLobbyMatchError += this.OnMatchInfo;
            matchNotifier.OnLobbyMatchInfo += this.OnMatchInfo;
            matchNotifier.OnPoll += this.OnPoll;
        }

        // Trigger initial view
        this.OnModPackageChange(handle.Settings[LobbyConstants.SETTING_MODPACK]);
        this.OnScenarioChange(handle.Settings[LobbyConstants.SETTING_MAP]);
        this.OnGamemodeChange(handle.Settings[LobbyConstants.SETTING_GAMEMODE]);

        // Inform others
        if (this.GetSelf() is ILobbySlot self && self.Occupant is not null) {
            this.m_handle.MemberState(self.Occupant.MemberID, self.TeamID, self.SlotID, LobbyMemberState.Waiting);
        }

    }

    private void OnMatchBegin() {

        // Set starting flag
        this.m_isStarting = true;

        // Inform user
        if (this.m_chatModel is not null) {
            this.m_chatModel.SystemMessage($"The match is starting", Colors.Gray);
        }

        // Invoke on GUI
        Application.Current.Dispatcher.Invoke(() => {

            // Update cancel button
            this.StartMatchButton.IsEnabled = true;

        });

    }

    private void OnLaunchGame() {

        // Invoke on GUI
        Application.Current.Dispatcher.Invoke(() => {

            // Reset text
            this.StartMatchButton.Title = LOCSTR_PLAYING();

            // Re-enable
            this.StartMatchButton.IsEnabled = false;

        });

        // Create overwatch strategy
        var overwatch = new MemberOverwatchStrategy();

        // Run task
        Task.Run(async () => {

            // Get time
            DateTime time = DateTime.Now;

            // Wait for gamemode
            while (!this.m_hasDownloadedGamemode) {
                if ((DateTime.Now - time).TotalSeconds > 15.0) {
                    if (this.m_chatModel is not null) {
                        this.m_chatModel.SystemMessage($"Failed to download gamemode file!", Colors.Gray);
                    }
                    // TODO: Report to host
                    return;
                }
                await Task.Delay(100);
            }

            // Begin
            overwatch.Launch();

            // Wait for exit
            overwatch.WaitForExit();

            // set received to false (may need to do some checksum stuff here so clients can reconnect if needed)
            this.m_hasDownloadedGamemode = false;

            // TODO: Check for bugsplats etc. and report accordingly

        });

    }

    private void OnResultsReleased(ServerAPI obj) {

        // Instruct download
        Task.Run(() => {
            obj.DownloadCompany(this.m_handle.Self.ID, (status, data) => {

                // Check status
                if (status is DownloadResult.DOWNLOAD_SUCCESS) {

                    // Make sure data aint null
                    if (data is null) {
                        this.m_chatModel?.SystemMessage($"Failed to download company results!", Colors.DarkRed);
                        return;
                    }

                    // Load it
                    if (CompanySerializer.GetCompanyFromJson(Encoding.UTF8.GetString(data)) is Company company) {

                        // Save it
                        PlayerCompanies.SaveCompany(company);

                    } else {

                        // Log
                        this.m_chatModel?.SystemMessage($"Failed to download valid company results!", Colors.DarkRed);

                    }

                } else {
                    Trace.WriteLine($"Failed to download company results!", nameof(LobbyHostModel));
                }


                // Invoke on GUI - now allow to leave
                Application.Current.Dispatcher.Invoke(() => {

                    // Reset text
                    this.StartMatchButton.Title = LOCSTR_WAIT();

                });


            });
        });

        // Inform user
        if (this.m_chatModel is not null) {
            this.m_chatModel.SystemMessage($"Match results saved.", Colors.Gray);
        }

    }

    private void OnGamemodeReleased(ServerAPI obj) {

        // Start background thread
        Task.Run(() => {

            // Log download operation
            Trace.WriteLine("Starting download of gamemode.", nameof(LobbyHostModel));

            // Download
            obj.DownloadGamemode((status, data) => {

                if (status is DownloadResult.DOWNLOAD_SUCCESS) {

                    // Make sure data aint null
                    if (data is null) {
                        this.m_chatModel?.SystemMessage($"Failed to download gamemode", Colors.DarkRed);
                        return;
                    }

                    // File sga to gamemode file
                    File.WriteAllBytes(WinconditionCompiler.GetArchivePath(), data);

                    // Set as true
                    this.m_hasDownloadedGamemode = true;

                    // Respond with download flag
                    this.m_handle.RespondPoll("gamemode_check", true);

                    // Inform user
                    this.m_chatModel?.SystemMessage($"Gamemode downloaded", Colors.Gray);

                } else {

                    // Inform user
                    this.m_chatModel?.SystemMessage($"Failed to download gamemode", Colors.DarkRed);

                    // Respond with download flag
                    this.m_handle.RespondPoll("gamemode_check", false);

                    // Log
                    Trace.WriteLine($"Failed to download gamemode! (E = {status})", nameof(LobbyHostModel));

                }

            });

        });

    }

    private void OnMatchInfo(LobbyMatchInfoEventArgs e) {

        // Check if error
        if (e.IsError) {

            //Log error
            this.m_chatModel?.SystemMessage($"Match Error - {e.Reason}", Colors.Red);
            Application.Current.Dispatcher.Invoke(() => {

                this.StartMatchButton.Title = LOCSTR_WAIT();
                this.StartMatchButton.IsEnabled = false;

            });

        } else {

            if (e.Type is "upload_status") {

                this.StartMatchButton.Title = LOCSTR_DOWNLOAD(e.Reason);

            } else {

                //Log info
                this.m_chatModel?.SystemMessage($"{e.Reason}", Colors.DarkGray);

            }

        }

    }

    private void OnPoll(string pollType) {

        if (pollType is "ready_check") {

            // Respond with is ready
            this.m_handle.RespondPoll("ready_check", this.IsReady());

        } else if (pollType is "gamomode_check") { // ignored for now
        } else {

            Trace.WriteLine($"OnPoll has recieved {pollType} that doesn't match anything.");

        }

    }

    private void OnLobbyChange(LobbySettingsChangedEventArgs e) {

        switch (e.SettingsKey) {
            case LobbyConstants.SETTING_MAP:
                this.OnScenarioChange(e.SettingsValue);
                break;
            case LobbyConstants.SETTING_GAMEMODE:
                this.OnGamemodeChange(e.SettingsValue);
                break;
            case LobbyConstants.SETTING_GAMEMODEOPTION:
                this.OnGamemodeOptionChanage(e.SettingsValue);
                break;
            case LobbyConstants.SETTING_WEATHER:
                this.WeatherDropdown.Label = (e.SettingsValue is "1") ? "On" : "Off";
                break;
            case LobbyConstants.SETTING_LOGISTICS:
                this.SupplySystemDropdown.Label = (e.SettingsValue is "1") ? "On" : "Off";
                break;
            case LobbyConstants.SETTING_MODPACK:
                this.OnModPackageChange(e.SettingsValue);
                break;
            default:
                Trace.WriteLine($"Unexpected setting key: {e.SettingsKey}", nameof(LobbyParticipantModel));
                break;
        }

    }

    private void OnScenarioChange(string map) {

        Application.Current.Dispatcher.Invoke(() => {

            // Check if valid
            if (ScenarioList.FromRelativeFilename(map) is not Scenario scenario) {
                this.MapDropdown.Label = map;
                this.ScenarioPreview = LobbySettingsLookup.TryGetMapSource(null);
                this.NotifyProperty(nameof(ScenarioPreview));
                return;
            }

            // Set visuals
            this.MapDropdown.Label = LobbySettingsLookup.GetScenarioName(scenario, map);
            this.ScenarioPreview = LobbySettingsLookup.TryGetMapSource(scenario);

            // Notify
            this.NotifyProperty(nameof(ScenarioPreview));

        });

    }

    private void OnGamemodeChange(string gamemode) 
        => this.GamemodeDropdown.Label = LobbySettingsLookup.GetGamemodeName(gamemode, this.m_package);

    private void OnGamemodeOptionChanage(string gamomodeOption) {

        // TODO: Lookup the option and set its string properly
        this.GamemodeOptionDropdown.Label = gamomodeOption;

    }

    private void OnModPackageChange(string modPackage) {

        if (ModManager.GetPackage(modPackage) is not ModPackage tunning) {
            this.ModPackageDropdown.Label = modPackage;
            return;
        }

        this.m_package = tunning;
        this.ModPackageDropdown.Label = tunning.PackageName;
    
    }

}
