using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;

using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Services;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.ViewModels;

public sealed class LobbyViewModel : INotifyPropertyChanged {

    public sealed record AddAIPlayerToSlotEventArgs(int SlotIndex, string Difficulty);

    public sealed record PickableCompany(bool IsNone, bool GenerateRandom, Company? Company) {
        public string DisplayName {
            get {
                if (IsNone) 
                    return "None";
                if (GenerateRandom) return "Random AI Company";
                return Company?.Name ?? "Unknown Company";
            }
        }
    }

    public sealed record LobbySlot( // Should probably be a class, but for now it's a record
        Team.Slot Slot, 
        string UserName, 
        string CompanyName, 
        bool IsAIPlayer,
        IAsyncRelayCommand<AddAIPlayerToSlotEventArgs> DifficultyCommand, 
        IAsyncRelayCommand<int> LockUnlockCommand,
        IAsyncRelayCommand<PickableCompany> SetCompanyCommand,
        LobbyViewModel ParentContext) {

        private PickableCompany? _selectedCompany = null;
        private string _companyId = Slot.CompanyId;

        public object AIDifficulty {
            get => string.IsNullOrEmpty(Slot.Difficulty) ? "Select AI Difficulty" : Slot.Difficulty;
            set {
                if (value is string)
                    return;
                if (value is ComboBoxItem item && item.Content is string content) {
                    if (item.Tag is "NONE") {
                        return;
                    }
                    DifficultyCommand.Execute(new(Slot.Index, content));
                }
            }
        }
        public string DisplayName {
            get {
                if (IsAIPlayer)
                    return $"AI - {Slot.Difficulty}";
                return UserName;
            }
        }
        public List<PickableCompany> AvailableCompanies {
            get {
                if (string.IsNullOrEmpty(Slot.Faction)) {
                    return [new PickableCompany(true, false, null)];
                }
                var companies = ParentContext._localPlayerCompaniesByFaction[Slot.Faction].Select(x => new PickableCompany(false, false, x));
                var available = (IsAIPlayer ? companies.Append(new PickableCompany(false, true, null)) : companies).ToList();
                // WARNING : SIDE_EFFECT!!!!
                if (string.IsNullOrEmpty(_companyId) && IsAIPlayer) {
                    _selectedCompany = available.FirstOrDefault(x => x.Company is not null) ?? new PickableCompany(true, false, null);
                    _companyId = _selectedCompany?.Company?.Id ?? string.Empty;
                }
                if (available.Count == 0)
                    return [new PickableCompany(true, false, null)];
                else
                    return available;
            }
        }
        public PickableCompany SelectedCompany {
            get {
                if (_selectedCompany is not null) {
                    return _selectedCompany;
                }
                if (string.IsNullOrEmpty(_companyId)) {
                    return new PickableCompany(true, false, null);
                }
                var company = ParentContext._lobbyCompanies[_companyId];
                return new PickableCompany(false, false, company);
            }
            set {
                if (_selectedCompany == value)
                    return;
                _selectedCompany = value;
                _companyId = _selectedCompany?.Company?.Id ?? string.Empty;
                SetCompanyCommand.Execute(value);
            }
        }
        public bool CanSetCompany => (ParentContext.IsHost && IsAIPlayer) || (Slot.ParticipantId == ParentContext._lobby.GetLocalPlayerId());
    }

    public sealed record LobbySettingWrapper(LobbySetting Setting, IAsyncRelayCommand<LobbySetting> SettingChangeCommand) { // TODO: Make bindings use the Setting object directly instead of duplicating references
        public string Name => Setting.Name;
        public LobbySettingType Type => Setting.Type;

        public bool BoolValue {
            get => Setting.Value != 0;
            set {
                if (value == (Setting.Value != 0)) return;
                Setting.Value = value ? 1 : 0;
                SettingChangeCommand.Execute(Setting);
            }
        }

        public int IntValue {
            get => Setting.Value;
            set {
                if (value == Setting.Value) return;
                Setting.Value = Math.Clamp(value, Setting.MinValue, Setting.MaxValue);
                SettingChangeCommand.Execute(Setting);
            }
        }

