using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using Battlegrounds;
using Battlegrounds.Game;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match;
using Battlegrounds.Modding;
using Battlegrounds.Online.Lobby;
using BattlegroundsApp.Controls.Lobby;
using BattlegroundsApp.LocalData;
using BattlegroundsApp.Models;
using BattlegroundsApp.Resources;
using BattlegroundsApp.Views.ViewComponent;

namespace BattlegroundsApp.Views {

    /// <summary>
    /// Interaction logic for GameLobbyView.xaml
    /// </summary>
    public partial class GameLobbyView : ViewState {

        private ServerMessageHandler m_smh;
        private LobbyTeamManagementModel m_teamManagement;
        private Task m_lobbyUpdate;

        private volatile bool m_updateLobby;

        public GameLobbyView() {

            // Initialize component
            InitializeComponent();

            // Setup team management
            this.m_teamManagement = new LobbyTeamManagementModel(this.TeamGridview);
            this.m_teamManagement.OnTeamEvent += this.OnTeamManagementCallbackHandler;

        }

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
                this.SendMessage_Click(null, null);
            }
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

            // Stop updating lobby
            this.m_updateLobby = false;

            // Try to leave lobby
            if (this.m_smh.Lobby != null) {
                this.m_smh.LeaveLobby(); // Send leave message
            }

