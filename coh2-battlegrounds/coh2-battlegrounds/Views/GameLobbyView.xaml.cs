using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using Battlegrounds;
using Battlegrounds.Game;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match;
using Battlegrounds.Modding;
using Battlegrounds.Networking;
using Battlegrounds.Networking.Lobby;
using Battlegrounds.Online.Lobby;

using BattlegroundsApp.Controls.Lobby;
using BattlegroundsApp.Dialogs.YesNo;
using BattlegroundsApp.LocalData;
using BattlegroundsApp.Models;
using BattlegroundsApp.Resources;
using BattlegroundsApp.Views.ViewComponent;

namespace BattlegroundsApp.Views {

    /// <summary>
    /// Interaction logic for GameLobbyView.xaml
    /// </summary>
    public partial class GameLobbyView : ViewState, INotifyPropertyChanged {

        private class GameLobbyViewScenarioItem {
            private Scenario m_scenario;
            public Scenario Scenario => this.m_scenario;
            private string m_display;
            public GameLobbyViewScenarioItem(Scenario scenario) {
                this.m_scenario = scenario;
                this.m_display = this.m_scenario.Name;
                if (this.m_scenario.Name.StartsWith("$")) {
                    if (uint.TryParse(this.m_scenario.Name[1..], out uint key)) {
                        this.m_display = GameLocale.GetString(key);
                    }
                }
            }
            public override string ToString() => this.m_display;
        }

        //private bool m_hasCreatedLobbyOnce;
        //private ILobbyPlayModel m_playModel;
        //private ServerMessageHandler m_smh;
        private LobbyTeamManagementModel m_teamManagement;
        //private LobbyGamemodeModel m_gamemode;
        //private Task m_lobbyUpdate;
        private LobbyHandler m_handler;

        private volatile bool m_updateLobby;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool CanLeave => true;

        public bool CanStartMatch => this.m_handler.IsHost && this.IsLegalMatch();

        public LobbyTeamManagementModel TeamManager => this.m_teamManagement;

        public GameLobbyView(LobbyHandler handler) {

            // Init components
            this.InitializeComponent();

            // Set handler
            this.m_handler = handler;

        }

        private void StartGame_Click(object sender, RoutedEventArgs e) {

        }

        private void LeaveLobby_Click(object sender, RoutedEventArgs e) {

            // Leave lobby
            this.m_handler.Lobby.Leave();

            // Change state
            if (this.StateChangeRequest.Invoke(MainWindow.GAMEBROWSERSTATE) is false) {
                Trace.WriteLine("Somehow failed to change state", nameof(GameLobbyView)); // TODO: Better error handling
            }

        }

        private void UpdateAvailableGamemodes() {

        }

        private void Map_SelectedItemChanged(object sender, SelectionChangedEventArgs e) {
            if (this.Map.State is OtherState) {
                return;
            }
            if (this.Map.SelectedItem is GameLobbyViewScenarioItem scenarioItem) {
                Scenario scenario = scenarioItem.Scenario;
                if (scenario is not null) {
                    HostedLobby lobby = this.m_handler.Lobby as HostedLobby;
                    lobby.SetMode(scenario.Name, null, null);
                    this.UpdateMapPreview(scenario);
                }
            }
        }

        private void UpdateMapPreview(Scenario scenario) {
            
            // Verify contract
            Contract.Requires(scenario is not null, "Scenario cannot be null");

            string fullpath = Path.GetFullPath($"bin\\gfx\\map_icons\\{scenario.RelativeFilename}_map.tga");
            if (File.Exists(fullpath)) {
                try {
                    this.mapImage.Source = TgaImageSource.TargaBitmapSourceFromFile(fullpath);
                    return;
                } catch (BadImageFormatException bife) {
                    Trace.WriteLine(bife, "GameLobbyView@UpdateMapPreview");
                }
            } else {
                fullpath = Path.GetFullPath($"usr\\mods\\map_icons\\{scenario.RelativeFilename}_map.tga");
                if (File.Exists(fullpath)) {
                    try {
                        this.mapImage.Source = TgaImageSource.TargaBitmapSourceFromFile(fullpath);
                        return;
                    } catch (BadImageFormatException bife) {
                        Trace.WriteLine(bife, "GameLobbyView@UpdateMapPreview");
                    }
                } else {
                    Trace.WriteLine($"Failed to locate file: {fullpath}");
                }
            }

            this.mapImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/unknown_map.png"));

        }

