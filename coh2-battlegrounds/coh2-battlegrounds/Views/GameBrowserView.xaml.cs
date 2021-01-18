using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

using Battlegrounds;
using Battlegrounds.Online.Services;
using BattlegroundsApp.Dialogs.HostGame;
using BattlegroundsApp.Dialogs.Service;
using BattlegroundsApp.Utilities;
using Battlegrounds.Online.Lobby;
using BattlegroundsApp.Dialogs.LobbyPassword;

namespace BattlegroundsApp.Views {

    /// <summary>
    /// Interaction logic for GameBrowserView.xaml
    /// </summary>
    public partial class GameBrowserView : ViewState {

        public ICommand HostGameCommand { get; private set; }

        private LobbyHub m_hub;

        /// <summary>
        /// Flag: Can connect to the external <see cref="LobbyHub"/>.
        /// </summary>
        public static bool HasLobbyHubConnection => LobbyHub.CanConnect();

        public GameBrowserView() {

            // Initialize component
            InitializeComponent();

            // Create lobby hub with local steam user
            this.m_hub = new LobbyHub {
                User = BattlegroundsInstance.Steam.User
            };

            HostGameCommand = new RelayCommand(HostLobby);

        }

        private void GetLobbyList() {

            // Clear the current lobby list
            GameLobbyList.Items.Clear();

            // Get connectable lobbies and add them
            this.m_hub.GetConnectableLobbies(x => this.UpdateGUI(() => this.GameLobbyList.Items.Add(x)), true);

            // Log refresh
            Trace.WriteLine("Refreshing lobby list", "GameBrowserView");

        }

        private void RefreshLobbyList_Click(object sender, RoutedEventArgs e) => this.GetLobbyList();

        private void JoinLobby_Click(object sender, RoutedEventArgs e) {

            if (GameLobbyList.SelectedItem is ConnectableLobby lobby) {

                // Get password (if any)
                string lobbyPassword = string.Empty;
                if (lobby.IsPasswordProtected) {
                    var result = LobbyPasswordDialogViewModel.ShowLobbyPasswordDialog("Connect to lobby", out lobbyPassword);
                    if (result == LobbyPasswordDialogResult.Cancel) {
                        return;
                    }
                }

                // Create new connecting view
                var connectingView = new GameLobbyConnectingView(this.m_hub, lobby.LobbyGUID, lobby.LobbyName, lobbyPassword);

                // Change state to connecting view
                this.StateChangeRequest?.Invoke(connectingView);

            }

        }

        private void HostLobby() {

            var result = HostGameDialogViewModel.ShowHostGameDialog("Host Game", out string lobbyName, out string lobbyPwd);
            
            if (result == HostGameDialogResult.Host) {

                // Call the host function
                ManagedLobby.Host(this.m_hub, lobbyName, lobbyPwd, this.HostLobbyServerResponse);

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
                this.GetLobbyList();

            }

            // Log state change
            Trace.WriteLine("GameBrowser was set as active state.", "ViewStateMachine");

        }

        public override void StateOnLostFocus() {

        }

    }
}
