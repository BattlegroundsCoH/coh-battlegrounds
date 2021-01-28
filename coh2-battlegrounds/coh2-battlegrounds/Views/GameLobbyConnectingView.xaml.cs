using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using Battlegrounds.Online.Lobby;
using Battlegrounds.Online.Services;

namespace BattlegroundsApp.Views {

    /// <summary>
    /// Interaction logic for GameLobbyConnectingView.xaml
    /// </summary>
    public partial class GameLobbyConnectingView : ViewState { // TODO: Add a time-out functionality in case the host disconnected or some other error occured (like a change in host).

        private LobbyHub m_hub;
        private string m_guid;
        private string m_name;
        private string m_passwd;
        private bool m_isConnecting;

        private ulong m_listenfor;

        private GameLobbyView m_lobby;
        private ManagedLobby m_vlobby;

        private Regex m_joinFullyRegex = new Regex(@"(?<id>\d+)-join-(?<state>\w+)");

        public GameLobbyConnectingView(LobbyHub hub, string guid, string name, string password) {
            
            // Init components
            InitializeComponent();

            // Set basic connection data
            this.m_hub = hub;
            this.m_guid = guid;
            this.m_name = name;
            this.m_passwd = password;
            this.m_listenfor = hub.User.ID;

            // Set flag
            this.m_isConnecting = false;

        }

        public void OnServerResponse(ManagedLobbyStatus status, ManagedLobby result) {

            // Success?
            if (status.Success) {

                // Set (virtual) lobby
                this.m_vlobby = result;

                // Call on GUI thread
                this.UpdateGUI(() => {
                    
                    // Create lobby view
                    this.m_lobby = new GameLobbyView();
                    this.m_lobby.CreateMessageHandler(result);
                    this.m_lobby.AddMetaMessageListener(this.OnMetaMessage);
                    this.m_lobby.AddJoinMessageListener(this.OnPlayerJoin);
                    this.m_lobby.EnableHostMode(false);
                    this.m_lobby.RefreshGameSettings();

                });

            } else {

                // Show error
                MessageBox.Show($"Failed to join lobby.\nServer Message: {status.Message}", "Server Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Return to gamebrowser
                this.StateChangeRequest?.Invoke(MainWindow.GAMEBROWSERSTATE);

            }

        }

        private void OnMetaMessage(string from, string msg) {

            // Check if we received the join meta message
            var match = this.m_joinFullyRegex.Match(msg);

            // Success?
            if (match.Success) {

                // Get index who's allowed to join?
                ulong ul = ulong.Parse(match.Groups["id"].Value);

                // The acceptable user to listen for?
                if (ul == this.m_listenfor) {

                    // Log success
                    Trace.WriteLine("Received join OK", "ConnectingState");

                    // TODO: Do more here

                    // Remove self as listener
                    this.m_lobby.RemoveMetaMessageListener(this.OnMetaMessage);
                    this.m_lobby.RemoveJoinMessageListener(this.OnPlayerJoin);

                    // Begin refresh
                    this.m_lobby.RefreshTeams(this.OnTeamsRefreshed);

                }

            }

        }

        private void OnTeamsRefreshed(ManagedLobby lobby) {

            // Teams data has been updated
            Trace.WriteLine("Received join OK", "ConnectingState");

            // Set is connecting flag to false (So we can goto the correct view without leaving immediately)
            this.m_isConnecting = false;

            // Request state change
            if (this.StateChangeRequest?.Invoke(this.m_lobby) is false) {
                Trace.WriteLine("Somehow failed to change state", "ConnectingState"); // TODO: Better error handling
            }

        }

        private void OnPlayerJoin(string player, ulong id) {

            // Update who we're listening for
            this.m_listenfor = id;

            // Log
            Trace.WriteLine($"Now listening for {player} to be allowed to join", "GameLobbyConnectingView");

        }

        public override void StateOnFocus() {

            // If we're not already connecting
            if (!this.m_isConnecting) {

                // Tell it to join
                ManagedLobby.Join(this.m_hub, this.m_guid, this.m_passwd, this.OnServerResponse);

                // Update connection flag
                this.m_isConnecting = true;

            }

        }

        public override void StateOnLostFocus() { 
            
            if (this.m_isConnecting) {
                // Send leave message
                this.m_vlobby.Leave(null); // don't wait for OK
                this.m_isConnecting = false;
            }

        }

    }

}
