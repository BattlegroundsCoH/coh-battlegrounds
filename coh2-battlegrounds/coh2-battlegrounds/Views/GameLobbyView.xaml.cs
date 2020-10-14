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
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Steam;
using BattlegroundsApp.Models;
using BattlegroundsApp.Views.ViewComponent;

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
            this.m_teamManagement.OnTeamEvent += this.OnTeamManagementCallbackHandler;

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
                    var def = WinconditionList.GetDefaultList().OrderBy(x => x.ToString()).ToList();
                    Gamemode.ItemsSource = def;
                    Gamemode.SelectedIndex = def.FindIndex(x => x.Name.CompareTo("Victory Points") == 0);
                }

            }

        }

        private void Map_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (Map.SelectedItem is Scenario scenario) {
                if (scenario.MaxPlayers < this.m_smh.AppLobby.LobbyPlayers) {
                    // TODO: Do something
                }
                this.UpdateAvailableGamemodes();
                this.m_teamManagement.SetMaxPlayers(scenario.MaxPlayers);
                this.m_smh.AppLobby.SetMap(this.m_smh, scenario);
            }
        }

        public SessionInfo CreateSessionInfo() {

            Wincondition selectedWincondition = WinconditionList.GetWinconditionByName(Gamemode.SelectedItem as string);
            if (selectedWincondition is null) {
                // TODO: Handle
            }

            Scenario selectedScenario = Map.SelectedItem as Scenario;
            if (selectedScenario is null) {
                // TODO: Handle
            }

            List<SessionParticipant> alliedTeam = this.m_teamManagement.GetParticipants(Lobby.LobbyTeam.TeamType.Allies);
            List<SessionParticipant> axisTeam = this.m_teamManagement.GetParticipants(Lobby.LobbyTeam.TeamType.Axis);

            SessionInfo sinfo = new SessionInfo() {
                SelectedGamemode = selectedWincondition,
                SelectedGamemodeOption = 1,
                SelectedScenario = selectedScenario,
                SelectedTuningMod = new BattlegroundsTuning(),
                Allies = alliedTeam.ToArray(),
                Axis = axisTeam.ToArray(),
                FillAI = false,
                DefaultDifficulty = AIDifficulty.AI_Hard,
            };

            return sinfo;

        }

        private async void UpdateLobby() {
            while (true && this is not null) {
                if (this.m_smh.Lobby.IsConnectedToServer) {
                    this.m_smh.AppLobby.UpdateLobby(this.m_smh, this.UpdateLobbyVisuals);
                    await Task.Delay(1500);
                } else {
                    break;
                }
            }
        }

        private void UpdateLobbyVisuals(Lobby lobby) {
            this.UpdateGUI(() => {
                this.m_teamManagement.UpdateTeamview(lobby, this.m_smh.Lobby.IsHost);
            });
        }

        private void OnTeamManagementCallbackHandler(Lobby.LobbyTeam.TeamType team, PlayercardView card, int teamPos, string reason) {

            switch (reason) {
                case "AddAI":
                    this.m_smh.AppLobby.AddAI(this.m_smh, team, card.Difficulty, teamPos, card.Playerarmy);
                    Trace.WriteLine($"Adding AI [{team}][{card.Difficulty}][{card.Playerarmy}]", "GameLobbyView");
                    break;
                case "ChangeArmy":

                    break;
                case "ChangedCompany":

                    break;
                default:
                    break;
            }

        }

    }

}
