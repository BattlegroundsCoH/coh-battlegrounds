using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

using Battlegrounds.DataLocal;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Scenarios;
using Battlegrounds.Game;
using Battlegrounds.Locale;
using Battlegrounds.Modding.Content;
using Battlegrounds.Modding;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Networking.Server;
using Battlegrounds.Steam;
using Battlegrounds.UI.Modals.Prompts;
using Battlegrounds.UI.Modals;
using Battlegrounds.UI;
using Battlegrounds.Lobby.Components;
using Battlegrounds.Lobby.Helpers;
using Battlegrounds.Lobby.Pages.Host;
using Battlegrounds.Lobby.Pages.Participants;

namespace Battlegrounds.Lobby.Pages;

using static Battlegrounds.Lobby.Components.Buttons;
using static Battlegrounds.UI.AppContext;

public abstract class BaseLobby : IViewModel, INotifyPropertyChanged {

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

    public record ModPackageOption(IModPackage ModPackage) {
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

    protected static readonly LocaleKey __leaveTitle = new("LobbyView_DialogLeaveTitle");
    protected static readonly LocaleKey __leaveDesc = new("LobbyView_DialogLeaveDesc");

    protected static readonly Func<string> LOCSTR_EXIT = () => BattlegroundsContext.Localize.GetString("LobbyView_LeaveLobby");
    protected static readonly Func<string> LOCSTR_EDIT = () => BattlegroundsContext.Localize.GetString("LobbyView_EditCompany");
    protected static readonly Func<string> LOCSTR_START = () => BattlegroundsContext.Localize.GetString("LobbyView_StartMatch");
    protected static readonly Func<string> LOCSTR_PREPARING = () => BattlegroundsContext.Localize.GetString("LobbyView_PREPARING");
    protected static readonly Func<string> LOCSTR_WAIT = () => BattlegroundsContext.Localize.GetString("LobbyView_WaitMatch");
    protected static readonly Func<string, string> LOCSTR_CANCEL = x => BattlegroundsContext.Localize.GetString("LobbyView_CancelMatch", x);

    protected readonly ILobbyHandle m_handle;
    protected ChatSpectator? m_chatModel;

    private bool m_hasLeft;
    protected bool m_isStarting;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool SingleInstanceOnly => false;

    public bool KeepAlive => true;

    public bool IsLocal => this.m_handle is not OnlineLobbyHandle;

    public SimpleButton ExitButton { get; }

    public SimpleButton EditCompanyButton { get; }

    public abstract MutableButton SwapRoles { get; }

    public abstract MutableButton StartMatchButton { get; }

    public abstract Setting<ScenOp> MapDropdown { get; }

    public abstract Setting<IGamemode> GamemodeDropdown { get; }

    public abstract Setting<IGamemodeOption> GamemodeOptionDropdown { get; }

    public abstract Setting<OnOffOption> WeatherDropdown { get; }

    public abstract Setting<OnOffOption> SupplySystemDropdown { get; }

    public abstract Setting<ModPackageOption> ModPackageDropdown { get; }

    public abstract IModPackage ModPackage { get; }

    public ImageSource? ScenarioPreview { get; set; }

    public Scenario? Scenario { get; set; }

    public string LobbyTitle { get; }

    public Team Allies { get; }

    public Team Axis { get; }

    public Gamemode Gamemode { get; set; }

    public ObservableCollection<Setting> MapSetting { get; }

    public ObservableCollection<Setting> GamemodeSettings { get; }

    public ObservableCollection<Setting> AuxSettings { get; }

    public bool ShowScrollbarForSettings => this.GamemodeSettings.Count + this.AuxSettings.Count >= 4;

    public BaseLobby(ILobbyHandle api, ILobbyTeam allies, ILobbyTeam axis) {

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
        this.m_handle.OnLobbyCompanyUpdate += this.OnCompanyUpdated;

        // Subscribe to online events
        if (this.m_handle is ILobbyMatchNotifier matchNotifier) {
            matchNotifier.OnLobbyConnectionLost += this.OnConnectionLost;
            matchNotifier.OnLobbyCancelStartup += this.OnMatchStartupCancelled;
            matchNotifier.OnLobbyRequestCompany += this.OnCompanyRequested;
            matchNotifier.OnLobbyCountdown += this.OnCountdownNotify;
        }

        // Create settings container
        this.MapSetting = new();
        this.GamemodeSettings = new();
        this.AuxSettings = new();

    }

