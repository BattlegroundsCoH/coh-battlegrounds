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

    public class LobbyHostModel : LobbyModel, INotifyPropertyChanged {

        private ModPackage? m_package;
        private bool m_hasSetDefaults;

        public event PropertyChangedEventHandler? PropertyChanged;

        public override LobbyButton StartMatchButton { get; }

        public ImageSource? SelectedMatchScenario { get; set; }

        public LobbyHostModel(LobbyAPI handle, LobbyAPIStructs.LobbyTeam allies, LobbyAPIStructs.LobbyTeam axis) : base(handle, allies, axis) {

            // Init buttons
            this.StartMatchButton = new(true, new(this.BeginMatchSetup));

            // Add handlers to remote updates and notifications
            /*this.m_handle.OnLobbySelfUpdate += this.OnSelfChanged;
            this.m_handle.OnLobbyTeamUpdate += this.OnTeamChanged;
            this.m_handle.OnLobbyCompanyUpdate += this.OnCompanyChanged;
            this.m_handle.OnLobbySlotUpdate += this.OnSlotChanged;
            this.m_handle.OnLobbyConnectionLost += this.OnConnectionLost;
            this.m_handle.OnLobbyRequestCompany += this.OnCompanyRequested;
            this.m_handle.OnLobbyNotifyGamemode += this.OnGamemodeReleased;
            this.m_handle.OnLobbyNotifyResults += this.OnResultsReleased;
            this.m_handle.OnLobbyLaunchGame += this.OnLaunchGame;
            this.m_handle.OnLobbyBeginMatch += this.OnMatchBegin;
            this.m_handle.OnLobbyCancelStartup += this.OnMatchStartupCancelled;*/

        }

        private void OnSelfChanged() {
            Application.Current.Dispatcher.Invoke(() => {

            });
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
                Trace.WriteLine($"Failed to set **null** scenario (Caller = {caller}).", nameof(LobbyHostModel));
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

    }

}