        public int SelectedOptionIndex {
            get => Setting.Value;
            set {
                if (value == Setting.Value) return;
                if (Setting.Options != null && value >= 0 && value < Setting.Options.Length) {
                    Setting.Value = value;
                    SettingChangeCommand.Execute(Setting);
                }
            }
        }

        public LobbySettingOption? SelectedOption =>
            Setting.Options != null && Setting.Value >= 0 && Setting.Value < Setting.Options.Length
                ? Setting.Options[Setting.Value]
                : null;

        public LobbySettingOption[]? Options => Setting.Options;

        public int MinValue => Setting.MinValue;
        public int MaxValue => Setting.MaxValue;
        public int Step => Setting.Step;
    }

    private readonly ILobby _lobby;
    private readonly ILobbyService _lobbyService;
    private readonly IPlayService _playService;
    private readonly IReplayService _replayService;
    private readonly ICompanyService _companyService;
    private readonly IGameMapService _gameMapService;
    private readonly ObservableCollection<string> _chatMessages = [];
    private readonly Dictionary<string, List<Company>> _localPlayerCompaniesByFaction = [];
    private readonly Dictionary<string, Company> _lobbyCompanies = [];

    private ICollection<LobbySlot> _team1Slots = [];
    private ICollection<LobbySlot> _team2Slots = [];
    private ICollection<Map> _availableMaps = [];
    private ICollection<LobbySettingWrapper> _settings = [];

    private Map _selectedMap;

    private string _chatMessage = string.Empty;

    private bool _isPlaying = false;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string LobbyName => _lobby.Name;

    public IAsyncRelayCommand LeaveCommand { get; }

    public IAsyncRelayCommand SendMessageCommand { get; }

    public IAsyncRelayCommand StartMatchCommand { get; }

    public IAsyncRelayCommand<Map> SetMapCommand { get; }

    public bool IsHost => _lobby.IsHost;

    public bool CanStartMatch => _lobby.IsHost && !_isPlaying /* && _lobby.Players.Count > 1*/;

    public ObservableCollection<string> ChatMessages => _chatMessages;

    public ICollection<LobbySlot> Team1Slots {
        get => _team1Slots;
        private set {
            if (value == _team1Slots) return;
            _team1Slots = value;
            PropertyChanged?.Invoke(this, new(nameof(Team1Slots)));
        }
    }

    public ICollection<LobbySlot> Team2Slots {
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

    public ICollection<LobbySettingWrapper> SelectedSettings {
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
            if (value == _chatMessage) return;
            _chatMessage = value;
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

    public LobbyViewModel(ILobby lobby, ILobbyService lobbyService, IPlayService playService, IReplayService replayService, ICompanyService companyService, IGameMapService gameMapService) {
        
        _lobby = lobby;
        _lobbyService = lobbyService;
        _playService = playService;
        _replayService = replayService;
        _companyService = companyService;
        _gameMapService = gameMapService;
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
    }

    private void SyncLobbySettings() {
        SelectedSettings = [.. _lobby.Settings.Select(x => new LobbySettingWrapper(x, new AsyncRelayCommand<LobbySetting>(SetSetting)))];
    }

    private async void LoadLocalPlayerCompanies() {

        string[] factions = _lobby.Game.FactionIds;
        foreach (string faction in factions) {
            _localPlayerCompaniesByFaction[faction] = [.. (await _companyService.GetLocalPlayerCompaniesForFaction(faction))];
            foreach (var factionCompany in _localPlayerCompaniesByFaction[faction]) {
                _lobbyCompanies[factionCompany.Id] = factionCompany;
            }
        }

        var (team, slotId) = _lobby.GetLocalPlayerSlot();
        if (team is null) {
            return;
        }

        var slot = team.Slots[slotId];
        var company = _localPlayerCompaniesByFaction[slot.Faction].FirstOrDefault();
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
                    ChatMessages.Add($"{chatEvent.Sender}: {chatEvent.Message}");
                    PropertyChanged?.Invoke(this, new(nameof(ChatMessages))); // Superfluous?
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

        }
    }

    private ValueTask<List<LobbySlot>> MapTeamSlotsToLobbySlots(int index, Team.Slot[] slots) 
        => slots.ToAsyncEnumerable().SelectAwait(x => MapToLobbySlot(index, x)).ToListAsync();

    private Task LeaveLobby() {
        throw new NotImplementedException();
    }

    private async Task SendChatMessage() {
        string msg = ChatMessage.Trim();
        ChatMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(msg)) {
            return;
        }
        await _lobby.SendMessage("all", msg); // TODO: Support team chat
    }

