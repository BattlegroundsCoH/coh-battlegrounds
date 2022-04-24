using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Battlegrounds;
using Battlegrounds.Locale;
using Battlegrounds.Networking;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Networking.Server;

using BattlegroundsApp.Lobby.MVVM.Models;
using BattlegroundsApp.LocalData;
using BattlegroundsApp.Modals;
using BattlegroundsApp.Modals.Dialogs.MVVM.Models;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.MVVM.Models;

public record LobbyBrowserButton(ICommand Click, Func<bool> IsEnabledCheck) : INotifyPropertyChanged {
    public bool IsEnabled => this.IsEnabledCheck();

    public event PropertyChangedEventHandler? PropertyChanged;
    public void Update(object sender) {
        this.PropertyChanged?.Invoke(this, new("Join"));
    }
}

public class LobbyBrowserViewModel : IViewModel, INotifyPropertyChanged {

    private static readonly LocaleKey _noMatches = new LocaleKey("GameBrowserView_NoLobbies");
    private static readonly LocaleKey _noConnection = new LocaleKey("GameBrowserView_NoConnection");

    private readonly bool __useMockData = false; // SET TO FALSE WHEN TESTING IS OVER
    private DateTime m_lastRefresh;

    public bool KeepAlive => true;

    public LobbyBrowserButton Refresh { get; }

    public LobbyBrowserButton Join { get; }

    public LobbyBrowserButton Host { get; }

    public ObservableCollection<ServerLobby> Lobbies { get; set; }

    public LocaleKey NameListWiewHeader { get; }

    public LocaleKey GamemodeListWiewHeader { get; }

    public LocaleKey StateListWiewHeader { get; }

    public LocaleKey PlayersListWiewHeader { get; }

    public LocaleKey PasswordListWiewHeader { get; }

    public LocaleKey? InfoKey { get; private set; }

    public Visibility InfoKeyVisible { get; private set; }

    public EventCommand JoinLobbyDirectly { get; }

    public bool SingleInstanceOnly => true;

    public object? SelectedLobby { get; set; }

    public int SelectedLobbyIndex { get; set; }

