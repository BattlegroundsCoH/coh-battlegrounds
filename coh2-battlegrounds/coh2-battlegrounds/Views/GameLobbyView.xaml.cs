using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
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
using Battlegrounds.Networking.Lobby;
using Battlegrounds.Online.Lobby;

using BattlegroundsApp.Controls.Lobby;
using BattlegroundsApp.Models;
using BattlegroundsApp.Resources;

namespace BattlegroundsApp.Views {

    /// <summary>
    /// Interaction logic for GameLobbyView.xaml
    /// </summary>
    public partial class GameLobbyView : ViewState, INotifyPropertyChanged {

        private class GameLobbyViewScenarioItem {
            public Scenario Scenario { get; }
            private string m_display;
            public GameLobbyViewScenarioItem(Scenario scenario) {
                this.Scenario = scenario;
                this.m_display = this.Scenario.Name;
                if (this.Scenario.Name.StartsWith("$") && uint.TryParse(this.Scenario.Name[1..], out uint key)) {
                    this.m_display = GameLocale.GetString(key);
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

        private void UpdateAvailableGamemodes(Scenario scenario) {
            Contract.Requires(scenario is not null, "Scenario cannot be null");

            // Keep track of old gamemode
            Wincondition currentGamemode = this.Gamemode.SelectedItem as Wincondition;
            int currentOption = this.GamemodeOption.SelectedIndex;

            // Set available gamemodes
            List<Wincondition> source = scenario.Gamemodes.Count > 0 ? scenario.Gamemodes : WinconditionList.GetDefaultList();
            if (source.All(this.Gamemode.Items.Contains)) { // if nore changes are made, just dont update
                return;
            }

            this.Gamemode.ItemsSource = source;
            int oldgamemode = this.Gamemode.Items.IndexOf(currentGamemode);
            this.Gamemode.SelectedIndex = oldgamemode == -1 ? 0 : oldgamemode;

            // If current gamemode was found
            if (this.Gamemode.SelectedItem is Wincondition wincon) {
                if (wincon.Options?.Length > 0) {
                    this.Gamemode.Visibility = Visibility.Visible;
                    if (oldgamemode != -1) {
                        this.Gamemode.SelectedIndex = currentOption;
                    } else {
                        this.GamemodeOption.SelectedIndex = wincon.DefaultOptionIndex;
                    }
                } else {
                    this.GamemodeOption.Visibility = Visibility.Hidden;
                }
            } else {
                Trace.WriteLine($"Failed to get wincondition list for scenario {scenario.RelativeFilename}", nameof(GameLobbyView));
            }

        }

        private void UpdateMapPreview(Scenario scenario) {
            Contract.Requires(scenario is not null, "Scenario cannot be null");

            // Get Path
            string fullpath = Path.GetFullPath($"bin\\gfx\\map_icons\\{scenario.RelativeFilename}_map.tga");

            // Check if file exists
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

            // If no image is set, set to unknown
            this.SetUnknownMapPreview();

        }

        private void SetUnknownMapPreview() => this.mapImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/unknown_map.png"));

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

            Scenario selectedScenario = (this.Map.SelectedItem as GameLobbyViewScenarioItem).Scenario;
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

        private void Map_SelectedItemChanged(object sender, SelectionChangedEventArgs e) {

            if (this.Map.State is OtherState) {
                return;
            }

            if (this.Map.SelectedItem is GameLobbyViewScenarioItem scenarioItem) {
                Scenario scenario = scenarioItem.Scenario;
                if (scenario is not null) {

                    // Update lobby data
                    HostedLobby lobby = this.m_handler.Lobby as HostedLobby;
                    lobby.SetMode(scenario.Name, null, null);

                    this.UpdateMapPreview(scenario);
                    this.UpdateAvailableGamemodes(scenario);
                }
            }

        }

        private void Gamemode_SelectedItemChanged(object sender, SelectionChangedEventArgs e) {

            // Do not update if not able to
            if (this.Gamemode.State is OtherState) {
                return;
            }

            // If gamemode is selected, update option
            if (this.Gamemode.SelectedItem is Wincondition wincon) {

                this.GamemodeOption.Visibility = (wincon.Options is null || wincon.Options.Length == 0) ? Visibility.Hidden : Visibility.Visible;
                this.GamemodeOption.ItemsSource = wincon.Options;
                this.GamemodeOption.SelectedIndex = wincon.DefaultOptionIndex;

                // Update lobby data
                HostedLobby lobby = this.m_handler.Lobby as HostedLobby;
                lobby.SetMode(null, wincon.Name, null);

            }

        }

        private void GamemodeOption_SelectedItemChanged(object sender, SelectionChangedEventArgs e) {

            // Do not update if not able to
            if (this.GamemodeOption.State is OtherState) {
                return;
            }

            // If valid option
            if (this.GamemodeOption.SelectedItem is WinconditionOption option) {

                // Update lobby data
                HostedLobby lobby = this.m_handler.Lobby as HostedLobby;
                lobby.SetMode(null, null, option.Title);

            }

        }

        private void UpdateStartMatchButton() => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CanStartMatch)));

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

        private void PopulateDropdowns() {

            // Get the scenarios and set source
            List<GameLobbyViewScenarioItem> scenarioSource = ScenarioList.GetList().Select(x => new GameLobbyViewScenarioItem(x)).ToList();
            this.Map.ItemsSource = scenarioSource;

            // If no mapp has been selected
            if (this.Map.SelectedIndex == -1) {

                // Find map to select
                int selectedScenario = scenarioSource.FindIndex(x => x.Scenario.RelativeFilename.CompareTo(BattlegroundsInstance.LastPlayedMap) == 0);
                this.Map.SelectedIndex = selectedScenario != -1 ? selectedScenario : 0;

            }

        }

        public void RefreshTeams(ManagedLobbyTaskDone overrideDone) { }

    }

}
