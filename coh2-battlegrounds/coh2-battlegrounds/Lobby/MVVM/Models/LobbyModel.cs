using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Battlegrounds;
using Battlegrounds.Game.Database;
using Battlegrounds.Locale;
using Battlegrounds.Networking.LobbySystem;

using BattlegroundsApp.Modals;
using BattlegroundsApp.Modals.Dialogs.MVVM.Models;
using BattlegroundsApp.MVVM;
using BattlegroundsApp.MVVM.Models;
using BattlegroundsApp.Resources;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Lobby.MVVM.Models;

public abstract class LobbyModel : IViewModel, INotifyPropertyChanged {

    public record LobbyButton(bool IsEnabled, RelayCommand Click, Visibility Visible) : INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;
        // TODO: Even change stuff...
    }

    public record LobbyDropdown<T>(bool IsEnabled, Visibility Visibility, ObservableCollection<T> Items, Action<int> SelectionChanged) : INotifyPropertyChanged {
        private int m_selected;
        public event PropertyChangedEventHandler? PropertyChanged;
        public int Selected {
            get => this.m_selected;
            set {
                this.m_selected = value;
                this.SelectionChanged.Invoke(value);
            }
        }
    }

    protected static readonly ImageSource __mapNotFound = new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/unknown_map.png"));

    protected static readonly LocaleKey __playabilityAlliesInvalid = new("LobbyView_StartMatchAlliesInvalid");
    protected static readonly LocaleKey __playabilityAlliesNoPlayers = new("LobbyView_StartMatchAlliesNoPlayers");
    protected static readonly LocaleKey __playabilityAxisInvalid = new("LobbyView_StartMatchAxisInvalid");
    protected static readonly LocaleKey __playabilityAxisNoPlayers = new("LobbyView_StartMatchAxisNoPlayers");

    protected static readonly LocaleKey __leaveTitle = new("LobbyView_DialogLeaveTitle");
    protected static readonly LocaleKey __leaveDesc = new("LobbyView_DialogLeaveDesc");

    protected readonly LobbyAPI m_handle;
    protected LobbyChatSpectatorModel? m_chatModel;

    private bool m_hasLeft;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool SingleInstanceOnly => false;

    public LobbyAPIStructs.LobbyTeam Allies { get; }

    public LobbyAPIStructs.LobbyTeam Axis { get; }

    public LobbyButton ExitButton { get; }

    public LobbyButton EditCompanyButton { get; }

    public abstract LobbyButton StartMatchButton { get; }

    public abstract LobbyDropdown<Scenario> MapDropdown { get; }

    public ImageSource? ScenarioPreview { get; set; }

    public string LobbyTitle { get; }

    public bool UnloadViewModel() => true;

    public LobbyModel(LobbyAPI api, LobbyAPIStructs.LobbyTeam allies, LobbyAPIStructs.LobbyTeam axis) {

        // Set basics
        this.m_handle = api;

        // Set teams
        this.Allies = allies;
        this.Axis = axis;

        // Create exit button (always behave the same)
        this.ExitButton = new(true, new(this.LeaveLobby), Visibility.Visible);

        // Create edit company button (always behaves the same)
        this.EditCompanyButton = new(false, new(this.EditCompany), Visibility.Visible);

        // Set title
        this.LobbyTitle = this.m_handle.Title;
        
        // Subscribe to common events
        this.m_handle.OnLobbyConnectionLost += this.OnConnectionLost;

    }

    public void SetChatModel(LobbyChatSpectatorModel chatModel)
        => this.m_chatModel = chatModel;

    protected void LeaveLobby() {

        // Null check
        if (App.ViewManager.GetModalControl() is not ModalControl mControl) {
            return;
        }

        // Lookup strings
        string title = BattlegroundsInstance.Localize.GetString(__leaveTitle);
        string desc = BattlegroundsInstance.Localize.GetString(__leaveDesc);

        // Do modal
        YesNoDialogViewModel.ShowModal(mControl, (vm, resault) => {

            // Check return value
            if (resault is not ModalDialogResult.Confirm) {
                return;
            }

            // Set flag
            this.m_hasLeft = true;

            // Leave lobby
            Task.Run(this.m_handle.Disconnect);

            // Go back to browser view
            App.ViewManager.SetDisplay(AppDisplayState.LeftRight, typeof(LeftMenu), typeof(LobbyBrowserViewModel));

        }, title, desc);

    }

    protected void EditCompany() {
        // TODO: Implement me :D
    }

    private void OnConnectionLost(string reason) {

        // Null check
        if (App.ViewManager.GetModalControl() is not ModalControl mControl) {
            return;
        }

        // Do nothing if already left
        if (this.m_hasLeft) {
            return;
        }

        // Decide on title
        string modalTitle = BattlegroundsInstance.Localize.GetString(reason switch {
            "KICK" => "LobbyView_DialogKickTitle",
            "CLOSED" => "LobbyView_DialogCloseTitle",
            _ => "LobbyView_DialogLostTitle"
        });

        // Decide on desc
        string modalDesc = BattlegroundsInstance.Localize.GetString(reason switch {
            "KICK" => "LobbyView_DialogKickDesc",
            "CLOSED" => "LobbyView_DialogCloseDesc",
            _ => "LobbyView_DialogLostDesc"
        });

        // Goto GUI thread and show connection lost.
        Application.Current.Dispatcher.Invoke(() => {

            // Do modal
            OKDialogViewModel.ShowModal(mControl, (vm, resault) => {

                // Check return value
                if (resault is not ModalDialogResult.Confirm) {
                    return;
                }

                // Go back to browser view
                App.ViewManager.SetDisplay(AppDisplayState.LeftRight, typeof(LeftMenu), typeof(LobbyBrowserViewModel));

            }, modalTitle, modalDesc);

        });

    }

    protected BitmapSource? TryGetMapSource(Scenario? scenario, [CallerMemberName] string caller = "") {

        // Set to default case
        //this.SelectedMatchScenario = __mapNotFound;

        // Check scenario
        if (scenario is null) {
            Trace.WriteLine($"Failed to set **null** scenario (Caller = {caller}).", nameof(LobbyHostModel));
            return (BitmapSource?)__mapNotFound;
        }

        // Get Path
        string fullpath = Path.GetFullPath($"bg_common\\gfx\\map_icons\\{scenario.RelativeFilename}_map.tga");

        // Check if file exists
        if (File.Exists(fullpath)) {
            try {
                return TgaImageSource.TargaBitmapSourceFromFile(fullpath);
            } catch (BadImageFormatException bife) {
                Trace.WriteLine(bife, nameof(this.TryGetMapSource));
            }
        } else {
            fullpath = Path.GetFullPath($"usr\\mods\\map_icons\\{scenario.RelativeFilename}_map.tga");
            if (File.Exists(fullpath)) {
                try {
                    return TgaImageSource.TargaBitmapSourceFromFile(fullpath);
                } catch (BadImageFormatException bife) {
                    Trace.WriteLine(bife, nameof(this.TryGetMapSource));
                }
            } else {
                Trace.WriteLine($"Failed to locate file: {fullpath}", nameof(this.TryGetMapSource));
            }
        }

        // Nothing found
        return (BitmapSource?)__mapNotFound;

    }

    public static LobbyModel? CreateModelAsHost(LobbyAPI handler) {

        // Check allies
        if (handler.Allies is null) {
            return null;
        }

        // Check axis
        if (handler.Axis is null) {
            return null;
        }

        // Create model
        LobbyHostModel model = new(handler, handler.Allies, handler.Axis);

        // Return model
        return model;

    }

    public static LobbyModel? CreateModelAsParticipant(LobbyAPI handler) {

        // Check allies
        if (handler.Allies is null) {
            return null;
        }

        // Check axis
        if (handler.Axis is null) {
            return null;
        }

        // Create model
        LobbyParticipantModel model = new(handler, handler.Allies, handler.Axis);

        // Return model
        return model;

    }

}
