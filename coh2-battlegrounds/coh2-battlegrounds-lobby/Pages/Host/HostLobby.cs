using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using Battlegrounds.AI;
using Battlegrounds.Functional;
using Battlegrounds.Game;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Scenarios;
using Battlegrounds.Lobby.Components;
using Battlegrounds.Lobby.Lookups;
using Battlegrounds.Lobby.Playing;
using Battlegrounds.Locale;
using Battlegrounds.Modding;
using Battlegrounds.Networking.LobbySystem;

using static Battlegrounds.Lobby.Components.Buttons;

namespace Battlegrounds.Lobby.Pages.Host;

/// <summary>
/// 
/// </summary>
public sealed class HostLobby : BaseLobby {

    private static readonly string __playabilityAlliesInvalid = BattlegroundsContext.Localize.GetString("LobbyView_StartMatchAlliesInvalid");
    private static readonly string __playabilityAlliesNoPlayers = BattlegroundsContext.Localize.GetString("LobbyView_StartMatchAlliesNoPlayers");
    private static readonly string __playabilityAxisInvalid = BattlegroundsContext.Localize.GetString("LobbyView_StartMatchAxisInvalid");
    private static readonly string __playabilityAxisNoPlayers = BattlegroundsContext.Localize.GetString("LobbyView_StartMatchAxisNoPlayers");
    private static readonly string __playabilityNoticePersistency = BattlegroundsContext.Localize.GetString("LobbyView_PersistencyDisabled");

    private static readonly Func<int, string> LOCSTR_GAMEMODEUPLOAD = x => BattlegroundsContext.Localize.GetString("LobbyView_UploadGamemode", x);

    private IModPackage? m_package;

    public override MutableButton SwapRoles { get; }

    public override MutableButton StartMatchButton { get; }

    public override Setting<ScenOp> MapDropdown { get; }

    public override Setting<IGamemode> GamemodeDropdown { get; }

    public override Setting<IGamemodeOption> GamemodeOptionDropdown { get; }

    public override Setting<OnOffOption> WeatherDropdown { get; }

    public override Setting<OnOffOption> SupplySystemDropdown { get; }

    public override Setting<ModPackageOption> ModPackageDropdown { get; }

    public override IModPackage ModPackage => this.m_package ?? throw new Exception("No Mod Package Defined");

    public HostLobby(ILobbyHandle handle, ILobbyTeam allies, ILobbyTeam axis) : base(handle, allies, axis) {

        // Get & set tunning list
        var tunlist = new List<ModPackageOption>();
        BattlegroundsContext.ModManager.EachPackage(x => tunlist.Add(new ModPackageOption(x)));

        // Init mod package dropdown
        this.ModPackageDropdown = Setting<ModPackageOption>.NewDropdown("LobbyView_SettingTuning", new(tunlist), this.ModPackageSelectionChanged);

        // Set default package
        this.m_package = this.ModPackageDropdown.Items[0].ModPackage;

        // Init buttons
        this.StartMatchButton = new(new(() => this.m_isStarting.IfFalse().Then(this.BeginMatchSetup).Else(this.CancelMatch)), Visibility.Visible) {
            Title = LOCSTR_START(),
            NotificationVisible = Visibility.Hidden
        };
        this.SwapRoles = new(new(this.SwapTeamRoles), Visibility.Collapsed);

        // Get scenario list
        var _scenlist = BattlegroundsContext.DataSource.GetScenarioList(m_package, GameCase.CompanyOfHeroes2)!.GetList()
            .Where(x => x.IsVisibleInLobby)
            .OrderBy(x => x.MaxPlayers);
        var scenlist = new List<ScenOp>(_scenlist.Select(x => new ScenOp(x)));

        // Get On & Off collection
        ObservableCollection<OnOffOption> onOfflist = new(new[] { new OnOffOption(true), new OnOffOption(false) });

        // Init dropdowns 
        this.MapDropdown = Setting<ScenOp>.NewDropdown("LobbyView_SettingScenario", new(scenlist), this.MapSelectionChanged);
        this.GamemodeDropdown = Setting<IGamemode>.NewDropdown("LobbyView_SettingGamemode", new(), this.GamemodeSelectionChanged);
        this.GamemodeOptionDropdown = Setting<IGamemodeOption>.NewDropdown("LobbyView_SettingOption", new(), this.GamemodeOptionSelectionChanged);
        this.WeatherDropdown = Setting<OnOffOption>.NewDropdown("LobbyView_SettingWeather", onOfflist, this.WeatherSelectionChanged);
        this.SupplySystemDropdown = Setting<OnOffOption>.NewDropdown("LobbyView_SettingSupply", onOfflist, this.SupplySystemSelectionChanged);

        // Add dropdowns
        this.MapSetting.Add(this.MapDropdown);
        this.MapSetting.Add(this.GamemodeDropdown);
        this.GamemodeSettings.Add(this.GamemodeOptionDropdown);
        this.AuxSettings.Add(this.WeatherDropdown);
        this.AuxSettings.Add(this.SupplySystemDropdown);
        this.AuxSettings.Add(this.ModPackageDropdown);

        // Set dropdown index, cascade effect
        this.MapDropdown.Selected = 0;

        // Set opt-in settings
        this.WeatherDropdown.Selected = 0;
        this.SupplySystemDropdown.Selected = 0;
        this.ModPackageDropdown.Selected = 0;

        // Subscribe to events
        this.m_handle.OnLobbyTeamUpdate += this.OnTeamUpdated;
        this.m_handle.OnLobbySlotUpdate += this.OnSlotUpdated;

        // Set lobby status
        this.m_handle.SetLobbyState(LobbyState.InLobby);

        // Inform others
        if (this.GetSelf() is ILobbySlot self && self.Occupant is not null) {
            this.m_handle.MemberState(self.Occupant.MemberID, self.TeamID, self.SlotID, LobbyMemberState.Waiting);
        }

    }

