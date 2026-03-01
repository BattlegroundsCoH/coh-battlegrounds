using System.Collections.ObjectModel;
using System.ComponentModel;

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

public sealed class LobbyViewModel : INotifyPropertyChanged {

    public const int MAX_CHAT_MESSAGE_LENGTH = 180; // Maximum length of a chat message
    public const string MAX_MESSAGE_LENGTH_REACHED = "Chat message truncated to 180 characters.";

    private readonly ILogger<LobbyViewModel> _logger;
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

    private ICollection<LobbySlotViewModel> _team1Slots = [];
    private ICollection<LobbySlotViewModel> _team2Slots = [];
    private ICollection<Map> _availableMaps = [];
    private ICollection<LobbySettingViewModel> _settings = [];

    private PickableChatChannel _selectedChatChannel = new PickableChatChannel("all"); // TODO: Support chat channels properly
    private Map _selectedMap;

    private string _chatMessage = string.Empty;
    private string _state = "Loading match information";

    private bool _isPlaying = false;
    private bool _isMatchStarting = false;
    private bool _isWaitingForMatchOver = false;

    private MatchOverViewModel? _matchOverResult;

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

    public string LobbyName => _lobby.Name;

    public ILobby Model => _lobby;

    public IAsyncRelayCommand LeaveCommand { get; }

    public IAsyncRelayCommand SendMessageCommand { get; }

    public IAsyncRelayCommand StartMatchCommand { get; }

    public IAsyncRelayCommand ToggleReadyCommand { get; }

    public IAsyncRelayCommand<Map> SetMapCommand { get; }

    public bool IsHost => _lobby.IsHost;

    public bool IsReady => _lobby.IsReady;

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

    public Map SelectedMap {
        get => _selectedMap;
        set {
            if (_selectedMap == value) return;
            _selectedMap = value;
            SetMapCommand.Execute(value);
            PropertyChanged?.Invoke(this, new(nameof(SelectedMap)));
            PropertyChanged?.Invoke(this, new(nameof(SelectedMapPreview)));
        }
    }

    public string SelectedMapPreview => $"pack://siteoforigin:,,,/Assets/Scenarios/{_lobby.Game.Id}/mm/{_selectedMap.Preview}.png";

    public ICollection<LobbySettingViewModel> SelectedSettings {
        get => _settings;
        private set {
            if (value == _settings) return;
            _settings = value;
            PropertyChanged?.Invoke(this, new(nameof(SelectedSettings)));
        }
    }

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
        get => field;
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

    public LobbyViewModel(ILobby lobby, IServiceProvider serviceProvider, ILogger<LobbyViewModel> logger) {
        // Probably an anti-pattern to pass IServiceProvider instead of the specific services, but this class has many dependencies 
        // So... collect the services in a facade class to make it easier to test and maintain (Probably also solves the comment regarding a separate controller class for the StartGame method)

        _lobby = lobby;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _lobbyService = serviceProvider.GetRequiredService<ILobbyService>();
        _playService = serviceProvider.GetRequiredService<IPlayService>();
        _replayService = serviceProvider.GetRequiredService<IReplayService>();
        _companyService = serviceProvider.GetRequiredService<ICompanyService>();
        _gameMapService = serviceProvider.GetRequiredService<IGameMapService>();
        _statisticsService = serviceProvider.GetRequiredService<IStatisticsService>();
        _mainWindowVm = serviceProvider.GetRequiredService<MainWindowViewModel>();
        _selectedMap = lobby.Map;

        LeaveCommand = new AsyncRelayCommand(LeaveLobby);
        SendMessageCommand = new AsyncRelayCommand(SendChatMessage);
        StartMatchCommand = new AsyncRelayCommand(StartGame);
        ToggleReadyCommand = new AsyncRelayCommand(ToggleReady);
        SetMapCommand = new AsyncRelayCommand<Map>(SetMap);

        // Sync view with lobby state
        SyncLobbyView();

    }

    private async void SyncLobbyView() {
        SyncLobbySettings();
        AvailableMaps = [.. (await _gameMapService.GetMapsForGame(_lobby.Game.Id)).Select(Map.FromScenario)];
        Team1Slots = await MapTeamSlotsToLobbySlots(0, _lobby.Team1.Slots);
        Team2Slots = await MapTeamSlotsToLobbySlots(1, _lobby.Team2.Slots);
        PollLobbyEvents();
        LoadLocalPlayerCompanies();
        SyncState();
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
        SelectedSettings = [.. _lobby.Settings.Select(x => new LobbySettingViewModel(x, new AsyncRelayCommand<LobbySetting>(SetSetting)))];
    }

