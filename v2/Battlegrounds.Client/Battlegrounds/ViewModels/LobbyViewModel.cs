using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;
using Battlegrounds.Models.Statistics;
using Battlegrounds.Services;
using Battlegrounds.ViewModels.LobbyHelpers;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Battlegrounds.ViewModels;

public sealed class LobbyViewModel : INotifyPropertyChanged, IAsyncDisposable {

    public const int MAX_CHAT_MESSAGE_LENGTH = 180; // Maximum length of a chat message
    public const string MAX_MESSAGE_LENGTH_REACHED = "Chat message truncated to 180 characters.";

    private readonly ILogger<LobbyViewModel> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly ILobby _lobby;
    private readonly ILobbyService _lobbyService;
    private readonly IPlayService _playService;
    private readonly IReplayService _replayService;
    private readonly ICompanyService _companyService;
    private readonly IGameMapService _gameMapService;
    private readonly IStatisticsService _statisticsService;
    private readonly ObservableCollection<ChatMessageViewModel> _chatMessages = [];
    private readonly Dictionary<FactionAlliance, List<Company>> _localPlayerCompaniesByAlliance = [];
    private readonly Dictionary<string, Company> _lobbyCompanies = [];
    private readonly MainWindowViewModel _mainWindowVm;
    private readonly SynchronizationContext? _uiContext;
    private readonly CancellationTokenSource _lifetimeCts = new();
    private readonly Task _lifetimeTask;

    private ICollection<LobbySlotViewModel> _team1Slots = [];
    private ICollection<LobbySlotViewModel> _team2Slots = [];
    private ICollection<Map> _availableMaps = [];
    private readonly ObservableCollection<LobbySettingViewModel> _settings = [];

    private PickableChatChannel _selectedChatChannel = new PickableChatChannel("all"); // TODO: Support chat channels properly
    private Map _selectedMap;
    private Map _draftSelectedMap;

    private string _chatMessage = string.Empty;
    private string _state = "Loading match information";

    private bool _isPlaying = false;
    private bool _isMatchStarting = false;
    private bool _isWaitingForMatchOver = false;
    private bool _disposed;
    private long _lastAppliedRevision;
    private LobbyConnectionState _connectionState;

    private MatchOverViewModel? _matchOverResult;
    private CompanyPreviewViewModel? _companyPreviewResult;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the view model for the post-match results overlay, or <see langword="null"/> when the overlay is not visible.
    /// </summary>
    public MatchOverViewModel? MatchOverResult {
        get => _matchOverResult;
        private set {
            if (value == _matchOverResult) return;
            _matchOverResult = value;
            PropertyChanged?.Invoke(this, new(nameof(MatchOverResult)));
        }
    }

    /// <summary>
    /// Gets the view model for the company preview overlay, or <see langword="null"/> when the overlay is not visible.
    /// </summary>
    public CompanyPreviewViewModel? CompanyPreviewResult {
        get => _companyPreviewResult;
        private set {
            if (value == _companyPreviewResult) return;
            _companyPreviewResult = value;
            PropertyChanged?.Invoke(this, new(nameof(CompanyPreviewResult)));
        }
    }

    public string LobbyName => _lobby.Name;

    public ILobby Model => _lobby;

    public IAsyncRelayCommand LeaveCommand { get; }

    public IAsyncRelayCommand SendMessageCommand { get; }

    public IAsyncRelayCommand StartMatchCommand { get; }

    public IAsyncRelayCommand ToggleReadyCommand { get; }

    public IAsyncRelayCommand<Map> SetMapCommand { get; }

    public bool IsHost => _lobby.IsHost;

    public bool IsReady => _lobby.IsReady;

    public LobbyConnectionState ConnectionState {
        get => _connectionState;
        private set {
            if (_connectionState == value) return;
            _connectionState = value;
            PropertyChanged?.Invoke(this, new(nameof(ConnectionState)));
            PropertyChanged?.Invoke(this, new(nameof(IsConnected)));
        }
    }

    public bool IsConnected => ConnectionState == LobbyConnectionState.Connected;

    public IReadOnlyDictionary<FactionAlliance, List<Company>> CompaniesByAlliance => _localPlayerCompaniesByAlliance;

    public IReadOnlyDictionary<string, Company> LobbyCompanies => _lobbyCompanies;

    public bool CanStartMatch {
        get {
            if (!_lobby.IsHost)
                return false;
            if (_isPlaying || _isMatchStarting || _isWaitingForMatchOver)
                return false;
            var team1Ready = _lobby.Team1.Slots.Any(x => x.ParticipantId is not null && !string.IsNullOrEmpty(x.CompanyId));
            var team2Ready = _lobby.Team2.Slots.Any(x => x.ParticipantId is not null && !string.IsNullOrEmpty(x.CompanyId));
            return team1Ready && team2Ready;
        }
    }

    public ObservableCollection<ChatMessageViewModel> ChatMessages => _chatMessages;

    public string GameId => _lobby.Game.Id;

    public ICollection<LobbySlotViewModel> Team1Slots {
        get => _team1Slots;
        private set {
            if (value == _team1Slots) return;
            _team1Slots = value;
            PropertyChanged?.Invoke(this, new(nameof(Team1Slots)));
        }
    }

    public ICollection<LobbySlotViewModel> Team2Slots {
        get => _team2Slots;
        private set {
            if (value == _team2Slots) return;
            _team2Slots = value;
            PropertyChanged?.Invoke(this, new(nameof(Team2Slots)));
        }
    }

    public ICollection<Map> AvailableMaps {
        get => _availableMaps;
        private set {
            if (value == _availableMaps) return;
            _availableMaps = value;
            PropertyChanged?.Invoke(this, new(nameof(AvailableMaps)));
        }
    }

    public Map SelectedMap => _selectedMap;