    private async void OnSlotUpdated(ILobbySlot args) {
        await Task.Delay(100);
        Application.Current.Dispatcher.Invoke(() => {
            this.RefreshPlayability();
        });
    }

    private async void OnTeamUpdated(ILobbyTeam args) {
        await Task.Delay(100);
        Application.Current.Dispatcher.Invoke(() => {
            this.RefreshPlayability();
        });
    }

    public void RefreshPlayability() {

        // Skip check if not host
        if (!this.m_handle.IsHost) {
            Trace.WriteLine($"Attempt to invoke '{nameof(RefreshPlayability)}' but is not host!.", nameof(HostLobby));
            return;
        }

        // Check allies
        var (x1, y1) = this.Allies.CanPlay();
        bool allied = x1 && y1;

        // Check axis
        var (x2, y2) = this.Axis.CanPlay();
        bool axis = x2 && y2;

        // If both playable
        if (allied && axis) {
            var notVis = this.AllowsPersitency();
            this.StartMatchButton.IsEnabled = true;
            this.StartMatchButton.Tooltip = notVis ? null : __playabilityNoticePersistency;
            this.StartMatchButton.NotificationVisible = !notVis ? Visibility.Visible : Visibility.Hidden;
        } else if (!allied) {
            this.StartMatchButton.IsEnabled = false;
            this.StartMatchButton.Tooltip = x1 ? __playabilityAlliesInvalid : __playabilityAlliesNoPlayers;
            this.StartMatchButton.NotificationVisible = Visibility.Visible;
        } else {
            this.StartMatchButton.IsEnabled = false;
            this.StartMatchButton.Tooltip = x2 ? __playabilityAxisInvalid : __playabilityAxisNoPlayers;
            this.StartMatchButton.NotificationVisible = Visibility.Visible;
        }

    }

    private bool AllowsPersitency() {
        uint totalPlayers = this.m_handle.GetPlayerCount(false);
        uint totalHumans = this.m_handle.GetPlayerCount(true);
        uint totalAI = totalPlayers - totalHumans;
        int hardsum = this.m_handle.Allies.Slots.Count(x => x.IsAI(AIDifficulty.AI_Hard)) + this.m_handle.Axis.Slots.Count(x => x.IsAI(AIDifficulty.AI_Hard));
        return hardsum == totalAI;
    }

