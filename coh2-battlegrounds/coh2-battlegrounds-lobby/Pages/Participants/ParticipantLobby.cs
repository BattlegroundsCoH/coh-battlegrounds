using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

using Battlegrounds.Game.Match.Play;
using Battlegrounds.Modding.Content;
using Battlegrounds.Modding;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Networking.Server;
using Battlegrounds.Lobby.Components;
using Battlegrounds.Lobby.Lookups;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.DataLocal;
using Battlegrounds.Compiler;
using Battlegrounds.Game.Scenarios;
using Battlegrounds.Functional;
using Battlegrounds.Misc.Collections;
using Battlegrounds.Locale;
using Battlegrounds.UI;
using Battlegrounds.Resources;

namespace Battlegrounds.Lobby.Pages.Participants;

using static Battlegrounds.Lobby.Components.Buttons;
using static Battlegrounds.UI.AppContext;

/// <summary>
/// 
/// </summary>
public sealed class ParticipantLobby : BaseLobby {

    private bool m_hasDownloadedGamemode = false;

    private ModPackage? m_package;

    public override MutableButton SwapRoles { get; }

    public override MutableButton StartMatchButton { get; }

    public override Setting<ScenOp> MapDropdown { get; }

    public override Setting<IGamemode> GamemodeDropdown { get; }

    public override Setting<IGamemodeOption> GamemodeOptionDropdown { get; }

    public override Setting<OnOffOption> WeatherDropdown { get; }

    public override Setting<OnOffOption> SupplySystemDropdown { get; }

    public override Setting<ModPackageOption> ModPackageDropdown { get; }

    public override ModPackage ModPackage => this.m_package ?? throw new Exception("No Mod Package Defined");

