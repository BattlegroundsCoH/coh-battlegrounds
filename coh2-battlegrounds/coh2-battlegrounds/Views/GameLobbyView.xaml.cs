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
using BattlegroundsApp.Controls.Lobby.Chatting;
using BattlegroundsApp.LocalData;
using BattlegroundsApp.Models;
using BattlegroundsApp.Resources;
using BattlegroundsApp.Views.ViewComponent;

namespace BattlegroundsApp.Views {

    /// <summary>
    /// Interaction logic for GameLobbyView.xaml
    /// </summary>
    public partial class GameLobbyView : ViewState, INotifyPropertyChanged, IChatController {

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
        private ILobbyPlayModel m_playModel;

        private GameLobbyViewScenarioItem m_lastScenario;

        //private Task m_lobbyUpdate;
        private LobbyHandler m_handler;

        public event PropertyChangedEventHandler PropertyChanged;

        public string LobbyName => $"Game Lobby: {this.m_handler.Lobby.LobbyName}";

        public bool CanLeave => this.m_playModel is null;

        public bool CanStartMatch => this.m_handler.IsHost && this.IsLegalMatch();

        public LobbyTeamManagementModel TeamManager { get; private set; }

        public ChatMessageSent OnSend => this.OnSendChatMessage;

        public GameLobbyView(LobbyHandler handler) {

            // Init components
            this.InitializeComponent();

            // Set handler
            this.m_handler = handler;

        }