    public void UnloadViewModel(OnModelClosed closeCallback, bool requestDestroyed) {

        // Check if a destroy command is requested.
        if (requestDestroyed) {
            // Destroy this isntance (to avoid annoying stuff, like trying to reconnect to an existing one...)
            GetViewManager().DestroyView(this);
        }

        // use default here, since we cannot exit unless exit lobby button is pressed or hard exit
        closeCallback(false);

    }

    public void Swapback() {

        if (this.GetSelf() is ILobbySlot self && self.Occupant is not null) {
            Task.Run(() => this.m_handle.MemberState(self.Occupant.MemberID, self.TeamID, self.SlotID, LobbyMemberState.Waiting));
        }

    }

    protected void EditCompany() {

        // Try get company
        if (this.TryGetSelectedCompany(out var _) is Company company) {

            // Grab VM
            var vm = GetViewManager();

            // Create company builder
            IViewModel builder = vm.CreateView("CompanyEditor", company);
            if (builder is IReturnable returnable) {
                returnable.ReturnTo = this;
            }

            // Set RHS
            GetViewManager().UpdateDisplay(AppDisplayTarget.Right, new(builder), false);

            // Inform others
            if (this.GetSelf() is ILobbySlot self && self.Occupant is not null) {
                Task.Run(() => this.m_handle.MemberState(self.Occupant.MemberID, self.TeamID, self.SlotID, LobbyMemberState.EditCompany));
            }

        }

    }

    public void SetChatModel(ChatSpectator chatModel)
        => this.m_chatModel = chatModel;