            // Change state
            this.StateChangeRequest?.Invoke(MainWindow.GAMEBROWSERSTATE);

        }

        public void CreateMessageHandler(ManagedLobby result) {
            this.m_smh = new ServerMessageHandler(this, result);
            this.m_smh.OnCompanyRequested += this.OnCompanyRequested;
        }

        #region Server Message Listeners

        public void AddMetaMessageListener(MetaMessageListener listener) => this.m_smh.MetaMessageReceived += listener;

        public void RemoveMetaMessageListener(MetaMessageListener listener) => this.m_smh.MetaMessageReceived -= listener;

        public void AddJoinMessageListener(PlayerJoinedListener listener) => this.m_smh.OnPlayerJoined += listener;

        public void RemoveJoinMessageListener(PlayerJoinedListener listener) => this.m_smh.OnPlayerJoined -= listener;

        public void AddMapChangedListener(MapChangedListener listener) => this.m_smh.OnMapChanged += listener;

        public void RemoveMapChangedListener(MapChangedListener listener) => this.m_smh.OnMapChanged -= listener;

        public void AddGamemodeChangedListener(GamemodechangedListener listener) => this.m_smh.OnGamemodeChanged += listener;

        public void RemoveGamemodeChangedListener(GamemodechangedListener listener) => this.m_smh.OnGamemodeChanged -= listener;

        #endregion

        private void UpdateSelectedMap(string arg1, string arg2) {
            this.Dispatcher.Invoke(() => {
                if (ScenarioList.TryFindScenario(arg1, out Scenario scenario)) {
                    this.Map.ItemsSource = new List<Scenario>() { scenario };
                    this.Map.SelectedIndex = 0;
                    this.m_teamManagement.SetMaxPlayers(scenario.MaxPlayers);
                    this.m_smh.Lobby.SetLobbyCapacity(scenario.MaxPlayers, false);
                    UpdateMapPreview(scenario);
                } else {
                    Trace.WriteLine("Unknown scenario from server (Probably workshop map).", "GameLobbyView");
                    this.Map.ItemsSource = new List<string>() { arg1 };
                    this.Map.SelectedIndex = 0;
                    this.m_smh.Lobby.GetLobbyCapacity(x => this.UpdateGUI(() => this.m_teamManagement.SetMaxPlayers(x)));
                    UpdateMapPreview(null);
                    // TODO: Report error to host
                }
            });
        }

        private void UpdateSelectedGamemode(string arg1, string arg2) {
            this.Dispatcher.Invoke(() => {
                var temp = WinconditionList.GetWinconditionByName(arg1);
                this.Gamemode.ItemsSource = new List<Wincondition>() { temp };
                this.Gamemode.SelectedIndex = 0;
                if (temp.Options is null || temp.Options.Length == 0) {
                    this.GamemodeOption.Visibility = Visibility.Hidden;
                } else {
                    this.GamemodeOption.Visibility = Visibility.Visible;
                }
            });
        }

        private void UpdateSelectedOption(string arg1, string arg2) {
            this.Dispatcher.Invoke(() => {
                if (int.TryParse(arg1, out int option)) {
                    if (this.Gamemode.SelectedItem is Wincondition gamemode) {
                        var opt = gamemode.Options[option];
                        this.GamemodeOption.ItemsSource = new List<string> { opt.Title };
                        this.GamemodeOption.SelectedIndex = 0;
                        this.GamemodeOption.Visibility = Visibility.Visible;
                    } else {
                        this.GamemodeOption.Visibility = Visibility.Hidden;
                    }
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

        private void Map_SelectedItemChanged(object sender, SelectionChangedEventArgs e) {
            if (Map.State is OtherState) {
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
            if (scenario is not null) {
                string fullpath = Path.GetFullPath($"usr\\mods\\map_icons\\{scenario.Name}_map.tga");
                    if (File.Exists(fullpath)) {
                        try {
                            mapImage.Source = TgaImageSource.TargaBitmapSourceFromFile(fullpath);
                            return;
                        } catch (BadImageFormatException bife) {
                            Trace.WriteLine(bife);
                        }
                    } else {
                        Trace.WriteLine($"Failed to locate file: {fullpath}");
                    }
            } else {
                Trace.WriteLine("Failed to find minimap for null scenario.");
            }
            mapImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/unknown_map.png"));

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
            while (this is not null && this.m_updateLobby) {
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
                this.UpdateStartMatchButton();
            });
        }

        private void OnTeamManagementCallbackHandler(ManagedLobbyTeamType team, PlayerCardView card, object arg, string reason) {
            switch (reason) {
                case "AddAI":
                    int aiid = this.m_smh.Lobby.CreateAIPlayer(card.Difficulty, card.PlayerArmy, team, (int)card.GetValue(Grid.RowProperty));
                    if (aiid != -1) {
                        card.UpdatePlayerID((ulong)aiid);
                        Trace.WriteLine($"Adding AI [{team}][{card.Difficulty}][{card.PlayerArmy}]", "GameLobbyView");
                    } else {
                        Trace.WriteLine("Failed to add AI...");
                        card.SetCardState(PlayercardViewstate.Open);
                    }
                    break;
                case "ChangedArmy":
                    if (card.PlayerArmy.CompareTo(this.m_smh.Lobby.TryFindPlayerFromID(card.PlayerSteamID)?.Faction) != 0) {
                        this.m_smh.Lobby.SetFaction(card.PlayerSteamID, card.PlayerArmy);
                        Trace.WriteLine($"Changing faction [{card.PlayerSteamID}][{card.Difficulty}][{card.PlayerArmy}]", "GameLobbyView");
                    }
                    break;
                case "ChangedCompany":
                    PlayercardCompanyItem companyItem = card.PlayerSelectedCompanyItem;
                    if (companyItem.Name.CompareTo(this.m_smh.Lobby.TryFindPlayerFromID(card.PlayerSteamID)?.CompanyName) != 0) {
                        bool allowSet = card.PlayerSteamID == this.m_smh.Lobby.Self.ID || (this.m_smh.Lobby.IsHost && card.IsAI);
                        if (companyItem.State == CompanyItemState.Company && allowSet) {
                            this.m_smh.Lobby.SetCompany(card.PlayerSteamID, companyItem.Name, companyItem.Strength);
                            Trace.WriteLine($"Changing company [{card.PlayerSteamID}][{card.Difficulty}][{card.PlayerArmy}][{companyItem.Name}]", "GameLobbyView");
                        } else if (companyItem.State == CompanyItemState.Generate && allowSet) {
                            this.m_smh.Lobby.SetCompany(card.PlayerSteamID, "AUGEN", -1.0);
                            Trace.WriteLine($"Changing company [{card.PlayerSteamID}][{card.Difficulty}][{card.PlayerArmy}][Auto-generated]", "GameLobbyView");
                        } else {
                            this.m_smh.Lobby.SetCompany(card.PlayerSteamID, "NULL", -1.0);
                            Trace.WriteLine($"Changing company [{card.PlayerSteamID}][{card.Difficulty}][{card.PlayerArmy}][NULL]", "GameLobbyView");
                        }
                    }
                    break;
                case "RemovePlayer":
                    card.IsRegistered = false;
                    this.m_smh.Lobby.RemovePlayer(card.PlayerSteamID, true);
                    Trace.WriteLine($"Removing player [{team}][{arg}][{card.Difficulty}][{card.PlayerArmy}]", "GameLobbyView");
                    break;
                case "LockSlot":
                    throw new NotImplementedException();
                case "UnlockSlot":
                    throw new NotImplementedException();
                case "MoveTo":
                    throw new NotImplementedException();
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

        private void Gamemode_SelectedItemChanged(object sender, SelectionChangedEventArgs e) {
            if (Gamemode.State is OtherState) {
                return;
            }
            if (Gamemode.SelectedItem is Wincondition wincon) {
                this.m_smh.Lobby.SetGamemode(wincon.Name);
                this.UpdateGamemodeOptions(wincon);
            }
        }

        private void GamemodeOption_SelectedItemChanged(object sender, SelectionChangedEventArgs e) {
            if (GamemodeOption.State is OtherState) {
                return;
            }
            if (GamemodeOption.SelectedItem is WinconditionOption) {
                this.m_smh.Lobby.SetGamemodeOption(GamemodeOption.SelectedIndex);
            }
        }

        private void UpdateStartMatchButton() => StartGameBttn.IsEnabled = this.IsLegalMatch();

        private bool IsLegalMatch() 
            => this.m_smh.Lobby.IsHost && this.m_teamManagement.GetTeamSize(ManagedLobbyTeamType.Allies) > 0 && this.m_teamManagement.GetTeamSize(ManagedLobbyTeamType.Axis) > 0;

        public Company GetLocalCompany() {
            var card = this.m_teamManagement.GetLocalPlayercard();
            if (card is not null) {
                return PlayerCompanies.FromNameAndFaction(card.PlayerSelectedCompanyItem.Name, Faction.FromName(card.PlayerArmy));
            } else {
                return null;
            }
        }

        private void LobbyTeamRefreshDone(ManagedLobby lobby) {

            // Update lobby visuals
            this.UpdateLobbyVisuals();

            // Update start match button
            this.UpdateStartMatchButton();

        }

        public override void StateOnFocus() {

            // Update lobby
            this.m_lobbyUpdate = new Task(this.UpdateLobby);

            // Create lobby data
            this.SetupLobby();

        }

        private void SetupLobby() {

            // If host, setup everything
            if (this.m_smh.Lobby.IsHost) {

                // Enable host mode
                this.EnableHostMode(true);

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
                this.EnableHostMode(false);

                // TODO: Hook into info messages so we can update properly
                this.AddMapChangedListener(this.MapChangedCallback);
                this.AddGamemodeChangedListener(this.GamemodeChangedCallback);

            }
            
            // Update visuals
            this.UpdateLobbyVisuals();

            // Start lobby
            this.m_updateLobby = true;
            this.m_lobbyUpdate.Start();

        }

        private void GamemodeChangedCallback(string game, string setting) {
            this.UpdateSelectedGamemode(game, string.Empty);
            this.UpdateSelectedOption(setting, string.Empty);
        }

        private void MapChangedCallback(string scenario) => this.UpdateSelectedMap(scenario, string.Empty);

        public override void StateOnLostFocus() {

            // Stop lobby update
            this.m_updateLobby = false;

        }

        public void EnableHostMode(bool hostMode) {

            // Set team management
            this.m_teamManagement.SetIsHost(hostMode);

            // Enable or disable game settings (AI and ID properties not relevant to these elements)
            this.Map.SetStateBasedOnContext(hostMode, hostMode, 0);
            this.Gamemode.SetStateBasedOnContext(hostMode, hostMode, 0);
            this.GamemodeOption.SetStateBasedOnContext(hostMode, hostMode, 0);

        }

        public void RefreshGameSettings() {

            // Fetch selected map and gamemode
            this.m_smh.Lobby.GetLobbyInformation("selected_map", this.UpdateSelectedMap);
            this.m_smh.Lobby.GetLobbyInformation("selected_wc", this.UpdateSelectedGamemode);
            this.m_smh.Lobby.GetLobbyInformation("selected_wcs", this.UpdateSelectedOption);

        }

        public void RefreshTeams(ManagedLobbyTaskDone overrideDone) => this.m_smh.Lobby.RefreshTeamAsync(overrideDone ?? this.LobbyTeamRefreshDone);

        private void OnCompanyRequested(string reason) {
            if (reason.CompareTo("HOST") == 0) {
                var company = this.GetLocalCompany();
                if (company is not null) {
                    this.m_smh.Lobby.UploadCompany(company);
                    Trace.WriteLine("Uploading company");
                } else {
                    this.UpdateGUI(() => {
                        this.lobbyChat.Text += "[User Error] You've not selected a company.";
                    });
                }
            }
        }

    }

}
