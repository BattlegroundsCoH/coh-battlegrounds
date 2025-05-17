using System.Collections.ObjectModel;
using System.ComponentModel;

using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Services;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.ViewModels;

public sealed class LobbyViewModel : INotifyPropertyChanged {

    private readonly ILobby _lobby;
    private readonly ILobbyService _lobbyService;
    private readonly IPlayService _playService;
    private readonly IReplayService _replayService;
    private readonly ICompanyService _companyService;
    private readonly ObservableCollection<string> _chatMessages = [];
    private readonly Dictionary<string, List<Company>> _localPlayerCompanies = [];

    private string _chatMessage = string.Empty;

    private bool _isPlaying = false;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string LobbyName => _lobby.Name;

    public IAsyncRelayCommand LeaveCommand { get; }

    public IAsyncRelayCommand SendMessageCommand { get; }

    public IAsyncRelayCommand StartMatchCommand { get; }

    public bool IsHost => _lobby.IsHost;

    public bool CanStartMatch => _lobby.IsHost && !_isPlaying /* && _lobby.Players.Count > 1*/;

    public ObservableCollection<string> ChatMessages => _chatMessages;

    public ICollection<Team.Slot> Team1Slots => _lobby.Team1.Slots;

    public ICollection<Team.Slot> Team2Slots => _lobby.Team2.Slots;

    public Map SelectedMap => new Map("MapName", "MapDescription", 10, "Preview.png");

    public Dictionary<string, string> SelectedSettings => [];

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

    public LobbyViewModel(ILobby lobby, ILobbyService lobbyService, IPlayService playService, IReplayService replayService, ICompanyService companyService) {
        
        _lobby = lobby;
        _lobbyService = lobbyService;
        _playService = playService;
        _replayService = replayService;
        _companyService = companyService;

        LeaveCommand = new AsyncRelayCommand(LeaveLobby);
        SendMessageCommand = new AsyncRelayCommand(SendChatMessage);
        StartMatchCommand = new AsyncRelayCommand(StartGame, () => CanStartMatch);
        
        PollLobbyEvents();
        LoadLocalPlayerCompanies();

    }

    private async void LoadLocalPlayerCompanies() {

        string[] factions = _lobby.Game.FactionIds;
        foreach (string faction in factions) {
            _localPlayerCompanies[faction] = await _companyService.GetLocalPlayerCompaniesForFaction(faction);
        }

        var (team, slotId) = _lobby.GetLocalPlayerSlot();
        if (team is null) {
            return;
        }

        var slot = team.Slots[slotId];
        var company = _localPlayerCompanies[slot.Faction].FirstOrDefault();
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
                default:
                    break;
            }

        }
    }

    private Task LeaveLobby() {
        throw new NotImplementedException();
    }

    private Task SendChatMessage() {
        throw new NotImplementedException();
    }

    private async Task StartGame() {

        try {

            IsPlaying = true;

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

            var replayAnalysis = await _replayService.AnalyseReplay(matchResult.ReplayFilePath);
            if (replayAnalysis.Failed) {
                // TODO: Show error message
                return;
            }

            await _lobby.ReportMatchResult(replayAnalysis);

        } finally {
            IsPlaying = false;
        }

    }

}
