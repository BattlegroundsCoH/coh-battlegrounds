using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Collections.ObjectModel;
using System.Diagnostics;

using Battlegrounds.AI;

using Battlegrounds.Networking.Server;
using Battlegrounds.UI;
using Battlegrounds.DataLocal;
using Battlegrounds.Game.Scenarios;
using Battlegrounds.Locale;
using Battlegrounds.Modding;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.UI.Modals.Prompts;
using Battlegrounds.UI.Modals;
using Battlegrounds.Lobby.Lookups;
using Battlegrounds.Networking;
using Battlegrounds.Resources;
using Battlegrounds.Functional;
using Battlegrounds.Lobby.Components;
using Battlegrounds.Lobby.Modals;

namespace Battlegrounds.Lobby.Pages;

using static Battlegrounds.UI.AppContext;

public record LobbyBrowserButton(ICommand Click, Func<bool> IsEnabledCheck) : INotifyPropertyChanged {
    public bool IsEnabled => this.IsEnabledCheck();

    public event PropertyChangedEventHandler? PropertyChanged;
    public void Update(object sender) {
        this.PropertyChanged?.Invoke(this, new(nameof(IsEnabled)));
    }
}

public record LobbySettingPreview(string Key, string Value);

public record LobbySlotPreview(ServerSlot Slot) {
    public string SlotTitle => this.Slot.State switch {
        0 => BattlegroundsInstance.Localize.GetString("GameBrowserView_PreviewSlotOpen"),
        1 => this.Slot.Difficulty switch {
            0 => this.Slot.DisplayName,
            1 => BattlegroundsInstance.Localize.GetEnum(AIDifficulty.AI_Easy),
            2 => BattlegroundsInstance.Localize.GetEnum(AIDifficulty.AI_Standard),
            3 => BattlegroundsInstance.Localize.GetEnum(AIDifficulty.AI_Hard),
            4 => BattlegroundsInstance.Localize.GetEnum(AIDifficulty.AI_Expert),
            _ => throw new InvalidOperationException()
        },
        2 => BattlegroundsInstance.Localize.GetString("GameBrowserView_PreviewSlotLocked"),
        _ => ""
    };
    public ImageSource? SlotImage => this.Slot.State switch {
        1 => VisualsLookup.FactionHoverIcons[this.Slot.Army],
        2 => VisualsLookup.FactionHoverIcons[string.Empty],
        _ => null
    };
    public Visibility SlotVisibility => this.Slot.State == 3 ? Visibility.Collapsed : Visibility.Visible;
}

public sealed class LobbyBrowser : IViewModel, INotifyPropertyChanged {


    private static readonly LocaleKey _noMatches = new LocaleKey("GameBrowserView_NoLobbies");
    private static readonly LocaleKey _noConnection = new LocaleKey("GameBrowserView_NoConnection");

    private static readonly Dictionary<string, LocaleKey> _settingKeys = new() {
        [LobbyConstants.SETTING_GAMEMODE] = new("LobbyView_SettingGamemode"),
        [LobbyConstants.SETTING_GAMEMODEOPTION] = new("LobbyView_SettingOption"),
        [LobbyConstants.SETTING_LOGISTICS] = new("LobbyView_SettingSupply"),
        [LobbyConstants.SETTING_MAP] = new("LobbyView_SettingScenario"),
        [LobbyConstants.SETTING_MODPACK] = new("LobbyView_SettingTuning"),
        [LobbyConstants.SETTING_WEATHER] = new("LobbyView_SettingWeather")
    };

    private DateTime m_lastRefresh;

    public bool KeepAlive => false;

    public LobbyBrowserButton Refresh { get; }

    public LobbyBrowserButton Join { get; }

    public LobbyBrowserButton Host { get; }

    public LobbyBrowserButton Local { get; }

    public ObservableCollection<ServerLobby> Lobbies { get; set; }

    public LocaleKey NameListWiewHeader { get; }