    private void BeginMatchSetup() {

        // Log being match setup
        Trace.WriteLine("Begin match setup button pressed.", nameof(HostLobby));

        // If not host -> bail.
        if (!this.m_handle.IsHost) {
            Trace.WriteLine($"Attempt to invoke '{nameof(BeginMatchSetup)}' but is not host!.", nameof(HostLobby));
            return;
        }

        // Bail if no chat model
        if (this.m_chatModel is null) {
            Trace.WriteLine($"Attempt to invoke '{nameof(BeginMatchSetup)}' but chat model was null!.", nameof(HostLobby));
            return;
        }

        // Bail if no package defined
        if (this.m_package is null) {
            this.m_chatModel.SystemMessage("Mod package not set.", Colors.Gray);
            return;
        }

        // Bail if not ready
        if (!this.IsReady()) {
            this.m_chatModel.SystemMessage("Cannot start match (Host not ready).", Colors.Yellow);
            return;
        }

        // Grab gamemode
        var gamemode = this.m_package.Gamemodes[this.GamemodeDropdown.Selected];

        // Do on a worker thread (especially needed now that AI planning is added -- which can take ~1-3s to generate *some* plan).
        Task.Run(() => {

            // Log startup on new thread
            Trace.WriteLine("Conducting startup poll.", nameof(HostLobby));

            // Get status from other participants
            if (!this.m_handle.ConductPoll("ready_check", 2)) {
                this.m_chatModel.SystemMessage("One or more lobby members are not ready.", Colors.Gray);
                return;
            }

            // Decide what to do, based on planning
            if (gamemode.Planning) {

                // Log Screen change
                Trace.WriteLine("Changing screen to planning phase.", nameof(HostLobby));

                // Inform others we're entering the planning phase
                this.m_handle.NotifyScreen("planning");

                // Begin plan match
                Application.Current.Dispatcher.Invoke(this.PlanMatch);

            } else {

                // Log startup
                Trace.WriteLine("Starting play model selection and preparation.", nameof(HostLobby));

                // Set starting flag
                this.m_isStarting = true;

                // Set lobby status here
                this.m_handle.SetLobbyState(LobbyState.Starting);

                // Get play model
                var play = PlayModelFactory.GetModel(this.m_handle, this.m_chatModel, 5, this.UploadGamemodeCallback);

                // Log Screen change
                Trace.WriteLine($"{nameof(PlayModelFactory)} picked play model '{play.GetType().Name}'.", nameof(HostLobby));

                // prepare
                play.Prepare(this.m_package, this.BeginMatch, x => this.EndMatch(x is IPlayModel y ? y : throw new ArgumentNullException()));

            }

        });

    }

    internal void BeginMatch(IPlayModel model) {

        // Set lobby status here
        this.m_handle.SetLobbyState(LobbyState.Playing);

        // Play match
        model.Play(this.GameLaunching, this.EndMatch);

    }

    internal void UploadGamemodeCallback(int curr, int exp, bool cancelled) {

        // Calculate percentage
        int p = Math.Min(100, (int)(curr / (double)exp * 100.0));

        // Notify users of upload status
        this.m_handle.NotifyMatch("upload_status", p.ToString());

        // Update visually
        Application.Current.Dispatcher.Invoke(() => {
            if (p == 100) {
                this.StartMatchButton.Title = VisualsLookup.LOCSTR_PLAYING();
            } else {
                this.StartMatchButton.Title = LOCSTR_GAMEMODEUPLOAD(p);
            }
        });

    }

    private void GameLaunching() {

        // Notify
        Application.Current.Dispatcher.Invoke(() => {
            this.StartMatchButton.IsEnabled = false;
            this.StartMatchButton.Title = VisualsLookup.LOCSTR_PLAYING();
        });

    }

    internal void EndMatch(IPlayModel model) {

        // Set lobby status here
        this.m_handle.SetLobbyState(LobbyState.InLobby);

        // Re-enable
        Application.Current.Dispatcher.Invoke(() => {
            this.StartMatchButton.IsEnabled = true;
            this.StartMatchButton.Title = LOCSTR_START();
        });

    }

    private void MapSelectionChanged(int newIndex, int oldIndex, object value) {

        // Get scenario
        var scen = this.MapDropdown.Items[newIndex].Scenario;

        if (!this.m_handle.SetTeamsCapacity(scen.MaxPlayers / 2)) {
            Task.Run(() => {
                Task.Delay(1);
                Application.Current.Dispatcher.Invoke(() => {
                    this.MapDropdown.Selected = oldIndex;
                });
            });
            return;
        }

        // Update
        this.Scenario = scen;
        this.NotifyProperty(nameof(Scenario));

        // Update last played
        BattlegroundsContext.LastPlayedMap = scen.RelativeFilename;

        // Update gamemode
        this.UpdateGamemodeAndOptionsSelection(scen);

        // Update lobby
        this.m_handle.SetLobbySetting(LobbyConstants.SETTING_MAP, scen.RelativeFilename);

    }