    public ParticipantLobby(ILobbyHandle handle, ILobbyTeam allies, ILobbyTeam axis) : base(handle, allies, axis) {

        // Define start match buttnn
        this.StartMatchButton = new(new(this.CancelMatch), Visibility.Hidden) { Title = LOCSTR_WAIT(), IsEnabled = false, NotificationVisible = Visibility.Hidden };
        this.SwapRoles = new(new(() => { }), Visibility.Collapsed);

        // Init dropdowns 
        this.MapDropdown = Setting<ScenOp>.NewValue("LobbyView_SettingScenario", string.Empty);
        this.GamemodeDropdown = Setting<IGamemode>.NewValue("LobbyView_SettingGamemode", string.Empty);
        this.GamemodeOptionDropdown = Setting<IGamemodeOption>.NewValue("LobbyView_SettingOption", handle.Settings[LobbyConstants.SETTING_GAMEMODEOPTION]);
        this.WeatherDropdown = Setting<OnOffOption>.NewValue("LobbyView_SettingWeather", handle.Settings[LobbyConstants.SETTING_WEATHER] is "1" ? "On" : "Off");
        this.SupplySystemDropdown = Setting<OnOffOption>.NewValue("LobbyView_SettingSupply", handle.Settings[LobbyConstants.SETTING_LOGISTICS] is "1" ? "On" : "Off");
        this.ModPackageDropdown = Setting<ModPackageOption>.NewValue("LobbyView_SettingTuning", string.Empty);

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
            matchNotifier.OnLobbyScreen += this.OnScreenChange;
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

    private void OnScreenChange(string args) {

        Application.Current.Dispatcher.Invoke(() => {
            switch (args) {
                case "planning":

                    // Goto match planning
                    this.PlanMatch();

                    break;
                case "lobby":

                    // Change our view back to this
                    GetViewManager().UpdateDisplay(AppDisplayTarget.Right, this);

                    break;
                default:
                    Trace.WriteLine($"Unknown screen change '{args}'", nameof(ParticipantLobby));
                    break;
            }
        });

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
            this.StartMatchButton.Title = VisualsLookup.LOCSTR_PLAYING();

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
                        Companies.SaveCompany(company);

                    } else {

                        // Log
                        this.m_chatModel?.SystemMessage($"Failed to download valid company results!", Colors.DarkRed);

                    }

                } else {
                    Trace.WriteLine($"Failed to download company results!", nameof(ParticipantLobby));
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
            Trace.WriteLine("Starting download of gamemode.", nameof(ParticipantLobby));

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
                    Trace.WriteLine($"Failed to download gamemode! (E = {status})", nameof(ParticipantLobby));

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

                this.StartMatchButton.Title = VisualsLookup.LOCSTR_DOWNLOAD(e.Reason);

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

        Application.Current.Dispatcher.Invoke(() => {

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
                    // Try set custom gamemode settings if there
                    foreach (var opt in this.GamemodeSettings) {
                        if (e.SettingsKey.Equals(opt.Tag)) {
                            this.OnGamemodeAuxOptionChanged(opt, e);
                            return;
                        }
                    }
                    Trace.WriteLine($"Unexpected setting key: {e.SettingsKey}", nameof(ParticipantLobby));
                    break;
            }
        });
    }

    private void OnScenarioChange(string map) {

        Application.Current.Dispatcher.Invoke(() => {

            // Check if valid
            if (ScenarioList.FromRelativeFilename(map) is not Scenario scenario) {
                this.MapDropdown.Label = map;
                this.ScenarioPreview = ScenarioPreviewLookup.TryGetMapSource(null);
                this.NotifyProperty(nameof(ScenarioPreview));
                return;
            }

            // Set visuals
            this.MapDropdown.Label = SettingsLookup.GetScenarioName(scenario, map);
            this.ScenarioPreview = ScenarioPreviewLookup.TryGetMapSource(scenario);

            // Set current scenario
            this.Scenario = scenario;

            // Notify
            this.NotifyProperty(nameof(Scenario));

        });

    }

    private void OnGamemodeChange(string gamemode) {

        // Get settings
        var settings = this.m_handle.Settings;

        // Invoke on UI thread
        Application.Current.Dispatcher.Invoke(() => {

            // Set gamemode label
            this.GamemodeDropdown.Label = SettingsLookup.GetGamemodeName(gamemode, this.m_package);

            // Set gamemode
            this.Gamemode = this.ModPackage.Gamemodes
                .Filter(x => x.ID == gamemode)
                .IfTrue(x => x.Length == 1)
                .ThenDo(x => x[0])
                .OrDefaultTo(() => throw new IndexOutOfRangeException());

            // Clean additional options
            this.GamemodeSettings.RemoveAll(x => x != this.GamemodeOptionDropdown);

            // Ensure additional options are not null
            if (this.Gamemode.AdditionalOptions is not null) {

                // Get locale
                var loc = this.ModPackage.GetLocale(ModType.Gamemode, BattlegroundsInstance.Localize.Language);

                // Read gamemode options
                foreach (var (k, v) in this.Gamemode.AdditionalOptions) {

                    // Grab name
                    var name = loc is not null && uint.TryParse(v.Title, out uint titleKey) ?
                        new LocaleValueKey(loc[titleKey].ToUpperInvariant()) :
                        new LocaleValueKey(v.Title);

                    // Grab proper value
                    var val = settings.GetOrDefault(k, v.Min.ToString());

                    // Grab setting
                    var setting = Setting<string>.NewValue(name, GetAdditionalGamemodeValue(v, val), k);

                    // Add setting
                    this.GamemodeSettings.Add(setting);

                }

            }

        });

    }

    private void OnGamemodeOptionChanage(string gamomodeOption) {

        Application.Current.Dispatcher.Invoke(() => {
            // TODO: Lookup the option and set its string properly
            this.GamemodeOptionDropdown.Label = gamomodeOption;
        });


    }

    private void OnGamemodeAuxOptionChanged(Setting setting, LobbySettingsChangedEventArgs e) {

        // Make sure there are options
        if (this.Gamemode.AdditionalOptions is not null) {

            // Get locale
            //var loc = this.ModPackage.GetLocale(ModType.Gamemode, BattlegroundsInstance.Localize.Language);

            // Read gamemode options
            foreach (var (k, v) in this.Gamemode.AdditionalOptions) {

                // Make sure we got the correct one
                if (k != e.SettingsKey)
                    continue;

                // Grab setting
                setting.Label = GetAdditionalGamemodeValue(v, e.SettingsValue);

            }

        } else {

            setting.Label = e.SettingsValue;

        }

    }

    private static string GetAdditionalGamemodeValue(Gamemode.GamemodeAdditionalOption option, string value)
        => option.Type switch {
            "Checkbox" => value == "0" ? "Off" : "On",
            "Dropdown" => int.TryParse(value, out int selectIndex) ? option.Options[selectIndex].LocStr : "", // TODO: Proper Locale
            "Slider" => string.Format(option.Value, value),
            _ => value
        };

    private void OnModPackageChange(string modPackage) {

        if (ModManager.GetPackage(modPackage) is not ModPackage tunning) {
            this.ModPackageDropdown.Label = modPackage;
            return;
        }

        this.m_package = tunning;
        this.ModPackageDropdown.Label = tunning.PackageName;

    }

}