    public LobbyBrowserViewModel() {

        // Create refresh
        this.Refresh = new(new RelayCommand(this.RefreshButton), () => true);

        // Create join
        this.Join = new(new RelayCommand(this.JoinButton), () => PlayerCompanies.HasCompanyForBothAlliances() && this.SelectedLobbyIndex is not -1);

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

        // Define info key
        this.InfoKey = _noMatches;
        this.InfoKeyVisible = Visibility.Visible;

        // Set selected index
        this.SelectedLobbyIndex = -1;

        // Check connection and update
        Task.Run(() => {
            if (!NetworkInterface.HasInternetConnection()) {
                Application.Current.Dispatcher.Invoke(() => {
                    this.InfoKey = _noConnection;
                    this.InfoKeyVisible = Visibility.Visible;
                    this.PropertyChanged?.Invoke(this, new(nameof(InfoKeyVisible)));
                });
            }
        });

    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void RefreshJoin() => this.Join.Update(this);

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
                
                // Set flag
                if (lobbies.Count == 0) {
                    this.InfoKey = _noMatches;
                    this.InfoKeyVisible = Visibility.Visible;
                } else {
                    this.InfoKey = null;
                    this.InfoKeyVisible = Visibility.Collapsed;
                }

                // Notify change
                this.PropertyChanged?.Invoke(this, new(nameof(InfoKey)));
                this.PropertyChanged?.Invoke(this, new(nameof(InfoKeyVisible)));

                // Add all lobbies
                lobbies.ForEach(this.Lobbies.Add);

            });

        });

    }

    public void HostButton() {

        // Null check
        if (App.ViewManager.GetModalControl() is not ModalControl mControl) {
            return;
        }

        // Ensure the interface object is set
        if (NetworkInterface.APIObject is null) {
            return;
        }

        // Ensure steam user is verified
        if (!BattlegroundsInstance.Steam.HasVerifiedUser && !BattlegroundsInstance.Steam.GetSteamUser()) {
            //Trace.WriteLine("Failed to verify steam user in attempt to join game.", nameof(LobbyBrowserViewModel));
            //return;
        }

        // Show modal
        HostGameDialogViewModel.ShowModal(mControl, (vm, resault) => {

            // Check return value
            if (resault is not ModalDialogResult.Confirm) {
                return;
            }

            // Check for null pwd
            if (vm.LobbyPassword is null) {
                vm.LobbyPassword = string.Empty;
            }

            // Create lobby
            _ = Task.Run(() => LobbyUtil.HostLobby(NetworkInterface.APIObject, vm.LobbyName, vm.LobbyPassword, this.HostLobbyResponse));
            

        });

    }

    private void HostLobbyResponse(bool isSuccess, LobbyAPI? lobby) {

        // If lobby was created.
        if (isSuccess && lobby is not null) {

            // Log success
            Trace.WriteLine("Succsefully hosted lobby.", nameof(LobbyBrowserViewModel));

            // Invoke on GUI
            Application.Current.Dispatcher.Invoke(() => {

                // Create lobby models.
                var lobbyModel = LobbyModel.CreateModelAsHost(lobby);
                if (lobbyModel is null) {
                    throw new Exception("BAAAAAAD : FIX ASAP");
                }

                LobbyChatSpectatorModel chatMode = new(lobby);
                lobbyModel.SetChatModel(chatMode);

                // Display it
                App.ViewManager.UpdateDisplay(AppDisplayTarget.Left, chatMode);
                App.ViewManager.UpdateDisplay(AppDisplayTarget.Right, lobbyModel);

            });

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

        // Ensure the interface object is set
        if (NetworkInterface.APIObject is null) {
            return;
        }

        // Ensure steam user is verified
        if (!BattlegroundsInstance.Steam.HasVerifiedUser && !BattlegroundsInstance.Steam.GetSteamUser()) {
            Trace.WriteLine("Failed to verify steam user in attempt to join game.", nameof(LobbyBrowserViewModel));
            return;
        }

        // Get selected lobby
        if (this.SelectedLobby is ServerLobby lobby) {

            // If password, ask for it
            if (lobby.HasPassword) {

                // Null check
                if (App.ViewManager.GetModalControl() is not ModalControl mControl) {
                    return;
                }

                // Do modal
                LobbyJoinDialogViewModel.ShowModal(mControl, (vm, resault) => {

                    // Check return value
                    if (resault is not ModalDialogResult.Confirm) {
                        return;
                    }

                    _ = Task.Run(() => {
                        LobbyUtil.JoinLobby(NetworkInterface.APIObject, lobby, vm.Password, this.JoinLobbyResponse);
                    });

                });

            } else {

                _ = Task.Run(() => {
                    LobbyUtil.JoinLobby(NetworkInterface.APIObject, lobby, string.Empty, this.JoinLobbyResponse);
                });

            }

        }
    }

    private void JoinLobbyResponse(bool joined, LobbyAPI? lobby) { 
        
        if (joined && lobby is not null) {

            // Ensure this now runs on the GUI thread
            Application.Current.Dispatcher.Invoke(() => {

                // Log success
                Trace.WriteLine("Succsefully joined lobby.", nameof(LobbyBrowserViewModel));

                // Create lobby models.
                var lobbyModel = LobbyModel.CreateModelAsParticipant(lobby);
                if (lobbyModel is null) {
                    throw new Exception("BAAAAAAD : FIX ASAP");
                }

                LobbyChatSpectatorModel chatMode = new(lobby);
                lobbyModel.SetChatModel(chatMode);

                // Display it
                App.ViewManager.UpdateDisplay(AppDisplayTarget.Left, chatMode);
                App.ViewManager.UpdateDisplay(AppDisplayTarget.Right, lobbyModel);

            });

        } else {

            // Log failure
            Trace.WriteLine("Failed to join lobby.", nameof(LobbyBrowserViewModel));

            // Give feedback to user.
            _ = MessageBox.Show("Failed to join lobby (Failed to connect to server).", "Failure", MessageBoxButton.OK, MessageBoxImage.Error);

        }

    }

    public bool CanJoinLobby
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
            
            // Get lobbies (if any)
            List<ServerLobby> lobs;
            if (NetworkInterface.APIObject is null) {
                lobs =  new();
            } else {
                lobs = NetworkInterface.APIObject.GetLobbies();
            }

            // Return lobbies
            return lobs;

        }
    }

    public void UnloadViewModel(OnModelClosed closeCallback, bool destroy) => closeCallback(false);

    public void Swapback() {
        Task.Run(async () => {
            await Task.Delay(200);
            this.RefreshLobbies();
        });
    }

}
