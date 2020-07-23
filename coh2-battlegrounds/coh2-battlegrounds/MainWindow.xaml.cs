using Battlegrounds;
using Battlegrounds.Online.Services;
using Battlegrounds.Steam;
using Battlegrounds.Game.Database;
using BattlegroundsApp;
using System;
using System.Collections.Generic;
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

namespace coh2_battlegrounds
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static MainWindow Instance { get; private set; }

        private List<string> m_allPlayers;

        public SteamUser user;
        private LobbyHub hub;

        public MainWindow()
        {

            Instance = this;

            // Load the database(s)
            DatabaseManager.LoadAllDatabases(null);

            InitializeComponent();

            // Create (find) user first
            this.user = SteamUser.FromLocalInstall();

            // Then create lobby and assign user
            this.hub = new LobbyHub();
            this.hub.User = this.user;

            // Create list of all players
            this.m_allPlayers = new List<string>();

            GetLobbyList();

        }

        public void GetLobbyList()
        {
            var lobbies = hub.GetConnectableLobbies();

            foreach (var lobby in lobbies)
            {
                LobbyList.Items.Add(new Lobby { _lobbyName = lobby.lobby_name, _lobbyPasswordProtected = lobby.lobby_passwordProtected, _lobbyGuid = lobby.lobby_guid });
            }
        }

        private void hostGame_Click(object sender, RoutedEventArgs e)
        {
            HostGameDialogWindow dialog = new HostGameDialogWindow();
            dialog.ShowDialog();

            if (dialog.DialogResult.Equals(true))
            {
                string _lobbyName = dialog.lobbyName.Text;
                string _lobbyPassword = dialog.lobbyPassword.Text;

                ManagedLobby.Host(hub, _lobbyName, _lobbyPassword, ServerMessageHandler.OnServerResponse);

            }
        }

        private void joinLobby_Click(object sender, RoutedEventArgs e)
        {

            if (LobbyList.SelectedItem is Lobby lobby)
            {
                //if (lobby._lobbyPasswordProtected == true)
                //{
                //     show dialog

                //}
                string lobbyToJoin = lobby._lobbyGuid;
                ManagedLobby.Join(hub, lobbyToJoin, String.Empty, ServerMessageHandler.OnServerResponse);

            }

        }

        public void OnLobbyEnter(ManagedLobby lobby) { // This is called when the server says OK, lobby created or lobby joined

            // Hide browser, show lobby
            GameBrowser.Visibility = Visibility.Collapsed;
            LobbyView.Visibility = Visibility.Visible;

            // We do have to handle some stuff seperately
            if (lobby.IsHost) {

                // Add ourselves
                this.AddPlayer(user.Name);

            } else {

                // Setup lobby data
                SetupLobbyData();

            }

        }

        private async void SetupLobbyData() {

            // Get player names
            m_allPlayers.AddRange(await ServerMessageHandler.CurrentLobby.GetPlayerNamesAsync());

            // Get max capacity
            int maxCapacity = await ServerMessageHandler.CurrentLobby.GetLobbyCapacityAsync();

            for (int i = 0; i < maxCapacity; i++)
            {

                // Team slot we're getting info from
                string teamSlot = $"Slot_{i}_state";

                // Get that information
                ServerMessageHandler.CurrentLobby.GetLobbyInformation(teamSlot, (x, y) =>
                {
                    this.Dispatcher.Invoke(() => { this.AddPlayer(x, i); } );
                });

            }
                
        }

        private void refreshLobbyList_Click(object sender, RoutedEventArgs e)
        {
            LobbyList.Items.Clear();
            GetLobbyList();
        }

        private void LobbyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void leaveLobby_Click(object sender, RoutedEventArgs e)
        {
            LobbyView.Visibility = Visibility.Collapsed;
            GameBrowser.Visibility = Visibility.Visible;
            ClearLobby();
            if (ServerMessageHandler.CurrentLobby != null) {
                ServerMessageHandler.LeaveLobby();
            }
        }

        private void sendMessage_Click(object sender, RoutedEventArgs e)
        {
            string messageContent = messageText.Text;
            string messageSender = user.Name;

            string message = $"{messageSender}: {messageContent}";

            chatBox.Text += $"{message}\n";
            chatBox.ScrollToEnd();

            messageText.Clear();

            // Send message to server (so other players can see)
            ServerMessageHandler.CurrentLobby.SendChatMessage(messageContent);

        }

        private void ClearLobby()
        {
            chatBox.Clear();
            messageText.Clear();
            LobbyTeam1.Items.Clear();
            LobbyTeam2.Items.Clear();

        }

        internal void AddPlayer(string _user, int pos = -1)
        {
            if (pos != -1) {

            }
            if (!m_allPlayers.Contains(_user)) {
                m_allPlayers.Add(_user);
            }
            if (LobbyTeam1.Items.Count <= LobbyTeam2.Items.Count)
            {
                LobbyTeam1.Items.Add(_user);
            } else {
                LobbyTeam2.Items.Add(_user);
            }

        }

        internal void RemovePlayer(string _user)
        {
            if (LobbyTeam1.Items.Contains(_user))
            {
                LobbyTeam1.Items.Remove(_user);
            } else
            {
                LobbyTeam2.Items.Remove(_user);
            }
        }

        private void changeTeam_Click(object sender, RoutedEventArgs e)
        {
            if (LobbyTeam1.Items.Contains(user.Name))
            {
                LobbyTeam1.Items.Remove(user.Name);
                LobbyTeam2.Items.Add(user.Name);
            }
            else
            {
                LobbyTeam2.Items.Remove(user.Name);
                LobbyTeam1.Items.Add(user.Name);
            }
        }

        private void BroadcastTeamChange(bool changeToTeam2) {

        }

        private void OnStartMatchCancelled(string reason) {

        }

        private void startMatch_Click(object sender, RoutedEventArgs e)
        {

            ServerMessageHandler.CurrentLobby.CompileAndStartMatch(this.OnStartMatchCancelled);

        }
    }
}
