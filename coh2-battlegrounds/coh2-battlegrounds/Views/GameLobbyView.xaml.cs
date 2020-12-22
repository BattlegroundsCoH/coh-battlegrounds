using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using BattlegroundsApp.Resources;
using BattlegroundsApp.Utilities;
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

        private void OnKeyHandler(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                SendMessage_Click(null, null);
            }
        }

        private void ChangeTeam_Click(object sender, RoutedEventArgs e) {

           // TODO: 

        }

        private void StartGame_Click(object sender, RoutedEventArgs e) {
            this.m_smh.Lobby.CompileAndStartMatch(this.OnStartMatchCancelled);
            this.UpdateGUI(() => this.lobbyChat.AppendText("[Info] Starting match\n"));
        }

        private void OnStartMatchCancelled(string reason) {
            Trace.WriteLine(reason, "GameLobbyView-OnMatchCancelled.cs");
            if (reason.CompareTo(SessionStatus.S_Compiling.ToString()) == 0) {
                this.UpdateGUI(() => this.lobbyChat.AppendText("[Info] Generating ingame match details...\n"));
            } else if (reason.CompareTo(SessionStatus.S_Playing.ToString()) == 0) {
                this.UpdateGUI(() => this.lobbyChat.AppendText("[Info] Starting game...\n"));
            }
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

                this.m_teamManagement.SetIsHost(true);

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
                this.m_teamManagement.SetIsHost(false);
                this.Map.IsEnabled = false;
                this.Gamemode.IsEnabled = false;
                this.GamemodeOption.IsEnabled = false;

                // Fetch selected map
                this.m_smh.Lobby.GetLobbyInformation("selected_map", this.UpdateSelectedMap);
                this.m_smh.Lobby.GetLobbyInformation("selected_wc", this.UpdateSelectedGamemode);
                this.m_smh.Lobby.GetLobbyInformation("selected_wcs", this.UpdateSelectedOption);

                // TODO: Hook into info messages so we can update properly

                this.m_smh.Lobby.InvokeDelayed(500, x => x.RefreshTeamAsync(this.LobbyTeamRefreshDone));

                //throw new NotImplementedException();

            }
            this.UpdateLobbyVisuals();
            this.m_lobbyUpdate.Start();
        }

        private void UpdateSelectedMap(string arg1, string arg2) {
            this.Dispatcher.Invoke(() => {
                if (ScenarioList.TryFindScenario(arg1, out Scenario scenario)) {
                    this.Map.ItemsSource = new List<Scenario>() { scenario };
                    this.Map.SelectedIndex = 0;
                    UpdateMapPreview(scenario);
                }
            });
        }

        private void UpdateSelectedGamemode(string arg1, string arg2) {
            this.Dispatcher.Invoke(() => {
                var temp = WinconditionList.GetWinconditionByName(arg1);
                this.Gamemode.ItemsSource = new List<Wincondition>() { temp };
                this.Gamemode.SelectedIndex = 0;
            });
        }

        private void UpdateSelectedOption(string arg1, string arg2) {
            this.Dispatcher.Invoke(() => {
                if (int.TryParse(arg1, out int option)) {
                    var gamemode = Gamemode.SelectedItem as Wincondition;
                    var opt = gamemode.Options[option];
                    this.GamemodeOption.ItemsSource = new List<string> { opt.Title };
                    this.GamemodeOption.SelectedIndex = 0;
                }
            });
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
            if (!Map.IsEnabled) {
                return;
            }
            if (Map.SelectedItem is Scenario scenario) {
                if (scenario.RelativeFilename.CompareTo(this.m_smh.Lobby.SelectedMap) != 0) {
                    if (scenario.MaxPlayers < this.m_smh.Lobby.PlayerCount) {
                        // Do something
                    }
                    this.UpdateAvailableGamemodes();
                    this.m_teamManagement.SetMaxPlayers(scenario.MaxPlayers);
                    this.m_smh.Lobby.SetMap(scenario);
                    this.m_smh.Lobby.SetLobbyCapacity(scenario.MaxPlayers);
                    UpdateMapPreview(scenario);
                }
            }
        }

        private void UpdateMapPreview(Scenario scenario) {
            string fullpath = System.IO.Path.GetFullPath($"usr\\mods\\map_icons\\{scenario.Name}_map.tga");
            if (File.Exists(fullpath)) {
                mapImage.Source = TgaImageSource.TargaBitmapSourceFromFile(fullpath);
            } else {
                Trace.WriteLine($"Failed to locate file: {fullpath}");
            }
        }

        public SessionInfo CreateSessionInfo() {

            Wincondition selectedWincondition = Gamemode.SelectedItem as Wincondition;
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
                    int aiid = this.m_smh.Lobby.CreateAIPlayer(card.Difficulty, card.Playerarmy, team);
                    if (aiid != -1) {
                        card.UpdatePlayerID((ulong)aiid);
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
            if (!Gamemode.IsEnabled) {
                return;
            }
            if (Gamemode.SelectedItem is Wincondition wincon) {
                this.m_smh.Lobby.SetGamemode(wincon.Name);
                this.UpdateGamemodeOptions(wincon);
            }
        }

        private void GamemodeOption_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!GamemodeOption.IsEnabled) {
                return;
            }
            if (GamemodeOption.SelectedItem is WinconditionOption) {
                this.m_smh.Lobby.SetGamemodeOption(GamemodeOption.SelectedIndex);
            }
        }

        private void UpdateStartMatchButton() => StartGameBttn.IsEnabled = this.IsLegalMatch();

        private bool IsLegalMatch() 
            => this.m_teamManagement.GetTeamSize(ManagedLobbyTeamType.Allies) > 0 && this.m_teamManagement.GetTeamSize(ManagedLobbyTeamType.Axis) > 0;

        public Company GetLocalCompany() {
            var card = this.m_teamManagement.GetLocalPlayercard();
            if (card is not null) {
                return PlayerCompanies.FromNameAndFaction(card.Playercompany, Faction.FromName(card.Playerarmy));
            } else {
                return null;
            }
        }

        private void LobbyTeamRefreshDone(ManagedLobby lobby) {

            // Update lobby visuals
            this.UpdateLobbyVisuals();

            // Do more stuff?

        }

    }

}