        public SessionInfo CreateSessionInfo() {

            int option = 0;
            Wincondition selectedWincondition = null;
            /*if (this.m_gamemode.GetGamemode()) {
                selectedWincondition = this.m_gamemode.Wincondition;
                option = this.m_gamemode.GamemodeOptionIndex;
            } else {
                selectedWincondition = WinconditionList.GetWinconditionByName(WinconditionList.VictoryPoints);
                option = selectedWincondition.DefaultOptionIndex;
                Trace.WriteLine("Failed to set gamemode", "GameLobbyView");
            }*/

            //this.m_gamemode.SaveDefaults();

            Scenario selectedScenario = (Map.SelectedItem as GameLobbyViewScenarioItem).Scenario;
            if (selectedScenario is null) {
                // TODO: Handle
            }

            List<SessionParticipant> alliedTeam = this.m_teamManagement.GetParticipants(ManagedLobbyTeamType.Allies);
            List<SessionParticipant> axisTeam = this.m_teamManagement.GetParticipants(ManagedLobbyTeamType.Axis);

            SessionInfo sinfo = new SessionInfo() {
                SelectedGamemode = selectedWincondition,
                SelectedGamemodeOption = option,
                SelectedScenario = selectedScenario,
                IsOptionValue = true,
                SelectedTuningMod = new BattlegroundsTuning(), // TODO: Allow users to change this somewhere
                Allies = alliedTeam.ToArray(),
                Axis = axisTeam.ToArray(),
                FillAI = false,
                DefaultDifficulty = AIDifficulty.AI_Hard,
            };

            return sinfo;

        }

        private void UpdateGamemodeOptions(Wincondition wc) {
        }

        private void Gamemode_SelectedItemChanged(object sender, SelectionChangedEventArgs e) {
            if (Gamemode.State is OtherState) {
                return;
            }
            if (Gamemode.SelectedItem is Wincondition wincon) {
                this.UpdateGamemodeOptions(wincon);
            }
        }

        private void GamemodeOption_SelectedItemChanged(object sender, SelectionChangedEventArgs e) {
            if (GamemodeOption.State is OtherState) {
                return;
            }
            if (GamemodeOption.SelectedItem is WinconditionOption) {
            }
        }

        private void UpdateStartMatchButton() => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanStartMatch)));

        private bool IsLegalMatch()
            => false;

        public Company GetLocalCompany() {
            /*var card = this.m_teamManagement.GetLocalPlayercard();
            if (card is not null) {
                return PlayerCompanies.FromNameAndFaction(card.PlayerSelectedCompanyItem.Name, Faction.FromName(card.PlayerArmy));
            } else {
                */return null;
            //}
        }

        public override void StateOnFocus() {

            // Setup the lobby
            this.SetupLobby();

        }

        private void SetupLobby() {

            // If host, setup everything
            if (this.m_handler.IsHost) {

                // Enable host mode (and because true, will update populate the dropdowns).
                this.EnableHostMode(true);

            } else {

                // lock everything
                this.EnableHostMode(false);

            }

        }

        public override void StateOnLostFocus() {

        }

        public void EnableHostMode(bool hostMode) {

            // Set team management
            this.m_teamManagement?.SetIsHost(hostMode);

            // Enable or disable game settings (AI and ID properties not relevant to these elements)
            this.Map.SetStateBasedOnContext(hostMode, hostMode, 0);
            this.Gamemode.SetStateBasedOnContext(hostMode, hostMode, 0);
            this.GamemodeOption.SetStateBasedOnContext(hostMode, hostMode, 0);

            // If host-mode is enabled, populate the dropdowns
            if (hostMode) {
                this.PopulateDropdowns();
            }

        }

        public void RefreshGameSettings() {

        }

        private void PopulateDropdowns() {

            // Get the scenarios and set source
            var scenarioSource = ScenarioList.GetList().Select(x => new GameLobbyViewScenarioItem(x)).ToList();
            this.Map.ItemsSource = scenarioSource;

            // If no mapp has been selected
            if (this.Map.SelectedIndex == -1) {

                // Find map to select
                int selectedScenario = scenarioSource.FindIndex(x => x.Scenario.RelativeFilename.CompareTo(BattlegroundsInstance.LastPlayedMap) == 0);
                this.Map.SelectedIndex = selectedScenario != -1 ? selectedScenario : 0;

                // Get selected scenario and update lobby accordingly
                var scen = (this.Map.SelectedItem as GameLobbyViewScenarioItem).Scenario;

                // Update available gamemodes
                this.UpdateAvailableGamemodes();

            }

        }

        public void RefreshTeams(ManagedLobbyTaskDone overrideDone) { }

    }

}
