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
using Battlegrounds.Modding;
using Battlegrounds.Online.Lobby;

using BattlegroundsApp.LocalData;
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

        private void OnStartMatchCancelled(string reason) => Trace.WriteLine(reason);

        private void LeaveLobby_Click(object sender, RoutedEventArgs e) {

            if (this.m_smh.Lobby != null) {
                this.m_smh.LeaveLobby();
                this.m_hostWindow.SetView(new GameBrowserView(m_hostWindow));
            }

        }

        public void UpdateGUI(Action a) {
            try {
                this.Dispatcher.Invoke(a);
            } catch (ObjectDisposedException) {

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

                var scen = Map.SelectedItem as Scenario;
                this.m_smh.Lobby.SetMap(scen);
                this.m_smh.Lobby.SetLobbyCapacity(scen.MaxPlayers);

                this.UpdateAvailableGamemodes();

            } else {

                // lock everything

                throw new NotImplementedException(); // TODO: Fetch lobby data

            }
            this.UpdateLobbyVisuals();
            this.m_lobbyUpdate.Start();
        }

        private void UpdateAvailableGamemodes() {

            if (Map.SelectedItem is Scenario scenario) {

                if (scenario.Gamemodes.Count > 0) {
                    Gamemode.ItemsSource = scenario.Gamemodes.OrderBy(x => x.ToString()).ToList();
                    Gamemode.SelectedIndex = 0;
                } else {
                    var def = WinconditionList.GetDefaultList().OrderBy(x => x.ToString()).ToList();
                    Gamemode.ItemsSource = def;
                    Gamemode.SelectedIndex = def.FindIndex(x => x.Name.CompareTo("Victory Points") == 0);
                }

                this.UpdateGamemodeOptions(Gamemode.SelectedItem as Wincondition);

            }

        }

        private void Map_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (Map.SelectedItem is Scenario scenario) {
                if (scenario.RelativeFilename.CompareTo(this.m_smh.Lobby.SelectedMap) != 0) {
                    if (scenario.MaxPlayers < this.m_smh.Lobby.PlayerCount) {
                        // Do something
                    }
                    this.UpdateAvailableGamemodes();
                    this.m_teamManagement.SetMaxPlayers(scenario.MaxPlayers);
                    this.m_smh.Lobby.SetMap(scenario);
                    this.m_smh.Lobby.SetLobbyCapacity(scenario.MaxPlayers);
                }
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

            List<SessionParticipant> alliedTeam = this.m_teamManagement.GetParticipants(ManagedLobbyTeamType.Allies);
            List<SessionParticipant> axisTeam = this.m_teamManagement.GetParticipants(ManagedLobbyTeamType.Axis);

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
                    this.UpdateLobbyVisuals();
                    await Task.Delay(1500);
                } else {
                    break;
                }
            }
        }

        private void UpdateLobbyVisuals() {
            this.UpdateGUI(() => {
                this.m_teamManagement.UpdateTeamview(this.m_smh.Lobby, this.m_smh.Lobby.IsHost);
            });
        }

        private void OnTeamManagementCallbackHandler(ManagedLobbyTeamType team, PlayercardView card, object arg, string reason) {
            switch (reason) {
                case "AddAI":
                    card.IsRegistered = false;
                    if (this.m_smh.Lobby.CreateAIPlayer(card.Difficulty, card.Playerarmy, team)) {
                        card.IsRegistered = true;
                        Trace.WriteLine($"Adding AI [{team}][{card.Difficulty}][{card.Playerarmy}]", "GameLobbyView");
                    } else {
                        Trace.WriteLine("Failed to add AI...");
                        card.SetCardState(PlayercardViewstate.Open);
                    }
                    break;
                case "ChangedArmy":
                    if (!card.IsRegistered) {
                        break;
                    }
                    if (card.Playerarmy.CompareTo(this.m_smh.Lobby.TryFindPlayerFromID(card.Playerid)?.Faction) != 0) {
                        this.m_smh.Lobby.SetFaction(card.Playerid, card.Playerarmy);
                        Trace.WriteLine($"Changing faction [{card.Playerid}][{card.Difficulty}][{card.Playerarmy}]", "GameLobbyView");
                    }
                    break;
                case "ChangedCompany":
                    if (!card.IsRegistered) {
                        break;
                    }
                    if (card.Playercompany.CompareTo(this.m_smh.Lobby.TryFindPlayerFromID(card.Playerid)?.CompanyName) != 0) {
                        if (card.IsAI || card.Playerid == this.m_smh.Lobby.Self.ID) {
                            PlayercardCompanyItem companyItem = (PlayercardCompanyItem)arg;
                            if (companyItem.State == PlayercardCompanyItem.CompanyItemState.Company && card.Playerid == this.m_smh.Lobby.Self.ID) {
                                Company company = PlayerCompanies.FromNameAndFaction(companyItem.Name, Faction.FromName(card.Playerarmy));
                                if (company is not null) {
                                    this.m_smh.Lobby.SetCompany(company);
                                    Trace.WriteLine($"Changing company [{card.Playerid}][{card.Difficulty}][{card.Playerarmy}][{card.Playercompany}]", "GameLobbyView");
                                } else {
                                    throw new NotImplementedException();
                                }
                            } else if (companyItem.State == PlayercardCompanyItem.CompanyItemState.Generate && card.IsAI) {
                                this.m_smh.Lobby.SetCompany(card.Playerid, "AUGEN", -1.0);
                                Trace.WriteLine($"Changing company [{card.Playerid}][{card.Difficulty}][{card.Playerarmy}][Auto-generated]", "GameLobbyView");
                            } else {
                                this.m_smh.Lobby.SetCompany(card.Playerid, "NULL", -1.0);
                                Trace.WriteLine($"Changing company [{card.Playerid}][{card.Difficulty}][{card.Playerarmy}][{card.Playercompany}]", "GameLobbyView");
                            }
                        }
                    }
                    break;
                case "RemovePlayer":
                    card.IsRegistered = false;
                    this.m_smh.Lobby.RemovePlayer(card.Playerid, true);
                    Trace.WriteLine($"Removing player [{team}][{arg}][{card.Difficulty}][{card.Playerarmy}]", "GameLobbyView");
                    break;
                default:
                    break;
            }
            this.UpdateStartMatchButton();
        }

        private void UpdateGamemodeOptions(Wincondition wc) {
            if (wc.Options is not null) {
                GamemodeOption.Visibility = Visibility.Visible;
                GamemodeOption.ItemsSource = wc.Options.OrderBy(x => x.Value).ToList();
                GamemodeOption.SelectedIndex = wc.DefaultOptionIndex;
            } else {
                GamemodeOption.Visibility = Visibility.Collapsed;
            }
        }

        private void Gamemode_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (Gamemode.SelectedItem is Wincondition wincon) {
                this.m_smh.Lobby.SetGamemode(wincon.Name);
                this.UpdateGamemodeOptions(wincon);
            }
        }

        private void GamemodeOption_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (GamemodeOption.SelectedItem is WinconditionOption) {
                this.m_smh.Lobby.SetGamemodeOption(GamemodeOption.SelectedIndex);
            }
        }

        private void UpdateStartMatchButton() => StartGameBttn.IsEnabled = this.IsLegalMatch();

        private bool IsLegalMatch() 
            => this.m_teamManagement.GetTeamSize(ManagedLobbyTeamType.Allies) > 0 && this.m_teamManagement.GetTeamSize(ManagedLobbyTeamType.Axis) > 0;

    }

}
