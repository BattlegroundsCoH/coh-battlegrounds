using System;
using System.Diagnostics;
using System.Globalization;
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

using BattlegroundsApp.LocalData;
using BattlegroundsApp.Resources;

namespace BattlegroundsApp.Lobby.MVVM.Models;

public class LobbyParticipantModel : LobbyModel {

    private static readonly Func<string> LOCSTR_WAIT = () => BattlegroundsInstance.Localize.GetString("LobbyView_WaitMatch");

    private bool m_hasDownloadedGamemode = false;

    private ModPackage? m_package;

    public override LobbyMutButton StartMatchButton { get; }

    public override LobbyDropdown<ScenOp> MapDropdown { get; }

    public override LobbyDropdown<IGamemode> GamemodeDropdown { get; }

    public override LobbyDropdown<IGamemodeOption> GamemodeOptionDropdown { get; }

    public override LobbyDropdown<OnOffOption> WeatherDropdown { get; }

    public override LobbyDropdown<OnOffOption> SupplySystemDropdown { get; }

    public override LobbyDropdown<ModPackageOption> ModPackageDropdown { get; }

    public LobbyParticipantModel(LobbyAPI handle, LobbyAPIStructs.LobbyTeam allies, LobbyAPIStructs.LobbyTeam axis) : base(handle, allies, axis) {

        // Define start match buttnn
        this.StartMatchButton = new(new(this.CancelMatch), Visibility.Hidden) { Title = LOCSTR_WAIT(), IsEnabled = false };

        // Init dropdowns 
        this.MapDropdown = new(false, Visibility.Hidden, new(), (x,y) => { });
        this.GamemodeDropdown = new(false, Visibility.Hidden, new(), (x, y) => { });
        this.GamemodeOptionDropdown = new(false, Visibility.Hidden, new(), (x, y) => { }) { LabelContent = handle.Settings["selected_wco"] };
        this.WeatherDropdown = new(false, Visibility.Hidden, new(), (x, y) => { }) { LabelContent = (handle.Settings["selected_daynight"] is "1") ? "On" : "Off" };
        this.SupplySystemDropdown = new(false, Visibility.Hidden, new(), (x, y) => { }) { LabelContent = (handle.Settings["selected_supply"] is "1") ? "On" : "Off" };
        this.ModPackageDropdown = new(false, Visibility.Hidden, new(), (x, y) => { });

        // Subscribe to lobby events
        handle.OnLobbySettingUpdate += this.OnLobbyChange;
        handle.OnLobbyNotifyGamemode += this.OnGamemodeReleased;
        handle.OnLobbyNotifyResults += this.OnResultsReleased;
        handle.OnLobbyBeginMatch += this.OnMatchBegin;
        handle.OnLobbyLaunchGame += this.OnLaunchGame;

        // Trigger initial view
        this.OnModPackageChange(handle.Settings["selected_tuning"]);
        this.OnScenarioChange(handle.Settings["selected_map"]);
        this.OnGamemodeChange(handle.Settings["selected_wc"]);

    }

    private void OnMatchBegin() {

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

            // Inform user
            if (this.m_chatModel is not null) {
                this.m_chatModel.SystemMessage($"Laucnhing game", Colors.Gray);
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

                    // Allow exit lobby
                    //this.ExitLobby.Enabled = true;

                });


            });
        });

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

                    // Inform user
                    this.m_chatModel?.SystemMessage($"Gamemode downloaded", Colors.Gray);

                } else {

                    // Inform user
                    this.m_chatModel?.SystemMessage($"Failed to download gamemode", Colors.DarkRed);

                    // Log
                    Trace.WriteLine($"Failed to download gamemode! (E = {status})", nameof(LobbyHostModel));

                }

            });

        });

    }

    private void OnLobbyChange(LobbySettingsChangedEventArgs e) {

        switch (e.SettingsKey) {
            case "selected_map":
                this.OnScenarioChange(e.SettingsValue);
                break;
            case "selected_wc":
                this.OnGamemodeChange(e.SettingsValue);
                break;
            case "selected_wco":
                this.OnGamemodeOptionChanage(e.SettingsValue);
                break;
            case "selected_daynight":
                this.OnWeatherSettingChange(e.SettingsValue);
                break;
            case "selected_supply":
                this.OnSupplySettingChange(e.SettingsValue);
                break;
            case "selected_tuning":
                this.OnModPackageChange(e.SettingsValue);
                break;
            default:
                Trace.WriteLine($"Unexpected setting key: {e.SettingsKey}", nameof(LobbyParticipantModel));
                break;
        }

    }

    private void OnScenarioChange(string map) {

        Application.Current.Dispatcher.Invoke(() => {

            if (ScenarioList.FromRelativeFilename(map) is not Scenario scenario) {
                this.MapDropdown.LabelContent = map;
                this.ScenarioPreview = this.TryGetMapSource(null);
                this.NotifyProperty(nameof(ScenarioPreview));
                return;
            }

            this.MapDropdown.LabelContent = scenario.Name.StartsWith("$", false, CultureInfo.InvariantCulture) && uint.TryParse(scenario.Name[1..], out uint key) ?
                                            GameLocale.GetString(key) : scenario.Name;

            this.ScenarioPreview = this.TryGetMapSource(scenario);

            this.NotifyProperty(nameof(ScenarioPreview));

        });

    }

    private void OnGamemodeChange(string gamemode) {

        if (m_package is null) {
            return;
        }

        if (WinconditionList.GetGamemodeByName(m_package.GamemodeGUID, gamemode) is not IGamemode wincondition) {
            this.GamemodeDropdown.LabelContent = gamemode;
            return;
        }

        this.GamemodeDropdown.LabelContent = wincondition.ToString() ?? "Unknown Wincondition";

    }

    private void OnGamemodeOptionChanage(string gamomodeOption) {

        // TODO: Lookup the option and set its string properly
        this.GamemodeOptionDropdown.LabelContent = gamomodeOption;

    }

    private void OnWeatherSettingChange(string weatherSetting) { 
        
        this.WeatherDropdown.LabelContent = (weatherSetting is "1") ? "On" : "Off";

    }

    private void OnSupplySettingChange(string supplySetting) { 

        this.SupplySystemDropdown.LabelContent = (supplySetting is "1") ? "On" : "Off";
    
    }

    private void OnModPackageChange(string modPackage) {

        if (ModManager.GetPackage(modPackage) is not ModPackage tunning) {
            this.ModPackageDropdown.LabelContent = modPackage;
            return;
        }

        this.m_package = tunning;
        this.ModPackageDropdown.LabelContent = tunning.PackageName;
    
    }

}
