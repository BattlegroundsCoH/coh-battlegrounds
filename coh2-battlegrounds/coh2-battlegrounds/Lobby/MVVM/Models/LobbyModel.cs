using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Battlegrounds;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;
using Battlegrounds.Modding;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Networking.Server;

using BattlegroundsApp.LocalData;
using BattlegroundsApp.Modals;
using BattlegroundsApp.Modals.Dialogs.MVVM.Models;
using BattlegroundsApp.MVVM;
using BattlegroundsApp.MVVM.Models;
using BattlegroundsApp.Resources;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Lobby.MVVM.Models;

public abstract class LobbyModel : IViewModel, INotifyPropertyChanged {

    public record LobbyButton(string Title, bool IsEnabled, RelayCommand Click, Visibility Visible, string Tooltip);

    public record LobbyMutButton(RelayCommand Click, Visibility Visible) : INotifyPropertyChanged {
        private bool m_isEnabled;
        private string? m_tooltip;
        private string? m_title;
        public event PropertyChangedEventHandler? PropertyChanged;
        public bool IsEnabled {
            get => this.m_isEnabled;
            set {
                this.m_isEnabled = value;
                this.PropertyChanged?.Invoke(this, new(nameof(IsEnabled)));
            }
        }
        public string? Tooltip {
            get => this.m_tooltip;
            set {
                this.m_tooltip = value;
                this.PropertyChanged?.Invoke(this, new(nameof(Tooltip)));
            }
        }
        public string? Title {
            get => this.m_title;
            set {
                this.m_title = value;
                this.PropertyChanged?.Invoke(this, new(nameof(Title)));
            }
        }
    }

    public record ScenOp(Scenario Scenario) {
        private static readonly Dictionary<string, string> _cachedNames = new();
        private string GetDisplay()
            => this.Scenario.Name.StartsWith("$", false, CultureInfo.InvariantCulture) && uint.TryParse(this.Scenario.Name[1..], out uint key) ? 
            GameLocale.GetString(key) : this.Scenario.Name;
        public override string ToString()
            => _cachedNames.TryGetValue(Scenario.Name, out string? s) ? (s ?? Scenario.Name) : (_cachedNames[this.Scenario.Name] = GetDisplay());
    }

    public record OnOffOption(bool IsOn) {
        // TODO: Localize this :)
        public override string ToString() => IsOn ? "On" : "Off";
    }

    public record ModPackageOption(ModPackage ModPackage) {
        public override string ToString() => this.ModPackage.PackageName;
    }

    public record LobbyDropdown<T>(bool IsEnabled, Visibility IsVisible, ObservableCollection<T> Items, Action<int, int> SelectionChanged) : INotifyPropertyChanged {

        private int m_selected;
        private Visibility m_visibility = IsVisible;
        private string m_label = "";

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Selected {
            get => this.m_selected;
            set {
                int old = this.m_selected;
                this.m_selected = value;
                this.SelectionChanged.Invoke(value, old);
                this.PropertyChanged?.Invoke(this, new(nameof(Selected)));
            }
        }

        public Visibility Visibility { 
            get => this.m_visibility;
            set { 
                this.m_visibility = value;
                this.PropertyChanged?.Invoke(this, new(nameof(Visibility)));
            }
        }

        public Visibility ShouldShow => IsVisible;

        public string LabelContent {
            get => this.m_label;
            set {
                this.m_label = value;
                this.PropertyChanged?.Invoke(this, new(nameof(LabelContent)));
            }
        }

        public Visibility LabelVisibility { 
            get => (this.ShouldShow is Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
            set {
                //this.LabelVisibility = value;
                this.PropertyChanged?.Invoke(this, new(nameof(LabelVisibility)));
            }
        } 

    }

    protected static readonly ImageSource __mapNotFound = new BitmapImage(new Uri("pack://application:,,,/Resources/ingame/unknown_map.png"));

    protected static readonly LocaleKey __leaveTitle = new("LobbyView_DialogLeaveTitle");
    protected static readonly LocaleKey __leaveDesc = new("LobbyView_DialogLeaveDesc");

    protected static readonly Func<string> LOCSTR_EXIT = () => BattlegroundsInstance.Localize.GetString("LobbyView_LeaveLobby");
    protected static readonly Func<string> LOCSTR_EDIT = () => BattlegroundsInstance.Localize.GetString("LobbyView_EditCompany");
    protected static readonly Func<string> LOCSTR_START = () => BattlegroundsInstance.Localize.GetString("LobbyView_StartMatch");
    protected static readonly Func<string> LOCSTR_PLAYING = () => BattlegroundsInstance.Localize.GetString("LobbyView_PLAYING");
    protected static readonly Func<string> LOCSTR_PREPARING = () => BattlegroundsInstance.Localize.GetString("LobbyView_PREPARING");
    protected static readonly Func<string, string> LOCSTR_CANCEL = x => BattlegroundsInstance.Localize.GetString("LobbyView_CancelMatch", x);
    protected static readonly Func<string> LOCSTR_WAIT = () => BattlegroundsInstance.Localize.GetString("LobbyView_WaitMatch");

    protected readonly LobbyAPI m_handle;
    protected LobbyChatSpectatorModel? m_chatModel;

    private bool m_hasLeft;
    protected bool m_isStarting;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool SingleInstanceOnly => false;

    public LobbyButton ExitButton { get; }

    public LobbyButton EditCompanyButton { get; }

    public abstract LobbyMutButton StartMatchButton { get; }

    public abstract LobbyDropdown<ScenOp> MapDropdown { get; }

    public abstract LobbyDropdown<IGamemode> GamemodeDropdown { get; }

