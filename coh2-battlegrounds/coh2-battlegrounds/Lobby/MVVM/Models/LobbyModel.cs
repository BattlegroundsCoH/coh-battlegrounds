using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Battlegrounds;
using Battlegrounds.Compiler;
using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match.Play;
using Battlegrounds.Locale;
using Battlegrounds.Modding;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Networking.Server;

using BattlegroundsApp.Lobby.MatchHandling;
using BattlegroundsApp.LocalData;
using BattlegroundsApp.Modals;
using BattlegroundsApp.MVVM;
using BattlegroundsApp.MVVM.Models;
using BattlegroundsApp.Resources;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Lobby.MVVM.Models {

    public class LobbyModel : IViewModel, INotifyPropertyChanged {

        private static readonly ImageSource __mapNotFound = new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/unknown_map.png"));

        private static readonly LocaleKey __playabilityAlliesInvalid = new("LobbyView_StartMatchAlliesInvalid");
        private static readonly LocaleKey __playabilityAlliesNoPlayers = new("LobbyView_StartMatchAlliesNoPlayers");
        private static readonly LocaleKey __playabilityAxisInvalid = new("LobbyView_StartMatchAxisInvalid");
        private static readonly LocaleKey __playabilityAxisNoPlayers = new("LobbyView_StartMatchAxisNoPlayers");

        private readonly LobbyAPI m_handle;
        private LobbyChatSpectatorModel? m_chatModel;
        private ModPackage? m_package;
        private bool m_hasSetDefaults;
        private LobbyDropdownModel[] m_settings;

        public event PropertyChangedEventHandler? PropertyChanged;

        public LobbyButtonModel EditCompany { get; }

        public LobbyButtonModel ExitLobby { get; }

        public LobbyButtonModel StartMatch { get; }

        public ImageSource? SelectedMatchScenario { get; set; }

        public LobbyDropdownModel<LobbyScenarioItem> ScenarioSelection { get; }

        public LobbyDropdownModel<LobbyGamemodeItem> GamemodeSelection { get; }

        public LobbyDropdownModel<LobbyGamemodeOptionItem> GamemodeOptionSelection { get; }

        public LobbyDropdownModel<LobbyBinaryOptionItem> WeatherSelection { get; }

        public LobbyDropdownModel<LobbyBinaryOptionItem> SupplySystemSelection { get; }

        public LobbyDropdownModel<LobbyModPackageItem> ModPackageSelection { get; }

        public LobbyTeam Allies { get; }

        public LobbyTeam Axis { get; }

        public bool SingleInstanceOnly => false;

        public LocaleKey ScenarioLabel { get; } = new("LobbyView_SettingScenario");

        public LocaleKey GamemodeLabel { get; } = new("LobbyView_SettingGamemode");

        public LocaleKey GamemodeOptionLabel { get; } = new("LobbyView_SettingOption");

        public LocaleKey SupplyLabel { get; } = new("LobbyView_SettingSupply");

        public LocaleKey WeatherLabel { get; } = new("LobbyView_SettingWeather");

        public LocaleKey PackageLabel { get; } = new("LobbyView_SettingTuning");

        private LobbyModel(LobbyAPI handle, LobbyAPIStructs.LobbyTeam allies, LobbyAPIStructs.LobbyTeam axis) {

            // Set handler
            this.m_handle = handle;

            // Create teams
            this.Allies = new(allies);
            this.Axis = new(axis);

            // Create edit company button
            this.EditCompany = new() {
                Click = new RelayCommand(this.EditSelfCompany),
                Enabled = false,
                Visible = Visibility.Visible,
                Text = new("LobbyView_EditCompany")
            };

            // Create exit lobby button
            this.ExitLobby = new() {
                Click = new RelayCommand(this.LeaveLobby),
                Enabled = false,
                Visible = Visibility.Visible,
                Text = new("LobbyView_LeaveLobby")
            };

            // Create start match button
            this.StartMatch = new() {
                Click = new RelayCommand(this.BeginMatchSetup),
                Enabled = false,
                Visible = Visibility.Visible,
                Text = new("LobbyView_StartMatch")
            };

            // Create package list
            List<LobbyModPackageItem> modPackages = new();
            ModManager.EachPackage(x => modPackages.Add(new(x)));

            // Create mod package dropdown
            this.ModPackageSelection = new(true, this.m_handle.IsHost, "selected_tuning") {
                Items = new(modPackages),
                OnSelectionChanged = this.OnPackageChanged
            };

            // Set package here
            if (this.m_handle.IsHost) {
                this.m_package = this.ModPackageSelection.Items[0].Package;
            }

            // Create scenario selection dropdown
            this.ScenarioSelection = new(true, this.m_handle.IsHost, "selected_map") {
                Items = new(ScenarioList.GetList()
                    .Where(x => x.IsVisibleInLobby)
                    .Select(x => new LobbyScenarioItem(x))),
                OnSelectionChanged = this.OnScenarioChanged
            };

            // Create gamemode selection dropdown
            this.GamemodeSelection = new(true, this.m_handle.IsHost, "selected_wc") {
                Items = new(),
                OnSelectionChanged = this.OnGamemodeChanged
            };

            // Create gamemode option selection dropdown
            this.GamemodeOptionSelection = new(true, this.m_handle.IsHost, "selected_wco") {
                Items = new(),
                OnSelectionChanged = this.OnGamemodeOptionChanged
            };

            // Create weather selection dropdown
            this.WeatherSelection = new(true, this.m_handle.IsHost, "selected_daynight") {
                Items = LobbyBinaryOptionItem.CreateCollection(),
                OnSelectionChanged = this.OnWeatherChanged
            };

            // Create supply selection dropdown
            this.SupplySystemSelection = new(true, this.m_handle.IsHost, "selected_supply") {
                Items = LobbyBinaryOptionItem.CreateCollection(),
                OnSelectionChanged = this.OnSupplyChanged
            };

            // Save setting dropdowns
            this.m_settings = new LobbyDropdownModel[] {
                this.ScenarioSelection, this.GamemodeSelection, this.GamemodeOptionSelection,
                this.ModPackageSelection, this.WeatherSelection, this.SupplySystemSelection
            };

            // Init dropdown values (if host)
            if (handle.IsHost) {
                this.ScenarioSelection.SetSelection(x => x.Scenario.RelativeFilename == BattlegroundsInstance.LastPlayedMap);
            }

            // Add handlers to remote updates and notifications
            this.m_handle.OnLobbySelfUpdate += this.OnSelfChanged;
            this.m_handle.OnLobbyTeamUpdate += this.OnTeamChanged;
            this.m_handle.OnLobbyCompanyUpdate += this.OnCompanyChanged;
            this.m_handle.OnLobbyMemberUpdate += this.OnMemberChanged;
            this.m_handle.OnLobbySlotUpdate += this.OnSlotChanged;
            this.m_handle.OnLobbyConnectionLost += this.OnConnectionLost;
            this.m_handle.OnLobbyRequestCompany += this.OnCompanyRequested;
            this.m_handle.OnLobbyNotifyGamemode += this.OnGamemodeReleased;
            this.m_handle.OnLobbyNotifyResults += this.OnResultsReleased;
            this.m_handle.OnLobbyLaunchGame += this.OnLaunchGame;

        }

        private void OnLaunchGame() {

            // Create overwatch strategy
            var overwatch = new MemberOverwatchStrategy();

            Task.Run(() => {

                // Begin
                overwatch.Launch();

                // Wait for exit
                overwatch.WaitForExit();

                // Do some more?

            });

        }

        private void OnResultsReleased(ServerAPI obj) {

            // Instruct download
            Task.Run(() => {
                obj.DownloadCompany(this.m_handle.Self.ID, (status, data) => {
                    if (status is DownloadResult.DOWNLOAD_SUCCESS) {


                        // Load it
                        var company = CompanySerializer.GetCompanyFromJson(Encoding.UTF8.GetString(data));

                        // Save it
                        PlayerCompanies.SaveCompany(company);


                    } else {
                        Trace.WriteLine($"Failed to download company results!", nameof(LobbyModel));
                    }
                });
            });

        }

        private void OnGamemodeReleased(ServerAPI obj) {

            // Start background thread
            Task.Run(() => {

                // Download
                obj.DownloadGamemode((status, data) => {

                    if (status is DownloadResult.DOWNLOAD_SUCCESS) {

                        // File sga to gamemode file
                        File.WriteAllBytes(WinconditionCompiler.GetArchivePath(), data);

                    } else {

                        Trace.WriteLine($"Failed to download gamemode! (E = {status})", nameof(LobbyModel));

                    }

                });

            });

        }

        private void OnCompanyRequested(ServerAPI obj) {

            // Log request
            Trace.WriteLine("Received request to upload company file", nameof(LobbyModel));

            // Get self
            ulong selfid = this.m_handle.Self.ID;
            var self = this.m_handle.Allies.GetSlotOfMember(selfid) ?? this.m_handle.Axis.GetSlotOfMember(selfid);
            if (self is not null && self.Occupant is not null) {

                // Make sure there's a company
                if (self.Occupant.Company is null) {
                    return;
                }

                // Get company name
                string companyName = self.Occupant.Company.Name;

                // Get company faction
                Faction faction = Faction.FromName(self.Occupant.Company.Army);

                // Get company json
                string companyJson = CompanySerializer.GetCompanyAsJson(PlayerCompanies.FromNameAndFaction(companyName, faction), indent: false);
                if (string.IsNullOrEmpty(companyJson)) {
                    Trace.WriteLine($"Failed to upload company json file (Company '{companyName}' not found).", nameof(LobbyModel));
                    return;
                }

                // Upload file
                if (obj.UploadCompany(selfid, companyJson, (a,b) => Trace.WriteLine($"Upload company progress {a}/{b}", nameof(LobbyModel))) is not UploadResult.UPLOAD_SUCCESS) {
                    Trace.WriteLine("Failed to upload company json file.", nameof(LobbyModel));
                }

            } else {

                // Log request
                Trace.WriteLine("Failed to find self-instance and cannot upload company file.", nameof(LobbyModel));

            }

        }

        private void OnSelfChanged() {
            Application.Current.Dispatcher.Invoke(() => {

                // Eval match launchability
                this.EvaluateMatchLaunchable();

            });
        }

        private void EditSelfCompany() {
            Trace.WriteLine("Editing own company in lobby is currently not implemented!");
        }

        private void LeaveLobby() {

            // Show leave modal
            App.ViewManager.GetModalControl()?.ShowModal(ModalDialog.CreateModal("Leave Lobby", "Are you sure you'd like to leave?", (sender, success, value) => {
                if (success && value == ModalDialogResult.Confirm) {

                    // Leave lobby
                    Task.Run(this.m_handle.Disconnect);

                    // Go back to browser view
                    App.ViewManager.SetDisplay(AppDisplayState.LeftRight, typeof(LeftMenu), typeof(LobbyBrowserViewModel));

                }
            }));

        }

        private void BeginMatchSetup() {

            // If not host -> bail.
            if (!this.m_handle.IsHost)
                return;

            // Bail if no chat model
            if (this.m_chatModel is null)
                return;

            // Bail if no package defined
            if (this.m_package is null)
                return; // TODO: Show error

            // Set lobby status here
            this.m_handle.SetLobbyState(LobbyAPIStructs.LobbyState.Starting);

            // Get play model
            var play = PlayModelFactory.GetModel(this.m_handle, this.m_chatModel);

            // prepare
            play.Prepare(this.m_package, this.BeginMatch, x => this.EndMatch(x is IPlayModel y ? y : throw new ArgumentNullException()));

        }

        private void BeginMatch(IPlayModel model) {

            // Set lobby status here
            this.m_handle.SetLobbyState(LobbyAPIStructs.LobbyState.Playing);

            // Play match
            model.Play(this.EndMatch);

        }

        private void EndMatch(IPlayModel model) {

            // Set lobby status here
            this.m_handle.SetLobbyState(LobbyAPIStructs.LobbyState.InLobby);

        }

        private int OnPackageChanged(int current, int next, LobbyModPackageItem item) {

            // Bail if item is null
            if (item is null) {
                return next;
            }

            // Set package
            this.m_package = item.Package;

            // Update lobby
            this.m_handle.SetLobbySetting(this.ModPackageSelection.DropdownID, item.Package.ID);

            // Return selected
            return next;

        }

        private int OnSupplyChanged(int current, int next, LobbyBinaryOptionItem item) {

            // Update lobby
            this.m_handle.SetLobbySetting(this.SupplySystemSelection.DropdownID, item.IsOn ? "1" : "0");

            // Return selected
            return next;

        }

        private int OnWeatherChanged(int current, int next, LobbyBinaryOptionItem item) {

            // Update lobby
            this.m_handle.SetLobbySetting(this.WeatherSelection.DropdownID, item.IsOn ? "1" : "0");

            // Return selected
            return next;

        }

        private void TrySetMapSource(Scenario? scenario, [CallerMemberName] string caller = "") {

            // Set to default case
            this.SelectedMatchScenario = __mapNotFound;

            // Check scenario
            if (scenario is null) {
                Trace.WriteLine($"Failed to set **null** scenario (Caller = {caller}).", nameof(LobbyModel));
                return;
            }

            // Get Path
            string fullpath = Path.GetFullPath($"bin\\gfx\\map_icons\\{scenario.RelativeFilename}_map.tga");

            // Check if file exists
            if (File.Exists(fullpath)) {
                try {
                    this.SelectedMatchScenario = TgaImageSource.TargaBitmapSourceFromFile(fullpath);
                } catch (BadImageFormatException bife) {
                    Trace.WriteLine(bife, nameof(this.TrySetMapSource));
                }
            } else {
                fullpath = Path.GetFullPath($"usr\\mods\\map_icons\\{scenario.RelativeFilename}_map.tga");
                if (File.Exists(fullpath)) {
                    try {
                        this.SelectedMatchScenario = TgaImageSource.TargaBitmapSourceFromFile(fullpath);
                    } catch (BadImageFormatException bife) {
                        Trace.WriteLine(bife, nameof(this.TrySetMapSource));
                    }
                } else {
                    Trace.WriteLine($"Failed to locate file: {fullpath}", nameof(this.TrySetMapSource));
                }
            }

            // Inform view the map selection was changed
            this.PropertyChanged?.Invoke(this, new(nameof(this.SelectedMatchScenario)));

        }

        private int OnScenarioChanged(int current, int next, LobbyScenarioItem item) {

            // If item is not null
            if (item is not null) {

                // Update team capacity and go back to previous selection if capacity change fails
                if (!this.m_handle.SetTeamsCapacity(item.Scenario.MaxPlayers / 2)) {
                    return current;
                }

                // Update map
                _ = Application.Current.Dispatcher.BeginInvoke(() => this.TrySetMapSource(item.Scenario));

                // Update gamemode (and option)
                this.UpdateGamemodeAndOption(item.Scenario);

                // Update lobby
                this.m_handle.SetLobbySetting(this.ScenarioSelection.DropdownID, item.Scenario.RelativeFilename);

            }

            // Return selected value
            return next;

        }

        private void UpdateGamemodeAndOption(Scenario scenario) {

            // Bail if no package defined
            if (this.m_package is null)
                return;

            // Get available gamemodes
            var guid = this.m_package.GamemodeGUID;
            List<LobbyGamemodeItem> available = (scenario.Gamemodes.Count > 0 ? WinconditionList.GetGamemodes(guid, scenario.Gamemodes) : WinconditionList.GetGamemodes(guid))
                .Select(x => new LobbyGamemodeItem(x)).ToList();

            // Update if there's any change in available gamemodes 
            if (this.GamemodeSelection.Items.Count != available.Count || available.Any(x => !this.GamemodeSelection.Items.Contains(x))) {

                // Clear current gamemode selection
                this.GamemodeSelection.Items.Clear();
                available.ForEach(x => this.GamemodeSelection.Items.Add(x));

                // Set default if not set
                if (!this.m_hasSetDefaults) {
                    this.GamemodeSelection.SetSelection(x => x.Gamemode.Name == BattlegroundsInstance.LastPlayedGamemode);
                } else {
                    this.GamemodeSelection.SetSelection(_ => true);
                }

            }

        }

        private int OnGamemodeChanged(int current, int next, LobbyGamemodeItem item) {

            // Get options
            var options = item.Gamemode.Options;

            // Clear available options
            this.GamemodeOptionSelection.Items.Clear();

            // Hide options
            if (options is null || options.Length is 0) {

                // Set options to hidden
                this.GamemodeOptionSelection.IsVisible = false;

            } else {

                // Update options
                _ = options.ForEach(x => this.GamemodeOptionSelection.Items.Add(new(x)));

                // Set default
                if (!this.m_hasSetDefaults) {
                    this.GamemodeOptionSelection.SetSelection(x => x.Option.Value == BattlegroundsInstance.LastPlayedGamemodeSetting);
                    this.m_hasSetDefaults = true;
                } else {
                    var defaultOption = item.Gamemode.Options[item.Gamemode.DefaultOptionIndex];
                    this.GamemodeOptionSelection.SetSelection(x => x.Option == defaultOption);
                }

                // Set options to hidden
                this.GamemodeOptionSelection.IsVisible = true;

            }

            // Update lobby
            this.m_handle.SetLobbySetting(this.GamemodeSelection.DropdownID, item.Gamemode.Name);

            // Return selected
            return next;

        }

        private int OnGamemodeOptionChanged(int current, int next, LobbyGamemodeOptionItem item) {

            // Update lobby
            this.m_handle.SetLobbySetting(this.GamemodeOptionSelection.DropdownID, item.Option.Value.ToString(CultureInfo.InvariantCulture));

            // Return selected
            return next;

        }

        private void EvaluateMatchLaunchable() {

            // Skip check if not host
            if (!this.m_handle.IsHost) {
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
                this.StartMatch.Enabled = true;
                this.StartMatch.Tooltip = null;
            } else if (!allied) {
                this.StartMatch.Enabled = false;
                this.StartMatch.Tooltip = x1 ? __playabilityAlliesNoPlayers : __playabilityAlliesInvalid;
            } else {
                this.StartMatch.Enabled = false;
                this.StartMatch.Tooltip = x2 ? __playabilityAxisNoPlayers : __playabilityAxisInvalid;
            }


        }

        private void OnTeamChanged(LobbyAPIStructs.LobbyTeam team) {

            // Refresh allies
            if (team.TeamID == 0) {
                this.Allies?.RefreshTeam(team);
            }

            // Refresh axis
            if (team.TeamID == 1) {
                this.Axis?.RefreshTeam(team);
            }

            // Trigger self change
            if (this.m_handle.IsHost) {
                this.OnSelfChanged(); // Trigger a playability check
            }

        }

        private void OnSlotChanged(int teamID, LobbyAPIStructs.LobbySlot slot) {

            // Get team
            var team = teamID == 0 ? this.Allies : this.Axis;

            // Trigger slot update
            team.RefreshSlot(team.Slots[slot.SlotID], slot);

            // Trigger self change
            if (this.m_handle.IsHost) {
                this.OnSelfChanged(); // Trigger a playability check
            }

        }

        private void OnMemberChanged(int teamID, int slotID, LobbyAPIStructs.LobbyMember member) {

            // Get team
            var team = teamID == 0 ? this.Allies : this.Axis;

            // Get slot
            var slot = team.Slots[slotID];

            // Set occupant and refresh
            slot.Interface.Occupant = member;
            slot.RefreshVisuals();

            // Trigger self change
            if (this.m_handle.IsHost) {
                this.OnSelfChanged(); // Trigger a playability check
            }

        }

        private void OnCompanyChanged(int teamID, int slotID, LobbyAPIStructs.LobbyCompany company) {

            // Get team
            var team = teamID == 0 ? this.Allies : this.Axis;

            // Get slot
            var slot = team.Slots[slotID];

            // Verify there's an occupant
            if (slot.Interface.Occupant is null) {
                Trace.WriteLine("Failed to set company of null occupant - OnCompanyChanged", nameof(LobbyModel));
                return;
            }

            // Set company and refresh
            slot.Interface.Occupant.Company = company;
            slot.RefreshCompany();

            // Trigger self change
            if (this.m_handle.IsHost) {
                this.OnSelfChanged(); // Trigger a playability check
            }

        }

        private void OnSettingChanged(string key, string value) {

            // If host; do nothing
            if (this.m_handle.IsHost) {
                return;
            }

            // Invoke changes
            Application.Current.Dispatcher.Invoke(() => { 
                
                // Loop over settings
                for (int i = 0; i < this.m_settings.Length; i++) {
                    if (this.m_settings[i].DropdownID == key) {
                        this.m_settings[i].LabelContent = value;
                        
                        // Update scenario preview if that is what was changed
                        if (this.m_settings[i].DropdownID == this.ScenarioSelection.DropdownID) {
                            this.TrySetMapSource(ScenarioSelection.Items.Select(x => x.Scenario).FirstOrDefault(x => x.RelativeFilename == value));
                        }

                        return;
                    }
                }

                // Log missing k-v pair
                Trace.WriteLine($"Failed to set setting '{key}' to '{value}' as setting dropdown was not defined.", nameof(LobbyModel));

            });

        }

        private void OnConnectionLost(string reason) {

            // Decide on title and desc
            string modalTitle = reason switch {
                "KICK" => "Kicked from lobby",
                _ => "Connection lost"
            };
            string modalDesc = reason switch {
                "KICK" => "You were kicked from the lobby by the host",
                _ => "Connection to server was lost."
            };

            // Goto GUI thread and show connection lost.
            Application.Current.Dispatcher.Invoke(() => {

                // Show leave modal
                App.ViewManager.GetModalControl()?.ShowModal(ModalDialog.CreateModal(modalTitle, modalDesc, (sender, success, value) => {
                    if (success && value == ModalDialogResult.Confirm) {

                        // Go back to browser view
                        App.ViewManager.SetDisplay(AppDisplayState.LeftRight, typeof(LeftMenu), typeof(LobbyBrowserViewModel));

                    }
                }));

            });

        }

        public void SetChatModel(LobbyChatSpectatorModel chatModel)
            => this.m_chatModel = chatModel;

        public static LobbyModel CreateModelAsHost(LobbyAPI handler) {

            // Create model
            LobbyModel model = new(handler, handler.Allies, handler.Axis);

            // Return model
            return model;

        }

        public static LobbyModel CreateModelAsParticipant(LobbyAPI handler) {

            // Create model
            LobbyModel model = new(handler, handler.Allies, handler.Axis);
            model.m_handle.OnLobbySettingUpdate += model.OnSettingChanged;

            // Update settings
            foreach (var (k, v) in handler.Settings) {
                model.OnSettingChanged(k, v);
            }

            // Return model
            return model;

        }

    }

}