    protected void CancelMatch() {
        if (this.StartMatchButton.IsEnabled) {
            this.m_handle.CancelMatch();
            this.m_isStarting = false;
            if (this is ParticipantLobby model) {
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

    protected void PlanMatch() {

        // Bail if chat for some reason is null
        if (this.m_chatModel is null) {
            Trace.WriteLine("Chat model was null!", nameof(PlanMatch));
            return;
        }

        // Create match planner
        var planner = new Planner(new(this, this.m_chatModel, this.m_handle));

        // Set RHS
        GetViewManager().UpdateDisplay(AppDisplayTarget.Right, planner, false);

        // Inform others
        if (this.GetSelf() is ILobbySlot self && self.Occupant is not null) {
            Task.Run(() => this.m_handle.MemberState(self.Occupant.MemberID, self.TeamID, self.SlotID, LobbyMemberState.Planning));
        }

    }

    protected void LeaveLobby() {

        // Do modal
        YesNoPrompt.Show((_, result) => {

            // Check return value
            if (result is not ModalDialogResult.Confirm) {
                return;
            }

            // Set flag
            this.m_hasLeft = true;

            // Leave lobby
            Task.Run(this.m_handle.CloseHandle);

            // Get the view manager
            var vm = GetViewManager();

            // Go back to browser view
            vm.SetDisplay(AppDisplayState.LeftRight, "LeftMenu", "LobbyBrowser");

            // Destroy chat
            if (this.m_chatModel is not null) {
                vm.DestroyView(this.m_chatModel);
            }

        }, __leaveTitle, __leaveDesc);

    }

    public ILobbySlot? GetSelf() {

        // Get self
        var selfid = this.m_handle.Self.ID;
        return this.m_handle.Allies.GetSlotOfMember(selfid) ?? this.m_handle.Axis.GetSlotOfMember(selfid);

    }

    internal Company? TryGetSelectedCompany(out ulong selfid) {

        var self = GetSelf();
        if (self is not null && self.Occupant is not null) {

            // Set selfid
            selfid = self.Occupant.MemberID;

            // Make sure there's a company
            if (self.Occupant.Company is null) {
                Trace.WriteLine("Attempt to get self company returned null (self company was null).", nameof(TryGetSelectedCompany));
                return null;
            }

            // Get company name
            string companyName = self.Occupant.Company.Name;

            // Get company faction
            Faction faction = Faction.FromName(self.Occupant.Company.Army);

            // Get company
            var company = Companies.FromNameAndFaction(companyName, faction);
            if (company is null) {
                Trace.WriteLine($"Failed to fetch company json file (Company '{companyName}' not found).", nameof(TryGetSelectedCompany));
                return null;
            }

            // Return the found company
            return company;

        }

        // Return nothing
        selfid = 0;
        return null;

    }

    private void OnCompanyRequested(ServerAPI obj) {

        // Log request
        Trace.WriteLine("Received request to upload company file.", nameof(OnCompanyRequested));

        // Try get
        if (this.TryGetSelectedCompany(out ulong selfid) is Company company) {

            // Get company json
            string companyJson = CompanySerializer.GetCompanyAsJson(company, indent: false);
            if (string.IsNullOrEmpty(companyJson)) {
                Trace.WriteLine($"Failed to upload company json file (Company '{company.Name}' not found).", nameof(OnCompanyRequested));
                return;
            }

            // Upload file
            var encoded = Encoding.UTF8.GetBytes(companyJson);
            if (this.m_handle.UploadCompanyFile(encoded, selfid, (a, b, _) => Trace.WriteLine($"Upload company progress {a}/{b}", nameof(OnCompanyRequested))) is not UploadResult.UPLOAD_SUCCESS) {
                Trace.WriteLine("Failed to upload company json file.", nameof(OnCompanyRequested));
            }

        } else {

            // Log request
            Trace.WriteLine("Failed to find self-instance and cannot upload company file.", nameof(OnCompanyRequested));

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
            if (this is HostLobby) {
                this.StartMatchButton.IsEnabled = true;
                this.StartMatchButton.Title = LOCSTR_START();
            }

        });

    }

    private void OnConnectionLost(string reason) {

        // Do nothing if already left
        if (this.m_hasLeft) {
            return;
        }

        // Decide on title
        string modalTitle = BattlegroundsContext.Localize.GetString(reason switch {
            "KICK" => "LobbyView_DialogKickTitle",
            "CLOSED" => "LobbyView_DialogCloseTitle",
            _ => "LobbyView_DialogLostTitle"
        });

        // Decide on desc
        string modalDesc = BattlegroundsContext.Localize.GetString(reason switch {
            "KICK" => "LobbyView_DialogKickDesc",
            "CLOSED" => "LobbyView_DialogCloseDesc",
            _ => "LobbyView_DialogLostDesc"
        });

        // Goto GUI thread and show connection lost.
        Application.Current.Dispatcher.Invoke(() => {

            // Do modal
            OKPrompt.Show(GetModalControl(), (_, resault) => {

                // Check return value
                if (resault is not ModalDialogResult.Confirm) {
                    return;
                }

                // Get view manager
                var vm = GetViewManager();

                // Go back to browser view
                //vm.SetDisplay(AppDisplayState.LeftRight, typeof(LeftMenu), typeof(LobbyBrowserViewModel));

                // Destroy chat
                if (this.m_chatModel is not null) {
                    vm.DestroyView(this.m_chatModel);
                }

            }, modalTitle, modalDesc);

        });

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

    public void NotifyProperty(string property) => this.PropertyChanged?.Invoke(this, new(property));

    protected bool IsReady() {

        // If not steam is running
        if (!SteamInstance.IsSteamRunning) {
            this.m_chatModel?.SystemMessage("You must be logged into Steam!", Colors.Yellow);
            return false;
        }

        // Make sure CoH2 is not running
        if (GameProcess.IsRunning(GameCase.CompanyOfHeroes2)) {
            this.m_chatModel?.SystemMessage("You must exit Company of Heroes 2!", Colors.Yellow);
            return false;
        }

        // Return true
        return true;

    }

    public static BaseLobby? CreateModelAsHost(ILobbyHandle handler) {

        // Check allies
        if (handler.Allies is null) {
            Trace.WriteLine($"{nameof(CreateModelAsHost)} is returning null has Allies were not defined.", nameof(CreateModelAsHost));
            return null;
        }

        // Check axis
        if (handler.Axis is null) {
            Trace.WriteLine($"{nameof(CreateModelAsHost)} is returning null has Axis were not defined.", nameof(CreateModelAsHost));
            return null;
        }

        // Return model
        return new HostLobby(handler, handler.Allies, handler.Axis);

    }

    public static BaseLobby? CreateModelAsParticipant(ILobbyHandle handler) {

        // Check allies
        if (handler.Allies is null) {
            Trace.WriteLine($"{nameof(CreateModelAsParticipant)} is returning null has Allies were not defined.", nameof(CreateModelAsParticipant));
            return null;
        }

        // Check axis
        if (handler.Axis is null) {
            Trace.WriteLine($"{nameof(CreateModelAsParticipant)} is returning null has Allies were not defined.", nameof(CreateModelAsParticipant));
            return null;
        }

        // Return model
        return new ParticipantLobby(handler, handler.Allies, handler.Axis);

    }

}
