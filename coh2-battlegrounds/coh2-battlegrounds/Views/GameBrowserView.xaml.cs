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

namespace BattlegroundsApp.Views {
    /// <summary>
    /// Interaction logic for GameBrowserView.xaml
    /// </summary>
    public partial class GameBrowserView : UserControl {

        public GameBrowserView() {

            InitializeComponent();

            GetLobbyList();

        }

        private void GetLobbyList() {

            // Clear lobby list
            gameLobbyList.Items.Clear();

            // Get lobby list async
            //hub.GetConnectableLobbies(x => this.UpdateGUI(() => {
            //    this.LobbyList.Items.Add(new Lobby { _lobbyName = x.lobby_name, _lobbyPasswordProtected = x.lobby_passwordProtected, _lobbyGuid = x.lobby_guid });
            //}));

        }

        private void RefreshLobbyList_Click(object sender, RoutedEventArgs e) => this.GetLobbyList();

        private void HostGame_Click(object sender, RoutedEventArgs e) {

        }

        private void JoinGame_Click(object sender, RoutedEventArgs e) {

        }
    }
}
