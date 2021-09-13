using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Battlegrounds.Locale;
using Battlegrounds.Networking;
using Battlegrounds.Networking.Lobby;
using Battlegrounds.Networking.Server;

using BattlegroundsApp.Dialogs.HostGame;
using BattlegroundsApp.Lobby.MVVM.Models;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.MVVM.Models {

    public class LobbyBrowserButton {
        public ICommand Click { get; init; }
        public LocaleKey Text { get; init; }
        public LocaleKey Tooltip { get; init; }
    }

    public class LobbyBrowserViewModel : IViewModel {

        private DateTime m_lastRefresh;

        public LobbyBrowserButton Refresh { get; }

        public LobbyBrowserButton Join { get; }

        public LobbyBrowserButton Host { get; }

        public ObservableCollection<ServerLobby> Lobbies { get; set; }

        public LocaleKey NameListWiewHeader { get; }

        public LocaleKey GamemodeListWiewHeader { get; }

        public LocaleKey StateListWiewHeader { get; }

        public LocaleKey PlayersListWiewHeader { get; }

        public LocaleKey PasswordListWiewHeader { get; }

        public bool SingleInstanceOnly => true;

        public LobbyBrowserViewModel() {

            // Create refresh
            this.Refresh = new() {
                Click = new RelayCommand(this.RefreshButton),
                Text = new("GameBrowserView_Refresh")
            };

            // Create join
            this.Join = new() {
                Click = new RelayCommand(this.JoinButton),
                Text = new("GameBrowserView_Join_Game")
            };

            // Create host
            this.Host = new() {
                Click = new RelayCommand(this.HostButton),
                Text = new("GameBrowserView_Host_Game")
            };

            // Create lobbies container (But do no populate it)
            this.Lobbies = new();

            // Define locales
            this.NameListWiewHeader = new LocaleKey("GameBrowserView_Name");
            this.GamemodeListWiewHeader = new LocaleKey("GameBrowserView_Gamemode");
            this.StateListWiewHeader = new LocaleKey("GameBrowserView_State");
            this.PlayersListWiewHeader = new LocaleKey("GameBrowserView_Players");
            this.PasswordListWiewHeader = new LocaleKey("GameBrowserView_Password");

        }

        public void RefreshButton() {
            if ((DateTime.Now - this.m_lastRefresh).TotalSeconds >= 2.5) {
                this.RefreshLobbies();
                this.m_lastRefresh = DateTime.Now;
            }
        }

        public void RefreshLobbies() {

            // Clear all lobbies
            this.Lobbies.Clear();

            // Log refresh
            Trace.WriteLine("Refreshing lobby list", nameof(LobbyBrowserViewModel));

            // Get lobbies async
            _ = Task.Run(() => {

                // Get lobbies
                var lobbies = NetworkInterface.APIObject.GetLobbies();

                // update lobbies
                lobbies.ForEach(x => this.Lobbies.Add(x));

            });

        }

        public void HostButton() {

            // Check if user actually wants to host.
            if (HostGameDialogViewModel.ShowHostGameDialog(new LocaleKey("GameBrowserView_HostGameDialog_Title"), out string lobbyName, out string lobbyPwd) is HostGameDialogResult.Host) {

                // Check for null
                if (lobbyPwd is null) {
                    lobbyPwd = string.Empty;
                }

                // Create lobby
                _ = Task.Run(() => LobbyUtil.HostLobby(NetworkInterface.APIObject, lobbyName, lobbyPwd, this.HostLobbyResponse));

            }

        }

        private void HostLobbyResponse(bool result, LobbyHandler lobby) {

            // If lobby was created.
            if (result) {

                // Log success
                Trace.WriteLine("Succsefully hosted lobby.", nameof(LobbyBrowserViewModel));

                // Create lobby models.
                LobbyModel lobbyModel = LobbyModel.CreateModelAsHost(lobby);
                LobbyChatSpectatorModel chatMode = new(lobby);

                // Display it
                App.ViewManager.UpdateDisplay(AppDisplayTarget.Left, chatMode);
                App.ViewManager.UpdateDisplay(AppDisplayTarget.Right, lobbyModel);

            } else {

                // Log failure
                Trace.WriteLine("Failed to host lobby.", nameof(LobbyBrowserViewModel));

                // Give feedback to user.
                _ = MessageBox.Show("Failed to host lobby (Failed to connect to server).", "Failure", MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }

        public void JoinButton() {

        }

    }

}
