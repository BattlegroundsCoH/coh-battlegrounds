using System.Threading.Channels;

using Battlegrounds.Facades.API;
using Battlegrounds.Factories;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;
using Battlegrounds.Proto.Lobbies;
using Battlegrounds.Services;
using Battlegrounds.Test.Helpers;

using Grpc.Core;

using NSubstitute;

using ChatMessage = Battlegrounds.Proto.Lobbies.ChatMessage;
using LobbySetting = Battlegrounds.Models.Lobbies.LobbySetting;
using Participant = Battlegrounds.Models.Lobbies.Participant;
using Team = Battlegrounds.Models.Lobbies.Team;

namespace Battlegrounds.Test.Models.Lobbies;

/// <summary>
/// Unit tests for <see cref="MultiplayerLobby"/>.
/// Uses <see cref="TestGrpcStreamReader"/> to push <see cref="LobbyStateUpdate"/> messages
/// without a real server, and NSubstitute for all service dependencies.
/// </summary>
[TestFixture]
public sealed class MultiplayerLobbyTests {

    // ── Per-test fields ──────────────────────────────────────────────────────

    private TestGrpcStreamReader _streamReader = null!;
    private LobbyService.LobbyServiceClient _grpcClient = null!;
    private IBattlegroundsServerAPI _serverAPI = null!;
    private IUserService _userService = null!;
    private ICompanyService _companyService = null!;
    private IGameMapService _mapService = null!;

    private Participant _localParticipant = null!;
    private Participant _remoteParticipant = null!;
    private Team _team1 = null!;
    private Team _team2 = null!;
    private Map _defaultMap = null!;
    private LobbySetup _setup;

    private MultiplayerLobby _hostLobby = null!;
    private MultiplayerLobby _participantLobby = null!;

    private const string LobbyId = "test-lobby-123";

    [SetUp]
    public void SetUp() {
        _streamReader = new TestGrpcStreamReader();

        _grpcClient = Substitute.For<LobbyService.LobbyServiceClient>();
        _serverAPI = Substitute.For<IBattlegroundsServerAPI>();
        _userService = Substitute.For<IUserService>();
        _companyService = Substitute.For<ICompanyService>();
        _mapService = Substitute.For<IGameMapService>();

        // Configure gRPC mock methods to return valid AsyncUnaryCall instances so that
        // production code can await them and NSubstitute assertion proxies (DidNotReceive/Received)
        // also return a non-null awaitable.
        var emptyCall = new AsyncUnaryCall<Empty>(
            Task.FromResult(new Empty()),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess, () => new Metadata(), () => { });
        var changeMapCall = new AsyncUnaryCall<ChangeMapResponse>(
            Task.FromResult(new ChangeMapResponse { Success = true }),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess, () => new Metadata(), () => { });
        _grpcClient.UpdateLobbyStateAsync(Arg.Any<LobbyStateUpdate>(), Arg.Any<Metadata>()).Returns(emptyCall);
        _grpcClient.LaunchGameAsync(Arg.Any<LaunchGameRequest>(), Arg.Any<Metadata>()).Returns(emptyCall);
        _grpcClient.SendChatMessageAsync(Arg.Any<ChatMessage>(), Arg.Any<Metadata>()).Returns(emptyCall);
        _grpcClient.InitiateDownloadAsync(Arg.Any<InitiateDownloadRequest>(), Arg.Any<Metadata>()).Returns(emptyCall);
        _grpcClient.ChangeMapAsync(Arg.Any<ChangeMapRequest>(), Arg.Any<Metadata>()).Returns(changeMapCall);

        _userService.GetLocalUserToken().Returns("test-bearer-token");

        var mockGame = Substitute.For<Game>();
        mockGame.Id.Returns("CoH3");

        _localParticipant = new Participant(0, "user-host", "HostPlayer", false, false);
        _remoteParticipant = new Participant(1, "user-remote", "RemotePlayer", false, false);

        _defaultMap = new Map("2p_test", "Test Map", 2, "test_preview", "2p_test");

        _team1 = new Team(TeamType.Allies, "Allies", [
            new Team.Slot(0, _localParticipant.ParticipantId, "british_africa", string.Empty, AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, string.Empty, string.Empty, AIDifficulty.HUMAN, false, false),
        ]);
        _team2 = new Team(TeamType.Axis, "Axis", [
            new Team.Slot(0, null, string.Empty, string.Empty, AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, string.Empty, string.Empty, AIDifficulty.HUMAN, false, false),
        ]);

        _setup = new LobbySetup {
            Name = "TestLobby",
            Game = mockGame,
            Self = _localParticipant,
            Team1 = _team1,
            Team2 = _team2,
            Map = _defaultMap,
            Settings = [new LobbySetting { Name = "gamemode", Type = LobbySettingType.Boolean, Value = 0 }],
            Participants = [_localParticipant, _remoteParticipant],
        };

        _hostLobby = BuildLobby(isHost: true);
        _participantLobby = BuildLobby(isHost: false);
    }