    private void GamemodeSelectionChanged(int newIndex, int _, object value) {

        // Grab gamemode
        var gamemode = this.GamemodeDropdown.Items[newIndex];

        // Get gamemode options
        var options = gamemode.Options;

        // Clear available options
        this.GamemodeOptionDropdown.Items.Clear();

        // Clear options
        this.GamemodeSettings.Clear();

        // Update team names
        this.m_handle.SetTeamRoles(gamemode.TeamName1 ?? "Team_Allies", gamemode.TeamName2 ?? "Team_Axis");

        // If gamemode has fixed positions, allow us to swap
        this.SwapRoles.Visibility = gamemode.RequireFixed ? Visibility.Visible : Visibility.Collapsed;

        // Set gamemode
        this.Gamemode = this.ModPackage.Gamemodes[newIndex];

        // Hide options
        if (options is null || options.Length is 0) {

            // Set setting to empty (Hidden)
            this.m_handle.SetLobbySetting(LobbyConstants.SETTING_GAMEMODEOPTION, string.Empty);

        } else {

            // Update options
            options.ForEach(this.GamemodeOptionDropdown.Items.Add);

            // TODO: Set gamemode option that was last selected
            this.GamemodeOptionDropdown.Selected = 0;

            // Set visibility to visible
            this.GamemodeSettings.Add(this.GamemodeOptionDropdown);

            // Add aux operations
            for (int i = 0; i < gamemode.AuxiliaryOptions.Length; i++) {

                // Grab option
                var custom = gamemode.AuxiliaryOptions[i];

                // Create handler
                void handler(int _, int __, object v) => this.GamemodeAuxOptionSelectionchanged(custom.Name, v);

                // Grab name and convert it to a direct LocaleValueKey --> Only do this when merging UCS and BGLOC stuff.
                var name = new LocaleValueKey(custom.Title.ToString().ToUpperInvariant());

                // Create control
                Setting settingControl = custom.OptionInputType switch {
                    AuxiliaryOptionType.Dropdown =>
                        Setting<IGamemodeOption>.NewDropdown(name, new(custom.Options.OrderBy(x => x.Value)), handler, custom.GetNumber("def")),
                    AuxiliaryOptionType.Slider =>
                        Setting<int>.NewSlider(name, custom.GetNumber("min"), custom.GetNumber("max"), custom.GetNumber("step"), custom.GetNumber("def"), custom.Format, handler),
                    _ => throw new Exception()
                };

                // Trigger a default set
                this.GamemodeAuxOptionSelectionchanged(custom.Name, custom.GetNumber("def"));

                // Add
                this.GamemodeSettings.Add(settingControl);

            }

        }

        // Update lobby
        this.m_handle.SetLobbySetting(LobbyConstants.SETTING_GAMEMODE, this.GamemodeDropdown.Items[newIndex].Name);

        // Update options
        this.NotifyProperty(nameof(ShowScrollbarForSettings));

    }

    private void GamemodeAuxOptionSelectionchanged(string option, object v)
        => this.m_handle.SetLobbySetting(option, v?.ToString() ?? "0");

    private void GamemodeOptionSelectionChanged(int newIndex, int _, object value) {

        // Bail
        if (newIndex == -1) {
            this.m_handle.SetLobbySetting(LobbyConstants.SETTING_GAMEMODEOPTION, string.Empty);
            return;
        }

        // Update lobby
        this.m_handle.SetLobbySetting(LobbyConstants.SETTING_GAMEMODEOPTION, this.GamemodeOptionDropdown.Items[newIndex].Value.ToString(CultureInfo.InvariantCulture));

    }

    private void WeatherSelectionChanged(int newIndex, int _, object value) {

        // Update lobby
        this.m_handle.SetLobbySetting(LobbyConstants.SETTING_WEATHER, this.WeatherDropdown.Items[newIndex].IsOn ? "1" : "0");

    }

    private void SupplySystemSelectionChanged(int newIndex, int _, object value) {

        // Update lobby
        this.m_handle.SetLobbySetting(LobbyConstants.SETTING_LOGISTICS, this.SupplySystemDropdown.Items[newIndex].IsOn ? "1" : "0");

    }

    private void ModPackageSelectionChanged(int newIndex, int oldIndex, object value) {

        // Set package
        this.m_package = this.ModPackageDropdown.Items[newIndex].ModPackage;

        // Update lobby
        this.m_handle.SetLobbySetting(LobbyConstants.SETTING_MODPACK, this.m_package.ID);

    }

    private void UpdateGamemodeAndOptionsSelection(Scenario scenario) {

        // Bail if no package defined
        if (this.m_package is null) {
            return;
        }

        // Get available gamemodes
        var guid = this.m_package.GamemodeGUID;
        List<IGamemode> gamemodes =
            (scenario.Gamemodes.Count > 0 ? Winconditions.GetGamemodes(guid, scenario.Gamemodes) : Winconditions.GetGamemodes(guid)).ToList();

        // Update if there's any change in available gamemodes 
        if (this.GamemodeDropdown.Items.Count != gamemodes.Count || gamemodes.Any(x => !this.GamemodeDropdown.Items.Contains(x))) {

            // Clear current gamemode selection
            this.GamemodeDropdown.Items.Clear();
            gamemodes.ForEach(this.GamemodeDropdown.Items.Add);

            // Set gamemode
            this.GamemodeDropdown.Selected = 0;

        }

    }

    private void SwapTeamRoles() {

        // Swap roles in handle
        this.m_handle.SwapTeamRoles();

    }

}
