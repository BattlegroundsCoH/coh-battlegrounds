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
    }
}