    public LocaleKey GamemodeListWiewHeader { get; }

    public LocaleKey StateListWiewHeader { get; }

    public LocaleKey PlayersListWiewHeader { get; }

    public LocaleKey PasswordListWiewHeader { get; }

    public LocaleKey? InfoKey { get; private set; }

    public Visibility InfoKeyVisible { get; private set; }

    public EventCommand JoinLobbyDirectly { get; }

    public bool SingleInstanceOnly => false;

    public object? SelectedLobby { get; set; }

    public int SelectedLobbyIndex { get; set; }

    public ObservableCollection<LobbySettingPreview> PreviewSettings { get; }

    public ImageSource? PreviewImage { get; set; }

    public string PreviewTitle { get; set; }

    public ObservableCollection<LobbySlotPreview> PreviewAllies { get; set; }

    public ObservableCollection<LobbySlotPreview> PreviewAxis { get; set; }

    public Visibility PreviewVisible { get; set; } = Visibility.Collapsed;

    public Visibility NoneVisible => this.PreviewVisible is Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

    public LobbyBrowser() {

        // Create refresh
        this.Refresh = new(new RelayCommand(this.RefreshButton), () => true);

        // Create join
        this.Join = new(new RelayCommand(this.JoinButton), () => Companies.HasCompanyForBothAlliances() && this.SelectedLobbyIndex is not -1);

        // Create host
        this.Host = new(new RelayCommand(this.HostButton), Companies.HasCompanyForBothAlliances);
        this.Local = new(new RelayCommand(LocalButton), Companies.HasCompanyForBothAlliances);

        // Create double-click
        this.JoinLobbyDirectly = new EventCommand<MouseButtonEventArgs>((sender, args) => {
            if (!Companies.HasCompanyForBothAlliances()) {
                return; // Bail on attempt to join when no companies are available.
            }
            this.JoinLobby(sender, args);
        });

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

        // Create preview
        this.PreviewSettings = new();
        this.PreviewImage = null;
        this.PreviewTitle = "No Lobby Selected";
        this.PreviewAllies = new();
        this.PreviewAxis = new();

        // Set basic preview
        this.ClearSelected();

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

    public void RefreshJoin() {

        // Update join
        this.Join.Update(this);

        // Refresh
        this.RefreshSelected();

    }

    private void RefreshSelected() {

        // Baild if invalid (but reset info)
        if (this.SelectedLobbyIndex is -1 || this.SelectedLobbyIndex > this.Lobbies.Count) {
            this.ClearSelected();
            return;
        }

        // Show
        this.PreviewVisible = Visibility.Visible;
        this.PropertyChanged?.Invoke(this, new(nameof(NoneVisible)));
        this.PropertyChanged?.Invoke(this, new(nameof(PreviewVisible)));

        // Get selected
        var selected = this.Lobbies[this.SelectedLobbyIndex];

        // Set name
        this.PreviewTitle = selected.Name;
        this.PropertyChanged?.Invoke(this, new(nameof(PreviewTitle)));

        // Read from settings
        var scen = ScenarioList.FromRelativeFilename(selected.Settings[LobbyConstants.SETTING_MAP]);

        // Set scenario
        this.PreviewImage = ScenarioPreviewLookup.TryGetMapSource(scen);
        this.PropertyChanged?.Invoke(this, new(nameof(PreviewImage)));

        // Clear current settings
        this.PreviewSettings.Clear();

        // Grab mod package
        var package = ModManager.GetPackage(selected.Settings.TryGetValue(LobbyConstants.SETTING_MODPACK, out string? s) ? s : string.Empty);

        // Set settings
        foreach (var setting in selected.Settings) {
            if (_settingKeys.TryGetValue(setting.Key, out var keyloc) && keyloc is not null) {
                bool show = setting.Key switch {
                    LobbyConstants.SETTING_GAMEMODE => package is not null,
                    LobbyConstants.SETTING_GAMEMODEOPTION => package is not null && setting.Key is not "",
                    _ => true
                };
                if (show) {
                    string k = BattlegroundsInstance.Localize.GetString(keyloc);
                    string v = setting.Key switch {
                        LobbyConstants.SETTING_MAP => SettingsLookup.GetScenarioName(scen, setting.Value),
                        LobbyConstants.SETTING_WEATHER or LobbyConstants.SETTING_LOGISTICS => setting.Value is "1" ? "On" : "Off",
                        LobbyConstants.SETTING_MODPACK => package?.PackageName ?? setting.Value,
                        LobbyConstants.SETTING_GAMEMODE => SettingsLookup.GetGamemodeName(setting.Value, package),
                        _ => setting.Value
                    };
                    this.PreviewSettings.Add(new(k, v));
                }
            }
        }

        // Refresh Allies
        this.PreviewAllies.Clear();
        selected.Teams[0].ForEach(x => this.PreviewAllies.Add(new(x)));

        // Refresh axis
        this.PreviewAxis.Clear();
        selected.Teams[1].ForEach(x => this.PreviewAxis.Add(new(x)));

    }

    private void ClearSelected() {

        // Hide
        this.PreviewVisible = Visibility.Collapsed;
        this.PropertyChanged?.Invoke(this, new(nameof(NoneVisible)));
        this.PropertyChanged?.Invoke(this, new(nameof(PreviewVisible)));

        // Give it a null
        this.PreviewImage = ScenarioPreviewLookup.TryGetMapSource(null);

        // Clear settings
        this.PreviewSettings.Clear();

        // Clear teams
        this.PreviewAllies.Clear();
        this.PreviewAxis.Clear();

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
        Trace.WriteLine("Refreshing lobby list", nameof(LobbyBrowser));

        // Get lobbies async
        Task.Run(() => {

            // Get lobbies
            var lobbies = GetLobbiesFromServer();

            // Log amount of lobbies fetched
            Trace.WriteLine($"Serverhub returned {lobbies.Count} lobbies.", nameof(LobbyBrowser));

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

    private static void LocalButton() {

        // Create dummy model
        LocalLobbyHandle localHandle = new(BattlegroundsInstance.Steam.User);

        // Create lobby models.
        var lobbyModel = BaseLobby.CreateModelAsHost(localHandle);
        if (lobbyModel is null) {
            throw new Exception("BAAAAAAD : FIX ASAP");
        }

        // Create chat
        ChatSpectator chatMode = new(localHandle);
        lobbyModel.SetChatModel(chatMode);

        // Get VM
        var vm = GetViewManager();

        // Display it
        vm.SetDisplay(AppDisplayState.LeftRight, chatMode, lobbyModel);

    }

    public void HostButton() {

        // Ensure the interface object is set
        if (NetworkInterface.APIObject is null) {

            // Show error modal
            OKPrompt.Show(OKPrompt.Nothing,
                "Network Failure", "No network connection was established to the Battlegrounds Server (NetworkInterface.APIObject=<NULL>).");

        } else {

            // Show modal
            HostLobby.Show((vm, resault) => {

                // Check return value
                if (resault is not ModalDialogResult.Confirm) {
                    return;
                }

                // Check for null pwd
                vm.LobbyPassword ??= string.Empty;

                // Create lobby
                Task.Run(() => LobbyUtil.HostLobby(NetworkInterface.APIObject, vm.LobbyName, vm.LobbyPassword, this.HostLobbyResponse));

            });

        }

    }

    private void HostLobbyResponse(bool isSuccess, ILobbyHandle? lobby) {

        // If lobby was created.
        if (isSuccess && lobby is not null) {

            // Log success
            Trace.WriteLine("Succsefully hosted lobby.", nameof(LobbyBrowser));

            // Invoke on GUI
            Application.Current.Dispatcher.Invoke(() => {

                // Create lobby models.
                var lobbyModel = BaseLobby.CreateModelAsHost(lobby);
                if (lobbyModel is null) {
                    throw new Exception("BAAAAAAD : FIX ASAP");
                }

                ChatSpectator chatMode = new(lobby);
                lobbyModel.SetChatModel(chatMode);

                // Get VM
                var vm = GetViewManager();

                // Display it
                vm.SetDisplay(AppDisplayState.LeftRight, chatMode, lobbyModel);

            });

        } else {

            // Log failure
            Trace.WriteLine("Failed to host lobby.", nameof(LobbyBrowser));

            // Give feedback to user.
            _ = MessageBox.Show("Failed to host lobby (Failed to connect to server).", "Failure", MessageBoxButton.OK, MessageBoxImage.Error);

        }

    }

    public void JoinLobby(object sender, MouseButtonEventArgs args)
        => this.JoinButton();

    public void JoinButton() {

        // Ensure the interface object is set
        if (NetworkInterface.APIObject is null) {
            Trace.WriteLine("Failed to show join modal (Network interface API is null)", nameof(LobbyBrowser));
            return;
        }

        // Ensure steam user is verified
        if (!BattlegroundsInstance.Steam.HasVerifiedUser && !BattlegroundsInstance.Steam.GetSteamUser()) {
            Trace.WriteLine("Failed to verify steam user in attempt to join game.", nameof(LobbyBrowser));
            return;
        }

        // Get selected lobby
        if (this.SelectedLobby is ServerLobby lobby) {

            // If password, ask for it
            if (lobby.IsPasswrodProtected) {

                // Do modal
                Modals.JoinLobby.Show((vm, resault) => {

                    // Check return value
                    if (resault is not ModalDialogResult.Confirm) {
                        return;
                    }

                    Task.Run(() => {
                        LobbyUtil.JoinLobby(NetworkInterface.APIObject, lobby, vm.Password, this.JoinLobbyResponse);
                    });

                });

            } else {

                Task.Run(() => {
                    LobbyUtil.JoinLobby(NetworkInterface.APIObject, lobby, string.Empty, this.JoinLobbyResponse);
                });

            }

        }
    }

    private void JoinLobbyResponse(bool joined, ILobbyHandle? lobby) {

        if (joined && lobby is not null) {

            // Ensure this now runs on the GUI thread
            Application.Current.Dispatcher.Invoke(() => {

                // Log success
                Trace.WriteLine("Succsefully joined lobby.", nameof(LobbyBrowser));

                // Create lobby models.
                var lobbyModel = BaseLobby.CreateModelAsParticipant(lobby);
                if (lobbyModel is null) {
                    throw new Exception("BAAAAAAD : FIX ASAP");
                }

                ChatSpectator chatMode = new(lobby);
                lobbyModel.SetChatModel(chatMode);

                // Get VM
                var vm = GetViewManager();

                // Display it
                vm.UpdateDisplay(AppDisplayTarget.Left, chatMode);
                vm.UpdateDisplay(AppDisplayTarget.Right, lobbyModel);

            });

        } else {

            // Log failure
            Trace.WriteLine("Failed to join lobby.", nameof(LobbyBrowser));

            // Give feedback to user.
            _ = MessageBox.Show("Failed to join lobby (Failed to connect to server).", "Failure", MessageBoxButton.OK, MessageBoxImage.Error);

        }

    }

    public bool CanJoinLobby
        => Companies.HasCompanyForBothAlliances() && this.SelectedLobby is not null;

    private static List<ServerLobby> GetLobbiesFromServer() {

        // Check if network interface has a server API object
        if (NetworkInterface.APIObject is null) {
            return new();
        }

        // Return lobbies
        return NetworkInterface.APIObject.GetLobbies();

    }

    public void UnloadViewModel(OnModelClosed closeCallback, bool destroy) => closeCallback(false);

    public void Swapback() {
        Task.Run(async () => {
            await Task.Delay(200);
            this.RefreshLobbies();
        });
    }

}