        private void StartGame_Click(object sender, RoutedEventArgs e) {

            if (!this.m_handler.IsHost) {
                return;
            }

            // Show a miss-click failsafe
            if (MessageBox.Show("Are you sure you want to start the match?", "Start Match?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {

                this.m_playModel = new LobbyHostPlayModel(this, this.m_handler);
                this.m_playModel.PlayGame(this.CancelGame);

            }

        }

        private void CancelGame() {

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

            // Get gamemode data
            int option = this.GamemodeOption.SelectedIndex;
            Wincondition selectedWincondition = this.Gamemode.SelectedItem as Wincondition;

            // Get scenario data
            Scenario selectedScenario = (this.Map.SelectedItem as GameLobbyViewScenarioItem).Scenario;
            if (selectedScenario is null) {
                // TODO: Handle
            }

            // Get team data
            List<SessionParticipant> alliedTeam = this.TeamManager.GetParticipants(LobbyTeamType.Allies);
            List<SessionParticipant> axisTeam = this.TeamManager.GetParticipants(LobbyTeamType.Axis);

            // Compile into session data
            SessionInfo sinfo = new SessionInfo() {
                SelectedGamemode = selectedWincondition,
                SelectedGamemodeOption = option,
                SelectedScenario = selectedScenario,
                IsOptionValue = false,
                SelectedTuningMod = new BattlegroundsTuning(), // TODO: Allow users to change this somewhere
                Allies = alliedTeam.ToArray(),
                Axis = axisTeam.ToArray(),
                FillAI = false,
                DefaultDifficulty = AIDifficulty.AI_Hard,
            };

            // Return session data
            return sinfo;

        }

        private void Map_SelectedItemChanged(object sender, SelectionChangedEventArgs e) {

            if (this.Map.State is OtherState) {
                return;
            }

            if (this.Map.SelectedItem is GameLobbyViewScenarioItem scenarioItem) {
                Scenario scenario = scenarioItem.Scenario;
                if (scenario is not null) {

                    // Get host
                    HostedLobby lobby = this.m_handler.Lobby as HostedLobby;

                    // Set capacity
                    if (lobby.SetCapacity(scenario.MaxPlayers)) {

                        // Update selected scenario
                        lobby.SetMode(scenario.Name, null, null);

                        // Set max players in team manager
                        this.TeamManager.SetMaxPlayers(scenario.MaxPlayers);

                        // Update preview and gamemodes
                        this.UpdateMapPreview(scenario);
                        this.UpdateAvailableGamemodes(scenario);

                        // Set last selection incase user picks unavailable scenario
                        this.m_lastScenario = scenarioItem;

                    } else {

                        // Reset selection
                        this.Map.SelectedItem = this.m_lastScenario;

                    }

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

        private bool IsLegalMatch() {

            // Get if allies are ready
            bool alliesPlayReady = this.TeamManager.All(LobbyTeamType.Allies, x => x.IsPlayReady());
            bool alliesAtLeastOnePlayer = this.TeamManager.Any(LobbyTeamType.Allies, 
                x => x.CardState is TeamPlayerCard.AISTATE or TeamPlayerCard.SELFSTATE or TeamPlayerCard.OBSERVERSTATE);

            bool allies = alliesAtLeastOnePlayer && alliesPlayReady;
            if (!allies) {
                return false;
            }

            // Get if axis are ready
            bool axisPlayReady = this.TeamManager.All(LobbyTeamType.Axis, x => x.IsPlayReady());
            bool axisAtLeastOnePlayer = this.TeamManager.Any(LobbyTeamType.Axis,
                x => x.CardState is TeamPlayerCard.AISTATE or TeamPlayerCard.SELFSTATE or TeamPlayerCard.OBSERVERSTATE);

            return axisPlayReady && axisAtLeastOnePlayer;

        }

        public Company GetLocalCompany() {
            
            TeamPlayerCard card = this.TeamManager.Self;
            if (card is null) {
                return null;
            }

            if (card.CompanySelector.SelectedItem is not TeamPlayerCompanyItem companyItem ) {
                return null;
            }

            // Get company
            Company company = companyItem.State == CompanyItemState.Company ? PlayerCompanies.FromNameAndFaction(companyItem.Name, Faction.FromName(companyItem.Army)) : null;
            return company is not null ? company : throw new Exception();

        }

        public override void StateOnFocus() {

            // Setup the lobby
            this.SetupLobby();

        }

        private void SetupLobby() {

            // Create card overview
            TeamPlayerCard[][] cards = new TeamPlayerCard[][]{
                new TeamPlayerCard[] { this.PlayerCard01, this.PlayerCard02, this.PlayerCard03, this.PlayerCard04 },
                new TeamPlayerCard[] { this.PlayerCard11, this.PlayerCard12, this.PlayerCard13, this.PlayerCard14 },
                new TeamPlayerCard[] { this.PlayerCard21, this.PlayerCard22, this.PlayerCard23, this.PlayerCard24 }
            };

            // Setup team management.
            this.TeamManager = new LobbyTeamManagementModel(cards, this.m_handler);
            this.TeamManager.RefreshAll(true);
            this.TeamManager.OnModelNotification += this.OnTeamManagerNotification;

            // Setup chat receiver
            this.m_handler.Lobby.ChatNotification = (channel, sender, message) => {
                string name = sender.Name; // Don't do this on the GUI thread in case of remoting!
                this.UpdateGUI(() => this.LobbyChat.DisplayMessage($"{name}: {message}", channel));
            };

            // Setup system receiver
            this.m_handler.Lobby.SystemNotification = systemInfo => {
                this.UpdateGUI(() => this.LobbyChat.DisplayMessage($"[System] {systemInfo}", 0));
            };

            // Set chat handler as self
            this.LobbyChat.Chat = this;

            // If host, setup everything
            if (this.m_handler.IsHost) {

                // Enable host mode (and because true, will update populate the dropdowns).
                this.EnableHostMode(true);

            } else {

                // lock everything
                this.EnableHostMode(false);

            }

        }

        private void OnSendChatMessage(int channel, string message) {
            if (channel is 0) {
                this.m_handler.Lobby.SendChatMessage(message);
            } else if (channel is 1) {
                /*this.m_handler.Lobby.SendTeamChatMessage(message);*/
            }
        }

        private void OnTeamManagerNotification() => this.UpdateGUI(() => {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanStartMatch)));
        });

        public override void StateOnLostFocus() {

        }

        public void EnableHostMode(bool hostMode) {

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
