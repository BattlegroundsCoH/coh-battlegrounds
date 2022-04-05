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
using BattlegroundsApp.Modals.Dialogs.MVVM.Models;
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

        private static readonly LocaleKey __leaveTitle = new("LobbyView_DialogLeaveTitle");
        private static readonly LocaleKey __leaveDesc = new("LobbyView_DialogLeaveDesc");

        private readonly LobbyAPI m_handle;
        private LobbyChatSpectatorModel? m_chatModel;
        private ModPackage? m_package;
        private bool m_hasSetDefaults;

        private bool m_hasDownloadedGamemode = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ImageSource? SelectedMatchScenario { get; set; }

        public LobbyTeam Allies { get; }

        public LobbyTeam Axis { get; }

        public bool SingleInstanceOnly => false;

        public string LobbyTitle { get; }

        private LobbyModel(LobbyAPI handle, LobbyAPIStructs.LobbyTeam allies, LobbyAPIStructs.LobbyTeam axis) {

            // Set handler
            this.m_handle = handle;

            // Set title
            this.LobbyTitle = handle.Title;

            // Create teams
            this.Allies = new(allies);
            this.Axis = new(axis);

            // Add handlers to remote updates and notifications
            this.m_handle.OnLobbySelfUpdate += this.OnSelfChanged;
            this.m_handle.OnLobbyTeamUpdate += this.OnTeamChanged;
            this.m_handle.OnLobbyCompanyUpdate += this.OnCompanyChanged;
            this.m_handle.OnLobbySlotUpdate += this.OnSlotChanged;
            this.m_handle.OnLobbyConnectionLost += this.OnConnectionLost;
            this.m_handle.OnLobbyRequestCompany += this.OnCompanyRequested;
            this.m_handle.OnLobbyNotifyGamemode += this.OnGamemodeReleased;
            this.m_handle.OnLobbyNotifyResults += this.OnResultsReleased;
            this.m_handle.OnLobbyLaunchGame += this.OnLaunchGame;
            this.m_handle.OnLobbyBeginMatch += this.OnMatchBegin;
            this.m_handle.OnLobbyCancelStartup += this.OnMatchStartupCancelled;

        }

        private void OnMatchStartupCancelled(LobbyMatchStartupCancelledEventArgs e) {

            // Inform user
            if (this.m_chatModel is not null) {
                this.m_chatModel.SystemMessage($"{e.CancelName} has cancelled the match.", Colors.Gray);
            }

            // Invoke on GUI
            Application.Current.Dispatcher.Invoke(() => {

                // Allow exit lobby
                //this.ExitLobby.Enabled = true;

            });

        }

        private void OnMatchBegin() {

            // Inform user
            if (this.m_chatModel is not null) {
                this.m_chatModel.SystemMessage($"The match is starting", Colors.Gray);
            }

            // Invoke on GUI
            Application.Current.Dispatcher.Invoke(() => {

                // Allow exit lobby
                //this.ExitLobby.Enabled = false;

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

                        // Load it
                        var company = CompanySerializer.GetCompanyFromJson(Encoding.UTF8.GetString(data));

                        // Save it
                        PlayerCompanies.SaveCompany(company);

                    } else {
                        Trace.WriteLine($"Failed to download company results!", nameof(LobbyModel));
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
                Trace.WriteLine("Starting download of gamemode.", nameof(LobbyModel));

                // Download
                obj.DownloadGamemode((status, data) => {

                    if (status is DownloadResult.DOWNLOAD_SUCCESS) {

                        // File sga to gamemode file
                        File.WriteAllBytes(WinconditionCompiler.GetArchivePath(), data);

                        // Set as true
                        this.m_hasDownloadedGamemode = true;

                        // Inform user
                        if (this.m_chatModel is not null) {
                            this.m_chatModel.SystemMessage($"Gamemode downloaded", Colors.Gray);
                        }

                    } else {

                        // Inform user
                        if (this.m_chatModel is not null) {
                            this.m_chatModel.SystemMessage($"Failed to download gamemode", Colors.DarkRed);
                        }

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

                // Get company
                var company = PlayerCompanies.FromNameAndFaction(companyName, faction);
                if (company is null) {
                    Trace.WriteLine($"Failed to upload company json file (Company '{companyName}' not found).", nameof(LobbyModel));
                    return;
                }

                // Get company json
                string companyJson = CompanySerializer.GetCompanyAsJson(company, indent: false);
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

            });
        }

        private void LeaveLobby() {

            // Null check
            if (App.ViewManager.GetModalControl() is not ModalControl mControl) {
                return;
            }

            // Lookup strings
            string title = BattlegroundsInstance.Localize.GetString(__leaveTitle);
            string desc = BattlegroundsInstance.Localize.GetString(__leaveDesc);

            // Do modal
            YesNoDialogViewModel.ShowModal(mControl, (vm, resault) => {

                // Check return value
                if (resault is not ModalDialogResult.Confirm) {
                    return;
                }

                // Leave lobby
                Task.Run(this.m_handle.Disconnect);

                // Go back to browser view
                App.ViewManager.SetDisplay(AppDisplayState.LeftRight, typeof(LeftMenu), typeof(LobbyBrowserViewModel));

            }, title, desc);

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

            // Disallow exit lobby
            //this.ExitLobby.Enabled = false;

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

            // Allow exit lobby
            //this.ExitLobby.Enabled = true;

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

        private void OnTeamChanged(LobbyAPIStructs.LobbyTeam team) {

            // Refresh allies
            if (team.TeamID == 0) {
                //this.Allies?.RefreshTeam(team);
            }

            // Refresh axis
            if (team.TeamID == 1) {
                //this.Axis?.RefreshTeam(team);
            }

            // Trigger self change
            if (this.m_handle.IsHost) {
                this.OnSelfChanged(); // Trigger a playability check
            }

        }

        private void OnSlotChanged(LobbyAPIStructs.LobbySlot slot) {

            // Get team
            var team = slot.TeamID == 0 ? this.Allies : this.Axis;

            // Trigger slot update
            //team.RefreshSlot(team.Slots[slot.SlotID], slot);

            // Trigger self change
            if (this.m_handle.IsHost) {
                this.OnSelfChanged(); // Trigger a playability check
            }

        }

        private void OnCompanyChanged(int teamID, int slotID, LobbyAPIStructs.LobbyCompany company) {

        }

        private void OnSettingChanged(string key, string value) {

        }

        private string GetRemoteSettingValue(string key, string value) => key switch {
            "selected_map" => GameLocale.GetString(ScenarioList.ScenarioNameFromRelativeFilename(value)),
            _ => value
        };

        private void OnConnectionLost(string reason) {

            // Null check
            if (App.ViewManager.GetModalControl() is not ModalControl mControl) {
                return;
            }

            // Decide on title
            string modalTitle = BattlegroundsInstance.Localize.GetString(reason switch {
                "KICK" => "LobbyView_DialogKickTitle",
                "CLOSED" => "LobbyView_DialogCloseTitle",
                _ => "LobbyView_DialogLostTitle"
            });

            // Decide on desc
            string modalDesc = BattlegroundsInstance.Localize.GetString(reason switch {
                "KICK" => "LobbyView_DialogKickDesc",
                "CLOSED" => "LobbyView_DialogCloseDesc",
                _ => "LobbyView_DialogLostDesc"
            });

            // Goto GUI thread and show connection lost.
            Application.Current.Dispatcher.Invoke(() => {

                // Do modal
                OKDialogViewModel.ShowModal(mControl, (vm, resault) => {

                    // Check return value
                    if (resault is not ModalDialogResult.Confirm) {
                        return;
                    }

                    // Go back to browser view
                    App.ViewManager.SetDisplay(AppDisplayState.LeftRight, typeof(LeftMenu), typeof(LobbyBrowserViewModel));

                }, modalTitle, modalDesc);

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

        public bool UnloadViewModel() => true;

    }

}