    public abstract LobbyDropdown<IGamemodeOption> GamemodeOptionDropdown { get; }

    public abstract LobbyDropdown<OnOffOption> WeatherDropdown { get; }

    public abstract LobbyDropdown<OnOffOption> SupplySystemDropdown { get; }

    public abstract LobbyDropdown<ModPackageOption> ModPackageDropdown { get; }

    public ImageSource? ScenarioPreview { get; set; }

    public string LobbyTitle { get; }

    public LobbyTeam Allies { get; }

    public LobbyTeam Axis { get; }

    public bool UnloadViewModel() => true;

    public LobbyModel(LobbyAPI api, LobbyAPIStructs.LobbyTeam allies, LobbyAPIStructs.LobbyTeam axis) {

        // Set basics
        this.m_handle = api;

        // Set teams
        this.Allies = new(api, allies, this);
        this.Axis = new(api, axis, this);

        // Create exit button (always behave the same)
        this.ExitButton = new(LOCSTR_EXIT(), true, new(this.LeaveLobby), Visibility.Visible, "");

        // Create edit company button (always behaves the same)
        this.EditCompanyButton = new(LOCSTR_EDIT(), false, new(this.EditCompany), Visibility.Visible, "");

        // Set title
        this.LobbyTitle = this.m_handle.Title;
        
        // Subscribe to common events
        this.m_handle.OnLobbyConnectionLost += this.OnConnectionLost;
        this.m_handle.OnLobbyCompanyUpdate += this.OnCompanyUpdated;
        this.m_handle.OnLobbyCancelStartup += this.OnMatchStartupCancelled;
        this.m_handle.OnLobbyRequestCompany += this.OnCompanyRequested;
        this.m_handle.OnLobbyCountdown += this.OnCountdownNotify;

    }

    public void SetChatModel(LobbyChatSpectatorModel chatModel)
        => this.m_chatModel = chatModel;

    protected void CancelMatch() {
        if (this.StartMatchButton.IsEnabled) {
            this.m_handle.CancelMatch();
            this.m_isStarting = false;
            if (this is LobbyParticipantModel model) {
                model.StartMatchButton.Title = LOCSTR_WAIT();
                model.StartMatchButton.IsEnabled = false;
            }
        }
    }

    protected void OnCountdownNotify(int second) {
        if (!this.m_isStarting) {
            return; // just bail
        }
        // Invoke on GUI
        Application.Current.Dispatcher.Invoke(() => {
            // Update cancel button text
            if (second < 1) {
                this.StartMatchButton.Title = LOCSTR_PREPARING();
                this.StartMatchButton.IsEnabled = false;
            } else {
                this.StartMatchButton.IsEnabled = true;
                this.StartMatchButton.Title = LOCSTR_CANCEL(second.ToString());
            }
        });
    }

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

    private void OnCompanyRequested(ServerAPI obj) {

        // Log request
        Trace.WriteLine("Received request to upload company file", nameof(LobbyHostModel));

        // Get self
        ulong selfid = this.m_handle.Self.ID;
        var self = this.m_handle.Allies.GetSlotOfMember(selfid) ?? this.m_handle.Axis.GetSlotOfMember(selfid);
        if (self is not null && self.Occupant is not null) {

            // Make sure there's a company
            if (self.Occupant.Company is null) {
                return;
            }

            // Get company name
            string companyName = self.Occupant.Company.Name;

            // Get company faction
            Faction faction = Faction.FromName(self.Occupant.Company.Army);

            // Get company
            var company = PlayerCompanies.FromNameAndFaction(companyName, faction);
            if (company is null) {
                Trace.WriteLine($"Failed to upload company json file (Company '{companyName}' not found).", nameof(LobbyHostModel));
                return;
            }

            // Get company json
            string companyJson = CompanySerializer.GetCompanyAsJson(company, indent: false);
            if (string.IsNullOrEmpty(companyJson)) {
                Trace.WriteLine($"Failed to upload company json file (Company '{companyName}' not found).", nameof(LobbyHostModel));
                return;
            }

            // Upload file
            if (obj.UploadCompany(selfid, companyJson, (a, b) => Trace.WriteLine($"Upload company progress {a}/{b}", nameof(LobbyHostModel))) is not UploadResult.UPLOAD_SUCCESS) {
                Trace.WriteLine("Failed to upload company json file.", nameof(LobbyHostModel));
            }

        } else {

            // Log request
            Trace.WriteLine("Failed to find self-instance and cannot upload company file.", nameof(LobbyHostModel));

        }

    }

    private void OnMatchStartupCancelled(LobbyMatchStartupCancelledEventArgs e) {

        // Inform user
        if (this.m_chatModel is not null) {
            this.m_chatModel.SystemMessage($"{e.CancelName} has cancelled the match.", Colors.Gray);
        }

        // Set OK
        this.m_isStarting = false;

        // Invoke on GUI
        Application.Current.Dispatcher.Invoke(() => {

            // Allow start match again
            if (this is LobbyHostModel) {
                this.StartMatchButton.IsEnabled = true;
                this.StartMatchButton.Title = LOCSTR_START();
            }

        });

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

    private void OnCompanyUpdated(LobbyCompanyChangedEventArgs e) {

        // Bail if outside accepted tids
        if (e.TeamId is < 0 or > 1) {
            return;
        }

        // Get team and notify of company change
        var team = e.TeamId == 0 ? this.Allies : this.Axis;
        team.OnTeamMemberCompanyUpdated(e.SlotId, e.Company);

    }

    protected void NotifyProperty(string property) => this.PropertyChanged?.Invoke(this, new(property));

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
