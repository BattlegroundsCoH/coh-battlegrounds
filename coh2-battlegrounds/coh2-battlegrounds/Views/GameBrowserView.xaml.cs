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

namespace BattlegroundsApp.Views {

    /// <summary>
    /// Interaction logic for GameBrowserView.xaml
    /// </summary>
    public partial class GameBrowserView : UserControl {

        private IDialogService _dialogService;

        public ICommand HostGameCommand { get; private set; }

        private LobbyHub m_hub;

        private MainWindow m_hostWindow;

        public GameBrowserView(MainWindow hostWindow) {

            this.m_hostWindow = hostWindow;

            InitializeComponent();

            this.m_hub = new LobbyHub();
            if (!LobbyHub.CanConnect()) {
                // TODO: Error report
            } else {
                this.m_hub.User = BattlegroundsInstance.LocalSteamuser;
            }

            // TODO: Make this with injection?
            _dialogService = new DialogService();

            HostGameCommand = new RelayCommand(HostLobby);

            // Get lobby list
            this.GetLobbyList();

        }

        public void UpdateGUI(Action a) => this.Dispatcher.BeginInvoke(a);

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

                // TODO: Psswd check

                GameLobbyView vw = new GameLobbyView(m_hostWindow);
                vw.OnServerAcceptanceResponse += this.OnServerConnectResponse;
                ServerMessageHandler smh = new ServerMessageHandler(vw);
                vw.SetSMH(smh);

                string lobbyToJoin = lobby.lobby_guid;

                ManagedLobby.Join(this.m_hub, lobbyToJoin, string.Empty, smh.OnServerResponse);

            }

        }

        private void HostLobby() {

            var dialog = new HostGameDialogViewModel("Host Game");
            var result = _dialogService.OpenDialog(dialog);
            
            if (result == Dialogs.DialogResults.Host) {

                GameLobbyView vw = new GameLobbyView(m_hostWindow);
                vw.OnServerAcceptanceResponse += this.OnServerConnectResponse;
                ServerMessageHandler smh = new ServerMessageHandler(vw);
                vw.SetSMH(smh);

                ManagedLobby.Host(this.m_hub, dialog.LobbyName, dialog.LobbyPassword, smh.OnServerResponse);

            }

        }

        private void OnServerConnectResponse(bool connected, GameLobbyView view) {
            if (connected) {
                this.m_hostWindow.SetView(view); // Returns a dispatcher; TODO: Disable navbar (Maybe could be done in .xaml => IsEnabled="{Binding}")
            } else {
                // TODO: Report error
            }
        }

    }
}
