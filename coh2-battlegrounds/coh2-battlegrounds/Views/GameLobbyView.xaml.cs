using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private bool m_hasCreatedLobbyOnce;
        private ILobbyPlayModel m_playModel;
        private ServerMessageHandler m_smh;
        private LobbyTeamManagementModel m_teamManagement;
        private Task m_lobbyUpdate;

        private volatile bool m_updateLobby;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool CanLeave => this.m_playModel is null;

        public bool CanStartMatch => (this.m_smh.Lobby.IsHost && this.IsLegalMatch()) || (this.m_playModel is not null && this.m_playModel.CanCancel);

        public LobbyTeamManagementModel TeamManager => this.m_teamManagement;

        public GameLobbyView() {

            // Initialize component
            InitializeComponent();

            // Setup team management
            this.m_teamManagement = new LobbyTeamManagementModel(this.TeamGridview);
            this.m_teamManagement.OnTeamEvent += this.OnTeamManagementCallbackHandler;

            // Set other variables
            this.m_hasCreatedLobbyOnce = false;

        }

        private void SendMessage_Click(object sender, RoutedEventArgs e) {

            string messageContent = messageText.Text;
            string messageSender = BattlegroundsInstance.Steam.User.Name;
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

            if (this.m_playModel is null) {

                // Prompt the user for confirmation.
                if (YesNoDialogViewModel.ShowYesNoDialog("Start Match", "Are you sure you'd like to start?") == YesNoDialogResult.Confirm) {

                    // Create play model and begin playing
                    this.m_playModel = new LobbyHostPlayModel(this, this.m_smh.Lobby);
                    this.m_playModel.PlayGame(this.MatchCancelled);

                }

            } else {

                // Cancel the game (Will invoke MatchCancelled when cancelled).
                this.m_playModel.CancelGame();

            }

        }

        private void MatchCancelled() {
            
            // Reset text
            this.StartGameBttn.Content = "Start Match";
            
            // Remove reference to play model
            this.m_playModel = null;

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

        public void AddMatchStartingListener(StartingMatchListener listener) => this.m_smh.OnMatchStarting += listener;

        #endregion

        private void UpdateSelectedMap(string arg1, string arg2) {
            this.Dispatcher.Invoke(() => {
                if (ScenarioList.TryFindScenario(arg1, out Scenario scenario)) {
                    this.Map.ItemsSource = new List<Scenario>() { scenario };
                    this.Map.SelectedIndex = 0;
                    this.m_teamManagement.SetMaxPlayers(scenario.MaxPlayers);
                    this.m_smh.Lobby.SetLobbyCapacity(scenario.MaxPlayers, false);
                    this.UpdateMapPreview(scenario);
                } else {
                    Trace.WriteLine($"Unknown scenario from server \"{arg1}\" (Probably workshop map).", "GameLobbyView");
                    this.Map.ItemsSource = new List<string>() { arg1 };
                    this.Map.SelectedIndex = 0;
                    this.m_smh.Lobby.GetLobbyCapacity(x => this.UpdateGUI(() => this.m_teamManagement.SetMaxPlayers(x)));
                    this.UpdateMapPreview(null);
                    this.m_smh.Lobby.SendClientProblem(false, "UNKNOWN_MAP", arg1);
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
                    if (this.Gamemode.SelectedItem is Wincondition gamemode && gamemode.Options is not null) {
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

            if (this.Map.SelectedItem is Scenario scenario) {

                if (scenario.Gamemodes.Count > 0) {
                    this.Gamemode.ItemsSource = scenario.Gamemodes.OrderBy(x => x.ToString()).ToList();
                    this.Gamemode.SelectedIndex = 0;
                } else {
                    var def = WinconditionList.GetDefaultList().OrderBy(x => x.ToString()).ToList();
                    this.Gamemode.ItemsSource = def;
                    this.Gamemode.SelectedIndex = def.FindIndex(x => x.Name.CompareTo("Victory Points") == 0);
                }

                this.UpdateGamemodeOptions(Gamemode.SelectedItem as Wincondition);

            }

        }

        private void Map_SelectedItemChanged(object sender, SelectionChangedEventArgs e) {
            if (this.Map.State is OtherState) {
                return;
            }
            if (this.Map.SelectedItem is Scenario scenario) {
                if (scenario.RelativeFilename.CompareTo(this.m_smh.Lobby.SelectedMap) != 0) {
                    if (scenario.MaxPlayers < this.m_smh.Lobby.PlayerCount) {
                        // Do something
                    }
                    this.UpdateAvailableGamemodes();
                    this.m_teamManagement.SetMaxPlayers(scenario.MaxPlayers);
                    this.m_smh.Lobby.SetMap(scenario);
                    this.m_smh.Lobby.SetLobbyCapacity(scenario.MaxPlayers);
                    this.UpdateMapPreview(scenario);
                }
            }
        }

        private void UpdateMapPreview(Scenario scenario) {
            if (scenario is not null) {
                string fullpath = Path.GetFullPath($"usr\\mods\\map_icons\\{scenario.Name}_map.tga");
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
            } else {
                Trace.WriteLine("Failed to find minimap for null scenario.");
            }
            this.mapImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/unknown_map.png"));
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
                SelectedTuningMod = new BattlegroundsTuning(), // TODO: Allow users to change this somewhere
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

                        // Log that AI is being added
                        Trace.WriteLine($"Adding AI [{team}][{card.Difficulty}][{card.PlayerArmy}]", "GameLobbyView");

                        // Update ID
                        card.UpdatePlayerID((ulong)aiid);
                        
                        // Update visuals
                        this.UpdateLobbyVisuals();

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
                    this.m_smh.Lobby.RemovePlayer(card.PlayerSteamID, true);
                    Trace.WriteLine($"Removing player [{team}][{arg}][{card.Difficulty}][{card.PlayerArmy}]", "GameLobbyView");
                    break;
                case "LockSlot":
                    break;
                case "UnlockSlot":
                    break;
                case "MoveTo":
                    this.MovePlayer(card, team, arg as PlayerCardView, team);
                    break;
                case "MoveToAllies":
                    this.MovePlayer(card, team, arg as PlayerCardView, ManagedLobbyTeamType.Allies);
                    break;
                case "MoveToAxis":
                    this.MovePlayer(card, team, arg as PlayerCardView, ManagedLobbyTeamType.Axis);
                    break;
                default:
                    break;
            }
            this.UpdateStartMatchButton();
        }

        private void MovePlayer(PlayerCardView from, ManagedLobbyTeamType fromTeam, PlayerCardView to, ManagedLobbyTeamType toTeam) {

            // Determine method to move player (and then move them).
            if (fromTeam == toTeam) {
                this.m_smh.Lobby.SetTeamPosition(from.PlayerSteamID, to.TeamIndex);
            } else {
                this.m_smh.Lobby.SwapTeam(from.PlayerSteamID, toTeam, to.TeamIndex);
            }

            // Update visuals
            this.UpdateLobbyVisuals();

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

        private void UpdateStartMatchButton() => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanStartMatch)));

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
            if (this.m_lobbyUpdate is null) {
                this.m_lobbyUpdate = new Task(this.UpdateLobby);
            }

            // Create lobby data
            if (!this.m_hasCreatedLobbyOnce) {
                
                // Setup the lobby
                this.SetupLobby();

            } else {

                // Re-enable lobby updates
                this.m_updateLobby = true;

            }

        }

        private void SetupLobby() {

            // If host, setup everything
            if (this.m_smh.Lobby.IsHost) {

                // Enable host mode (and because true, will update populate the dropdowns).
                this.EnableHostMode(true);

            } else {

                // lock everything
                this.EnableHostMode(false);

                // TODO: Hook into info messages so we can update properly
                this.AddMapChangedListener(this.MapChangedCallback);
                this.AddGamemodeChangedListener(this.GamemodeChangedCallback);
                this.AddMatchStartingListener(this.MatchStarting);

            }
            
            // Update visuals
            this.UpdateLobbyVisuals();

            // Start lobby
            this.m_updateLobby = true;
            this.m_lobbyUpdate.Start();

            // Set to true
            this.m_hasCreatedLobbyOnce = true;

        }

        private void GamemodeChangedCallback(string game, string setting) {
            this.UpdateSelectedGamemode(game, string.Empty);
            this.UpdateSelectedOption(setting, string.Empty);
        }

        private void MapChangedCallback(string scenario) => this.UpdateSelectedMap(scenario, string.Empty);

        private void MatchStarting(int time, string guid) {

            // Create new lobby member play model
            var model = new LobbyMemberPlayModel(this, this.m_smh.Lobby);
            model.PlayGame(MatchCancelled);
            model.CreateSession(guid);
            model.StartCountdown(time);

            // Set model
            this.m_playModel = model;

        }

        public override void StateOnLostFocus() {

            // Stop lobby update
            this.m_updateLobby = false;

            // If we're in the middle of launching a game
            if (this.m_playModel is not null) {

                // Forcefully change us back
                this.StateChangeRequest?.Invoke(this);

            }

        }

        public void EnableHostMode(bool hostMode) {

            // Set team management
            this.m_teamManagement.SetIsHost(hostMode);

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

            // Fetch selected map and gamemode
            this.m_smh.Lobby.GetLobbyInformation(ManagedLobby.LOBBYINFO_SELECTEDMAP, this.UpdateSelectedMap);
            this.m_smh.Lobby.GetLobbyInformation(ManagedLobby.LOBBYINFO_SELECTEDGAMEMODE, this.UpdateSelectedGamemode);
            this.m_smh.Lobby.GetLobbyInformation(ManagedLobby.LOBBYINFO_SELECTEDGAMEMODEOPTION, this.UpdateSelectedOption);

        }

        private void PopulateDropdowns() {

            // Get the scenarios and set source
            var scenarioSource = ScenarioList.GetList().OrderBy(x => x.ToString()).ToList();
            this.Map.ItemsSource = scenarioSource;

            // If no mapp has been selected
            if (this.Map.SelectedIndex == -1) {

                // Find map to select
                int selectedScenario = scenarioSource.FindIndex(x => x.RelativeFilename.CompareTo(BattlegroundsInstance.LastPlayedMap) == 0);
                this.Map.SelectedIndex = selectedScenario != -1 ? selectedScenario : 0;

                // Get selected scenario and update lobby accordingly
                var scen = this.Map.SelectedItem as Scenario;
                this.m_smh.Lobby.SetMap(scen);
                this.m_smh.Lobby.SetLobbyCapacity(scen.MaxPlayers);

                // Update available gamemodes
                this.UpdateAvailableGamemodes();

            }

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