    public Map DraftSelectedMap {
        get => _draftSelectedMap ?? _selectedMap;
        set {
            if (value is null || _draftSelectedMap == value) return;
            _draftSelectedMap = value;
            PropertyChanged?.Invoke(this, new(nameof(DraftSelectedMap)));
            PropertyChanged?.Invoke(this, new(nameof(SelectedMapPreview)));
            SetMapCommand.NotifyCanExecuteChanged();
        }
    }

    public string SelectedMapPreview => $"pack://siteoforigin:,,,/Assets/Scenarios/{_lobby.Game.Id}/mm/{DraftSelectedMap.Preview}.png";

    public ObservableCollection<LobbySettingViewModel> SelectedSettings => _settings;

    public string ChatMessage {
        get => _chatMessage;
        set {
            if (value == _chatMessage)
                return;
            if (value.Length > MAX_CHAT_MESSAGE_LENGTH) {
                SystemWarnMessageTooLong(); // Warn user that message was truncated
                _chatMessage = value[..MAX_CHAT_MESSAGE_LENGTH]; // Limit chat message length to MAX_CHAT_MESSAGE_LENGTH characters
            } else {
                _chatMessage = value;
            }
            PropertyChanged?.Invoke(this, new(nameof(ChatMessage)));
        }
    }

    public bool IsPlaying {
        get => _isPlaying;
        private set {
            if (value == _isPlaying) return;
            _isPlaying = value;
            PropertyChanged?.Invoke(this, new(nameof(IsPlaying)));
            PropertyChanged?.Invoke(this, new(nameof(CanStartMatch)));
        }
    }

    public bool IsMatchStarting {
        get => _isMatchStarting;
        private set {
            if (value == _isMatchStarting) return;
            _isMatchStarting = value;
            PropertyChanged?.Invoke(this, new(nameof(IsMatchStarting)));
            PropertyChanged?.Invoke(this, new(nameof(CanStartMatch)));
        }
    }

    public bool IsWaitingForMatchOver {
        get => _isWaitingForMatchOver;
        private set {
            if (value == _isWaitingForMatchOver) return;
            _isWaitingForMatchOver = value;
            PropertyChanged?.Invoke(this, new(nameof(IsWaitingForMatchOver)));
        }
    }

    public string LobbyState {
        get => _state;
        set {
            if (value == _state) return;
            _state = value;
            _logger.LogInformation("Lobby state changed to: {State}", _state);
            PropertyChanged?.Invoke(this, new(nameof(LobbyState)));
        }
    }

    public string TrayMessage {
        get;
        set {
            if (value == field) return;
            field = value;
            _logger.LogInformation("Tray message changed to: {Message}", field);
            PropertyChanged?.Invoke(this, new(nameof(TrayMessage)));
        }
    } = string.Empty;

    public PickableChatChannel[] AvailableChatChannels => [new PickableChatChannel("all"), new PickableChatChannel("team")];

    public PickableChatChannel SelectedChatChannel {
        get => _selectedChatChannel;
        set {
            if (_selectedChatChannel == value) return;
            _selectedChatChannel = value;
        }
    }

    public string? LocalParticipant => _lobby.GetLocalPlayerId();

    public LobbyViewModel(ILobby lobby, IServiceProvider serviceProvider, ILogger<LobbyViewModel> logger) {
        // Probably an anti-pattern to pass IServiceProvider instead of the specific services, but this class has many dependencies
        // So... collect the services in a facade class to make it easier to test and maintain (Probably also solves the comment regarding a separate controller class for the StartGame method)

        _lobby = lobby;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeProvider = serviceProvider.GetRequiredService<TimeProvider>();
        _lobbyService = serviceProvider.GetRequiredService<ILobbyService>();
        _playService = serviceProvider.GetRequiredService<IPlayService>();
        _replayService = serviceProvider.GetRequiredService<IReplayService>();
        _companyService = serviceProvider.GetRequiredService<ICompanyService>();
        _gameMapService = serviceProvider.GetRequiredService<IGameMapService>();
        _statisticsService = serviceProvider.GetRequiredService<IStatisticsService>();
        _mainWindowVm = serviceProvider.GetRequiredService<MainWindowViewModel>();
        _uiContext = SynchronizationContext.Current is System.Windows.Threading.DispatcherSynchronizationContext
            ? SynchronizationContext.Current
            : null;
        _connectionState = lobby.ConnectionState;
        _selectedMap = lobby.Map;
        _draftSelectedMap = lobby.Map;

        LeaveCommand = new AsyncRelayCommand(_ => LeaveLobby());
        SendMessageCommand = new AsyncRelayCommand(SendChatMessage);
        StartMatchCommand = new AsyncRelayCommand(StartGame);
        ToggleReadyCommand = new AsyncRelayCommand(ToggleReady);
        SetMapCommand = new AsyncRelayCommand<Map>(SetMap, map => map is not null && map != _selectedMap);

        _lifetimeTask = RunSupervisedAsync(_lifetimeCts.Token);

    }