    private async void LoadLocalPlayerCompanies() {

        string[] factions = _lobby.Game.FactionIds;
        foreach (string faction in factions) {
            var alliance = _lobby.Game.GetFactionAlliance(faction);
            if (!_localPlayerCompaniesByAlliance.TryGetValue(alliance, out var existingCompanies)) {
                _localPlayerCompaniesByAlliance[alliance] = existingCompanies = [];
            }
            var localPlayerCompanies = await _companyService.GetLocalCompaniesAsync();
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

        var company = _localPlayerCompaniesByAlliance[factionAlliance].FirstOrDefault();
        if (company is null) {
            return;
        }

        await _lobby.SetCompany(team, slotId, company.Id);

    }

    private async void PollLobbyEvents() {
        while (_lobby.IsActive) {
            LobbyEvent? lobbyEvent = await _lobby.GetNextEvent();
            if (lobbyEvent is null) {
                break;
            }

            switch (lobbyEvent.EventType) {
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
                case LobbyEventType.TeamUpdated:
                    bool updateTeam1 = lobbyEvent is null || (lobbyEvent.Arg is TeamType t1t && t1t == _lobby.Team1.TeamType);
                    bool updateTeam2 = lobbyEvent is null || (lobbyEvent.Arg is TeamType t2t && t2t == _lobby.Team2.TeamType);
                    if (updateTeam1) {
                        Team1Slots = await MapTeamSlotsToLobbySlots(0, _lobby.Team1.Slots);
                    }
                    if (updateTeam2) {
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
                    _selectedMap = updatedMap; // NOP if already selected (so NOP for host)

                    // Update team slots as well, since some slots may become hidden/unhidden based on map selection
                    Team1Slots = await MapTeamSlotsToLobbySlots(0, _lobby.Team1.Slots);
                    Team2Slots = await MapTeamSlotsToLobbySlots(1, _lobby.Team2.Slots);
                    break;
                case LobbyEventType.SettingUpdated:
                    PropertyChanged?.Invoke(this, new(nameof(SelectedSettings)));
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
                    ShowMatchResults();
                    break;
                default:
                    break;
            }
            SyncState();

        }
    }

    private async Task<List<LobbySlotViewModel>> MapTeamSlotsToLobbySlots(int index, Team.Slot[] slots) {
        List<LobbySlotViewModel> result = [];
        foreach (var slot in slots) {
            var task = await MapToLobbySlot(index, slot);
            result.Add(task);
        }
        return result;
    }

    private async Task LeaveLobby() {
        // TODO: Show confirmation dialog before leaving lobby?
        if (!_lobby.IsActive) {
            return; // Already left
        }
        await _lobbyService.LeaveLobbyAsync(_lobby);
        _mainWindowVm.SetContent(null); // Return to default content (probably multiplayer view or home view)
        // TODO: Tell main window to return to multiplayer view (if multiplayer lobby) or home if singleplayer lobby
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
                await Task.Delay(1000);
            }

            await synced; // Ensure companies are synced before building gamemode

            LobbyState = "Building gamemode...";
            var buildResult = await _playService.BuildGamemode(_lobby);
            if (buildResult.Failed) {
                LobbyState = "Failed to build gamemode, please check logs for details.";
                await Task.Delay(5000); // Wait for 5 seconds before resetting state
                return;
            }

            LobbyState = "Uploading gamemode...";
            var uploadResult = await _lobby.UploadGamemode(buildResult.GamemodeSgaFileLocation); // NOP operation in singleplayer mode
            if (uploadResult.Failed) {
                LobbyState = "Failed to upload gamemode, please check logs for details.";
                await Task.Delay(5000); // Wait for 5 seconds before resetting state
                return;
            }

            LobbyState = "Waiting for all players to download the gamemode...";
            var allDownloaded = await _lobby.WaitForAllPlayersHaveGamemode();
            if (!allDownloaded) { 
                LobbyState = "Failed while waiting for players to download gamemode, please check logs for details.";
                await Task.Delay(5000); // Wait for 5 seconds before resetting state
                return;
            }

            LobbyState = "Launching game...";
            var launchResult = await _lobby.LaunchGame(); // for multiplayer this means tell other players to launch (NOP in singleplayer)
            if (launchResult.Failed) {
                LobbyState = "Failed to launch game, please check logs for details.";
                await Task.Delay(5000); // Wait for 5 seconds before resetting state
                return;
            }

            IsMatchStarting = false;
            IsWaitingForMatchOver = true;
            IsPlaying = true;
            LobbyState = "Waiting for ingame results...";

            var playResult = await _playService.LaunchGameApp(_lobby.Game);
            if (playResult.Failed) {
                LobbyState = "Failed to launch game application.";
                await Task.Delay(5000); // Wait for 5 seconds before resetting state
                return;
            }

            IsPlaying = false;
            var matchResult = await playResult.GameInstance.WaitForMatch();
            if (matchResult.Failed) {
                LobbyState = "Match failed to complete, please check logs for details.";
                reason = EndMatchReason.GameCancelled;
                await Task.Delay(5000); // Wait for 5 seconds before resetting state
                return;
            } else if (matchResult.ScarError) {
                LobbyState = "Fatal SCAR error occurred during match, please check logs.";
                reason = EndMatchReason.ScarError;
                await Task.Delay(5000); // Wait for 5 seconds before resetting state
                return;
            } else if (matchResult.BugSplat) {
                LobbyState = "BugSplat occurred during match, please check logs.";
                await Task.Delay(5000); // Wait for 5 seconds before resetting state
                return;
            }

            LobbyState = "Match over, analysing replay...";
            var replayAnalysis = await _replayService.AnalyseReplay(matchResult.ReplayFilePath, _lobby.Game.Id);
            if (replayAnalysis.Failed) {
                LobbyState = "Failed to analyse replay, please check logs for details.";
                await Task.Delay(5000); // Wait for 5 seconds before resetting state
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

            await Task.Delay(5000); // Wait for 5 seconds before resetting state

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
        var localCompany = _lobby.Companies.TryGetValue(_lobby.GetLocalPlayerSlot().team?.Slots[_lobby.GetLocalPlayerSlot().slotId].CompanyId ?? string.Empty, out var company) ? company : null;
        return new MatchPlayed {
            ClientVersion = BattlegroundsApp.Version,
            DatePlayed = DateTime.Now.Subtract(replayAnalysis.Replay?.Duration ?? TimeSpan.Zero),
            Duration = replayAnalysis.Replay?.Duration ?? TimeSpan.Zero,
            IsSinglePlayer = _lobby is SingleplayerLobby,
            IsVictory = result.Winners.Contains(_lobby.GetLocalPlayerId() ?? string.Empty),
            GameId = _lobby.Game.Id,
            CompanyVersion = localCompany?.Version ?? 0,
            PlayerCompanyId = localCompany?.Id ?? string.Empty,
            PlayedMap = result.Scenario,
            PlayerFaction = localCompany?.Faction ?? string.Empty,
            MatchId = result.MatchId,
            TotalKills = 0,
            TotalLosses = 0
        };
    }

    private async void ShowMatchResults() {
        var matchResult = await _lobby.GetMatchResults();
        if (matchResult is null) {
            _logger.LogWarning("Received MatchOver event but GetMatchResults returned null.");
            return;
        }
        MatchOverResult = new MatchOverViewModel(matchResult, _lobby.Game, () => MatchOverResult = null);
    }

    private async Task SyncLobbyCompanies() {
        _lobby.Companies.Clear();
        var t1PickedCompanies = from slot in Team1Slots where !slot.Slot.Hidden && !slot.Slot.Locked select slot.SelectedCompany;
        var t2PickedCompanies = from slot in Team2Slots where !slot.Slot.Hidden && !slot.Slot.Locked select slot.SelectedCompany;
        var t1MappedCompanies = t1PickedCompanies.ToAsyncEnumerable().Select(MapPickableCompanyToCompany);
        var t2MappedCompanies = t2PickedCompanies.ToAsyncEnumerable().Select(MapPickableCompanyToCompany);
        await foreach (var company in t1MappedCompanies.Concat(t2MappedCompanies)) {
            var resolved = await company;
            _lobby.Companies.Add(resolved!.Id, resolved);
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
        var addAICommand = new AsyncRelayCommand<AIDifficulty>(args => AddAIToSlot(teamIndex, slot.Index, args));
        var lockUnlockCommand = new AsyncRelayCommand<int>(args => LockOrUnlockSlot(teamIndex, args));
        var setCompanyCommand = new AsyncRelayCommand<PickableCompany>(args => SetSlotCompany(teamIndex, slot.Index, args));
        Participant? p = (from participant in _lobby.Participants where participant.ParticipantId == slot.ParticipantId select participant).FirstOrDefault();
        Company? c = string.IsNullOrEmpty(slot.CompanyId) ? null : (from company in _lobby.Companies where company.Key == slot.CompanyId select company.Value).FirstOrDefault();
        if (c is null && !string.IsNullOrEmpty(slot.CompanyId)) {
            c = await _companyService.GetCompanyAsync(slot.CompanyId); // Fetch from remote (or local cache) (TODO: Handle case where company was changed on remote server)
        }
        FactionAlliance alliance = teamIndex == 0 ? FactionAlliance.Allies : FactionAlliance.Axis;
        if (p is null) {
            string companyName = c?.Name ?? string.Empty;
            return new LobbySlotViewModel(slot, string.Empty, companyName, true, alliance, addAICommand, lockUnlockCommand, setCompanyCommand, this);
        }
        return new LobbySlotViewModel(slot, p.ParticipantName, c?.Name ?? string.Empty, p.IsAIParticipant, alliance, addAICommand, lockUnlockCommand, setCompanyCommand, this);
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
            await _lobby.SetCompany(teamIndex == 0 ? _lobby.Team1 : _lobby.Team2, slotIndex, company.Company.Id);
            return;
        }
    }

    private async Task SetMap(Map? map) {
        if (map is null) {
            return;
        }
        if (!await _lobby.SetMap(map)) {
            _selectedMap = _lobby.Map; // RESET to _lobby map
            PropertyChanged?.Invoke(this, new(nameof(SelectedMap)));
            PropertyChanged?.Invoke(this, new(nameof(SelectedMapPreview)));
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

}
