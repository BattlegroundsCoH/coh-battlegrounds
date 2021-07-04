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

using Battlegrounds.Networking.Lobby;
using Battlegrounds.Networking.Server;

namespace BattlegroundsApp.Views {

    /// <summary>
    /// Interaction logic for GameLobbyConnectingView.xaml
    /// </summary>
    public partial class GameLobbyConnectingView : ViewState { // TODO: Add a time-out functionality in case the host disconnected or some other error occured (like a change in host).

        private bool m_isConnecting;
        private string m_passwd;
        private ServerLobby m_target;
        private ServerAPI m_api;

        public GameLobbyConnectingView(ServerAPI api, ServerLobby lobby, string password) {
            
            // Init components
            this.InitializeComponent();

            // Set target
            this.m_target = lobby;
            this.m_passwd = password;
            this.m_api = api;

            // Set flag
            this.m_isConnecting = false;

        }

        public override void StateOnFocus() {

            // If we're not already connecting
            if (!this.m_isConnecting) {

                // Invoke utility
                Task.Run(() => LobbyUtil.JoinLobby(this.m_api, this.m_target, this.m_passwd, this.OnLobbyJoined));

                // Set connecting flag
                this.m_isConnecting = true; // TODO: Add check to see if not connected after 1 min. -> Then assume failure

            }

        }

        private void OnLobbyJoined(bool joined, LobbyHandler handler) {
            if (joined) {

                Trace.WriteLine("Sucessfully joined lobby -> Transfering to game lobby view.");

                // Call on UI thread
                this.UpdateGUI(() => {

                    // Create new lobby view
                    GameLobbyView lobbyView = new GameLobbyView(handler);

                    // Request state change
                    if (this.StateChangeRequest?.Invoke(lobbyView) is false) {
                        Trace.WriteLine("Somehow failed to change state"); // TODO: Better error handling
                    }

                });

            } else {

            }
        }

        public override void StateOnLostFocus() { 
            
        }

    }

}