    private async Task StartGame() {

        try {

            IsPlaying = true;

            // Sync corrent lobby view status with backing model based on selected PickableCompany (based on host client view!)
            await SyncLobbyCompanies();

            var buildResult = await _playService.BuildGamemode(_lobby);
            if (buildResult.Failed) {
                // TODO: Show error message
                return;
            }

            var uploadResult = await _lobby.UploadGamemode(buildResult.GamemodeSgaFileLocation); // NOP operation in singleplayer mode
            if (uploadResult.Failed) {
                // TODO: Show error message
                return;
            }

            var launchResult = await _lobby.LaunchGame(); // for multiplayer this means tell other players to launch (NOP in singleplayer)
            if (launchResult.Failed) {
                // TODO: Show error message
                return;
            }

            var playResult = await _playService.LaunchGameApp(_lobby.Game);
            if (playResult.Failed) {
                // TODO: Show error message
                return;
            }

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

    private async ValueTask<LobbySlot> MapToLobbySlot(int teamIndex, Team.Slot slot) {
        var addAICommand = new AsyncRelayCommand<AddAIPlayerToSlotEventArgs>(args => AddAIToSlot(teamIndex, args));
        var lockUnlockCommand = new AsyncRelayCommand<int>(args => LockOrUnlockSlot(teamIndex, args));
        var setCompanyCommand = new AsyncRelayCommand<PickableCompany>(args => SetSlotCompany(teamIndex, slot.Index, args));
        Participant? p = (from participant in _lobby.Participants where participant.ParticipantId == slot.ParticipantId select participant).FirstOrDefault();
        Company? c = string.IsNullOrEmpty(slot.CompanyId) ? null : (from company in _lobby.Companies where company.Key == slot.CompanyId select company.Value).FirstOrDefault();
        if (c is null && !string.IsNullOrEmpty(slot.CompanyId)) {
            c = await _companyService.GetCompanyAsync(slot.CompanyId); // Fetch from remote (or local cache) (TODO: Handle case where company was changed on remote server)
        }
        if (p is null) {
            string companyName = string.Empty;
            if (!string.IsNullOrEmpty(slot.Difficulty)) {
                companyName = c?.Name ?? string.Empty;
            }
            return new LobbySlot(slot, string.Empty, companyName, true, addAICommand, lockUnlockCommand, setCompanyCommand, this);
        }
        return new LobbySlot(slot, p.ParticipantName, c?.Name ?? string.Empty, p.IsAIParticipant, addAICommand, lockUnlockCommand, setCompanyCommand, this);
    }

    private async Task AddAIToSlot(int teamIndex, AddAIPlayerToSlotEventArgs? args) {
        if (args is null) {
            return;
        }
        if (string.IsNullOrEmpty(args.Difficulty)) {
            await _lobby.RemoveAI(teamIndex == 0 ? _lobby.Team1 : _lobby.Team2, args.SlotIndex);
        }
        await _lobby.SetSlotAIDifficulty(teamIndex == 0 ? _lobby.Team1 : _lobby.Team2, args.SlotIndex, args.Difficulty);
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
        }
    }

    private async Task SetSetting(LobbySetting? newSetting) {
        if (newSetting is null) {
            return;
        }
        await _lobby.SetSetting(newSetting);
    }

}
