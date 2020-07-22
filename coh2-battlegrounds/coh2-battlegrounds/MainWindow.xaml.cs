using Battlegrounds.Online.Services;
using Battlegrounds.Steam;
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

        public SteamUser user = SteamUser.FromLocalInstall();

        public void GetLobbyList()
        {
            var lobbies = ServerMessageHandler.hub.GetConnectableLobbies();

            foreach (var lobby in lobbies)
            {
                LobbyList.Items.Add(new Lobby { _lobbyName = lobby.lobby_name, _lobbyPasswordProtected = lobby.lobby_passwordProtected, _lobbyGuid = lobby.lobby_guid});
            }
        }

        public MainWindow()
        {
            
            InitializeComponent();

            GetLobbyList();

        }

        private void hostGame_Click(object sender, RoutedEventArgs e)
        {
            HostGameDialogWindow dialog = new HostGameDialogWindow();
            dialog.ShowDialog();

            if (dialog.DialogResult.Equals(true))
            {
                string _lobbyName = dialog.lobbyName.Text;
                string _lobbyPassword = dialog.lobbyPassword.Text;

                ManagedLobby.Host(ServerMessageHandler.hub, _lobbyName, _lobbyPassword, ServerMessageHandler.OnServerResponse);

                GameBrowser.Visibility = Visibility.Collapsed;
                LobbyView.Visibility = Visibility.Visible;
            }
        }

        private void joinLobby_Click(object sender, RoutedEventArgs e)
        {

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
        }

        private void sendMessage_Click(object sender, RoutedEventArgs e)
        {
            string messageContent = messageText.Text;
            string messageSender = user.Name;

            string message = $"{messageSender}: {messageContent}";

            chatBox.Text = chatBox.Text += $"{message}\n";
            chatBox.ScrollToEnd();

            messageText.Clear();
        }

        private void ClearLobby()
        {
            chatBox.Clear();
            messageText.Clear();
            LobbyTeam1.Items.Clear();
            LobbyTeam2.Items.Clear();

        }

        private void AddPlayer()
        {
            LobbyTeam1.Items.Add(user.Name);
        }
    }
}
