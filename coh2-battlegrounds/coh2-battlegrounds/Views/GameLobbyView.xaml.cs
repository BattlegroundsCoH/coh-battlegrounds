using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Battlegrounds;
using Battlegrounds.Game;
using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Game.Database;
using Battlegrounds.Steam;
using BattlegroundsApp.Models;

namespace BattlegroundsApp.Views {
    /// <summary>
    /// Interaction logic for GameLobbyView.xaml
    /// </summary>
    public partial class GameLobbyView : UserControl {

        ServerMessageHandler m_smh;

        private MainWindow m_hostWindow;
        private Task m_lobbyUpdate;
        private LobbyTeamManagementModel m_teamManagement;

        public event Action<bool, GameLobbyView> OnServerAcceptanceResponse;

        public GameLobbyView(MainWindow hostWindow) {

            this.m_hostWindow = hostWindow;
            this.m_lobbyUpdate = new Task(this.UpdateLobby);

            InitializeComponent();

            this.m_teamManagement = new LobbyTeamManagementModel(this.TeamGridview);

        }

        public void SetSMH(ServerMessageHandler smh) => this.m_smh = smh;

        private void SendMessage_Click(object sender, RoutedEventArgs e) {

            string messageContent = messageText.Text;
            string messageSender = BattlegroundsInstance.LocalSteamuser.Name;

            string message = $"{messageSender}: {messageContent}";

            lobbyChat.Text += $"{message}\n";
            lobbyChat.ScrollToEnd();

            messageText.Clear();

            // Send message to server (so other players can see)
            this.m_smh.Lobby.SendChatMessage(messageContent);

        }

        private void ChangeTeam_Click(object sender, RoutedEventArgs e) {

           // TODO: 

        }

        private void StartGame_Click(object sender, RoutedEventArgs e) => this.m_smh.Lobby.CompileAndStartMatch(this.OnStartMatchCancelled);

        private void OnStartMatchCancelled(string reason) {
            Trace.WriteLine(reason);
        }

        private void LeaveLobby_Click(object sender, RoutedEventArgs e) {

            if (this.m_smh.Lobby != null) {
                this.m_smh.LeaveLobby();
                this.m_hostWindow.SetView(new GameBrowserView(m_hostWindow));
            }

        }

        public void UpdateGUI(Action a) {
            try {
                this.Dispatcher.Invoke(a);
            } catch {

            }
        }

        public void ServerConnectResponse(bool connected) {
            this.OnServerAcceptanceResponse?.Invoke(connected, this);
            if (!connected) {
                this.m_hostWindow.SetView(new GameBrowserView(this.m_hostWindow));
                MessageBox.Show("An unexpected server error occured and it was not possible to join the lobby.", "Server Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            } else {
                this.UpdateGUI(this.CreateLobbyData);
            }
        }

        private void CreateLobbyData() {
            if (this.m_smh.Lobby.IsHost) {

                var scenarioSource = ScenarioList.GetList().OrderBy(x => x.ToString()).ToList();
                Map.ItemsSource = scenarioSource;

                int selectedScenario = scenarioSource.FindIndex(x => x.RelativeFilename.CompareTo(BattlegroundsInstance.LastPlayedMap) == 0);
                Map.SelectedIndex = selectedScenario != -1 ? selectedScenario : 0;

                this.UpdateAvailableGamemodes();

                this.m_smh.AppLobby.CreateHost(BattlegroundsInstance.LocalSteamuser);

            } else {

                // lock everything

                throw new NotImplementedException(); // TODO: Fetch lobby data

            }
            this.UpdateLobbyVisuals(this.m_smh.AppLobby);
            this.m_lobbyUpdate.Start();
        }

        private void UpdateAvailableGamemodes() {

            if (Map.SelectedItem is Scenario scenario) {

                if (scenario.Gamemodes.Count > 0) {
                    Gamemode.ItemsSource = scenario.Gamemodes;
                    Gamemode.SelectedIndex = 0;
                } else {
                    Gamemode.ItemsSource = WinconditionList.GetDefaultList().OrderBy(x => x.ToString()).ToList();
                    Gamemode.SelectedIndex = 0;
                }

            }

        }

        private void Map_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (Map.SelectedItem is Scenario scenario) {
                if (scenario.MaxPlayers < this.m_smh.AppLobby._lobbyPlayers) {
                    // TODO: Do something
                }
                this.UpdateAvailableGamemodes();
                this.m_teamManagement.SetMaxPlayers(scenario.MaxPlayers);
            }
        }

        public SessionInfo CreateSessionInfo() {

            SessionInfo sinfo = new SessionInfo() {
                SelectedGamemode = WinconditionList.GetWinconditionByName("Victory Points"),
                SelectedGamemodeOption = 1,
                SelectedScenario = ScenarioList.FromFilename("2p_angoville_farms"),
                SelectedTuningMod = new BattlegroundsTuning(),
                Allies = new SessionParticipant[] { new SessionParticipant(SteamUser.FromID(76561198003529969UL), null, 0, 0) }, // We'll have to solve participant setup later
                Axis = new SessionParticipant[] { new SessionParticipant(SteamUser.FromID(76561198157626935UL), null, 0, 0) },
                FillAI = false,
                DefaultDifficulty = AIDifficulty.AI_Hard,
            };

            return sinfo;

        }

        private async void UpdateLobby() {
            while (true && this is not null) {
                this.m_smh.AppLobby.UpdateLobby(this.m_smh, this.UpdateLobbyVisuals);
                await Task.Delay(750);
            }
        }

        private void UpdateLobbyVisuals(Lobby lobby) {
            this.UpdateGUI(() => {
                this.m_teamManagement.UpdateTeamview(lobby);
            });
        }

    }

}
