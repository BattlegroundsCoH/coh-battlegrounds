using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading.Tasks;

using Battlegrounds;
using Battlegrounds.Networking;
using Battlegrounds.Networking.Server;
using Battlegrounds.Online.Lobby;
using Battlegrounds.Online.Services;

using BattlegroundsApp.Dialogs.HostGame;
using BattlegroundsApp.Utilities;
using BattlegroundsApp.Dialogs.LobbyPassword;
using Battlegrounds.Networking.Lobby;

namespace BattlegroundsApp.Views {

    /// <summary>
    /// Interaction logic for GameBrowserView.xaml
    /// </summary>
    public partial class GameBrowserView : ViewState {

        public ICommand HostGameCommand { get; private set; }

        private LobbyHub m_hub;
        private ServerAPI m_api;

        public GameBrowserView() {

            // Initialize component
            this.InitializeComponent();

            // Create API Instance
            this.m_api = new ServerAPI("194.37.80.249");

            // Create lobby hub with local steam user
            /*this.m_hub = new LobbyHub {
                User = BattlegroundsInstance.Steam.User
            };*/

            // Set host game command
            this.HostGameCommand = new RelayCommand(this.HostLobby);

        }

        public void RefreshLobby() {

            // Clear the current lobby list
            this.GameLobbyList.Items.Clear();

            // Log refresh
            Trace.WriteLine("Refreshing lobby list", "GameBrowserView");

            // Get lobbies async
            Task.Run(() => {

                // Get lobbies
                var lobbies = this.m_api.GetLobbies();

                // update lobbies
                this.UpdateGUI(() => lobbies.ForEach(x => this.GameLobbyList.Items.Add(x)));

            });

        }

        private void RefreshLobbyList_Click(object sender, RoutedEventArgs e) => this.RefreshLobby();

        private void JoinLobby_Click(object sender, RoutedEventArgs e) {

            if (this.GameLobbyList.SelectedItem is ConnectableLobby lobby) {

                // Get password (if any)
                string lobbyPassword = string.Empty;
                if (lobby.IsPasswordProtected) {
                    LobbyPasswordDialogResult result = LobbyPasswordDialogViewModel.ShowLobbyPasswordDialog("Connect to lobby", out lobbyPassword);
                    if (result == LobbyPasswordDialogResult.Cancel) {
                        return;
                    }
                }

                // Create new connecting view
                GameLobbyConnectingView connectingView = new GameLobbyConnectingView(this.m_hub, lobby.LobbyGUID, lobby.LobbyName, lobbyPassword);

                // Change state to connecting view
                this.StateChangeRequest?.Invoke(connectingView);

            }

        }

        private void HostLobby() {

            // Get host information
            HostGameDialogResult result = HostGameDialogViewModel.ShowHostGameDialog("Host Game", out string lobbyName, out string lobbyPwd);
            
            // Check if user actually wants to host.
            if (result == HostGameDialogResult.Host) {

                // Check for null
                if (lobbyPwd is null) {
                    lobbyPwd = string.Empty;
                }

                // Create lobby
                Task.Run(() => LobbyUtil.HostLobby(this.m_api, lobbyName, lobbyPwd, this.HostLobbyResponse));

            }

        }

        private void HostLobbyResponse(bool result, LobbyHandler lobby) {

            if (result) {

                Trace.WriteLine("Succsefully hosted lobby.", nameof(GameBrowserView));

            } else {

                Trace.WriteLine("Failed to host host lobby.", nameof(GameBrowserView));

            }

        }

        private void HostLobbyServerResponse(ManagedLobbyStatus status, ManagedLobby result) {

            // Make sure it was a success
            if (status.Success) {

                this.UpdateGUI(() => {

                    // Create lobby view
                    GameLobbyView lobbyView = new GameLobbyView();
                    lobbyView.CreateMessageHandler(result);

                    // Request state change
                    if (this.StateChangeRequest?.Invoke(lobbyView) is false) {
                        Trace.WriteLine("Somehow failed to change state"); // TODO: Better error handling
                    }

                });

            } else {
                MessageBox.Show($"Failed to create lobby.\nServer Message: {status.Message}", "Server Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        public override void StateOnFocus() {

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