    private async Task RunSupervisedAsync(CancellationToken cancellationToken) {
        try {
            var maps = (await _gameMapService.GetMapsForGame(_lobby.Game.Id)).Select(Map.FromScenario).ToArray();
            var team1Slots = await MapTeamSlotsToLobbySlots(0, _lobby.Team1.Slots);
            var team2Slots = await MapTeamSlotsToLobbySlots(1, _lobby.Team2.Slots);
            cancellationToken.ThrowIfCancellationRequested();
            await InvokeOnUiAsync(() => {
                SyncLobbySettings();
                AvailableMaps = maps;
                Team1Slots = team1Slots;
                Team2Slots = team2Slots;
                SyncState();
            }, cancellationToken);
            await LoadLocalPlayerCompanies(cancellationToken);
            await PollLobbyEvents(cancellationToken);
        } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            // Normal view disposal.
        } catch (Exception ex) {
            _logger.LogError(ex, "Lobby view initialization/event loop failed.");
            await InvokeOnUiAsync(() => {
                LobbyState = "Lobby synchronization failed";
                ConnectionState = LobbyConnectionState.Disconnected;
            }, CancellationToken.None);
        }
    }

    private void SyncState() {
        if (!CanStartMatch) {
            LobbyState = "Waiting for players to select companies and factions";
            PropertyChanged?.Invoke(this, new(nameof(CanStartMatch)));
            return;
        }
        LobbyState = "Ready to start the match";
        PropertyChanged?.Invoke(this, new(nameof(CanStartMatch)));
    }

    private void SyncLobbySettings() {
        foreach (var setting in _lobby.Settings.OrderBy(x => x.Priority)) {
            var existing = SelectedSettings.FirstOrDefault(x => x.Name == setting.Name);
            if (existing is not null) {
                existing.ApplyServerValue(setting.Value);
                continue;
            }
            SelectedSettings.Add(new LobbySettingViewModel(setting, SetSetting));
        }
    }

    private async Task LoadLocalPlayerCompanies(CancellationToken cancellationToken) {

        string[] factions = _lobby.Game.FactionIds ?? [];
        var localPlayerCompanies = (await _companyService.GetLocalCompaniesAsync()).ToArray();
        foreach (string faction in factions) {
            cancellationToken.ThrowIfCancellationRequested();
            var alliance = _lobby.Game.GetFactionAlliance(faction);
            if (!_localPlayerCompaniesByAlliance.TryGetValue(alliance, out var existingCompanies)) {
                _localPlayerCompaniesByAlliance[alliance] = existingCompanies = [];
            }
            var factionCompanies = (from c in localPlayerCompanies where c.Faction == faction select c).ToArray();
            if (factionCompanies.Length == 0) {
                continue; // No companies for this faction
            }
            foreach (var toCache in factionCompanies) {
                if (!_lobbyCompanies.ContainsKey(toCache.Id)) {
                    _lobbyCompanies[toCache.Id] = toCache; // Cache company in lobby
                }
            }
            existingCompanies.AddRange(factionCompanies); // Filter existing?
        }

        var (team, slotId) = _lobby.GetLocalPlayerSlot();
        if (team is null) {
            return;
        }

        var slot = team.Slots[slotId];
        if (!string.IsNullOrEmpty(slot.CompanyId)) {
            return; // Preserve the server-confirmed company selected in the lobby snapshot
        }
        var factionAlliance = _lobby.Game.GetFactionAlliance(slot.Faction);
        if (string.IsNullOrEmpty(slot.Faction)) {
            var teamAlliance = team.TeamType switch {
                TeamType.Allies => FactionAlliance.Allies,
                TeamType.Axis => FactionAlliance.Axis,
                _ => FactionAlliance.Unspecified
            };
            if (teamAlliance is FactionAlliance.Unspecified) {
                return; // Cannot determine alliance for team, so cannot determine which companies to show
            }
            factionAlliance = teamAlliance;
        }

        if (!_localPlayerCompaniesByAlliance.TryGetValue(factionAlliance, out var allianceCompanies)) {
            return;
        }
        var company = allianceCompanies.FirstOrDefault();
        if (company is null) {
            return;
        }

        await _lobby.SetCompany(team, slotId, company.Id, company.Faction);

    }

    private async Task PollLobbyEvents(CancellationToken cancellationToken) {
        while (_lobby.IsActive && !cancellationToken.IsCancellationRequested) {
            LobbyEvent? lobbyEvent;
            try {
                lobbyEvent = await _lobby.GetNextEvent().AsTask().WaitAsync(cancellationToken);
            } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                break;
            }
            if (lobbyEvent is null) {
                break;
            }

            await InvokeOnUiAsync(async () => {
                if (IsRevisionedStateEvent(lobbyEvent.EventType)
                    && lobbyEvent.Revision > 0
                    && lobbyEvent.Revision <= _lastAppliedRevision) {
                    return;
                }
                if (IsRevisionedStateEvent(lobbyEvent.EventType)) {
                    _lastAppliedRevision = Math.Max(_lastAppliedRevision, lobbyEvent.Revision);
                }
                switch (lobbyEvent.EventType) {
                    case LobbyEventType.ConnectionStateChanged:
                        if (lobbyEvent.Arg is LobbyConnectionState connectionState) {
                            ConnectionState = connectionState;
                        } else if (lobbyEvent.Arg is false)
                        {
                            await LeaveLobby();
                        }
                        break;
                    case LobbyEventType.SnapshotApplied:
                        _selectedMap = _lobby.Map;
                        _draftSelectedMap = _lobby.Map;
                        SyncLobbySettings();
                        Team1Slots = await MapTeamSlotsToLobbySlots(0, _lobby.Team1.Slots);
                        Team2Slots = await MapTeamSlotsToLobbySlots(1, _lobby.Team2.Slots);
                        PropertyChanged?.Invoke(this, new(nameof(SelectedMap)));
                        PropertyChanged?.Invoke(this, new(nameof(DraftSelectedMap)));
                        PropertyChanged?.Invoke(this, new(nameof(SelectedMapPreview)));
                        SetMapCommand.NotifyCanExecuteChanged();
                        break;
                    case LobbyEventType.ParticipantMessage:
                        if (lobbyEvent.Arg is not ChatMessage chatEvent) {
                            break; // Error?
                        }
                        bool isSelf = chatEvent.SenderId == _lobby.GetLocalPlayerId();
                        var (localPlayerTeam, _) = _lobby.GetLocalPlayerSlot();
                        bool isAllied = localPlayerTeam?.Participants.Any(x => x == chatEvent.SenderId) ?? false;
                        ChatMessages.Add(new ChatMessageViewModel(DateTime.Now, chatEvent.Channel, isSelf, isAllied, chatEvent.Sender, chatEvent.Message));
                        PropertyChanged?.Invoke(this, new(nameof(ChatMessages)));
                        break;
                    case LobbyEventType.ParticipantJoined:
                        if (lobbyEvent.Arg is not Participant newParticipant) {
                            break;
                        }
                        ChatMessages.Add(new ChatMessageViewModel(DateTime.Now, ChatChannel.All, false, false, "System", $"{newParticipant.ParticipantName} has joined the lobby.", IsSystemMessage: true));
                        PropertyChanged?.Invoke(this, new(nameof(ChatMessages)));
                        break;
                    case LobbyEventType.ParticipantLeft:
                        if (lobbyEvent.Arg is not Participant leftParticipant) {
                            break;
                        }
                        ChatMessages.Add(new ChatMessageViewModel(DateTime.Now, ChatChannel.All, false, false, "System", $"{leftParticipant.ParticipantName} has left the lobby.", IsSystemMessage: true));
                        PropertyChanged?.Invoke(this, new(nameof(ChatMessages)));
                        break;
                    case LobbyEventType.TeamUpdated:
                        var updatedTeam = lobbyEvent.Arg switch {
                            TeamType teamType => teamType,
                            int teamId when teamId == 0 => _lobby.Team1.TeamType,
                            int teamId when teamId == 1 => _lobby.Team2.TeamType,
                            _ => (TeamType?)null
                        };
                        if (updatedTeam is null) {
                            _logger.LogWarning("Received TeamUpdated lobby event with invalid argument: {Arg}", lobbyEvent.Arg);
                            break;
                        }
                        if (updatedTeam.Value == _lobby.Team1.TeamType) {
                            Team1Slots = await MapTeamSlotsToLobbySlots(0, _lobby.Team1.Slots);
                        }
                        if (updatedTeam.Value == _lobby.Team2.TeamType) {
                            Team2Slots = await MapTeamSlotsToLobbySlots(1, _lobby.Team2.Slots);
                        }
                        break;
                    case LobbyEventType.UpdatedCompany:
                        if (lobbyEvent.Arg is not Company updatedCompany) {
                            break;
                        }
                        _lobbyCompanies[updatedCompany.Id] = updatedCompany;
                        break;
                    case LobbyEventType.MapUpdated:
                        if (lobbyEvent.Arg is not Map updatedMap) {
                            break;
                        }
                        if (updatedMap == _selectedMap) {
                            break; // No change
                        }
                        _selectedMap = updatedMap; // NOP if already selected (so NOP for host)
                        _draftSelectedMap = updatedMap;

                        // Update team slots as well, since some slots may become hidden/unhidden based on map selection
                        Team1Slots = await MapTeamSlotsToLobbySlots(0, _lobby.Team1.Slots);
                        Team2Slots = await MapTeamSlotsToLobbySlots(1, _lobby.Team2.Slots);
                        PropertyChanged?.Invoke(this, new(nameof(SelectedMap)));
                        PropertyChanged?.Invoke(this, new(nameof(DraftSelectedMap)));
                        PropertyChanged?.Invoke(this, new(nameof(SelectedSettings)));
                        PropertyChanged?.Invoke(this, new(nameof(SelectedMapPreview)));
                        SetMapCommand.NotifyCanExecuteChanged();
                        break;
                    case LobbyEventType.SettingUpdated:
                        if (lobbyEvent.Arg is LobbySetting newLobbySetting) {
                            var settingVm = SelectedSettings.FirstOrDefault(x => x.Name == newLobbySetting.Name);
                            if (settingVm is not null) {
                                settingVm.Visibility = newLobbySetting.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                                settingVm.ApplyServerValue(newLobbySetting.Value);
                                PropertyChanged?.Invoke(this, new(nameof(SelectedSettings)));
                            } else {
                                SyncLobbySettings(); // If we can't find the setting, just resync all settings (should be rare)
                            }
                        }
                        break;
                    case LobbyEventType.GameStarted:
                        var launched = await _playService.LaunchGameApp(_lobby.Game); // Will never happen in singleplayer, but will happen for non-host participants in multiplayer when host starts the game
                        if (launched.Failed) {
                            // TODO: Inform the lobby that the local player failed to launch the game, so host can handle it (probably by cancelling the game start and returning to lobby)
                        }
                        break;
                    case LobbyEventType.TrayMessage:
                        if (lobbyEvent.Arg is not string trayMessage) {
                            _logger.LogWarning("Received TrayMessage lobby event with invalid argument: {Arg}", lobbyEvent.Arg);
                            break;
                        }
                        TrayMessage = trayMessage;
                        break;
                    case LobbyEventType.TrayMessageHide:
                        TrayMessage = string.Empty;
                        break;
                    case LobbyEventType.MatchOver:
                        await ShowMatchResults();
                        break;
                    case LobbyEventType.SlotCompanyDownloadProgress:
                        if (lobbyEvent.Arg is SlotCompanyDownloadUpdate download
                            && download.TeamId is >= 0 and < 2
                            && _lobby.GetSlotRevision(download.TeamId, download.SlotId) == download.SlotRevision) {
                            var slots = download.TeamId == 0 ? Team1Slots : Team2Slots;
                            var slotVm = slots.FirstOrDefault(x => x.Slot.Index == download.SlotId);
                            if (slotVm is null
                                || !string.Equals(slotVm.Slot.CompanyId, download.CompanyId, StringComparison.Ordinal)) {
                                break;
                            }
                            if (download.Progress is float progress) {
                                slotVm.CompanyDownloadProgress = progress;
                                PropertyChanged?.Invoke(this, new(nameof(Team1Slots)));
                                PropertyChanged?.Invoke(this, new(nameof(Team2Slots)));
                                if (progress >= 1.0f) {
                                    _ = HideDownloadProgressAfterDelay(download.TeamId, download.SlotId, download.CompanyId, download.SlotRevision, cancellationToken);
                                }
                            }
                            if (download.Company is Company company && company.Id == download.CompanyId) {
                                var updatedSlot = slotVm.WithServerCompany(company);
                                ICollection<LobbySlotViewModel> updatedSlots = [.. slots.Except([slotVm]).Append(updatedSlot).OrderBy(x => x.Slot.Index)];
                                if (download.TeamId == 0) {
                                    Team1Slots = updatedSlots;
                                } else {
                                    Team2Slots = updatedSlots;
                                }
                                PropertyChanged?.Invoke(this, new(nameof(Team1Slots)));
                                PropertyChanged?.Invoke(this, new(nameof(Team2Slots)));
                            }
                        } else if (lobbyEvent.Arg is ValueTuple<int, int, float> legacyProgress) {
                            var (teamId, slotId, progress) = legacyProgress;
                            var slotVm = (teamId == 0 ? Team1Slots : Team2Slots).FirstOrDefault(x => x.Slot.Index == slotId);
                            if (slotVm is not null) {
                                slotVm.CompanyDownloadProgress = progress;
                                PropertyChanged?.Invoke(this, new(nameof(Team1Slots)));
                                PropertyChanged?.Invoke(this, new(nameof(Team2Slots)));
                                if (progress >= 1.0f) {
                                    _ = HideDownloadProgressAfterDelay(
                                        teamId,
                                        slotId,
                                        slotVm.Slot.CompanyId,
                                        _lobby.GetSlotRevision(teamId, slotId),
                                        cancellationToken);
                                }
                            }
                        } else if (lobbyEvent.Arg is ValueTuple<int, int, Company> legacyCompany) {
                            var (teamId, slotId, company) = legacyCompany;
                            var slots = teamId == 0 ? Team1Slots : Team2Slots;
                            var slotVm = slots.FirstOrDefault(x => x.Slot.Index == slotId);
                            if (slotVm is not null) {
                                var updatedSlot = slotVm.WithServerCompany(company);
                                ICollection<LobbySlotViewModel> updatedSlots = [.. slots.Except([slotVm]).Append(updatedSlot).OrderBy(x => x.Slot.Index)];
                                if (teamId == 0) Team1Slots = updatedSlots;
                                else Team2Slots = updatedSlots;
                            }
                        }
                        break;
                    default:
                        break;
                }
                SyncState();
            }, cancellationToken);
        }
    }

    private static bool IsRevisionedStateEvent(LobbyEventType eventType) => eventType is
        LobbyEventType.ParticipantJoined or
        LobbyEventType.ParticipantLeft or
        LobbyEventType.ParticipantUpdated or
        LobbyEventType.ParticipantReady or
        LobbyEventType.ParticipantUnready or
        LobbyEventType.TeamUpdated or
        LobbyEventType.SlotUpdated or
        LobbyEventType.SettingUpdated or
        LobbyEventType.MapUpdated or
        LobbyEventType.SnapshotApplied;

    private async Task<List<LobbySlotViewModel>> MapTeamSlotsToLobbySlots(int index, Team.Slot[] slots) {
        List<LobbySlotViewModel> result = [];
        foreach (var slot in slots) {
            var task = await MapToLobbySlot(index, slot);
            result.Add(task);
        }
        return result;
    }

    private async Task LeaveLobby(bool forceLeave = false) {
        if (!forceLeave) {
            // TODO: Show confirmation dialog before leaving lobby?
            if (!_lobby.IsActive) {
                return; // Already left
            }

            await _lobbyService.LeaveLobbyAsync(_lobby);
        }

        if (_lobby is SingleplayerLobby) {
            _mainWindowVm.IsHomeButtonActive = true; // Return to home view for singleplayer lobby
            _mainWindowVm.SetContent(null);
        } else {
            _mainWindowVm.IsMultiplayerButtonActive = true; // Return to multiplayer view for multiplayer lobby
            _mainWindowVm.SetContent(null);
        }
    }

    private async Task SendChatMessage() {
        string msg = ChatMessage.Trim();
        ChatMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(msg)) {
            return;
        }
        if (msg.Length > MAX_CHAT_MESSAGE_LENGTH) {
            msg = msg[..MAX_CHAT_MESSAGE_LENGTH]; // Limit chat message length to MAX_CHAT_MESSAGE_LENGTH characters
            SystemWarnMessageTooLong(); // Warn user that message was truncated
        }
        await _lobby.SendMessage(SelectedChatChannel.ChannelName switch {
            "all" => ChatChannel.All,
            "team" => ChatChannel.Team,
            _ => ChatChannel.All // Default to All if unknown channel
        }, msg);
    }

    private void SystemWarnMessageTooLong() {
        if (ChatMessages.OrderByDescending(x => x.Timestamp).FirstOrDefault() is not ChatMessageViewModel { IsSystemMessage: true, Message: MAX_MESSAGE_LENGTH_REACHED }) {
            ChatMessages.Add(new ChatMessageViewModel(DateTime.Now, ChatChannel.All, true, false, "System", MAX_MESSAGE_LENGTH_REACHED, IsSystemMessage: true));
        }
    }

    private async Task StartGame() { // TODO: Move to a separate controller class?

        if (!CanStartMatch) {
            return; // Should never happen, but just in case
        }

        // Notify server we're starting
        // Will freeze lobby state for all other participants and prevent any further changes to the lobby (e.g. changing companies, maps, settings, etc.) and will also prevent new participants from joining
        // (NOP in singleplayer)
        await _lobby.BeginMatch();

        EndMatchReason reason = EndMatchReason.Unknown;
        try {
            IsMatchStarting = true;
            LobbyState = "Starting match...";

            // Sync corrent lobby view status with backing model based on selected PickableCompany (based on host client view!)
            var synced = SyncLobbyCompanies(); // Start syncing companies (but do not await yet, as we can do this in parallel count down)

            // Check all players have marked themselves ready
            int realPlayerCount = _lobby.GetRealPlayersCount();
            int markedReadyCount = 0;
            foreach (var slot in _lobby.Team1.Slots.Concat(_lobby.Team2.Slots)) {
                if (string.IsNullOrEmpty(slot.ParticipantId)) {
                    continue; // Slot not occupied
                }
                var particpant = _lobby.GetParticipant(slot.ParticipantId);
                if (particpant is not null && particpant.IsReady) {
                    markedReadyCount++;
                }
            }

            // Wait a few seconds to allow players to mark themselves ready (if they haven't already), but do not wait too long as host has already decided to start the match
            // If there is only 1 player, do not wait at all. If all players are marked ready, wait 3 seconds. Otherwise, wait 10 seconds to give players a chance to mark themselves ready
            int waitSeconds = realPlayerCount switch {
                1 => 0,
                _ when realPlayerCount == markedReadyCount => 3,
                _ => 10
            };
            for (int i = waitSeconds; i > 0; i--) {
                LobbyState = $"Starting match in {i} second{(i > 1 ? "s" : string.Empty)}...";
                await _lobby.PublishSystemMessage($"Match starting in {i} second{(i > 1 ? "s" : string.Empty)}...");
                await Task.Delay(TimeSpan.FromSeconds(1), _timeProvider);
            }

            await synced; // Ensure companies are synced before building gamemode

            LobbyState = "Building gamemode...";
            var buildResult = await _playService.BuildGamemode(_lobby);
            if (buildResult.Failed) {
                LobbyState = "Failed to build gamemode, please check logs for details.";
                await Task.Delay(TimeSpan.FromSeconds(5), _timeProvider); // Wait for 5 seconds before resetting state
                return;
            }

            LobbyState = "Uploading gamemode...";
            var uploadResult = await _lobby.UploadGamemode(buildResult.GamemodeSgaFileLocation); // NOP operation in singleplayer mode
            if (uploadResult.Failed) {
                LobbyState = "Failed to upload gamemode, please check logs for details.";
                await Task.Delay(TimeSpan.FromSeconds(5), _timeProvider); // Wait for 5 seconds before resetting state
                return;
            }

            LobbyState = "Waiting for all players to download the gamemode...";
            var allDownloaded = await _lobby.WaitForAllPlayersHaveGamemode();
            if (!allDownloaded) {
                LobbyState = "Failed while waiting for players to download gamemode, please check logs for details.";
                await Task.Delay(TimeSpan.FromSeconds(5), _timeProvider); // Wait for 5 seconds before resetting state
                return;
            }

            LobbyState = "Launching game...";
            var launchResult = await _lobby.LaunchGame(); // for multiplayer this means tell other players to launch (NOP in singleplayer)
            if (launchResult.Failed) {
                LobbyState = "Failed to launch game, please check logs for details.";
                await Task.Delay(TimeSpan.FromSeconds(5), _timeProvider); // Wait for 5 seconds before resetting state
                return;
            }

            IsMatchStarting = false;
            IsWaitingForMatchOver = true;
            IsPlaying = true;
            LobbyState = "Waiting for ingame results...";

            var playResult = await _playService.LaunchGameApp(_lobby.Game);
            if (playResult.Failed) {
                LobbyState = "Failed to launch game application.";
                await Task.Delay(TimeSpan.FromSeconds(5), _timeProvider); // Wait for 5 seconds before resetting state
                return;
            }

            IsPlaying = false;
            var matchResult = await playResult.GameInstance.WaitForMatch();
            if (matchResult.Failed) {
                LobbyState = "Match failed to complete, please check logs for details.";
                reason = EndMatchReason.GameCancelled;
                await Task.Delay(TimeSpan.FromSeconds(5), _timeProvider); // Wait for 5 seconds before resetting state
                return;
            } else if (matchResult.ScarError) {
                LobbyState = "Fatal SCAR error occurred during match, please check logs.";
                reason = EndMatchReason.ScarError;
                await Task.Delay(TimeSpan.FromSeconds(5), _timeProvider); // Wait for 5 seconds before resetting state
                return;
            } else if (matchResult.BugSplat) {
                LobbyState = "BugSplat occurred during match, please check logs.";
                await Task.Delay(TimeSpan.FromSeconds(5), _timeProvider); // Wait for 5 seconds before resetting state
                return;
            }

            LobbyState = "Match over, analysing replay...";
            var replayAnalysis = await _replayService.AnalyseReplay(matchResult.ReplayFilePath, _lobby.Game.Id);
            if (replayAnalysis.Failed) {
                LobbyState = "Failed to analyse replay, please check logs for details.";
                await Task.Delay(TimeSpan.FromSeconds(5), _timeProvider); // Wait for 5 seconds before resetting state
                return;
            }

            // Register match result in statistics service (for both singleplayer and multiplayer, as we want to track statistics for both)
            // This is for local statistics only, as we do not want to rely on the server to track statistics for singleplayer matches (and also for multiplayer matches in case of server issues or if the player wants to keep their statistics private)
            await _statisticsService.RegisterPlayedMatchAsync(MapToPlayedMatch(replayAnalysis));

            LobbyState = "Match over, reporting results to server...";
            if (!await _lobby.ReportMatchResult(replayAnalysis)) {
                LobbyState = "Failed to report match results to server...";
            } else {
                LobbyState = "Match results reported successfully!";
            }

            reason = EndMatchReason.MatchEndedInSuccess;

            await Task.Delay(TimeSpan.FromSeconds(5), _timeProvider); // Wait for 5 seconds before resetting state

        } finally {
            IsMatchStarting = false;
            IsWaitingForMatchOver = false;
            IsPlaying = false;
            SyncState(); // Resync state after match is over (or an error occurred)
            await _lobby.EndMatch(reason); // End the match and return to lobby state (NOP in singleplayer)
        }

    }

    private MatchPlayed MapToPlayedMatch(ReplayAnalysisResult replayAnalysis) {
        var result = replayAnalysis.GetMatchResult(_lobby);
        var localPlayerId = _lobby.GetLocalPlayerId() ?? string.Empty;
        if (string.IsNullOrEmpty(localPlayerId) || !result.CompanyModifiers.ContainsKey(localPlayerId)) {
            return new MatchPlayed {
                ClientVersion = BattlegroundsApp.Version,
                DatePlayed = DateTime.Now.Subtract(replayAnalysis.Replay?.Duration ?? TimeSpan.Zero),
                Duration = replayAnalysis.Replay?.Duration ?? TimeSpan.Zero,
                IsSinglePlayer = _lobby is SingleplayerLobby,
                IsVictory = false,
                GameId = _lobby.Game.Id,
                CompanyVersion = 0,
                PlayerCompanyId = string.Empty,
                PlayedMap = result.Scenario,
                PlayerFaction = string.Empty,
                MatchId = result.MatchId,
                TotalKills = 0,
                TotalLosses = 0
            };
        }
        var localCompany = _lobby.Companies.TryGetValue(_lobby.GetLocalPlayerSlot().team?.Slots[_lobby.GetLocalPlayerSlot().slotId].CompanyId ?? string.Empty, out var company) ? company : null;
        return new MatchPlayed {
            ClientVersion = BattlegroundsApp.Version,
            DatePlayed = DateTime.Now.Subtract(replayAnalysis.Replay?.Duration ?? TimeSpan.Zero),
            Duration = replayAnalysis.Replay?.Duration ?? TimeSpan.Zero,
            IsSinglePlayer = _lobby is SingleplayerLobby,
            IsVictory = result.Winners.Contains(localPlayerId),
            GameId = _lobby.Game.Id,
            CompanyVersion = localCompany?.Version ?? 0,
            PlayerCompanyId = localCompany?.Id ?? string.Empty,
            PlayedMap = result.Scenario,
            PlayerFaction = localCompany?.Faction ?? string.Empty,
            MatchId = result.MatchId,
            TotalKills = result.CompanyModifiers[localPlayerId].Sum(GetKillsFromEvents),
            TotalLosses = result.CompanyModifiers[localPlayerId].Sum(GetLossesFromEvents)
        };
    }

    private static int GetKillsFromEvents(CompanyEventModifier e) => e.EventType is CompanyEventModifier.EVENT_TYPE_STATISTICS ? e.IntValue1 + e.IntValue2 : 0;

    private static int GetLossesFromEvents(CompanyEventModifier e) => e.EventType is CompanyEventModifier.EVENT_TYPE_STATISTICS ? e.IntValue3 : 0;

    private async Task HideDownloadProgressAfterDelay(int teamId, int slotId, string companyId, long slotRevision, CancellationToken cancellationToken) {
        try {
            await Task.Delay(TimeSpan.FromSeconds(2), _timeProvider, cancellationToken);
        } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            return;
        }

        await InvokeOnUiAsync(() => {
            var slot = (teamId == 0 ? Team1Slots : Team2Slots).FirstOrDefault(x => x.Slot.Index == slotId);
            if (slot is null || slot.Slot.CompanyId != companyId || _lobby.GetSlotRevision(teamId, slotId) != slotRevision) {
                return;
            }
            slot.CompanyDownloadProgress = 0;
            PropertyChanged?.Invoke(this, new(nameof(Team1Slots)));
            PropertyChanged?.Invoke(this, new(nameof(Team2Slots)));
        }, cancellationToken);
    }

    private async Task ShowMatchResults() {
        var matchResult = await _lobby.GetMatchResults();
        if (matchResult is null) {
            _logger.LogWarning("Received MatchOver event but GetMatchResults returned null.");
            return;
        }
        MatchOverResult = new MatchOverViewModel(matchResult, _lobby.Game, () => MatchOverResult = null);
    }

    private Task InvokeOnUiAsync(Action action, CancellationToken cancellationToken) =>
        InvokeOnUiAsync(() => {
            action();
            return Task.CompletedTask;
        }, cancellationToken);

    private Task InvokeOnUiAsync(Func<Task> action, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        if (_uiContext is null || ReferenceEquals(SynchronizationContext.Current, _uiContext)) {
            return action();
        }

        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _uiContext.Post(async _ => {
            if (cancellationToken.IsCancellationRequested) {
                completion.TrySetCanceled(cancellationToken);
                return;
            }
            try {
                await action();
                completion.TrySetResult();
            } catch (Exception ex) {
                completion.TrySetException(ex);
            }
        }, null);
        return completion.Task;
    }

    private async Task SyncLobbyCompanies() {
        _lobby.Companies.Clear();
        var t1PickedCompanies = from slot in Team1Slots where !slot.Slot.Hidden && !slot.Slot.Locked select slot.SelectedCompany;
        var t2PickedCompanies = from slot in Team2Slots where !slot.Slot.Hidden && !slot.Slot.Locked select slot.SelectedCompany;
        var t1MappedCompanies = t1PickedCompanies.ToAsyncEnumerable().Select(MapPickableCompanyToCompany);
        var t2MappedCompanies = t2PickedCompanies.ToAsyncEnumerable().Select(MapPickableCompanyToCompany);
        await foreach (var company in t1MappedCompanies.Concat(t2MappedCompanies)) {
            var resolved = await company;
            if (resolved is null) continue;
            _lobby.Companies.Add(resolved.Id, resolved);
        }
    }

    private ValueTask<Company?> MapPickableCompanyToCompany(PickableCompany pickableCompany) {
        if (pickableCompany.IsNone) {
            return ValueTask.FromResult<Company?>(null);
        }
        if (pickableCompany.GenerateRandom) {
            throw new NotImplementedException("Random AI company generation not implemented yet");
        }
        if (pickableCompany.Company is null) {
            return ValueTask.FromResult<Company?>(null);
        }
        // TODO: Check if newest version of company otherwise fetch from remote
        return ValueTask.FromResult<Company?>(pickableCompany.Company);
    }

    private async ValueTask<LobbySlotViewModel> MapToLobbySlot(int teamIndex, Team.Slot slot) {
        var addAICommand = new AsyncRelayCommand<AIDifficulty>(
            args => AddAIToSlot(teamIndex, slot.Index, args),
            args => args != slot.Difficulty);
        var lockUnlockCommand = new AsyncRelayCommand<int>(args => LockOrUnlockSlot(teamIndex, args));
        var setCompanyCommand = new AsyncRelayCommand<PickableCompany>(
            args => SetSlotCompany(teamIndex, slot.Index, args),
            args => args?.Company is Company company && company.Id != slot.CompanyId);
        var moveToSlotCommand = new AsyncRelayCommand<int>(args => MoveToSlot(teamIndex, args));
        Participant? p = (from participant in _lobby.Participants where participant.ParticipantId == slot.ParticipantId select participant).FirstOrDefault();
        Company? c = string.IsNullOrEmpty(slot.CompanyId) ? null : (from company in _lobby.Companies where company.Key == slot.CompanyId select company.Value).FirstOrDefault();
        if (c is null && !string.IsNullOrEmpty(slot.CompanyId)) {
            c = await _companyService.GetCompanyAsync(slot.CompanyId); // Fetch from remote (or local cache) (TODO: Handle case where company was changed on remote server)
        }
        FactionAlliance alliance = teamIndex == 0 ? FactionAlliance.Allies : FactionAlliance.Axis;
        if (p is null) {
            string companyName = c?.Name ?? string.Empty;
            return new LobbySlotViewModel(slot, string.Empty, companyName, true, alliance, addAICommand, lockUnlockCommand, setCompanyCommand, moveToSlotCommand, this);
        }
        return new LobbySlotViewModel(slot, p.ParticipantName, c?.Name ?? string.Empty, p.IsAIParticipant, alliance, addAICommand, lockUnlockCommand, setCompanyCommand, moveToSlotCommand, this);
    }

    private async Task MoveToSlot(int teamIndex, int slotIndex) {
        await _lobby.MoveToSlot(teamIndex == 0 ? _lobby.Team1 : _lobby.Team2, slotIndex);
    }

    private async Task AddAIToSlot(int teamIndex, int slotIndex, AIDifficulty difficulty) {
        if (difficulty == AIDifficulty.HUMAN) {
            await _lobby.RemoveAI(teamIndex == 0 ? _lobby.Team1 : _lobby.Team2, slotIndex);
            return;
        }
        await _lobby.SetSlotAIDifficulty(teamIndex == 0 ? _lobby.Team1 : _lobby.Team2, slotIndex, difficulty);
        var slot = teamIndex == 0 ? _lobby.Team1.Slots[slotIndex] : _lobby.Team2.Slots[slotIndex];
        if (string.IsNullOrEmpty(slot.Faction)) {
            var alliance = teamIndex == 0 ? FactionAlliance.Allies : FactionAlliance.Axis;
            var faction = _lobby.Game.FactionIds.FirstOrDefault(f => _lobby.Game.GetFactionAlliance(f) == alliance);
            await _lobby.SetSlotFaction(teamIndex == 0 ? _lobby.Team1 : _lobby.Team2, slotIndex, faction);
        }
    }

    private async Task LockOrUnlockSlot(int teamIndex, int slotIndex) {
        await _lobby.ToggleSlotLock(teamIndex == 0 ? _lobby.Team1 : _lobby.Team2, slotIndex);
    }

    private async Task SetSlotCompany(int teamIndex, int slotIndex, PickableCompany? company) {
        if (company is null) {
            return;
        }
        if (!IsHost) { // Add guard against non-host clients trying to change companies for other players (as this should only be allowed for the host client, but just in case)
            var (selfTeam, selfSlot) = _lobby.GetLocalPlayerSlot();
            if (selfTeam != (teamIndex == 0 ? _lobby.Team1 : _lobby.Team2) || selfSlot != slotIndex) {
                return;
            }
        }
        if (company.Company is not null) {
            await _lobby.SetCompany(teamIndex == 0 ? _lobby.Team1 : _lobby.Team2, slotIndex, company.Company.Id, company.Company.Faction);
            return;
        }
    }

    private async Task SetMap(Map? map) {
        if (map is null) {
            return;
        }
        if (!await _lobby.SetMap(map)) {
            _draftSelectedMap = _selectedMap;
            PropertyChanged?.Invoke(this, new(nameof(DraftSelectedMap)));
            PropertyChanged?.Invoke(this, new(nameof(SelectedMapPreview)));
            SetMapCommand.NotifyCanExecuteChanged();
            SyncState();
        }
    }

    private async Task SetSetting(LobbySetting? newSetting) {
        if (newSetting is null) {
            return;
        }
        await _lobby.SetSetting(newSetting);
    }

    private async Task ToggleReady() {
        await _lobby.MarkReady(!_lobby.IsReady);
        PropertyChanged?.Invoke(this, new(nameof(IsReady)));
    }

    public Company? GetCompany(string companyId) {
        if (LobbyCompanies.TryGetValue(companyId, out var company)) {
            return company;
        }
        if (_lobby.Companies.TryGetValue(companyId, out var lobbyCompany)) {
            return lobbyCompany;
        }
        return null;
    }

    public void ShowCompanyPreview(Company? company) {
        if (company is null) {
            return;
        }
        CompanyPreviewResult = new CompanyPreviewViewModel(company, GameId, () => CompanyPreviewResult = null);
    }

    public async ValueTask DisposeAsync() {
        if (_disposed) {
            return;
        }
        _disposed = true;
        _lifetimeCts.Cancel();
        try {
            await _lifetimeTask.ConfigureAwait(false);
        } catch (OperationCanceledException) {
            // Expected during teardown.
        }
        _lifetimeCts.Dispose();
    }

}
