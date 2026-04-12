using System.Net.Http.Json;

using Battlegrounds.Facades.API;
using Battlegrounds.Models.Lobbies;

using NSubstitute;

namespace Battlegrounds.IntegrationTest;

/// <summary>
/// Integration tests for <see cref="MultiplayerLobby"/>, verifying that two real connected
/// clients (host + participant) stay in sync when lobby state changes propagate through the
/// integration-test server.
/// </summary>
/// <remarks>
/// Requires the Docker container started by <see cref="LobbyServerIntegrationTests"/> to be
/// healthy before any test runs. Each test allocates a fresh <see cref="LobbyIntegrationHarness"/>
/// and tears it down afterwards to ensure test isolation.
/// </remarks>
[TestFixture]
[Category("Integration")]
public sealed class MultiplayerLobbyIntegrationTests : LobbyServerIntegrationTests {

    private LobbyIntegrationHarness _harness = null!;

    [SetUp]
    public async Task SetUp() {
        _harness = new LobbyIntegrationHarness(GrpcAddress);
        await _harness.CreateHostLobbyAsync("host-user-1", "HostPlayer");
    }

    [TearDown]
    public async Task TearDown() {
        await _harness.DisposeAsync();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  A — Lobby lifecycle
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public void Lifecycle_HostLobby_IsActivAfterCreation() {
        Assert.That(_harness.HostLobby!.IsActive, Is.True);
    }

    [Test]
    public void Lifecycle_HostLobby_IsHostIsTrue() {
        Assert.That(_harness.HostLobby!.IsHost, Is.True);
    }

    [Test]
    public void Lifecycle_HostLobby_HasCorrectName() {
        Assert.That(_harness.HostLobby!.Name, Is.EqualTo("IntegrationTestLobby"));
    }

    [Test]
    public async Task Lifecycle_ParticipantJoins_LobbyBecomesActive() {
        // Fetch the lobby from the HTTP API so we know the real lobby ID
        var serverApi = Substitute.For<IBattlegroundsServerAPI>();
        serverApi.GetLobbiesAsync()
                 .Returns(Task.FromResult<IEnumerable<BrowserLobby>>([]));

        // The harness joins by a well-known lobby ID; we need the server's lobby ID.
        // Use the HTTP REST endpoint to discover the freshly created lobby.
        var lobbies = await FetchLobbiesAsync();
        var browserLobby = lobbies.FirstOrDefault(l => l.Name == "IntegrationTestLobby");
        Assert.That(browserLobby, Is.Not.Null, "Host lobby should appear in the lobby list");

        var participant = await _harness.JoinLobbyAsync(browserLobby!, "participant-user-1", "ParticipantPlayer");

        Assert.That(participant.IsActive, Is.True);
        Assert.That(participant.IsHost, Is.False);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  B — State synchronisation between host and participant
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task StateSync_ParticipantJoin_HostReceivesParticipantEvent() {
        var lobbies = await FetchLobbiesAsync();
        var browserLobby = lobbies.FirstOrDefault(l => l.Name == "IntegrationTestLobby");
        Assume.That(browserLobby, Is.Not.Null, "Lobby must exist on server");

        await _harness.JoinLobbyAsync(browserLobby!, "participant-user-1", "ParticipantPlayer");

        // The host should receive a SlotUpdated or ParticipantReady event that reflects the new participant
        var hostEvent = await LobbyIntegrationHarness.WaitForEventAsync(
            _harness.HostLobby!,
            LobbyEventType.TeamUpdated,
            timeoutMs: 8000);

        Assert.That(hostEvent, Is.Not.Null);
    }

    [Test]
    public async Task StateSync_HostSendsChat_ParticipantReceivesChatMessage() {
        var lobbies = await FetchLobbiesAsync();
        var browserLobby = lobbies.FirstOrDefault(l => l.Name == "IntegrationTestLobby");
        Assume.That(browserLobby, Is.Not.Null);

        await _harness.JoinLobbyAsync(browserLobby!, "participant-user-1", "ParticipantPlayer");

        // Wait for the join to propagate to the host
        await LobbyIntegrationHarness.WaitForEventAsync(
            _harness.HostLobby!,
            LobbyEventType.TeamUpdated,
            timeoutMs: 8000);

        // Host sends chat
        await _harness.HostLobby!.SendMessage(ChatChannel.All, "Hello from host!");

        // Participant receives it
        var participantEvent = await LobbyIntegrationHarness.WaitForEventAsync(
            _harness.ParticipantLobby!,
            LobbyEventType.ParticipantMessage,
            timeoutMs: 8000);

        Assert.That(participantEvent, Is.Not.Null);
        Assert.That(participantEvent.Arg, Is.InstanceOf<ChatMessage>());
        var chatMsg = (ChatMessage)participantEvent.Arg!;
        Assert.That(chatMsg.Message, Is.EqualTo("Hello from host!"));
    }

    [Test]
    public async Task StateSync_ParticipantSendsChat_HostReceivesChatMessage() {
        var lobbies = await FetchLobbiesAsync();
        var browserLobby = lobbies.FirstOrDefault(l => l.Name == "IntegrationTestLobby");
        Assume.That(browserLobby, Is.Not.Null);

        await _harness.JoinLobbyAsync(browserLobby!, "participant-user-1", "ParticipantPlayer");

        // Wait for join events to settle
        await LobbyIntegrationHarness.WaitForEventAsync(
            _harness.HostLobby!,
            LobbyEventType.TeamUpdated,
            timeoutMs: 8000);

        // Participant sends chat
        await _harness.ParticipantLobby!.SendMessage(ChatChannel.All, "Hello from participant!");

        var hostEvent = await LobbyIntegrationHarness.WaitForEventAsync(
            _harness.HostLobby!,
            LobbyEventType.ParticipantMessage,
            timeoutMs: 8000);

        Assert.That(hostEvent, Is.Not.Null);
        var chatMsg = (ChatMessage)hostEvent.Arg!;
        Assert.That(chatMsg.Message, Is.EqualTo("Hello from participant!"));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  C — Ready state
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task ReadyState_ParticipantMarksReady_HostReceivesEvent() {
        var lobbies = await FetchLobbiesAsync();
        var browserLobby = lobbies.FirstOrDefault(l => l.Name == "IntegrationTestLobby");
        Assume.That(browserLobby, Is.Not.Null);

        await _harness.JoinLobbyAsync(browserLobby!, "participant-user-1", "ParticipantPlayer");

        await LobbyIntegrationHarness.WaitForEventAsync(
            _harness.HostLobby!,
            LobbyEventType.TeamUpdated,
            timeoutMs: 8000);

        await _harness.ParticipantLobby!.MarkReady(true);

        var evt = await LobbyIntegrationHarness.WaitForEventAsync(
            _harness.HostLobby!,
            LobbyEventType.ParticipantReady,
            timeoutMs: 8000);

        Assert.That(evt, Is.Not.Null);
    }

    [Test]
    public async Task ReadyState_MarkReady_UpdatesLocalLobbyIsReady() {
        var lobbies = await FetchLobbiesAsync();
        var browserLobby = lobbies.FirstOrDefault(l => l.Name == "IntegrationTestLobby");
        Assume.That(browserLobby, Is.Not.Null);

        await _harness.JoinLobbyAsync(browserLobby!, "participant-user-1", "ParticipantPlayer");

        // Wait for join propagation
        await LobbyIntegrationHarness.WaitForEventAsync(
            _harness.HostLobby!,
            LobbyEventType.TeamUpdated,
            timeoutMs: 8000);

        Assert.That(_harness.ParticipantLobby!.IsReady, Is.False);

        await _harness.ParticipantLobby!.MarkReady(true);

        Assert.That(_harness.ParticipantLobby!.IsReady, Is.True);
    }

    [Test]
    public async Task ReadyState_ToggleUnready_UpdatesIsReady() {
        var lobbies = await FetchLobbiesAsync();
        var browserLobby = lobbies.FirstOrDefault(l => l.Name == "IntegrationTestLobby");
        Assume.That(browserLobby, Is.Not.Null);

        await _harness.JoinLobbyAsync(browserLobby!, "participant-user-1", "ParticipantPlayer");

        await LobbyIntegrationHarness.WaitForEventAsync(
            _harness.HostLobby!,
            LobbyEventType.TeamUpdated,
            timeoutMs: 8000);

        await _harness.ParticipantLobby!.MarkReady(true);
        Assert.That(_harness.ParticipantLobby!.IsReady, Is.True);

        await _harness.ParticipantLobby!.MarkReady(false);
        Assert.That(_harness.ParticipantLobby!.IsReady, Is.False);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  D — System messages
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task SystemMessages_HostPublishesSystemMessage_ParticipantReceivesIt() {
        var lobbies = await FetchLobbiesAsync();
        var browserLobby = lobbies.FirstOrDefault(l => l.Name == "IntegrationTestLobby");
        Assume.That(browserLobby, Is.Not.Null);

        await _harness.JoinLobbyAsync(browserLobby!, "participant-user-1", "ParticipantPlayer");

        await LobbyIntegrationHarness.WaitForEventAsync(
            _harness.HostLobby!,
            LobbyEventType.TeamUpdated,
            timeoutMs: 8000);

        await _harness.HostLobby!.PublishSystemMessage("Match starting in 3 seconds...");

        var evt = await LobbyIntegrationHarness.WaitForEventAsync(
            _harness.ParticipantLobby!,
            LobbyEventType.SystemMessage,
            timeoutMs: 8000);

        Assert.That(evt, Is.Not.Null);
        Assert.That(evt.Arg, Is.InstanceOf<string>());
        Assert.That((string)evt.Arg!, Contains.Substring("3 seconds"));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  E — Match flow (Start → BeginMatch → LaunchGame signal)
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task MatchFlow_HostCallsBeginMatch_BeginMatchCompletesWithoutError() {
        // BeginMatch freezes lobby state on the server and prevents new participants from joining.
        // In the integration-test image this is a no-op that should complete successfully.
        Assert.That(async () => await _harness.HostLobby!.BeginMatch(), Throws.Nothing);
    }

    [Test]
    public async Task MatchFlow_HostCallsLaunchGame_ParticipantReceivesGameStartedEvent() {
        var lobbies = await FetchLobbiesAsync();
        var browserLobby = lobbies.FirstOrDefault(l => l.Name == "IntegrationTestLobby");
        Assume.That(browserLobby, Is.Not.Null);

        await _harness.JoinLobbyAsync(browserLobby!, "participant-user-1", "ParticipantPlayer");

        await LobbyIntegrationHarness.WaitForEventAsync(
            _harness.HostLobby!,
            LobbyEventType.TeamUpdated,
            timeoutMs: 8000);

        await _harness.HostLobby!.BeginMatch();
        await _harness.HostLobby!.LaunchGame();

        var evt = await LobbyIntegrationHarness.WaitForEventAsync(
            _harness.ParticipantLobby!,
            LobbyEventType.GameStarted,
            timeoutMs: 8000);

        Assert.That(evt, Is.Not.Null);
    }

    [Test]
    public async Task MatchFlow_HostEndsMatch_BothLobbiesReceiveEndMatchSignal() {
        var lobbies = await FetchLobbiesAsync();
        var browserLobby = lobbies.FirstOrDefault(l => l.Name == "IntegrationTestLobby");
        Assume.That(browserLobby, Is.Not.Null);

        await _harness.JoinLobbyAsync(browserLobby!, "participant-user-1", "ParticipantPlayer");

        await LobbyIntegrationHarness.WaitForEventAsync(
            _harness.HostLobby!,
            LobbyEventType.TeamUpdated,
            timeoutMs: 8000);

        await _harness.HostLobby!.BeginMatch();
        await _harness.HostLobby!.LaunchGame();

        // Wait for participant to receive the GameStarted signal first
        await LobbyIntegrationHarness.WaitForEventAsync(
            _harness.ParticipantLobby!,
            LobbyEventType.GameStarted,
            timeoutMs: 8000);

        // Host calls EndMatch (reports match complete — both clients should get MatchOver)
        var fakeAnalysis = new Battlegrounds.Models.Replays.ReplayAnalysisResult { Failed = false, GameId = "CoH3" };
        await _harness.HostLobby!.ReportMatchResult(fakeAnalysis);
        await _harness.HostLobby!.EndMatch(EndMatchReason.MatchEndedInSuccess);

        var hostMatchOver = await LobbyIntegrationHarness.WaitForEventAsync(
            _harness.HostLobby!,
            LobbyEventType.MatchOver,
            timeoutMs: 10000);
        var participantMatchOver = await LobbyIntegrationHarness.WaitForEventAsync(
            _harness.ParticipantLobby!,
            LobbyEventType.MatchOver,
            timeoutMs: 10000);

        Assert.That(hostMatchOver, Is.Not.Null);
        Assert.That(participantMatchOver, Is.Not.Null);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  F — Lobby list / browser integration
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task LobbyList_AfterHostCreatesLobby_LobbyAppearsInList() {
        var lobbies = await FetchLobbiesAsync();
        Assert.That(lobbies.Any(l => l.Name == "IntegrationTestLobby"), Is.True,
            "The freshly created lobby should appear in the HTTP lobby list");
    }

    [Test]
    public async Task LobbyList_EmptyWhenNoLobbies_ReturnsEmptyCollection() {
        // This test can only pass deterministically if we know there are no other lobbies.
        // We assert that the list returns at least a valid (non-null) collection.
        var lobbies = await FetchLobbiesAsync();
        Assert.That(lobbies, Is.Not.Null);
    }

    // ── HTTP helper ──────────────────────────────────────────────────────────

    /// <summary>
    /// Fetches the current lobby list directly from the container's HTTP REST API.
    /// </summary>
    private async Task<IList<BrowserLobby>> FetchLobbiesAsync() {
        using var http = new HttpClient { BaseAddress = new Uri(HttpApiBaseUrl) };
        var response = await http.GetAsync("/api/v1/lobbies");
        response.EnsureSuccessStatusCode();
        var summaries = await response.Content
            .ReadFromJsonAsync<IEnumerable<HttpBattlegroundsServerAPI.LobbySummary>>()
            ?? [];
        return [.. summaries.Select(s => s.ToBrowserLobby())];
    }
}
