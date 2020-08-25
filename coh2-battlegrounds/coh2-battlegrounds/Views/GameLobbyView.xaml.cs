using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Battlegrounds;
using Battlegrounds.Steam;

namespace BattlegroundsApp.Views {
    /// <summary>
    /// Interaction logic for GameLobbyView.xaml
    /// </summary>
    public partial class GameLobbyView : UserControl {

        ServerMessageHandler m_smh;

        public event Action<bool, GameLobbyView> OnServerAcceptanceResponse;

        public GameLobbyView() {

            InitializeComponent();



        }

        public void SetSMH(ServerMessageHandler smh) {
            this.m_smh = smh;
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

        private void ChangeTeam_Click(object sender, RoutedEventArgs e) {

           // TODO: 

        }

        private void StartGame_Click(object sender, RoutedEventArgs e) => this.m_smh.Lobby.CompileAndStartMatch(this.OnStartMatchCancelled);

        private void OnStartMatchCancelled(string reason) {
            Trace.WriteLine(reason);
        }

        private void LeaveLobby_Click(object sender, RoutedEventArgs e) {

            ClearLobby();
            if (this.m_smh.Lobby != null) {
                this.m_smh.LeaveLobby();
            }

        }

        internal void AddPlayer(string _user, int pos = -1) {

        }

        internal void RemovePlayer(string _user) {

        }

        public void ClearLobby() {
            lobbyChat.Clear();
            messageText.Clear();
        }

        public void UpdateGUI(Action a) => this.Dispatcher.Invoke(a);

        public void ServerConnectResponse(bool connected) => this.OnServerAcceptanceResponse?.Invoke(connected, this);

    }

}
