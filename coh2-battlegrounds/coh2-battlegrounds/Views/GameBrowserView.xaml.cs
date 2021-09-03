using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading.Tasks;

using Battlegrounds.Networking;
using Battlegrounds.Networking.Lobby;
using Battlegrounds.Networking.Server;

using BattlegroundsApp.Dialogs.HostGame;
using BattlegroundsApp.Utilities;
using BattlegroundsApp.Dialogs.LobbyPassword;
using Battlegrounds.Locale;

namespace BattlegroundsApp.Views {

    /// <summary>
    /// Interaction logic for GameBrowserView.xaml
    /// </summary>
    public partial class GameBrowserView : ViewState {

        public ICommand HostGameCommand { get; private set; }

        private ServerAPI m_api;

        public LocaleKey NameListWiewHeader { get; }
        public LocaleKey GamemodeListWiewHeader { get; }
        public LocaleKey StateListWiewHeader { get; }
        public LocaleKey PlayersListWiewHeader { get; }
        public LocaleKey PasswordListWiewHeader { get; }
        public LocaleKey RefreshButtonContent { get; }
        public LocaleKey HostGameButtonContent { get; }
        public LocaleKey JoinGameButtonContent { get; }

        public GameBrowserView() {

            //this.DataContext = this;

            // Define locales
            NameListWiewHeader = new LocaleKey("GameBrowserView_Name");
            GamemodeListWiewHeader = new LocaleKey("GameBrowserView_Gamemode");
            StateListWiewHeader = new LocaleKey("GameBrowserView_State");
            PlayersListWiewHeader = new LocaleKey("GameBrowserView_Players");
            PasswordListWiewHeader = new LocaleKey("GameBrowserView_Password");
            RefreshButtonContent = new LocaleKey("GameBrowserView_Refresh");
            HostGameButtonContent = new LocaleKey("GameBrowserView_Host_Game");
            JoinGameButtonContent = new LocaleKey("GameBrowserView_Join_Game");

            // Initialize component
            this.InitializeComponent();

            // Set host game command
            this.HostGameCommand = new RelayCommand(this.HostLobby);

        }

        public void RefreshLobby() {

            // Clear the current lobby list
            this.GameLobbyList.Items.Clear();

            // Log refresh
            Trace.WriteLine("Refreshing lobby list", "GameBrowserView");

            // Get lobbies async
            _ = Task.Run(() => {

                // Get lobbies
                var lobbies = this.m_api.GetLobbies();

                // update lobbies
                _ = this.UpdateGUI(() => lobbies.ForEach(x => this.GameLobbyList.Items.Add(x)));

            });

        }

        private void RefreshLobbyList_Click(object sender, RoutedEventArgs e) => this.RefreshLobby();

        private void JoinLobby_Click(object sender, RoutedEventArgs e) {

            if (this.GameLobbyList.SelectedItem is ServerLobby lobby) {

                // Bail fast if capacity is reached.
                if (lobby.Members >= lobby.Capacity) {
                    return;
                }

                // Get password (if any)
                string lobbyPassword = string.Empty;
                if (lobby.HasPassword) {
                    if (LobbyPasswordDialogViewModel.ShowLobbyPasswordDialog("Connect to lobby", out lobbyPassword) is LobbyPasswordDialogResult.Cancel) {
                        return;
                    }
                }

                // Create connecting view and start joining
                GameLobbyConnectingView connectingView = new(this.m_api, lobby, lobbyPassword);
                _ = this.StateChangeRequest?.Invoke(connectingView);

            }

        }

        private void HostLobby() {

            // Check if user actually wants to host.
            if (HostGameDialogViewModel.ShowHostGameDialog("Host Game", out string lobbyName, out string lobbyPwd) is HostGameDialogResult.Host) {

                // Check for null
                if (lobbyPwd is null) {
                    lobbyPwd = string.Empty;
                }

                // Create lobby
                _ = Task.Run(() => LobbyUtil.HostLobby(this.m_api, lobbyName, lobbyPwd, this.HostLobbyResponse));

            }

        }

        private void HostLobbyResponse(bool result, LobbyHandler lobby) {

            if (result) {

                Trace.WriteLine("Succsefully hosted lobby.", nameof(GameBrowserView));
                _ = this.UpdateGUI(() => {

                    // Create lobby view
                    GameLobbyView lobbyView = new(lobby);

                    // Request state change
                    if (this.StateChangeRequest?.Invoke(lobbyView) is false) {
                        Trace.WriteLine("Somehow failed to change state"); // TODO: Better error handling
                    }

                });

            } else {

                Trace.WriteLine("Failed to host lobby.", nameof(GameBrowserView));
                _ = MessageBox.Show("Failed to host lobby (Failed to connect to server).", "Failure", MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }

        public override void StateOnFocus() {

            if (this.m_api is null) {

                // Log if local instance was found
                Trace.WriteLine($"Local server instance detected = {NetworkInterface.HasLocalServer()}", nameof(GameBrowserView));

                // Create API Instance
                this.m_api = NetworkInterface.APIObject;

            }

            // Should only do this if there are no servers already listed
            if (this.GameLobbyList.Items.Count == 0) {

                // Get lobby list
                this.RefreshLobby();

            }

            // Log state change
            Trace.WriteLine("GameBrowser was set as active state.", "ViewStateMachine");

        }

        public override void StateOnLostFocus() {

        }

    }
}
