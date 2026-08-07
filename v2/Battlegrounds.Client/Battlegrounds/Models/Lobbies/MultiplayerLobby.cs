using System.Threading.Channels;

using System.Collections.Concurrent;

using Battlegrounds.Facades.API;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;
using Battlegrounds.Proto.Lobbies;
using Battlegrounds.Services;
using Battlegrounds.Services.Infrastructure;

using Grpc.Core;

using Serilog;

using LobbySetup = Battlegrounds.Factories.LobbySetup;

namespace Battlegrounds.Models.Lobbies;

/// <summary>
/// Represents a multiplayer lobby that manages participants, teams, game settings, and real-time state synchronization
/// for a battlegrounds session.
/// </summary>
/// <remarks>The MultiplayerLobby class is designed for real-time multiplayer scenarios where lobby state must be
/// kept consistent across multiple clients. It supports participant management, team assignments, chat messaging, and
/// game configuration. Only the host can perform certain actions, such as changing settings or starting the game. The
/// class implements IDisposable to ensure proper cleanup of resources, including the gRPC streaming
/// connection.</remarks>
/// <param name="lobbyId">The unique identifier for the lobby, used to reference and manage the lobby in server communications.</param>
/// <param name="stateUpdater">An asynchronous server streaming call that provides real-time updates to the lobby state, enabling synchronization
/// of lobby events between the client and server.</param>
/// <param name="gRPCClient">The gRPC client used to communicate with the lobby service for operations such as sending messages, updating state,
/// and managing lobby membership.</param>
/// <param name="setup">An object containing the initial configuration for the lobby, including the local participant, teams, game settings,
/// and map.</param>
/// <param name="serverAPI">An interface for interacting with the battlegrounds server API, providing methods for server-side operations related
/// to the lobby.</param>
/// <param name="userService">A service for managing user-related operations, such as retrieving the local user's token and information.</param>
/// <param name="companyService">A service for managing company-related operations, such as retrieving and updating company data for participants.</param>
public sealed class MultiplayerLobby(
    string lobbyId, 
    AsyncServerStreamingCall<LobbyStateUpdate> stateUpdater, 
    LobbyService.LobbyServiceClient gRPCClient, 
    LobbySetup setup,
    IBattlegroundsServerAPI serverAPI,
    IUserService userService,
    ICompanyService companyService,
    IGameMapService mapService,
    Func<CancellationToken, AsyncServerStreamingCall<LobbyStateUpdate>>? reconnect = null) : ILobby, IDisposable, IAsyncDisposable {

    private const string __SERVER_MAP_SETTING_KEY = "$map";

    private readonly ILogger _logger = Log.ForContext<MultiplayerLobby>();
    private readonly string _lobbyId = lobbyId;

    private AsyncServerStreamingCall<LobbyStateUpdate> _stateUpdater = stateUpdater;
    private readonly Func<CancellationToken, AsyncServerStreamingCall<LobbyStateUpdate>>? _reconnect = reconnect;
    private readonly LobbyService.LobbyServiceClient _gRPCClient = gRPCClient;
    private readonly IBattlegroundsServerAPI _serverAPI = serverAPI;
    private readonly ICompanyService _companyService = companyService;
    private readonly IUserService _userService = userService;
    private readonly IGameMapService _mapService = mapService;

    private readonly Participant _localParticipant = setup.Self;
    private readonly HashSet<Participant> _participants = setup.Participants;
    private readonly List<LobbySetting> _settings = setup.Settings;
    private readonly Dictionary<string, Company> _companies = [];
    private readonly Channel<LobbyEvent> _internalEvents = Channel.CreateUnbounded<LobbyEvent>();
    private readonly CancellationTokenSource _lifetimeCts = new();
    private readonly Dictionary<(int TeamId, int SlotId), CancellationTokenSource> _slotDownloadCts = [];
    private readonly ConcurrentDictionary<(int TeamId, int SlotId), long> _slotRevisions = [];
    private readonly object _slotDownloadLock = new();

    private readonly Team _team1 = setup.Team1;
    private readonly Team _team2 = setup.Team2;
    private readonly Team[] _teams = [setup.Team1, setup.Team2];

    private readonly int _victoryPointsSettingIndex = setup.Settings.FindIndex(x => x.Name == LobbySetting.SETTING_VICTORY_POINTS);

    private bool _isActive = true;
    private bool _isReady = false;
    private bool _disposedValue = false;
    private Task? _updateLoopTask;
    private long _revision;
    private LobbyConnectionState _connectionState = LobbyConnectionState.Connected;

    private Dictionary<string, Company>? _latestMatchCompanies;

    private Map _map = setup.Map;

    public string Name { get; } = setup.Name;

    public string Id => _lobbyId;

    public bool IsHost { get; init; } = true; // Assuming the host is the one who created the lobby

    public bool IsActive => _isActive;

    public LobbyConnectionState ConnectionState => _connectionState;

    public long Revision => Interlocked.Read(ref _revision);

    public ISet<Participant> Participants => _participants;

    public Team Team1 => _team1;

    public Team Team2 => _team2;

    public Game Game { get; } = setup.Game;

    public Dictionary<string, Company> Companies => _companies;

    public IList<LobbySetting> Settings => _settings;

    public Map Map => _map;

    public bool IsReady {
        get => _isReady;
        init => _isReady = value;
    }

    public string? GetLocalPlayerId() => _localParticipant.ParticipantId;

    private Metadata GetGrpcMetadata() {
        var token = $"Bearer {_userService.GetLocalUserToken()}";
        return new Metadata {
            { "authorization", token },
            { "x-lobby-id", _lobbyId },
            { "x-participant-id", _localParticipant.ParticipantId }
        };
    }

    private int GetIndexOfTeam(Team? team) {
        if (team == null) {
            return -1;
        }
        if (team == _team1) {
            return 0;
        }
        if (team == _team2) {
            return 1;
        }
        return -1; // Team not found
    } 

    public (Team? team, int slotId) GetLocalPlayerSlot() {
        var id = Array.FindIndex(_team1.Slots, x => x.ParticipantId == _localParticipant.ParticipantId);
        if (id != -1) {
            return (_team1, id);
        }
        id = Array.FindIndex(_team2.Slots, x => x.ParticipantId == _localParticipant.ParticipantId);
        if (id != -1) {
            return (_team2, id);
        }
        return (null, -1);
    }

    public long GetSlotRevision(int teamId, int slotId) =>
        _slotRevisions.TryGetValue((teamId, slotId), out var revision) ? revision : 0;

    public ValueTask<LobbyEvent?> GetNextEvent() => GetNextEvent(CancellationToken.None);

    public async ValueTask<LobbyEvent?> GetNextEvent(CancellationToken cancellationToken) {
        try {
            return await _internalEvents.Reader.ReadAsync(cancellationToken);
        } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            return null;
        } catch (Exception ex) {
            _logger.Error(ex, "Error while getting next lobby event");
            return null;
        }
    }

    /// <summary>
    /// Continuously polls the gRPC stream for lobby updates and forwards them as lobby events to the internal event
    /// channel for UI consumption.
    /// </summary>
    /// <remarks>This method runs as long as the instance remains active, handling lobby updates received from
    /// the gRPC stream. If the stream is cancelled, such as when leaving the lobby or shutting down, polling stops
    /// gracefully. Any unrecognized lobby updates are converted to a default system message event. Errors encountered
    /// during polling are logged for diagnostic purposes.</remarks>
    /// <returns>A task that represents the asynchronous polling operation.</returns>
    public Task PollGrpcUpdates() => PollGrpcUpdates(CancellationToken.None);

    public void StartPolling() {
        if (_updateLoopTask is not null) {
            return;
        }
        _updateLoopTask = PollGrpcUpdates(_lifetimeCts.Token);
    }

    public async Task PollGrpcUpdates(CancellationToken cancellationToken) {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_lifetimeCts.Token, cancellationToken);
        var token = linkedCts.Token;
        var reconnectAttempt = 0;
        SetConnectionState(LobbyConnectionState.Connected);
        // Polls the gRPC stream for lobby updates and pushes them to the internal channel as LobbyEvents for the UI to consume
        // That avoids the issue of reading from the gRPC stream and fetching the next internal event (ie. for client-side actions) at the same time
        while (_isActive && !token.IsCancellationRequested) {
            try {
                if (await _stateUpdater.ResponseStream.MoveNext(token)) {
                    reconnectAttempt = 0;
                    var lobbyEvent = MapAndApplyGrpcEvent(_stateUpdater.ResponseStream.Current);
                    if (lobbyEvent is not null) {
                        _internalEvents.Writer.TryWrite(lobbyEvent with { Revision = Revision }); // Map the gRPC update to a LobbyEvent and push it to the internal channel
                    }
                } else {
                    if (!await TryReconnect(++reconnectAttempt, token)) {
                        break;
                    }
                }
            } catch (OperationCanceledException) when (token.IsCancellationRequested) {
                break;
            } catch (RpcException rpcEx) when (rpcEx.StatusCode is StatusCode.Cancelled or StatusCode.Unavailable) {
                _logger.Warning(rpcEx, "Lobby update stream was interrupted.");
                if (!await TryReconnect(++reconnectAttempt, token)) {
                    break;
                }
            } catch (Exception ex) {
                _logger.Error(ex, "Error while polling gRPC lobby updates");
                if (!await TryReconnect(++reconnectAttempt, token)) {
                    break;
                }
            }
        }
        if (_reconnect is not null && _isActive && !token.IsCancellationRequested) {
            SetConnectionState(LobbyConnectionState.Disconnected);
        }
    }

    private async Task<bool> TryReconnect(int attempt, CancellationToken cancellationToken) {
        if (_reconnect is null || !_isActive || cancellationToken.IsCancellationRequested) {
            return false;
        }

        SetConnectionState(LobbyConnectionState.Reconnecting);
        CancelAllSlotDownloads();
        try
        {
            var delay = TimeSpan.FromSeconds(Math.Min(30, Math.Pow(2, Math.Min(attempt - 1, 4))));
            await Task.Delay(delay, cancellationToken);
            var replacement = _reconnect(cancellationToken);
            if (!await replacement.ResponseStream.MoveNext(cancellationToken)
                || replacement.ResponseStream.Current.LobbyState is null)
            {
                replacement.Dispose();
                return true;
            }

            _stateUpdater.Dispose();
            _stateUpdater = replacement;
            ApplySnapshot(replacement.ResponseStream.Current.LobbyState);
            SetConnectionState(LobbyConnectionState.Connected);
            _internalEvents.Writer.TryWrite(new LobbyEvent(
                LobbyEventType.SnapshotApplied,
                null,
                Revision));
            _ = SyncRemoteCompanies(cancellationToken);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (RpcException rpcEx) when (rpcEx.StatusCode is StatusCode.Cancelled or StatusCode.Unavailable or StatusCode.NotFound)
        {
            _isActive = false;
            _logger.Warning(rpcEx, "Lobby update stream was interrupted.");
            await _internalEvents.Writer.WriteAsync(
                new LobbyEvent(LobbyEventType.ConnectionStateChanged, false), cancellationToken);
            return false;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Lobby reconnection attempt {Attempt} failed.", attempt);
            return true;
        }
    }

    private void SetConnectionState(LobbyConnectionState state) {
        if (_connectionState == state) {
            return;
        }
        _connectionState = state;
        _internalEvents.Writer.TryWrite(new LobbyEvent(
            LobbyEventType.ConnectionStateChanged,
            state,
            Revision));
    }

    private void ApplySnapshot(Proto.Lobbies.Lobby snapshot) {
        Interlocked.Increment(ref _revision);

        _participants.Clear();
        foreach (var participant in snapshot.Participants) {
            _participants.Add(new Participant(
                -1,
                participant.ParticipantId,
                participant.Name,
                participant.IsAi,
                participant.Ready));
        }

        for (var teamId = 0; teamId < Math.Min(_teams.Length, snapshot.Teams.Count); teamId++) {
            var sourceTeam = snapshot.Teams[teamId];
            var targetSlots = _teams[teamId].Slots;
            for (var slotId = 0; slotId < targetSlots.Length; slotId++) {
                if (slotId < sourceTeam.Slots.Count) {
                    var sourceSlot = sourceTeam.Slots[slotId];
                    targetSlots[slotId] = new Team.Slot(
                        slotId,
                        sourceSlot.ParticipantId,
                        sourceSlot.Faction,
                        sourceSlot.CompanyId,
                        AIDifficulty.FromName(sourceSlot.AiDifficulty),
                        sourceSlot.Hidden,
                        sourceSlot.Locked);
                } else {
                    targetSlots[slotId] = targetSlots[slotId] with {
                        ParticipantId = string.Empty,
                        CompanyId = string.Empty,
                        Hidden = true
                    };
                }
                IncrementSlotRevision(teamId, slotId);
            }
        }

        foreach (var setting in _settings) {
            if (!snapshot.Settings.TryGetValue(setting.Name, out var value)) {
                continue;
            }
            setting.Value = setting.Type switch {
                LobbySettingType.Boolean => value.Equals("true", StringComparison.OrdinalIgnoreCase) || value == "1" ? 1 : 0,
                _ => int.TryParse(value, out var parsed) ? parsed : setting.Value
            };
        }

        if (snapshot.Settings.TryGetValue("$map", out var scenarioName)) {
            _map = Map.FromScenario(_mapService.GetMapByScenarioName(Game, scenarioName));
        }
    }

    // Maps a gRPC lobby state update to a LobbyEvent and applies the necessary changes to the local lobby state. If the update type is unrecognized, returns null.
    // The returned type is for triggering UI updates based on the event, as some events may not require a UI update.
    private LobbyEvent? MapAndApplyGrpcEvent(LobbyStateUpdate? update) { 
        if (update == null) {
            return null;
        }
        Interlocked.Increment(ref _revision);
        if (!Enum.TryParse(update.EventType, true, out LobbyEventType eventType)) {
            _logger.Warning("Unknown gRPC lobby event type: {EventType}", update.EventType);
            return null;
        }
        switch (eventType) {
            case LobbyEventType.ParticipantJoined:
                Participant joinedParticipant = new Participant(-1, update.ParticipantUpdate.ParticipantId, update.ParticipantUpdate.Name, update.ParticipantUpdate.IsAi, update.ParticipantUpdate.Ready);
                _participants.Add(joinedParticipant);
                return new LobbyEvent(LobbyEventType.ParticipantJoined, joinedParticipant);
            case LobbyEventType.ParticipantLeft:
                var leftParticipant = _participants.FirstOrDefault(p => p.ParticipantId == update.ParticipantUpdate.ParticipantId);
                if (leftParticipant is not null) {
                    _participants.Remove(leftParticipant);
                }
                return new LobbyEvent(LobbyEventType.ParticipantLeft, leftParticipant);
            case LobbyEventType.ParticipantMessage:
                var senderName = _participants.FirstOrDefault(p => p.ParticipantId == update.ChatMessage.SenderId)?.ParticipantName ?? "Unknown";
                var channel = Enum.TryParse(update.ChatMessage.Channel, true, out ChatChannel parsedChannel) ? parsedChannel : ChatChannel.All;
                return new LobbyEvent(LobbyEventType.ParticipantMessage,
                    new ChatMessage(update.ChatMessage.SenderId, senderName, channel, update.ChatMessage.Content));
            case LobbyEventType.TeamUpdated:
                for (int i = 0; i < update.TeamUpdate.Slots.Count; i++) {
                    var slot = update.TeamUpdate.Slots[i];
                    _teams[update.TeamUpdate.Id].Slots[i] = new Team.Slot(i, slot.ParticipantId, slot.Faction, slot.CompanyId, AIDifficulty.FromName(slot.AiDifficulty), slot.Hidden, slot.Locked);
                    var slotRevision = IncrementSlotRevision(update.TeamUpdate.Id, i);
                    StartCompanyDownload(update.TeamUpdate.Id, i, slot.ParticipantId, slot.CompanyId, slotRevision);
                }
                var teamType = update.TeamUpdate.Id == 0 ? _team1.TeamType : _team2.TeamType;
                return new LobbyEvent(LobbyEventType.TeamUpdated, teamType);
            case LobbyEventType.SlotUpdated:
                var updatedSlot = update.SlotUpdate.Slot;
                _teams[update.SlotUpdate.TeamId].Slots[updatedSlot.Id] = new(updatedSlot.Id, updatedSlot.ParticipantId, updatedSlot.Faction, updatedSlot.CompanyId, AIDifficulty.FromName(updatedSlot.AiDifficulty), updatedSlot.Hidden, updatedSlot.Locked);
                var updatedSlotRevision = IncrementSlotRevision(update.SlotUpdate.TeamId, updatedSlot.Id);
                StartCompanyDownload(update.SlotUpdate.TeamId, updatedSlot.Id, updatedSlot.ParticipantId, updatedSlot.CompanyId, updatedSlotRevision);
                return new LobbyEvent(LobbyEventType.TeamUpdated, _teams[update.SlotUpdate.TeamId].TeamType); // Make UI simply update the whole team when a slot is updated for simplicity, as that's what the UI currently supports
            case LobbyEventType.SettingUpdated:
                var newSetting = update.SettingsUpdate;
                if (newSetting.Key is __SERVER_MAP_SETTING_KEY) {
                    return null; // The map setting is handled separately in the MapUpdated event, so we ignore it here to avoid duplicate handling.
                }
                int indexOfSetting = _settings.FindIndex(x => x.Name == newSetting.Key);
                if (indexOfSetting != -1) {
                    var currentSetting = _settings[indexOfSetting];
                    int mappedValue = currentSetting.Type switch {
                        LobbySettingType.Boolean => newSetting.NewValue.Equals("true", StringComparison.OrdinalIgnoreCase) || newSetting.NewValue == "1" ? 1 : 0,
                        LobbySettingType.Integer => int.TryParse(newSetting.NewValue, out var intValue) ? intValue : currentSetting.Value,
                        LobbySettingType.Selection => int.TryParse(newSetting.NewValue, out var selectedIndex) ? selectedIndex : currentSetting.Value,
                        _ => currentSetting.Value
                    };
                    _settings[indexOfSetting].Value = mappedValue;
                    if (newSetting.Key is LobbySetting.SETTING_GAMEMODE && _victoryPointsSettingIndex != -1) {
                        if (newSetting.NewValue is "1") { // If the gamemode is set to "1" (which we assume corresponds to a mode that uses victory points), make the victory points setting visible (needs better semantics)
                            _settings[_victoryPointsSettingIndex].IsVisible = true;
                        } else {
                            _settings[_victoryPointsSettingIndex].IsVisible = false;
                        }
                        _internalEvents.Writer.TryWrite(new LobbyEvent(LobbyEventType.SettingUpdated, _settings[_victoryPointsSettingIndex])); // Notify the UI about the visibility change of the victory points setting
                    }
                    return new LobbyEvent(LobbyEventType.SettingUpdated, _settings[indexOfSetting]);
                } else {
                    _logger.Warning("Received update for unknown setting: {SettingKey}", newSetting.Key);
                    return null; // Setting not found, ignore the update
                }
            case LobbyEventType.MapUpdated:
                var newMap = _mapService.GetMapByScenarioName(Game, update.Map.MapId);
                _map = Map.FromScenario(newMap);
                return new LobbyEvent(LobbyEventType.MapUpdated, _map);
            case LobbyEventType.GameStarted:
                return new LobbyEvent(LobbyEventType.GameStarted); // Instructs the LobbyViewModel to start the game.
            case LobbyEventType.GameCancelled:
                throw new NotImplementedException($"Event type {eventType} is not yet implemented in the gRPC lobby handler.");
            case LobbyEventType.GameEnded:
                throw new NotImplementedException($"Event type {eventType} is not yet implemented in the gRPC lobby handler.");
            case LobbyEventType.SystemMessage:
            case LobbyEventType.SystemError:
                return new LobbyEvent(eventType, update.SystemMessage.Content);
            case LobbyEventType.DownloadInitiated:
                _ = BeginDownloadResource(update.DownloadState.ResourceId); // Start the download but don't await it, as we don't want to block the processing of further lobby updates while waiting for the download to complete
                return new LobbyEvent(LobbyEventType.DownloadInitiated, update.DownloadState.ResourceId); // Ignored by the UI for now, but could be used to trigger a download progress UI in the future
            case LobbyEventType.DownloadProgress:
                return new LobbyEvent(LobbyEventType.DownloadProgress); // NOP for now
            case LobbyEventType.DownloadCompleted:
                return new LobbyEvent(LobbyEventType.DownloadCompleted); // NOP for now
            case LobbyEventType.ParticipantReady:
                var participant = _participants.FirstOrDefault(p => p.ParticipantId == update.ParticipantId);
                if (participant is not null) {
                    _participants.Remove(participant);
                    _participants.Add(participant with { IsReady = true });
                }
                return new LobbyEvent(LobbyEventType.ParticipantReady, update.ParticipantId);
            case LobbyEventType.ParticipantUnready:
                var participantUnready = _participants.FirstOrDefault(p => p.ParticipantId == update.ParticipantId);
                if (participantUnready is not null) {
                    _participants.Remove(participantUnready);
                    _participants.Add(participantUnready with { IsReady = false });
                }
                return new LobbyEvent(LobbyEventType.ParticipantUnready, update.ParticipantId);
            case LobbyEventType.MatchOver:
                return new LobbyEvent(LobbyEventType.MatchOver); // Instructs the LobbyViewModel to show the match results screen.
            default:
                _logger.Warning("Unhandled gRPC lobby event type: {EventType}", eventType);
                break;
        }
        return null; // No event to return
    }

    private long IncrementSlotRevision(int teamId, int slotId) =>
        _slotRevisions.AddOrUpdate((teamId, slotId), 1, static (_, revision) => revision + 1);

    private void StartCompanyDownload(int teamId, int slotId, string? participantId, string? companyId, long slotRevision) {
        CancellationTokenSource cts;
        lock (_slotDownloadLock) {
            if (_slotDownloadCts.Remove((teamId, slotId), out var previous)) {
                previous.Cancel();
                previous.Dispose();
            }
            cts = CancellationTokenSource.CreateLinkedTokenSource(_lifetimeCts.Token);
            _slotDownloadCts[(teamId, slotId)] = cts;
        }

        if (string.IsNullOrWhiteSpace(participantId) || string.IsNullOrWhiteSpace(companyId)) {
            cts.Cancel();
            lock (_slotDownloadLock) {
                _slotDownloadCts.Remove((teamId, slotId));
            }
            cts.Dispose();
            return;
        }

        _ = DownloadCompanyForParticipant(teamId, slotId, participantId, companyId, slotRevision, cts);
    }

    private bool IsCurrentSlotDownload(int teamId, int slotId, string companyId, long slotRevision, CancellationToken token) =>
        !token.IsCancellationRequested
        && GetSlotRevision(teamId, slotId) == slotRevision
        && string.Equals(_teams[teamId].Slots[slotId].CompanyId, companyId, StringComparison.Ordinal);

    private async Task DownloadCompanyForParticipant(
        int teamId,
        int slotId,
        string participantId,
        string companyId,
        long slotRevision,
        CancellationTokenSource cts) {
        var cancellationToken = cts.Token;
        try {
            var company = await _companyService.GetCompanyAsync(companyId, participantId, downloadProgressUpdate: async (downloaded, total) => {
                if (!IsCurrentSlotDownload(teamId, slotId, companyId, slotRevision, cancellationToken)) {
                    return;
                }
                long totalBytes = total ?? 0;
                float progress = totalBytes > 0 ? (float)downloaded / totalBytes : 0;
                await _internalEvents.Writer.WriteAsync(
                    new LobbyEvent(
                        LobbyEventType.SlotCompanyDownloadProgress,
                        new SlotCompanyDownloadUpdate(teamId, slotId, companyId, slotRevision, Progress: progress),
                        Revision),
                    cancellationToken);
            }).AsTask().WaitAsync(cancellationToken);

            if (company is null || company.Id != companyId
                || !IsCurrentSlotDownload(teamId, slotId, companyId, slotRevision, cancellationToken)) {
                return;
            }

            _companies[company.Id] = company;
            await _internalEvents.Writer.WriteAsync(
                new LobbyEvent(
                    LobbyEventType.SlotCompanyDownloadProgress,
                    new SlotCompanyDownloadUpdate(teamId, slotId, companyId, slotRevision, Company: company),
                    Revision),
                cancellationToken);
        } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            // A newer slot revision superseded this download.
        } catch (Exception ex) {
            _logger.Error(ex, "Failed to download company data for participant {ParticipantId} with company ID {CompanyId}", participantId, companyId);
        } finally {
            lock (_slotDownloadLock) {
                if (_slotDownloadCts.TryGetValue((teamId, slotId), out var current) && ReferenceEquals(current, cts)) {
                    _slotDownloadCts.Remove((teamId, slotId));
                    cts.Dispose();
                }
            }
        }
    }

    private void CancelAllSlotDownloads() {
        lock (_slotDownloadLock) {
            foreach (var cts in _slotDownloadCts.Values) {
                cts.Cancel();
                cts.Dispose();
            }
            _slotDownloadCts.Clear();
        }
    }

    public async Task<LaunchGameResult> LaunchGame() {

        if (!IsHost) {
            _logger.Warning("Non-host participant attempted to launch the game. This action is only allowed for the host.");
            return new LaunchGameResult() {}; // Only the host can launch the game
        }

        await _gRPCClient.LaunchGameAsync(new(), GetGrpcMetadata());

        return new LaunchGameResult() {}; // TODO: Return actual result from gRPC call
    }

    public async Task RemoveAI(Team team, int slotIndex) {
        if (!IsHost) {
            return; // Only the host can remove AI
        }
        int teamId = GetIndexOfTeam(team);
        await _gRPCClient.UpdateLobbyStateAsync(new LobbyStateUpdate {
            LobbyId = _lobbyId,
            EventType = LobbyEventType.SlotUpdated.ToString(),
            SlotUpdate = new SlotUpdate {
                TeamId = teamId,
                Slot = new Slot {
                    Id = slotIndex,
                    ParticipantId = string.Empty,
                    Faction = string.Empty,
                    CompanyId = string.Empty,
                    AiDifficulty = AIDifficulty.HUMAN.Name,
                    Hidden = team.Slots[slotIndex].Hidden,
                    Locked = team.Slots[slotIndex].Locked
                }
            },
        }, GetGrpcMetadata());
        _participants.RemoveWhere(p => p.ParticipantId == team.Slots[slotIndex].ParticipantId); // Remove the participant from the lobby if it was an AI (ie. it won't be in the participants list if it was a human player)
        team.Slots[slotIndex] = team.Slots[slotIndex] with { ParticipantId = string.Empty, Faction = string.Empty, CompanyId = string.Empty, Difficulty = AIDifficulty.HUMAN }; // Update local state
        _internalEvents.Writer.TryWrite(new LobbyEvent(LobbyEventType.TeamUpdated, team.TeamType)); // Notify the UI of the change
    }

    public async ValueTask<bool> ReportMatchResult(ReplayAnalysisResult matchResult) {
        if (!IsHost) {
            return false; // Only the host can report match results
        }

        if (matchResult.Failed || matchResult.Replay is null) {
            _logger.Error("Match result for game {GameId} failed or replay is null", matchResult.GameId);
            return false; // Cannot report match result if it failed or replay is null
        }

        _latestMatchCompanies = _companies.ToDictionary(kvp => kvp.Key, kvp => kvp.Value); // Cache the latest company states before the match result is applied, so that we can show the changes to the player on the match results screen
        var result = matchResult.GetMatchResult(this);
        if (result == MatchResult.Unknown) {
            _logger.Error("Match result for game {GameId} could not be determined", matchResult.GameId);
            return false; // Cannot determine match result
        }

        if (!result.IsValid) {
            _logger.Error("Match result for game {GameId} is invalid", matchResult.GameId);
            return false;
        }

        result.LobbyId = _lobbyId; // Ensure the lobby ID is set on the match result

        var reported = await _serverAPI.ReportMatchResults(result, async (progress, done, totalBytes) => { 
            if (done) {
                await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TrayMessageHide)); // Notify the UI about the completed upload
            } else {
                await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TrayMessage, $"Uploading match result... {progress:P2} complete")); // Notify the UI about the upload progress
            }
        });
        if (!reported) {
            _logger.Error("Failed to report match result for game {GameId} to the server", matchResult.GameId);
            return false;
        } else {
            var participantDownloadTask = _gRPCClient.InitiateDownloadAsync(new InitiateDownloadRequest {
                ResourceId = "company_update" // After reporting the match result, initiate a download to update company data for all participants, as the match result may have caused changes to company stats, levels, etc.
            }, GetGrpcMetadata());
            // Download the host company changes (other participants have been told to download their company).
            var selfDownloadTask = DownloadCompany(false);
            await Task.WhenAll(participantDownloadTask.ResponseAsync, selfDownloadTask); // Wait for both the participant download initiation and the local company download to complete
        }

        return true;

    }

    public async Task SendMessage(ChatChannel channel, string msg) {
        var chatMessage = new ChatMessage(_localParticipant.ParticipantId, _localParticipant.ParticipantName, channel, msg);
        await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.ParticipantMessage, chatMessage));
        await _gRPCClient.SendChatMessageAsync(new Proto.Lobbies.ChatMessage {
            Content = chatMessage.Message,
            Channel = channel.ToString().ToLowerInvariant(),
        }, GetGrpcMetadata());
    }

    public async Task SetCompany(Team team, int slotId, string companyId, string faction) {
        var local = GetLocalPlayerSlot();
        await _gRPCClient.UpdateLobbyStateAsync(new LobbyStateUpdate {
            LobbyId = _lobbyId,
            EventType = LobbyEventType.SlotUpdated.ToString(),
            SlotUpdate = new SlotUpdate {
                TeamId = GetIndexOfTeam(team),
                Slot = new Slot {
                    Id = slotId,
                    ParticipantId = team.Slots[slotId].ParticipantId ?? string.Empty,
                    Faction = faction,
                    CompanyId = companyId,
                    AiDifficulty = team.Slots[slotId].Difficulty.Name,
                    Hidden = team.Slots[slotId].Hidden,
                    Locked = team.Slots[slotId].Locked
                }
            },
        }, GetGrpcMetadata());
        if (IsHost || (local.team == team && slotId == local.slotId)) {
            // Push the local event too to update the UI immediately
            team.Slots[slotId] = team.Slots[slotId] with { CompanyId = companyId, Faction = faction };
            await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TeamUpdated, team.TeamType)); // Notify the UI
        }
    }

    public async Task<bool> SetMap(Map map) {
        if (!IsHost) {
            return false; // Only the host can set the map
        }
        var updateMap = await _gRPCClient.ChangeMapAsync(new() {
            NewMap = new() {
                MaxPlayers = map.MaxPlayers,
                MapId = map.ScenarioName
            }
        }, GetGrpcMetadata());
        if (updateMap is null || !updateMap.Success) {
            var errorReason = updateMap?.ErrorReason switch {
                1 => "The specified map was not found on the server.",
                2 => "The map is invalid or corrupted.",
                3 => "Failed to load the map due to a server error.",
                4 => "Map max player count cannot be less than current number of participants",
                _ => "An unknown error occurred while updating the map."
            };
            await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.SystemError, "Failed to update the map. "+ errorReason)); // Notify the UI about the failure and reason
            return false;
        }
        return true;
    }

    public async Task SetSetting(LobbySetting newSetting) {
        if (!IsHost) {
            return; // Only the host can set settings
        }
        await PublishSetting(newSetting); // Confirmed state is updated only when the server stream publishes SettingUpdated
    }

    public async Task SetSlotAIDifficulty(Team team, int slotIndex, AIDifficulty difficulty) {
        if (!IsHost) {
            return; // Only the host can set AI difficulty
        }
        int teamId = GetIndexOfTeam(team);
        await _gRPCClient.UpdateLobbyStateAsync(new LobbyStateUpdate {
            LobbyId = _lobbyId,
            EventType = LobbyEventType.SlotUpdated.ToString(),
            SlotUpdate = new SlotUpdate {
                TeamId = teamId,
                Slot = new Slot {
                    Id = slotIndex,
                    ParticipantId = team.Slots[slotIndex].ParticipantId ?? string.Empty,
                    Faction = team.Slots[slotIndex].Faction,
                    CompanyId = team.Slots[slotIndex].CompanyId,
                    AiDifficulty = difficulty.Name,
                    Hidden = team.Slots[slotIndex].Hidden,
                    Locked = team.Slots[slotIndex].Locked
                }
            },
        }, GetGrpcMetadata());

        // Add participant to the lobby if not already added
        int participantId = teamId * 4 + slotIndex; // Generate a unique participant ID for the AI based on its team and slot index
        string participantIdStr = participantId.ToString();
        if (!_participants.Any(x => x.ParticipantId == participantIdStr)) {
            Participant aiParticipant = new Participant(participantId, participantIdStr, $"AI Player {participantId}", true, true);
            _participants.Add(aiParticipant);
        }

        team.Slots[slotIndex] = team.Slots[slotIndex] with { Difficulty = difficulty, ParticipantId = participantIdStr }; // Update local state
        await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TeamUpdated, team.TeamType)); // Notify the UI of the change
    }

    public async Task SetSlotFaction(Team team, int slotIndex, string? faction) {
        if (!IsHost) {
            return; // Only the host can set slot faction
        }
        int teamId = GetIndexOfTeam(team);
        await _gRPCClient.UpdateLobbyStateAsync(new LobbyStateUpdate {
            LobbyId = _lobbyId,
            EventType = LobbyEventType.SlotUpdated.ToString(),
            SlotUpdate = new SlotUpdate {
                TeamId = teamId,
                Slot = new Slot {
                    Id = slotIndex,
                    ParticipantId = team.Slots[slotIndex].ParticipantId ?? string.Empty,
                    Faction = faction ?? string.Empty,
                    CompanyId = team.Slots[slotIndex].CompanyId,
                    AiDifficulty = team.Slots[slotIndex].Difficulty.Name,
                    Hidden = team.Slots[slotIndex].Hidden,
                    Locked = team.Slots[slotIndex].Locked
                }
            },
        }, GetGrpcMetadata());
        team.Slots[slotIndex] = team.Slots[slotIndex] with { Faction = faction ?? string.Empty }; // Update local state
        await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TeamUpdated, team.TeamType)); // Notify the UI of the change
    }

    public async Task ToggleSlotLock(Team team, int slotIndex) {
        if (!IsHost) {
            return; // Only the host can toggle slot locks
        }
        var slot = team.Slots[slotIndex];
        var newLockState = !slot.Locked;
        int teamId = GetIndexOfTeam(team);
        await _gRPCClient.UpdateLobbyStateAsync(new LobbyStateUpdate {
            LobbyId = _lobbyId,
            EventType = LobbyEventType.SlotUpdated.ToString(),
            SlotUpdate = new SlotUpdate {
                TeamId = teamId,
                Slot = new Slot {
                    Id = slotIndex,
                    ParticipantId = team.Slots[slotIndex].ParticipantId ?? string.Empty,
                    Faction = team.Slots[slotIndex].Faction ?? string.Empty,
                    CompanyId = team.Slots[slotIndex].CompanyId,
                    AiDifficulty = team.Slots[slotIndex].Difficulty.Name,
                    Hidden = team.Slots[slotIndex].Hidden,
                    Locked = newLockState
                }
            },
        }, GetGrpcMetadata());
        team.Slots[slotIndex] = team.Slots[slotIndex] with { Locked = newLockState }; // Update local state
        await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TeamUpdated, team.TeamType)); // Notify the UI of the change
    }

    public async ValueTask<UploadGamemodeResult> UploadGamemode(string gamemodeLocation) {
        var result = await _serverAPI.UploadGamemodeAsync(_lobbyId, gamemodeLocation, async (progress, done, totalBytes) => {
            if (done) {
                await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TrayMessageHide)); // Notify the UI about the completed upload
            } else {
                await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TrayMessage, $"Uploading gamemode... {progress:P2} complete")); // Notify the UI about the upload progress
            }
        });
        if (!result) {
            await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.SystemError, "Failed to upload gamemode. Please report this issue.")); // Notify the UI about the failure
            return new UploadGamemodeResult() { Failed = true };
        }
        return new UploadGamemodeResult() { Failed = false };
    }

    public async ValueTask<bool> WaitForAllPlayersHaveGamemode() {
        if (!IsHost) {
            return false;
        }

        var initiateDownloadRequest = new InitiateDownloadRequest() {
            ResourceId = "gamemode"
        };

        var metadata = GetGrpcMetadata();

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(3)); // Set a timeout for waiting, in case something goes wrong with the download process
        var token = cts.Token;

        bool allDownloaded = false;
        var responseStream = _gRPCClient.BeginInitiateDownload(initiateDownloadRequest, metadata);
        while (await responseStream.ResponseStream.MoveNext(token)) {
            var update = responseStream.ResponseStream.Current;
            if (update.AllCompleted) {
                allDownloaded = true;
                break;
            }
            // TODO: Else, report progress to the UI about which participants have downloaded the gamemode so far
        }

        if (!allDownloaded) {
            await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.SystemError, "Timed out while waiting for all players to download the gamemode. Please report this issue.")); // Notify the UI about the timeout
        }

        return allDownloaded;
    }

    /// <summary>
    /// Publishes the initial state of the local multiplayer lobby to the server, including team configurations and
    /// lobby settings.
    /// </summary>
    /// <remarks>Call this method during lobby initialization to ensure the server receives the current team
    /// assignments and all lobby settings. This is typically required before players can join or interact with the
    /// lobby.</remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task PublishInitialState() {

        // Tell server about the local lobby state
        await PublishTeam(0, setup.Team1);
        await PublishTeam(1, setup.Team2);

        // Publish settings
        foreach (var setting in setup.Settings) {
            await PublishSetting(setting);
        }

        // Publish map (Minor hack, should use the proper gRPC procedure for changing the map.
        // But this avoids having the server re-send new team data
        await PublishSetting("$map", _map.ScenarioName);

    }

    private async Task PublishTeam(int tid, Team team) {
        await _gRPCClient.UpdateLobbyStateAsync(new LobbyStateUpdate {
            LobbyId = _lobbyId,
            EventType = LobbyEventType.TeamUpdated.ToString(),
            TeamUpdate = new Proto.Lobbies.Team {
                Id = tid,
                Alias = team.TeamAlias,
                Type = team.TeamType.ToString(),
                Slots = { team.Slots.Select(slot => new Slot {
                    Id = slot.Index,
                    ParticipantId = slot.ParticipantId ?? string.Empty,
                    Faction = slot.Faction,
                    CompanyId = slot.CompanyId,
                    AiDifficulty = slot.Difficulty.Name,
                    Hidden = slot.Hidden,
                    Locked = slot.Locked
                }) }
            }
        }, GetGrpcMetadata());
    }

    private async Task PublishSlot(int tid, int slotId, Team.Slot slot) {
        await _gRPCClient.UpdateLobbyStateAsync(new LobbyStateUpdate {
            LobbyId = _lobbyId,
            EventType = LobbyEventType.SlotUpdated.ToString(),
            SlotUpdate = new Proto.Lobbies.SlotUpdate {
                TeamId = tid,
                Slot = new Slot {
                    Id = slotId,
                    ParticipantId = slot.ParticipantId ?? string.Empty,
                    Faction = slot.Faction,
                    CompanyId = slot.CompanyId,
                    AiDifficulty = slot.Difficulty.Name,
                    Hidden = slot.Hidden,
                    Locked = slot.Locked
                }
            },
        }, GetGrpcMetadata());
    }

    private async Task PublishSetting(LobbySetting setting) {
        var metadata = GetGrpcMetadata();
        await _gRPCClient.UpdateLobbyStateAsync(new LobbyStateUpdate {
            LobbyId = _lobbyId,
            EventType = LobbyEventType.SettingUpdated.ToString(),
            SettingsUpdate = new Proto.Lobbies.LobbySetting {
                Key = setting.Name,
                NewValue = setting.Value.ToString(),
            },
        }, metadata);
    }


    private async Task PublishSetting(string key, string value) {
        var metadata = GetGrpcMetadata();
        await _gRPCClient.UpdateLobbyStateAsync(new LobbyStateUpdate {
            LobbyId = _lobbyId,
            EventType = LobbyEventType.SettingUpdated.ToString(),
            SettingsUpdate = new Proto.Lobbies.LobbySetting {
                Key = key,
                NewValue = value,
            },
        }, metadata);
    }


    public void Dispose() {
        if (!_disposedValue) {
            _isActive = false;
            _disposedValue = true;
            _lifetimeCts.Cancel();
            CancelAllSlotDownloads();
            SetConnectionState(LobbyConnectionState.Disposed);
            // Complete the internal event channel so any consumers waiting on GetNextEvent() will unblock
            _internalEvents.Writer.TryComplete();
            // Close connection with the server (and the lobby) and dispose of the gRPC client
            _stateUpdater.Dispose();
            _lifetimeCts.Dispose();
        }
    }

    public async ValueTask DisposeAsync() {
        Dispose();
        if (_updateLoopTask is null) {
            return;
        }
        try {
            await _updateLoopTask.ConfigureAwait(false);
        } catch (OperationCanceledException) {
            // Expected when disposal stops the connection loop.
        }
    }

    public async Task LeaveAsync() {
        await _gRPCClient.LeaveLobbyAsync(new(), GetGrpcMetadata());
    }

    private Task BeginDownloadResource(string resourceId) => resourceId switch {
        "gamemode" => DownloadGamemode(),
        "company_update" => DownloadCompany(reportProgress: true), // When downloading company updates after a match, we want to report progress to the UI as it can sometimes take a while if there are many participants in the lobby
        _ => UnknownResource(resourceId)
    };

    private Task UnknownResource(string resourceId) {
        _logger.Warning("Received download initiation for unknown resource ID: {ResourceId}", resourceId);
        return Task.CompletedTask;
    }

    private async Task DownloadGamemode() {

        string destination = Game switch {
            CoH3 => CoH3ArchiverService.ArchiveDestination,
            _ => throw new NotSupportedException($"Game {Game.GameName} is not supported for gamemode downloads.")
        };

        var downloadResult = await _serverAPI.DownloadGamemodeAsync(_lobbyId, destination, async (downloaded, total) => {
            var totalSafe = total ?? 0;
            float progress = totalSafe > 0 ? (float)downloaded / totalSafe : 0;
            _logger.Information("Gamemode download progress: {Progress:P2}", progress);
            _internalEvents.Writer.TryWrite(new LobbyEvent(LobbyEventType.TrayMessage, $"Downloading gamemode... {progress:P2} complete")); // Notify the UI about the download progress
            await _gRPCClient.ReportDownloadProgressAsync(new ReportDownloadProgressRequest {
                Progress = progress
            }, GetGrpcMetadata()); // Report the download progress to the server so it can update the lobby state and notify other participants
        });

        if (downloadResult) {
            _logger.Information("Gamemode download completed successfully.");
            _internalEvents.Writer.TryWrite(new LobbyEvent(LobbyEventType.TrayMessageHide)); // Notify the UI about the successful download

            // Notify server that the download is complete, so it can update the lobby state and notify other participants
            await _gRPCClient.ReportDownloadProgressAsync(new ReportDownloadProgressRequest {
                Progress = 1.0f,
                Completed = true
            }, GetGrpcMetadata());

        } else {
            _logger.Error("Gamemode download failed.");
            _internalEvents.Writer.TryWrite(new LobbyEvent(LobbyEventType.SystemError, "Failed to download gamemode. Please report this issue.")); // Notify the UI about the failure
        }

    }

    private async Task DownloadCompany(bool reportProgress) {

        var selfSlot = GetLocalPlayerSlot();
        if (selfSlot.team == null || selfSlot.slotId == -1) {
            _logger.Error("Local participant is not assigned to any slot in the lobby, cannot download company data.");
            return; // Local participant is not assigned to any slot, cannot determine which company to download
        }

        var selfCompany = selfSlot.team.Slots[selfSlot.slotId].CompanyId;
        var updatedCompany = await _serverAPI.GetCompanyAsync(selfCompany, GetLocalPlayerId() ?? throw new InvalidOperationException("Could not get local participant ID while attempting to download company data."), async (downloaded, total) => {
            _internalEvents.Writer.TryWrite(new LobbyEvent(LobbyEventType.TrayMessage, $"Downloading updated company data... {downloaded} / {total} bytes")); // Notify the UI about the download progress
        });
        if (updatedCompany is null) {
            _logger.Error("Failed to download company data for company ID {CompanyId}", selfCompany);
            await _internalEvents.Writer.WriteAsync(new LobbyEvent(LobbyEventType.SystemError, "Failed to download company data. Please report this issue.")); // Notify the UI about the failure
            return;
        }

        await _companyService.SaveCompany(updatedCompany, syncWithRemote: false);

        if (reportProgress) { 
            await _gRPCClient.ReportDownloadProgressAsync(new ReportDownloadProgressRequest {
                Progress = 1.0f,
                Completed = true
            }, GetGrpcMetadata()); // Report to the server that the company download is complete, so it can update the lobby state and notify other participants
        }

        _internalEvents.Writer.TryWrite(new LobbyEvent(LobbyEventType.TrayMessageHide)); // Notify the UI to hide the tray message about downloading company data

    }

    public Participant? GetParticipant(string participantId) => _participants.FirstOrDefault(p => p.ParticipantId == participantId);

    public int GetRealPlayersCount() => _participants.Count(x => !x.IsAIParticipant);

    public async Task BeginMatch() {
        await _gRPCClient.BeginMatchAsync(new Empty(), GetGrpcMetadata());
    }

    public async Task EndMatch(EndMatchReason reason) {
        await _gRPCClient.EndMatchAsync(new() {
            Reason = reason switch {
                EndMatchReason.GameCancelled => Proto.Lobbies.EndMatchReason.Aborted,
                EndMatchReason.MatchEndedInSuccess => Proto.Lobbies.EndMatchReason.Success,
                EndMatchReason.ScarError => Proto.Lobbies.EndMatchReason.ScarError,
                _ => Proto.Lobbies.EndMatchReason.Unknown
            }
        }, GetGrpcMetadata());
    }

    public async ValueTask PublishSystemMessage(string message) {
        await _gRPCClient.UpdateLobbyStateAsync(new LobbyStateUpdate {
            LobbyId = _lobbyId,
            EventType = LobbyEventType.SystemMessage.ToString(),
            SystemMessage = new SystemMessage {
                MessageType = "info",
                Content = message
            }
        }, GetGrpcMetadata());
    }

    public async Task MarkReady(bool isReady) {
        if (IsHost) {
            return; // The host cannot mark themselves as ready/unready, as they are always considered ready
        }
        _isReady = isReady;
        var eventType = isReady ? LobbyEventType.ParticipantReady : LobbyEventType.ParticipantUnready;
        await _gRPCClient.UpdateLobbyStateAsync(new LobbyStateUpdate {
            LobbyId = _lobbyId,
            EventType = eventType.ToString(),
        }, GetGrpcMetadata());
        await _internalEvents.Writer.WriteAsync(new LobbyEvent(eventType, isReady)); // Notify the UI about the ready state change
    }

    public Task KickPlayer(Team team, int slotIndex) {
        throw new NotImplementedException();
    }

    public async Task<MatchOverData?> GetMatchResults() {
        var serverVersion = await _serverAPI.GetLatestMatchResult(_lobbyId);
        if (serverVersion is null) {
            _logger.Error("Failed to retrieve match results from the server for lobby {LobbyId}", _lobbyId);
            return null;
        }

        Dictionary<string, Company> matchCompanies;
        if (_latestMatchCompanies is null) {
            _logger.Warning(
                "Latest match companies snapshot is null for lobby {LobbyId}; falling back to current company cache.",
                _lobbyId);
            matchCompanies = _companies.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        } else {
            matchCompanies = _latestMatchCompanies;
        }

        return MatchOverData.FromMatchResultForPlayer(serverVersion, _localParticipant.ParticipantId, matchCompanies);
    }

    public Task SyncRemoteCompanies(CancellationToken cancellationToken = default) {
        for (int teamId = 0; teamId < 2; teamId++) {
            var team = _teams[teamId];
            for (int slotIndex = 0; slotIndex < team.Slots.Length; slotIndex++) {
                var slot = team.Slots[slotIndex];
                if (slot.ParticipantId == _localParticipant.ParticipantId) {
                    continue; // Skip downloading company data for the local participant, as it should already be up to date
                }
                if (!string.IsNullOrEmpty(slot.CompanyId) && !string.IsNullOrEmpty(slot.ParticipantId)) {
                    var slotRevision = IncrementSlotRevision(teamId, slotIndex);
                    StartCompanyDownload(teamId, slotIndex, slot.ParticipantId, slot.CompanyId, slotRevision);
                }
            }
        }
        return Task.CompletedTask;
    }

    public async Task MoveToSlot(Team team, int slotIndex) {
        // Simple gRPC call to the server to move the local participant to the specified slot, the server will handle updating the lobby state and notifying all participants of the change
        await _gRPCClient.MoveSlotAsync(new MoveSlotRequest {
            TargetTeamId = GetIndexOfTeam(team),
            TargetSlotId = slotIndex
        }, GetGrpcMetadata());
    }

    public int GetTeam(Participant participant) {
        if (Team1.Participants.Contains(participant.ParticipantId))
            return 0;
        else
            return 1;
    }

}
