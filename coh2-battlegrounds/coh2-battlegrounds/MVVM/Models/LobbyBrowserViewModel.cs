using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Battlegrounds.Locale;
using Battlegrounds.Networking;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Networking.Server;

using BattlegroundsApp.Dialogs.HostGame;
using BattlegroundsApp.Dialogs.LobbyPassword;
using BattlegroundsApp.Lobby.MVVM.Models;
using BattlegroundsApp.LocalData;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.MVVM.Models {

    public record LobbyBrowserButton(ICommand Click, Func<bool> IsEnabledCheck) : INotifyPropertyChanged {
        public bool IsEnabled => this.IsEnabledCheck();

        public event PropertyChangedEventHandler? PropertyChanged;
        public void Update(object sender) => this.PropertyChanged?.Invoke(sender, new(nameof(IsEnabled)));
    }

    public class LobbyBrowserViewModel : IViewModel {

        private readonly bool __useMockData = false; // SET TO FALSE WHEN TESTING IS OVER
        private DateTime m_lastRefresh;

        private readonly LocaleKey m_askpasswordkey;

        public LobbyBrowserButton Refresh { get; }

        public LobbyBrowserButton Join { get; }

        public LobbyBrowserButton Host { get; }

        public ObservableCollection<ServerLobby> Lobbies { get; set; }

        public LocaleKey NameListWiewHeader { get; }

        public LocaleKey GamemodeListWiewHeader { get; }

        public LocaleKey StateListWiewHeader { get; }

        public LocaleKey PlayersListWiewHeader { get; }

        public LocaleKey PasswordListWiewHeader { get; }

        public EventCommand JoinLobbyDirectly { get; }

        public bool SingleInstanceOnly => true;

        public object? SelectedLobby { get; set; }

        public int SelectedLobbyIndex { get; set; }

        public LobbyBrowserViewModel() {

            // Create refresh
            this.Refresh = new(new RelayCommand(this.RefreshButton), () => true);

            // Create join
            this.Join = new(new RelayCommand(this.JoinButton), this.CanJoinLobby);

            // Create host
            this.Host = new(new RelayCommand(this.HostButton), PlayerCompanies.HasCompanyForBothAlliances);

            // Create double-click
            this.JoinLobbyDirectly = new EventCommand<MouseButtonEventArgs>(this.JoinLobby);

            // Create lobbies container (But do no populate it)
            this.Lobbies = new();

            // Define locales
            this.NameListWiewHeader = new LocaleKey("GameBrowserView_Name");
            this.GamemodeListWiewHeader = new LocaleKey("GameBrowserView_Gamemode");
            this.StateListWiewHeader = new LocaleKey("GameBrowserView_State");
            this.PlayersListWiewHeader = new LocaleKey("GameBrowserView_Players");
            this.PasswordListWiewHeader = new LocaleKey("GameBrowserView_Password");
            this.m_askpasswordkey = new LocaleKey("LobbyPasswordDialogView_Password_Title");

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
                var lobbies = this.GetLobbiesFromServer();

                // Log amount of lobbies fetched
                Trace.WriteLine($"Serverhub returned {lobbies.Count} lobbies.", nameof(LobbyBrowserViewModel));

                // update lobbies
                Application.Current.Dispatcher.Invoke(() => {
                    lobbies.ForEach(this.Lobbies.Add);
                });

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

        private void HostLobbyResponse(bool isSuccess, LobbyAPI? lobby) {

            // If lobby was created.
            if (isSuccess && lobby is not null) {

                // Log success
                Trace.WriteLine("Succsefully hosted lobby.", nameof(LobbyBrowserViewModel));

                // Create lobby models.
                LobbyModel lobbyModel = LobbyModel.CreateModelAsHost(lobby);
                LobbyChatSpectatorModel chatMode = new(lobby);
                lobbyModel.SetChatModel(chatMode);

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

        public void JoinLobby(object sender, MouseButtonEventArgs args)
            => this.JoinButton();

        public void JoinButton() {
            if (this.SelectedLobby is ServerLobby lobby) {

                // Set password
                string pswd = string.Empty;

                // If password, ask for it
                if (lobby.HasPassword) {
                    if (LobbyPasswordDialogViewModel.ShowLobbyPasswordDialog(this.m_askpasswordkey, out pswd) is LobbyPasswordDialogResult.Cancel) {
                        return;
                    }
                }

                _ = Task.Run(() => {
                    LobbyUtil.JoinLobby(NetworkInterface.APIObject, lobby, pswd, this.JoinLobbyResponse);
                });

            }
        }

        private void JoinLobbyResponse(bool joined, LobbyAPI? lobby) { 
            
            if (joined && lobby is not null) {

                // Log success
                Trace.WriteLine("Succsefully joined lobby.", nameof(LobbyBrowserViewModel));

                // Create lobby models.
                LobbyModel lobbyModel = LobbyModel.CreateModelAsParticipant(lobby);
                LobbyChatSpectatorModel chatMode = new(lobby);
                lobbyModel.SetChatModel(chatMode);

                // Display it
                App.ViewManager.UpdateDisplay(AppDisplayTarget.Left, chatMode);
                App.ViewManager.UpdateDisplay(AppDisplayTarget.Right, lobbyModel);


            } else {

                // Log failure
                Trace.WriteLine("Failed to join lobby.", nameof(LobbyBrowserViewModel));

                // Give feedback to user.
                _ = MessageBox.Show("Failed to join lobby (Failed to connect to server).", "Failure", MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }

        private bool CanJoinLobby()
            => PlayerCompanies.HasCompanyForBothAlliances() && this.SelectedLobby is not null;

        private List<ServerLobby> GetLobbiesFromServer() {
            if (this.__useMockData) {
                return new() {
                    new() {
                        Name = "Alfredo's Match",
                        Capacity = 2,
                        Members = 1,
                        Type = 1,
                        State = "2p_stalingrad, bg_vp (500)"
                    },
                    new() {
                        Name = "Super Real Match",
                        Capacity = 2,
                        Members = 1,
                        Type = 1,
                        State = "2p_stalingrad, bg_vp (500)"
                    },
                    new() {
                        Name = "WPF is bad",
                        Capacity = 2,
                        Members = 1,
                        Type = 1,
                        State = "2p_stalingrad, bg_vp (500)"
                    },
                };
            } else {
                return NetworkInterface.APIObject.GetLobbies();
            }
        }

        public bool UnloadViewModel() => true;

    }

}