    [TearDown]
    public void TearDown() {
        _streamReader.Complete();
        _hostLobby.Dispose();
        _participantLobby.Dispose();
    }

    private MultiplayerLobby BuildLobby(bool isHost) =>
        new(LobbyId, _streamReader.WrapInCall(), _grpcClient, _setup,
            _serverAPI, _userService, _companyService, _mapService) {
            IsHost = isHost, IsReady = isHost
        };

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<LobbyEvent?> PollOneEventAsync(MultiplayerLobby lobby, int timeoutMs = 1000) {
        using var cts = new CancellationTokenSource(timeoutMs);
        var pollTask = Task.Run(lobby.PollGrpcUpdates, cts.Token);
        var getNextTask = lobby.GetNextEvent().AsTask();
        var winner = await Task.WhenAny(getNextTask, Task.Delay(timeoutMs));
        cts.Cancel();
        return winner == getNextTask ? await getNextTask : null;
    }

    private async Task<LobbyEvent?> PushAndReceiveAsync(LobbyStateUpdate update, int timeoutMs = 1000) {
        var pollTask = Task.Run(_hostLobby.PollGrpcUpdates);
        await _streamReader.PushAsync(update);
        var evt = await _hostLobby.GetNextEvent().AsTask().WaitAsync(TimeSpan.FromMilliseconds(timeoutMs));
        _streamReader.Complete();
        return evt;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  A — Initial state
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public void WhenCreated_NameMatchesSetup() =>
        Assert.That(_hostLobby.Name, Is.EqualTo(_setup.Name));

    [Test]
    public void WhenCreated_IsActiveTrue_IsReadyFalse() {
        Assert.Multiple(() => {
            Assert.That(_hostLobby.IsActive, Is.True);
            Assert.That(_hostLobby.IsReady, Is.True);
        });
    }

    [Test]
    public void WhenCreated_TeamsMatchSetup() {
        Assert.Multiple(() => {
            Assert.That(_hostLobby.Team1.TeamType, Is.EqualTo(TeamType.Allies));
            Assert.That(_hostLobby.Team2.TeamType, Is.EqualTo(TeamType.Axis));
        });
    }

    [Test]
    public void WhenCreated_ParticipantsMatchSetup() {
        Assert.That(_hostLobby.Participants, Has.Count.EqualTo(2));
        Assert.That(_hostLobby.Participants.Select(p => p.ParticipantId),
            Is.EquivalentTo(new[] { _localParticipant.ParticipantId, _remoteParticipant.ParticipantId }));
    }

    [Test]
    public void WhenCreated_MapMatchesSetup() =>
        Assert.That(_hostLobby.Map.ScenarioName, Is.EqualTo(_defaultMap.ScenarioName));

    // ════════════════════════════════════════════════════════════════════════
    //  B — GetLocalPlayerSlot
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public void GetLocalPlayerSlot_WhenParticipantInTeam1_ReturnsCorrectTeamAndIndex() {
        var (team, slotId) = _hostLobby.GetLocalPlayerSlot();
        Assert.Multiple(() => {
            Assert.That(team, Is.SameAs(_hostLobby.Team1));
            Assert.That(slotId, Is.EqualTo(0));
        });
    }

    [Test]
    public void GetLocalPlayerSlot_WhenParticipantNotInAnySlot_ReturnsNullAndMinusOne() {
        var setup = _setup with {
            Self = new Participant(99, "unplaced-user", "Unplaced", false, false),
            Team1 = new Team(TeamType.Allies, "Allies", [
                new Team.Slot(0, null, string.Empty, string.Empty, AIDifficulty.HUMAN, false, false),
            ]),
            Team2 = new Team(TeamType.Axis, "Axis", [
                new Team.Slot(0, null, string.Empty, string.Empty, AIDifficulty.HUMAN, false, false),
            ]),
        };
        using var lobby = new MultiplayerLobby(LobbyId, _streamReader.WrapInCall(), _grpcClient, setup,
            _serverAPI, _userService, _companyService, _mapService);

        var (team, slotId) = lobby.GetLocalPlayerSlot();
        Assert.Multiple(() => {
            Assert.That(team, Is.Null);
            Assert.That(slotId, Is.EqualTo(-1));
        });
    }

    // ════════════════════════════════════════════════════════════════════════
    //  C — gRPC event mapping
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task GrpcEvent_ParticipantMessage_EmitsCorrectChatEvent() {
        var update = new LobbyStateUpdate {
            EventType = "ParticipantMessage",
            ChatMessage = new ChatMessage {
                SenderId = _remoteParticipant.ParticipantId,
                Channel = "All",
                Content = "Hello!"
            }
        };

        var evt = await PushAndReceiveAsync(update);

        Assert.Multiple(() => {
            Assert.That(evt, Is.Not.Null);
            Assert.That(evt!.EventType, Is.EqualTo(LobbyEventType.ParticipantMessage));
            Assert.That(evt.Arg, Is.InstanceOf<Battlegrounds.Models.Lobbies.ChatMessage>());
            var chat = (Battlegrounds.Models.Lobbies.ChatMessage)evt.Arg!;
            Assert.That(chat.Message, Is.EqualTo("Hello!"));
            Assert.That(chat.Sender, Is.EqualTo(_remoteParticipant.ParticipantName));
        });
    }

    [Test]
    public async Task GrpcEvent_SettingUpdated_BooleanTrue_SetsValueToOne() {
        var update = new LobbyStateUpdate {
            EventType = "SettingUpdated",
            SettingsUpdate = new Proto.Lobbies.LobbySetting { Key = "gamemode", NewValue = "true" }
        };

        var evt = await PushAndReceiveAsync(update);

        Assert.Multiple(() => {
            Assert.That(evt, Is.Not.Null);
            Assert.That(evt!.EventType, Is.EqualTo(LobbyEventType.SettingUpdated));
            Assert.That(_hostLobby.Settings.First(s => s.Name == "gamemode").Value, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task GrpcEvent_SettingUpdated_BooleanFalse_SetsValueToZero() {
        // Start with value = 1
        _hostLobby.Settings.First(s => s.Name == "gamemode").Value = 1;

        var update = new LobbyStateUpdate {
            EventType = "SettingUpdated",
            SettingsUpdate = new Proto.Lobbies.LobbySetting { Key = "gamemode", NewValue = "false" }
        };

        await PushAndReceiveAsync(update);
        Assert.That(_hostLobby.Settings.First(s => s.Name == "gamemode").Value, Is.EqualTo(0));
    }

    [Test]
    public async Task GrpcEvent_SettingUpdated_Selection_SetsConfirmedIndex() {
        _hostLobby.Settings.Clear();
        _hostLobby.Settings.Add(new LobbySetting {
            Name = "gamemode",
            Type = LobbySettingType.Selection,
            Value = 0,
            Options = [
                new LobbySettingOption("Annihilation", "annihilation"),
                new LobbySettingOption("Victory Points", "victory_points")
            ]
        });
        var update = new LobbyStateUpdate {
            EventType = "SettingUpdated",
            SettingsUpdate = new Proto.Lobbies.LobbySetting { Key = "gamemode", NewValue = "1" }
        };

        await PushAndReceiveAsync(update);

        Assert.That(_hostLobby.Settings.Single().Value, Is.EqualTo(1));
    }

    [Test]
    public async Task GrpcEvent_SettingUpdated_UnknownKey_ReturnsNull() {
        var update = new LobbyStateUpdate {
            EventType = "SettingUpdated",
            SettingsUpdate = new Proto.Lobbies.LobbySetting { Key = "nonexistent_setting", NewValue = "1" }
        };

        var pollTask = Task.Run(_hostLobby.PollGrpcUpdates);
        await _streamReader.PushAsync(update);
        _streamReader.Complete();
        await pollTask.WaitAsync(TimeSpan.FromSeconds(1));

        // Unknown setting key should be silently ignored; no LobbyEvent should be emitted
        var getNextTask = _hostLobby.GetNextEvent().AsTask();
        Assert.That(getNextTask.IsCompleted, Is.False,
            "Unknown setting key should not produce a LobbyEvent");
    }

    [Test]
    public async Task GrpcEvent_SlotUpdated_UpdatesSlotState() {
        var update = new LobbyStateUpdate {
            EventType = "SlotUpdated",
            SlotUpdate = new SlotUpdate {
                TeamId = 1,
                Slot = new Slot {
                    Id = 0,
                    ParticipantId = _remoteParticipant.ParticipantId,
                    Faction = "germans",
                    CompanyId = string.Empty,
                    AiDifficulty = "Human",
                    Hidden = false,
                    Locked = false
                }
            }
        };

        var evt = await PushAndReceiveAsync(update);

        Assert.Multiple(() => {
            Assert.That(evt, Is.Not.Null);
            Assert.That(evt!.EventType, Is.EqualTo(LobbyEventType.TeamUpdated));
            Assert.That(evt.Arg, Is.EqualTo(TeamType.Axis));
            Assert.That(_hostLobby.Team2.Slots[0].ParticipantId, Is.EqualTo(_remoteParticipant.ParticipantId));
            Assert.That(_hostLobby.Team2.Slots[0].Faction, Is.EqualTo("germans"));
        });
    }

    [Test]
    public async Task GrpcEvent_SlotUpdated_WithCompanyId_TriggersCompanyDownload() {
        _companyService.GetCompanyAsync(
            Arg.Any<string>(), Arg.Any<string>(),
            localOnly: Arg.Any<bool>(),
            downloadProgressUpdate: Arg.Any<DownloadProgressUpdateDelegate>())
            .Returns(ValueTask.FromResult<Company?>(null));

        var update = new LobbyStateUpdate {
            EventType = "SlotUpdated",
            SlotUpdate = new SlotUpdate {
                TeamId = 1,
                Slot = new Slot {
                    Id = 0,
                    ParticipantId = _remoteParticipant.ParticipantId,
                    Faction = "germans",
                    CompanyId = "company-remote-1",
                    AiDifficulty = "Human",
                }
            }
        };

        await PushAndReceiveAsync(update);
        await Task.Delay(100); // Give background download task time to start

        await _companyService.Received(1).GetCompanyAsync(
            "company-remote-1",
            _remoteParticipant.ParticipantId,
            localOnly: Arg.Any<bool>(),
            downloadProgressUpdate: Arg.Any<DownloadProgressUpdateDelegate?>());
    }

    [Test]
    public async Task GrpcEvent_ParticipantReady_SetsParticipantReadyTrue() {
        var update = new LobbyStateUpdate {
            EventType = "ParticipantReady",
            ParticipantId = _remoteParticipant.ParticipantId
        };

        var evt = await PushAndReceiveAsync(update);

        Assert.Multiple(() => {
            Assert.That(evt, Is.Not.Null);
            Assert.That(evt!.EventType, Is.EqualTo(LobbyEventType.ParticipantReady));
            Assert.That(
                _hostLobby.Participants.First(p => p.ParticipantId == _remoteParticipant.ParticipantId).IsReady,
                Is.True);
        });
    }

    [Test]
    public async Task GrpcEvent_ParticipantUnready_SetsParticipantReadyFalse() {
        // Set participant as ready first
        var readyUpdate = new LobbyStateUpdate {
            EventType = "ParticipantReady",
            ParticipantId = _remoteParticipant.ParticipantId
        };
        await _streamReader.PushAsync(readyUpdate);

        // Then unready
        var unreadyReader = new TestGrpcStreamReader();
        using var lobby2 = new MultiplayerLobby(LobbyId, unreadyReader.WrapInCall(), _grpcClient, _setup,
            _serverAPI, _userService, _companyService, _mapService);

        // Force participant as ready in lobby2
        var unreadyUpdate = new LobbyStateUpdate {
            EventType = "ParticipantUnready",
            ParticipantId = _remoteParticipant.ParticipantId
        };

        var pollTask = Task.Run(lobby2.PollGrpcUpdates);
        await unreadyReader.PushAsync(unreadyUpdate);
        var evt = await lobby2.GetNextEvent().AsTask().WaitAsync(TimeSpan.FromSeconds(1));
        unreadyReader.Complete();

        Assert.Multiple(() => {
            Assert.That(evt, Is.Not.Null);
            Assert.That(evt!.EventType, Is.EqualTo(LobbyEventType.ParticipantUnready));
            Assert.That(
                lobby2.Participants.First(p => p.ParticipantId == _remoteParticipant.ParticipantId).IsReady,
                Is.False);
        });
    }

    [Test]
    public async Task GrpcEvent_GameStarted_EmitsGameStartedEvent() {
        var update = new LobbyStateUpdate { EventType = "GameStarted" };
        var evt = await PushAndReceiveAsync(update);
        Assert.That(evt?.EventType, Is.EqualTo(LobbyEventType.GameStarted));
    }

    [Test]
    public async Task GrpcEvent_SystemMessage_EmitsMappedEvent() {
        var update = new LobbyStateUpdate {
            EventType = "SystemMessage",
            SystemMessage = new SystemMessage { Content = "Server says hello." }
        };

        var evt = await PushAndReceiveAsync(update);

        Assert.Multiple(() => {
            Assert.That(evt?.EventType, Is.EqualTo(LobbyEventType.SystemMessage));
            Assert.That(evt?.Arg, Is.EqualTo("Server says hello."));
        });
    }

    [Test]
    public async Task GrpcEvent_UnknownEventType_DoesNotEmitEvent() {
        var update = new LobbyStateUpdate { EventType = "CompletelyUnknownType" };

        var pollTask = Task.Run(_hostLobby.PollGrpcUpdates);
        await _streamReader.PushAsync(update);
        // Give time for the poll loop to process the message
        await Task.Delay(100);
        _streamReader.Complete();
        await pollTask.WaitAsync(TimeSpan.FromSeconds(1));

        // If an event had been written, GetNextEvent() would return immediately
        var getNextTask = _hostLobby.GetNextEvent().AsTask();
        Assert.That(getNextTask.IsCompleted, Is.False,
            "Unknown event type should not produce a LobbyEvent");
    }

    // ════════════════════════════════════════════════════════════════════════
    //  D — Host-only guards (non-host lobby)
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task LaunchGame_WhenNotHost_DoesNotCallGrpc() {
        await _participantLobby.LaunchGame();
        _grpcClient.DidNotReceive().LaunchGameAsync(Arg.Any<LaunchGameRequest>(), Arg.Any<Metadata>());
    }

    [Test]
    public async Task RemoveAI_WhenNotHost_DoesNotCallGrpc() {
        var team = _participantLobby.Team2;
        await _participantLobby.RemoveAI(team, 0);
        _grpcClient.DidNotReceive().UpdateLobbyStateAsync(Arg.Any<LobbyStateUpdate>(), Arg.Any<Metadata>());
    }

    [Test]
    public async Task SetMap_WhenNotHost_ReturnsFalse() {
        var result = await _participantLobby.SetMap(_defaultMap);
        Assert.That(result, Is.False);
        _grpcClient.DidNotReceive().ChangeMapAsync(Arg.Any<ChangeMapRequest>(), Arg.Any<Metadata>());
    }

    [Test]
    public async Task SetSetting_WhenNotHost_DoesNotCallGrpc() {
        var setting = new LobbySetting { Name = "gamemode", Type = LobbySettingType.Boolean };
        await _participantLobby.SetSetting(setting);
        _grpcClient.DidNotReceive().UpdateLobbyStateAsync(Arg.Any<LobbyStateUpdate>(), Arg.Any<Metadata>());
    }

    [Test]
    public async Task SetSlotAIDifficulty_WhenNotHost_DoesNotCallGrpc() {
        await _participantLobby.SetSlotAIDifficulty(_participantLobby.Team2, 0, AIDifficulty.EASY);
        _ = _grpcClient.DidNotReceive().UpdateLobbyStateAsync(Arg.Any<LobbyStateUpdate>(), Arg.Any<Metadata>());
    }

    [Test]
    public async Task SetSlotFaction_WhenNotHost_DoesNotCallGrpc() {
        await _participantLobby.SetSlotFaction(_participantLobby.Team1, 0, "british_africa");
        _ = _grpcClient.DidNotReceive().UpdateLobbyStateAsync(Arg.Any<LobbyStateUpdate>(), Arg.Any<Metadata>());
    }

    [Test]
    public async Task ToggleSlotLock_WhenNotHost_DoesNotCallGrpc() {
        await _participantLobby.ToggleSlotLock(_participantLobby.Team1, 1);
        _ = _grpcClient.DidNotReceive().UpdateLobbyStateAsync(Arg.Any<LobbyStateUpdate>(), Arg.Any<Metadata>());
    }

    [Test]
    public async Task ReportMatchResult_WhenNotHost_ReturnsFalse() {
        var fakeResult = new ReplayAnalysisResult { Failed = false };
        var result = await _participantLobby.ReportMatchResult(fakeResult);
        Assert.That(result, Is.False);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  E — Host operations
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task SetCompany_AsHost_CallsGrpcUpdateLobbyState() {
        await _hostLobby.SetCompany(_team1, 0, "company-abc", "british_africa");

        _ = _grpcClient.Received(1).UpdateLobbyStateAsync(
            Arg.Is<LobbyStateUpdate>(u =>
                u.EventType == LobbyEventType.SlotUpdated.ToString() &&
                u.SlotUpdate.Slot.CompanyId == "company-abc"),
            Arg.Any<Metadata>());
    }

    [Test]
    public async Task SetSlotFaction_AsHost_EmitsTeamUpdatedWithTeamType() {
        await _hostLobby.SetSlotFaction(_team1, 0, "british_africa");

        var lobbyEvent = await _hostLobby.GetNextEvent().AsTask().WaitAsync(TimeSpan.FromSeconds(1));

        Assert.Multiple(() => {
            Assert.That(lobbyEvent!.EventType, Is.EqualTo(LobbyEventType.TeamUpdated));
            Assert.That(lobbyEvent.Arg, Is.EqualTo(TeamType.Allies));
        });
    }

    [Test]
    public async Task GrpcEvent_SettingUpdated_BooleanOne_SetsValueToOne() {
        var update = new LobbyStateUpdate {
            EventType = "SettingUpdated",
            SettingsUpdate = new Proto.Lobbies.LobbySetting { Key = "gamemode", NewValue = "1" }
        };

        await PushAndReceiveAsync(update);

        Assert.That(_hostLobby.Settings.Single().Value, Is.EqualTo(1));
    }

    [Test]
    public async Task SetSetting_AsHost_WaitsForServerEventBeforeChangingConfirmedState() {
        var requestedSetting = new LobbySetting {
            Name = "gamemode",
            Type = LobbySettingType.Boolean,
            Value = 1
        };

        await _hostLobby.SetSetting(requestedSetting);

        Assert.That(_hostLobby.Settings.Single().Value, Is.Zero);
        _ = _grpcClient.Received(1).UpdateLobbyStateAsync(
            Arg.Is<LobbyStateUpdate>(update =>
                update.EventType == LobbyEventType.SettingUpdated.ToString() &&
                update.SettingsUpdate.NewValue == "1"),
            Arg.Any<Metadata>());
        Assert.That(_hostLobby.GetNextEvent().AsTask().IsCompleted, Is.False);
    }

    [Test]
    public async Task SendMessage_SendsLocalEventAndCallsGrpc() {
        await _hostLobby.SendMessage(Battlegrounds.Models.Lobbies.ChatChannel.All, "test message");

        // Event should be queued internally
        var internalEvent = await _hostLobby.GetNextEvent().AsTask().WaitAsync(TimeSpan.FromSeconds(1));
        Assert.That(internalEvent?.EventType, Is.EqualTo(LobbyEventType.ParticipantMessage));

        // gRPC should have been called
        _ = _grpcClient.Received(1).SendChatMessageAsync(Arg.Any<ChatMessage>(), Arg.Any<Metadata>());
    }

    [Test]
    public async Task MarkReady_AsHost_ReadyIsTrueAndGrpcNotCalled() {
        await _hostLobby.MarkReady(true);
        Assert.That(_hostLobby.IsReady, Is.True);
        _ = _grpcClient.Received(0).UpdateLobbyStateAsync(Arg.Any<LobbyStateUpdate>(), Arg.Any<Metadata>());
    }

    [Test]
    public async Task ToggleSlotLock_AsHost_TogglesLockedStateAndCallsGrpc() {
        Assert.That(_hostLobby.Team1.Slots[1].Locked, Is.False);

        await _hostLobby.ToggleSlotLock(_hostLobby.Team1, 1);

        Assert.That(_hostLobby.Team1.Slots[1].Locked, Is.True);
        _ = _grpcClient.Received(1).UpdateLobbyStateAsync(Arg.Any<LobbyStateUpdate>(), Arg.Any<Metadata>());
    }

    [Test]
    public async Task RemoveAI_AsHost_RemovesParticipantAndCallsGrpc() {
        // Place an AI in slot 0 of team 2 first via local state
        var aiParticipant = new Participant(99, "ai-1", "AI Easy", true, false);
        _hostLobby.Participants.Add(aiParticipant);

        var aiSlotTeam = _hostLobby.Team2;
        await _hostLobby.RemoveAI(aiSlotTeam, 0);

        _ = _grpcClient.Received(1).UpdateLobbyStateAsync(
            Arg.Is<LobbyStateUpdate>(u => u.EventType == LobbyEventType.SlotUpdated.ToString()),
            Arg.Any<Metadata>());
    }

    // ════════════════════════════════════════════════════════════════════════
    //  F — Company download
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task SlotUpdated_WithCompanyId_DownloadsAndUpdatesCompaniesDict() {
        var fakeCompany = new Company { Id = "company-desert-rats-1", Name = "Desert Rats", Faction = "british_africa", GameId = "CoH3" };
        _companyService.GetCompanyAsync(
            fakeCompany.Id, _remoteParticipant.ParticipantId,
            localOnly: Arg.Any<bool>(),
            downloadProgressUpdate: Arg.Any<DownloadProgressUpdateDelegate?>())
            .Returns(ValueTask.FromResult<Company?>(fakeCompany));

        var update = new LobbyStateUpdate {
            EventType = "SlotUpdated",
            SlotUpdate = new SlotUpdate {
                TeamId = 1,
                Slot = new Slot {
                    Id = 0,
                    ParticipantId = _remoteParticipant.ParticipantId,
                    Faction = "british_africa",
                    CompanyId = fakeCompany.Id,
                    AiDifficulty = "Human",
                }
            }
        };

        await PushAndReceiveAsync(update);
        await Task.Delay(200); // Allow fire-and-forget download task to complete

        Assert.That(_hostLobby.Companies.ContainsKey(fakeCompany.Id), Is.True);
    }

    [Test]
    public async Task SlotUpdated_WhenCompanyChanges_IgnoresSupersededDownload() {
        var firstCompletion = new TaskCompletionSource<Company?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondCompletion = new TaskCompletionSource<Company?>(TaskCreationOptions.RunContinuationsAsynchronously);
        _companyService.GetCompanyAsync(
                Arg.Any<string>(),
                _remoteParticipant.ParticipantId,
                localOnly: Arg.Any<bool>(),
                downloadProgressUpdate: Arg.Any<DownloadProgressUpdateDelegate?>())
            .Returns(call => new ValueTask<Company?>(
                call.ArgAt<string>(0) == "company-first"
                    ? firstCompletion.Task
                    : secondCompletion.Task));

        var pollTask = Task.Run(_hostLobby.PollGrpcUpdates);
        await _streamReader.PushAsync(CreateCompanySlotUpdate("company-first"));
        _ = await _hostLobby.GetNextEvent().AsTask().WaitAsync(TimeSpan.FromSeconds(1));
        var firstRevision = _hostLobby.GetSlotRevision(1, 0);

        await _streamReader.PushAsync(CreateCompanySlotUpdate("company-second"));
        _ = await _hostLobby.GetNextEvent().AsTask().WaitAsync(TimeSpan.FromSeconds(1));
        var secondRevision = _hostLobby.GetSlotRevision(1, 0);

        secondCompletion.SetResult(new Company {
            Id = "company-second",
            Name = "Second",
            Faction = "british_africa",
            GameId = "CoH3"
        });
        firstCompletion.SetResult(new Company {
            Id = "company-first",
            Name = "First",
            Faction = "british_africa",
            GameId = "CoH3"
        });
        await Task.Delay(100);

        Assert.Multiple(() => {
            Assert.That(secondRevision, Is.GreaterThan(firstRevision));
            Assert.That(_hostLobby.Companies.ContainsKey("company-second"), Is.True);
            Assert.That(_hostLobby.Companies.ContainsKey("company-first"), Is.False);
        });

        _streamReader.Complete();
        await pollTask.WaitAsync(TimeSpan.FromSeconds(1));
    }

    private LobbyStateUpdate CreateCompanySlotUpdate(string companyId) => new() {
        EventType = "SlotUpdated",
        SlotUpdate = new SlotUpdate {
            TeamId = 1,
            Slot = new Slot {
                Id = 0,
                ParticipantId = _remoteParticipant.ParticipantId,
                Faction = "british_africa",
                CompanyId = companyId,
                AiDifficulty = "Human",
            }
        }
    };

    [Test]
    public async Task Reconnection_AppliesAuthoritativeSnapshotAndReturnsConnected() {
        var reconnectReader = new TestGrpcStreamReader();
        var reconnectLobby = new MultiplayerLobby(
            LobbyId,
            _streamReader.WrapInCall(),
            _grpcClient,
            _setup,
            _serverAPI,
            _userService,
            _companyService,
            _mapService,
            _ => reconnectReader.WrapInCall()) {
            IsHost = true,
            IsReady = true
        };
        await reconnectReader.PushAsync(new LobbyStateUpdate {
            EventType = "LobbyState",
            LobbyState = new Proto.Lobbies.Lobby {
                Id = LobbyId,
                Name = "Snapshot Lobby",
                HostId = _localParticipant.ParticipantId,
                GameId = "CoH3",
                Participants = {
                    new Proto.Lobbies.Participant {
                        ParticipantId = _localParticipant.ParticipantId,
                        Name = "Host from snapshot",
                        Ready = true
                    }
                },
                Teams = {
                    new Proto.Lobbies.Team {
                        Id = 0,
                        Alias = "Allies",
                        Type = "Allies",
                        Slots = {
                            new Slot {
                                Id = 0,
                                ParticipantId = _localParticipant.ParticipantId,
                                Faction = "british_africa",
                                CompanyId = string.Empty,
                                AiDifficulty = "Human"
                            }
                        }
                    },
                    new Proto.Lobbies.Team {
                        Id = 1,
                        Alias = "Axis",
                        Type = "Axis",
                        Slots = {
                            new Slot {
                                Id = 0,
                                ParticipantId = "snapshot-player",
                                Faction = "germans",
                                CompanyId = string.Empty,
                                AiDifficulty = "Human"
                            }
                        }
                    }
                }
            }
        });

        var pollTask = reconnectLobby.PollGrpcUpdates();
        _streamReader.Complete();

        LobbyEvent? snapshotApplied = null;
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(4));
        while (snapshotApplied?.EventType != LobbyEventType.SnapshotApplied) {
            snapshotApplied = await reconnectLobby.GetNextEvent(timeout.Token);
        }

        Assert.Multiple(() => {
            Assert.That(reconnectLobby.ConnectionState, Is.EqualTo(LobbyConnectionState.Connected));
            Assert.That(reconnectLobby.Team2.Slots[0].ParticipantId, Is.EqualTo("snapshot-player"));
            Assert.That(reconnectLobby.Team2.Slots[0].Faction, Is.EqualTo("germans"));
            Assert.That(reconnectLobby.Revision, Is.GreaterThan(0));
            Assert.That(snapshotApplied!.Revision, Is.EqualTo(reconnectLobby.Revision));
        });

        await reconnectLobby.DisposeAsync();
        await pollTask.WaitAsync(TimeSpan.FromSeconds(1));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  G — Lifecycle
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public void Dispose_SetsIsActiveFalse() {
        _hostLobby.Dispose();
        Assert.That(_hostLobby.IsActive, Is.False);
    }

    [Test]
    public async Task PollGrpcUpdates_StopsWhenStreamEnds() {
        var pollTask = Task.Run(_hostLobby.PollGrpcUpdates);
        _streamReader.Complete(); // Signal end-of-stream
        await pollTask.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.That(pollTask.IsCompleted, Is.True);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Regression — GetRealPlayersCount bug
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Regression test: <see cref="MultiplayerLobby.GetRealPlayersCount"/> was incorrectly
    /// returning the count of AI participants instead of non-AI (human) players.
    /// </summary>
    [Test]
    public void GetRealPlayersCount_ReturnsHumanPlayerCount_NotAiCount() {
        // Setup: 2 human participants, no AI
        Assert.That(_hostLobby.GetRealPlayersCount(), Is.EqualTo(2),
            "GetRealPlayersCount() should return the number of human (non-AI) participants.");
    }
}
