using System.Collections.ObjectModel;
using System.ComponentModel;

using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services;
using Battlegrounds.ViewModels.LobbyHelpers;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;

namespace Battlegrounds.ViewModels;

public sealed class LobbyViewModel : INotifyPropertyChanged {

    public const int MAX_CHAT_MESSAGE_LENGTH = 180; // Maximum length of a chat message
    public const string MAX_MESSAGE_LENGTH_REACHED = "Chat message truncated to 180 characters.";

    private readonly ILobby _lobby;
    private readonly ILobbyService _lobbyService;
    private readonly IPlayService _playService;
    private readonly IReplayService _replayService;
    private readonly ICompanyService _companyService;
    private readonly IGameMapService _gameMapService;
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

    public event PropertyChangedEventHandler? PropertyChanged;

    public string LobbyName => _lobby.Name;

    public ILobby Model => _lobby;

    public IAsyncRelayCommand LeaveCommand { get; }

    public IAsyncRelayCommand SendMessageCommand { get; }

    public IAsyncRelayCommand StartMatchCommand { get; }

    public IAsyncRelayCommand<Map> SetMapCommand { get; }

    public bool IsHost => _lobby.IsHost;

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

    public string SelectedMapPreview => $"pack://siteoforigin:,,,/Assets/Scenarios/{_lobby.Game.Id}/{_selectedMap.Preview}.png";

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
            PropertyChanged?.Invoke(this, new(nameof(LobbyState)));
        }
    }

    public PickableChatChannel[] AvailableChatChannels => [new PickableChatChannel("all"), new PickableChatChannel("team")];

    public PickableChatChannel SelectedChatChannel {
        get => _selectedChatChannel;
        set {
            if (_selectedChatChannel == value) return;
            _selectedChatChannel = value;
        }
    }

    public LobbyViewModel(ILobby lobby, IServiceProvider serviceProvider) {
        // Probably an anti-pattern to pass IServiceProvider instead of the specific services, but this class has many dependencies 
        // So... collect the services in a facade class to make it easier to test and maintain (Probably also solves the comment regarding a separate controller class for the StartGame method)

        _lobby = lobby;
        _lobbyService = serviceProvider.GetRequiredService<ILobbyService>();
        _playService = serviceProvider.GetRequiredService<IPlayService>();
        _replayService = serviceProvider.GetRequiredService<IReplayService>();
        _companyService = serviceProvider.GetRequiredService<ICompanyService>();
        _gameMapService = serviceProvider.GetRequiredService<IGameMapService>();
        _mainWindowVm = serviceProvider.GetRequiredService<MainWindowViewModel>();
        _selectedMap = lobby.Map;

        LeaveCommand = new AsyncRelayCommand(LeaveLobby);
        SendMessageCommand = new AsyncRelayCommand(SendChatMessage);
        StartMatchCommand = new AsyncRelayCommand(StartGame, () => CanStartMatch);
        SetMapCommand = new AsyncRelayCommand<Map>(SetMap);

        // Sync view with lobby state
        SyncLobbyView();

    }

    private async void SyncLobbyView() {
        SyncLobbySettings();
        AvailableMaps = await _gameMapService.GetMapsForGame(_lobby.Game.Id);
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
            var factionCompanies = (await _companyService.GetLocalPlayerCompaniesForFaction(faction)).ToArray();
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
        var company = _localPlayerCompaniesByAlliance[_lobby.Game.GetFactionAlliance(slot.Faction)].FirstOrDefault();
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
                    break;
                case LobbyEventType.SettingUpdated:
                    PropertyChanged?.Invoke(this, new(nameof(SelectedSettings)));
                    break;
                default:
                    break;
            }
            SyncState();

        }
    }

    private ValueTask<List<LobbySlotViewModel>> MapTeamSlotsToLobbySlots(int index, Team.Slot[] slots) 
        => slots.ToAsyncEnumerable().SelectAwait(x => MapToLobbySlot(index, x)).ToListAsync();

    private async Task LeaveLobby() {
        // TODO: Show confirmation dialog before leaving lobby?
        if (!_lobby.IsActive) {
            return; // Already left
        }
        await _lobbyService.LeaveLobbyAsync(_lobby);
        _mainWindowVm.SetContent(null); // Return to multiplayer view
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
        await _lobby.SendMessage(SelectedChatChannel.ChannelName, msg); // TODO: Support team chat
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

        try {
            IsMatchStarting = true;

            // Sync corrent lobby view status with backing model based on selected PickableCompany (based on host client view!)
            await SyncLobbyCompanies();

            var buildResult = await _playService.BuildGamemode(_lobby);
            if (buildResult.Failed) {
                IsMatchStarting = false;
                // TODO: Show error message
                return;
            }

            var uploadResult = await _lobby.UploadGamemode(buildResult.GamemodeSgaFileLocation); // NOP operation in singleplayer mode
            if (uploadResult.Failed) {
                IsMatchStarting = false;
                // TODO: Show error message
                return;
            }

            var launchResult = await _lobby.LaunchGame(); // for multiplayer this means tell other players to launch (NOP in singleplayer)
            if (launchResult.Failed) {
                IsMatchStarting = false;
                // TODO: Show error message
                return;
            }

            IsMatchStarting = false;
            IsWaitingForMatchOver = true;
            IsPlaying = true;

            var playResult = await _playService.LaunchGameApp(_lobby.Game);
            if (playResult.Failed) {
                // TODO: Show error message
                IsWaitingForMatchOver = false;
                return;
            }

            IsPlaying = false;
            var matchResult = await playResult.GameInstance.WaitForMatch();
            if (matchResult.Failed) {
                // TODO: Show error message
                return;
            } else if (matchResult.ScarError) {
                // TODO: Show error message
                return;
            } else if (matchResult.BugSplat) {
                // TODO: Show error message
                return;
            }

            var replayAnalysis = await _replayService.AnalyseReplay(matchResult.ReplayFilePath, _lobby.Game.Id);
            if (replayAnalysis.Failed) {
                // TODO: Show error message
                return;
            }

            await _lobby.ReportMatchResult(replayAnalysis);

        } finally {
            IsMatchStarting = false;
            IsWaitingForMatchOver = false;
            IsPlaying = false;
        }

    }

    private async Task SyncLobbyCompanies() {
        _lobby.Companies.Clear();
        var t1PickedCompanies = from slot in Team1Slots where !slot.Slot.Hidden && !slot.Slot.Locked select slot.SelectedCompany;
        var t2PickedCompanies = from slot in Team2Slots where !slot.Slot.Hidden && !slot.Slot.Locked select slot.SelectedCompany;
        var t1MappedCompanies = t1PickedCompanies.ToAsyncEnumerable().SelectAwait(MapPickableCompanyToCompany);
        var t2MappedCompanies = t2PickedCompanies.ToAsyncEnumerable().SelectAwait(MapPickableCompanyToCompany);
        var picked = new List<Company>();
        await foreach (var company in t1MappedCompanies.Concat(t2MappedCompanies).Where(x => x is not null)) {
            _lobby.Companies.Add(company!.Id, company);
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
        }
        await _lobby.SetSlotAIDifficulty(teamIndex == 0 ? _lobby.Team1 : _lobby.Team2, slotIndex, difficulty);
    }

    private async Task LockOrUnlockSlot(int teamIndex, int slotIndex) {
        await _lobby.ToggleSlotLock(teamIndex == 0 ? _lobby.Team1 : _lobby.Team2, slotIndex);
    }

    private async Task SetSlotCompany(int teamIndex, int slotIndex, PickableCompany? company) {
        if (company is null) {
            return;
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

}
