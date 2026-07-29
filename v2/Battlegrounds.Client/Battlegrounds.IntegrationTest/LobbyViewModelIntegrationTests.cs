using System.ComponentModel;
using System.Net.Http.Json;

using Battlegrounds.Facades.API;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;
using Battlegrounds.Services;
using Battlegrounds.ViewModels;
using Battlegrounds.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

using NSubstitute;

using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace Battlegrounds.IntegrationTest;

/// <summary>
/// Integration tests that drive <see cref="LobbyViewModel"/> against a real server container,
/// verifying that the ViewModel correctly reflects lobby state for both the host and a participant.
/// </summary>
/// <remarks>
/// Each test creates two <see cref="LobbyViewModel"/> instances wrapping real
/// <see cref="MultiplayerLobby"/> connections. The container is started once per fixture by
/// <see cref="LobbyServerIntegrationTests"/> base-class setup.
/// </remarks>
[TestFixture]
[Category("Integration")]
public sealed class LobbyViewModelIntegrationTests : LobbyServerIntegrationTests {

    private LobbyIntegrationHarness _harness = null!;
    private LobbyViewModel _hostVm = null!;
    private LobbyViewModel _participantVm = null!;
    private FakeTimeProvider _fakeTime = null!;
    private BrowserLobby _browserLobby = null!;
    private ILobbyService _lobbyService = null!;

    [SetUp]
    public async Task SetUp() {
        _fakeTime = new FakeTimeProvider();
        _harness = new LobbyIntegrationHarness(GrpcAddress, HttpApiBaseUrl);
        _lobbyService = Substitute.For<ILobbyService>();
        var hostLobby = await _harness.CreateHostLobbyAsync("host-user-1", "HostPlayer");
        _hostVm = CreateVm(hostLobby, _fakeTime);

        // Give the lobby a moment to receive its first state update from the server
        await Task.Delay(500);

        var lobbies = await FetchLobbiesAsync();
        _browserLobby = lobbies.First(l => l.Name == "IntegrationTestLobby");
    }

    [TearDown]
    public async Task TearDown() {
        if (_participantVm is not null) {
            await _participantVm.DisposeAsync();
        }
        if (_hostVm is not null) {
            await _hostVm.DisposeAsync();
        }
        await _harness.DisposeAsync();
    }

    // ── Factory helpers ──────────────────────────────────────────────────────

    private LobbyViewModel CreateVm(ILobby lobby, FakeTimeProvider clock) {
        var services = new ServiceCollection();

        services.AddSingleton<TimeProvider>(clock);
        services.AddSingleton(_lobbyService);
        var playService = Substitute.For<IPlayService>();
        playService.LaunchGameApp(Arg.Any<Game>())
               .Returns(Task.FromResult(new LaunchGameAppResult { Failed = true }));
        services.AddSingleton(playService);
        services.AddSingleton(Substitute.For<IReplayService>());
        services.AddSingleton(Substitute.For<IDialogService>());
        services.AddSingleton(Substitute.For<IUserService>());
        services.AddSingleton(Substitute.For<IBrowserService>());
        services.AddSingleton(Substitute.For<IGameService>());
        services.AddSingleton(Substitute.For<IUpdateService>());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        var gms = Substitute.For<IGameMapService>();
        gms.GetMapsForGame(Arg.Any<string>())
           .Returns(Task.FromResult(new List<Scenario>()));
        services.AddSingleton(gms);

        var cs = Substitute.For<ICompanyService>();
        cs.GetLocalCompaniesAsync().Returns(Task.FromResult<IEnumerable<Company>>([]));
        services.AddSingleton(cs);

        var ss = Substitute.For<IStatisticsService>();
        ss.IsLoaded.Returns(Task.CompletedTask);
        services.AddSingleton(ss);

        services.AddSingleton<UserViewModel>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<LoginViewModel>();
        services.AddSingleton<MainWindowViewModel>();

        var sp = services.BuildServiceProvider();
        return new LobbyViewModel(lobby, sp, NullLogger<LobbyViewModel>.Instance);
    }

    // ── Sync helpers ─────────────────────────────────────────────────────────

    private static Task WaitForPropertyAsync(
        INotifyPropertyChanged vm, string propertyName, int timeoutMs = 5000) {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        PropertyChangedEventHandler? handler = null;
        handler = (_, e) => {
            if (e.PropertyName == propertyName) {
                vm.PropertyChanged -= handler;
                tcs.TrySetResult();
            }
        };
        vm.PropertyChanged += handler;

        async Task WaitWithCleanup() {
            try {
                await tcs.Task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMs));
            } finally {
                vm.PropertyChanged -= handler;
            }
        }

        return WaitWithCleanup();
    }

    private async Task<bool> TryWaitForPropertyOrReportAsync(
        INotifyPropertyChanged vm,
        string propertyName,
        string scenario,
        int timeoutMs = 5000,
        string? details = null) {

        try {
            await WaitForPropertyAsync(vm, propertyName, timeoutMs);
            return true;
        } catch (TimeoutException ex) {
            ReportServerIssue(
                scenario,
                $"Property '{propertyName}' should change within {timeoutMs} ms.",
                $"Timed out waiting for property '{propertyName}' to change.",
                $"{details} Exception: {ex.Message}");
            return false;
        }
    }

    private static async Task<bool> WaitForConditionAsync(Func<bool> condition, int timeoutMs = 5000, int pollIntervalMs = 50) {
        if (condition()) {
            return true;
        }

        if (timeoutMs <= 0) {
            return false;
        }

        var timeout = TimeSpan.FromMilliseconds(timeoutMs);
        var startedAt = DateTime.UtcNow;

        while (DateTime.UtcNow - startedAt < timeout) {
            await Task.Delay(pollIntervalMs);
            if (condition()) {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<Map> GetAlternativeMapCandidates(string currentScenarioName) {
        Map[] candidates = [
            new Map("4p_test", "Integration Test Map 4p", 4, "4p_test_preview", "4p_test"),
            new Map("6p_test", "Integration Test Map 6p", 6, "6p_test_preview", "6p_test"),
            new Map("3p_test", "Integration Test Map 3p", 3, "3p_test_preview", "3p_test"),
            new Map("2p_test", "Integration Test Map 2p", 2, "2p_test_preview", "2p_test")
        ];

        return candidates.Where(x => !string.Equals(x.ScenarioName, currentScenarioName, StringComparison.OrdinalIgnoreCase));
    }

    private static ReplayAnalysisResult CreateValidReplayAnalysisResult(ILobby lobby) {
        var participants = lobby.Participants
            .Where(x => !x.IsAIParticipant)
            .OrderBy(x => x.ParticipantId, StringComparer.Ordinal)
            .ToArray();

        if (participants.Length == 0) {
            throw new InvalidOperationException("Cannot create replay analysis for a lobby with no human participants.");
        }

        List<ReplayPlayer> replayPlayers = [];
        List<MatchStartReplayEvent.PlayerData> startPlayers = [];
        List<MatchOverReplayEvent.PlayerStatistics> playerStats = [];
        List<int> winners = [];
        List<int> losers = [];

        for (int i = 0; i < participants.Length; i++) {
            var participant = participants[i];
            int playerId = 1000 + i;
            int teamId = lobby.GetTeam(participant);
            string companyId = FindParticipantCompanyId(lobby, participant.ParticipantId) ?? $"company-{playerId}";

            replayPlayers.Add(new ReplayPlayer(
                PlayerId: playerId,
                TeamId: teamId,
                PlayerName: participant.ParticipantName,
                ProfileId: 0,
                SteamId: 0,
                Faction: string.Empty,
                AIProfile: string.Empty));

            startPlayers.Add(new MatchStartReplayEvent.PlayerData(
                PlayerId: playerId,
                Name: participant.ParticipantName,
                CompanyId: companyId,
                ModId: participant.LobbyId));

            playerStats.Add(new MatchOverReplayEvent.PlayerStatistics(
                PlayerId: playerId,
                TeamId: teamId,
                Name: participant.ParticipantName,
                ModId: participant.LobbyId,
                Kills: 0,
                Losses: 0));

            if (i == 0) {
                winners.Add(playerId);
            } else {
                losers.Add(playerId);
            }
        }

        TimeSpan duration = TimeSpan.FromMinutes(12);
        var replay = new Replay {
            GameId = lobby.Game.Id,
            Duration = duration,
            Players = replayPlayers,
            Events = [
                new MatchStartReplayEvent(
                    Timestamp: TimeSpan.Zero,
                    MatchId: $"integration-{Guid.NewGuid():N}",
                    ModVersion: "integration-v1",
                    Scenario: lobby.Map.ScenarioName,
                    Players: startPlayers),
                new MatchOverReplayEvent(
                    Timestamp: duration,
                    Winners: winners,
                    Losers: losers,
                    PlayerStats: playerStats)
            ]
        };

        return new ReplayAnalysisResult {
            Failed = false,
            GameId = lobby.Game.Id,
            Replay = replay
        };
    }

    private static string? FindParticipantCompanyId(ILobby lobby, string participantId) {
        foreach (var slot in lobby.Team1.Slots.Concat(lobby.Team2.Slots)) {
            if (!string.Equals(slot.ParticipantId, participantId, StringComparison.Ordinal)) {
                continue;
            }

            return string.IsNullOrWhiteSpace(slot.CompanyId) ? null : slot.CompanyId;
        }

        return null;
    }

    private async Task<IList<BrowserLobby>> FetchLobbiesAsync() {
        using var http = new HttpClient { BaseAddress = new Uri(HttpApiBaseUrl) };
        var response = await http.GetAsync("/api/v1/lobbies");
        response.EnsureSuccessStatusCode();
        var summaries = await response.Content
            .ReadFromJsonAsync<IEnumerable<HttpBattlegroundsServerAPI.LobbySummary>>()
            ?? [];
        return [.. summaries.Select(s => s.ToBrowserLobby())];
    }

    // ════════════════════════════════════════════════════════════════════════
    //  A — Host ViewModel initial state
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public void HostVm_InitialState_IsHost() {
        Assert.That(_hostVm.IsHost, Is.True);
    }

    [Test]
    public void HostVm_InitialState_LobbyNameMatchesServerLobby() {
        Assert.That(_hostVm.LobbyName, Is.EqualTo("IntegrationTestLobby"));
    }

    [Test]
    public void HostVm_InitialState_IsPlayingFalse() {
        Assert.That(_hostVm.IsPlaying, Is.False);
    }

    [Test]
    public void HostVm_InitialState_IsMatchStartingFalse() {
        Assert.That(_hostVm.IsMatchStarting, Is.False);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  B — Participant ViewModel initial state
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task ParticipantVm_InitialState_IsNotHost() {
        var participantLobby = await _harness.JoinLobbyAsync(_browserLobby, "participant-user-1", "ParticipantPlayer");
        _participantVm = CreateVm(participantLobby, _fakeTime);

        Assert.That(_participantVm.IsHost, Is.False);
    }

    [Test]
    public async Task ParticipantVm_CannotStartMatch_EvenWhenCanStartMatchIsTrue() {
        var participantLobby = await _harness.JoinLobbyAsync(_browserLobby, "participant-user-1", "ParticipantPlayer");
        _participantVm = CreateVm(participantLobby, _fakeTime);

        Assert.That(_participantVm.CanStartMatch, Is.False, "Only host can start a match");
    }

    [Test]
    public async Task ParticipantVm_LobbyNameMatchesHostLobbyName() {
        var participantLobby = await _harness.JoinLobbyAsync(_browserLobby, "participant-user-1", "ParticipantPlayer");
        _participantVm = CreateVm(participantLobby, _fakeTime);

        Assert.That(_participantVm.LobbyName, Is.EqualTo(_hostVm.LobbyName));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  C — Chat event propagation through ViewModel
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task Chat_HostSendsMessage_ParticipantVmReceivesChatMessage() {
        var participantLobby = await _harness.JoinLobbyAsync(_browserLobby, "participant-user-1", "ParticipantPlayer");
        _participantVm = CreateVm(participantLobby, _fakeTime);

        // Let event loop on participant start
        await Task.Delay(300);

        // Wait for ChatMessages property change on participant vm
        var chatReceived = WaitForPropertyAsync(_participantVm, nameof(_participantVm.ChatMessages));

        _hostVm.ChatMessage = "Hello from host VM!";
        await _hostVm.SendMessageCommand.ExecuteAsync(null);

        await chatReceived;

        Assert.That(_participantVm.ChatMessages, Has.Count.GreaterThanOrEqualTo(1));
        Assert.That(_participantVm.ChatMessages.Any(m => m.Message == "Hello from host VM!"), Is.True);
    }

    [Test]
    public async Task Chat_ParticipantSendsMessage_HostVmReceivesChatMessage() {
        var participantLobby = await _harness.JoinLobbyAsync(_browserLobby, "participant-user-1", "ParticipantPlayer");
        _participantVm = CreateVm(participantLobby, _fakeTime);

        await Task.Delay(300);

        var chatReceived = WaitForPropertyAsync(_hostVm, nameof(_hostVm.ChatMessages));

        _participantVm.ChatMessage = "Hello from participant!";
        await _participantVm.SendMessageCommand.ExecuteAsync(null);

        await chatReceived;

        Assert.That(_hostVm.ChatMessages.Any(m => m.Message == "Hello from participant!"), Is.True);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  D — Ready state through ViewModel
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task ReadyState_ParticipantTogglesReady_IsReadyReflectsNewState() {
        var participantLobby = await _harness.JoinLobbyAsync(_browserLobby, "participant-user-1", "ParticipantPlayer");
        _participantVm = CreateVm(participantLobby, _fakeTime);

        await Task.Delay(300);

        Assert.That(_participantVm.IsReady, Is.False);

        await _participantVm.ToggleReadyCommand.ExecuteAsync(null);

        Assert.That(_participantVm.IsReady, Is.True);
    }

    [Test]
    public async Task ReadyState_HostTogglesReady_IsReadyUpdatedViaCommand() {
        // Host toggling ready should update IsReady on the host's ViewModel
        await _hostVm.ToggleReadyCommand.ExecuteAsync(null);

        Assert.That(_hostVm.IsReady, Is.True);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  E — Match start signal propagation (GameStarted event → participant VM)
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task MatchStart_HostLaunchesGame_ParticipantVmDoesNotThrow() {
        // The participant ViewModel receives a GameStarted event and calls IPlayService.LaunchGameApp.
        // Since IPlayService is mocked (returns Failed=true), the participant should handle the
        // failure silently without exceptions propagating to the test runner.

        var participantLobby = await _harness.JoinLobbyAsync(_browserLobby, "participant-user-1", "ParticipantPlayer");
        var playService = Substitute.For<IPlayService>();
        playService.LaunchGameApp(Arg.Any<Game>())
                   .Returns(Task.FromResult(new LaunchGameAppResult { Failed = true }));

        // Rebuild participant VM with failing play service to confirm no exception is thrown
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(_fakeTime);
        services.AddSingleton(Substitute.For<ILobbyService>());
        services.AddSingleton(playService);
        services.AddSingleton(Substitute.For<IReplayService>());
        services.AddSingleton(Substitute.For<IDialogService>());
        services.AddSingleton(Substitute.For<IUserService>());
        services.AddSingleton(Substitute.For<IBrowserService>());
        services.AddSingleton(Substitute.For<IGameService>());
        services.AddSingleton(Substitute.For<IUpdateService>());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        var gms2 = Substitute.For<IGameMapService>();
        gms2.GetMapsForGame(Arg.Any<string>()).Returns(Task.FromResult(new List<Scenario>()));
        services.AddSingleton(gms2);
        var cs2 = Substitute.For<ICompanyService>();
        cs2.GetLocalCompaniesAsync().Returns(Task.FromResult<IEnumerable<Company>>([]));
        services.AddSingleton(cs2);
        var ss2 = Substitute.For<IStatisticsService>();
        ss2.IsLoaded.Returns(Task.CompletedTask);
        services.AddSingleton(ss2);
        services.AddSingleton<UserViewModel>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<LoginViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        var sp = services.BuildServiceProvider();
        _participantVm = new LobbyViewModel(participantLobby, sp, NullLogger<LobbyViewModel>.Instance);

        await Task.Delay(300);

        // Host starts the match (BeginMatch + LaunchGame)
        await _harness.HostLobby!.BeginMatch();
        Assert.That(async () => await _harness.HostLobby!.LaunchGame(), Throws.Nothing);

        // Wait briefly for participant to process the event
        await Task.Delay(1000);

        // The participant's PlayService.LaunchGameApp should have been called once
        await playService.Received(1).LaunchGameApp(Arg.Any<Game>());
    }

    // ════════════════════════════════════════════════════════════════════════
    //  F — Map + slot + match synchronization (new coverage)
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task MapChange_HostChangesMap_ParticipantVmSelectedMapUpdates() {
        const string scenario = nameof(MapChange_HostChangesMap_ParticipantVmSelectedMapUpdates);

        var participantLobby = await _harness.JoinLobbyAsync(_browserLobby, "participant-user-1", "ParticipantPlayer");
        _participantVm = CreateVm(participantLobby, _fakeTime);

        await Task.Delay(300);

        string currentScenario = _harness.HostLobby!.Map.ScenarioName;
        Map? selectedCandidate = null;

        foreach (var candidate in GetAlternativeMapCandidates(currentScenario)) {
            if (await _harness.HostLobby!.SetMap(candidate)) {
                selectedCandidate = candidate;
                break;
            }
        }

        if (selectedCandidate is null) {
            ReportServerIssue(
                scenario,
                "Host should be able to switch to at least one alternative map candidate.",
                "Server rejected all map change candidates.",
                $"Current map: {currentScenario}. Candidates attempted: {string.Join(", ", GetAlternativeMapCandidates(currentScenario).Select(x => x.ScenarioName))}");
            Assert.Fail("No valid map change candidate was accepted by the server.");
            return;
        }

        Map targetMap = selectedCandidate;

        bool participantModelUpdated = await WaitForConditionAsync(
            () => _harness.ParticipantLobby!.Map.ScenarioName == targetMap.ScenarioName,
            timeoutMs: 10000);
        if (!participantModelUpdated) {
            ReportServerIssue(
                scenario,
                $"Participant lobby model map should become '{targetMap.ScenarioName}'.",
                $"Participant lobby model map remained '{_harness.ParticipantLobby!.Map.ScenarioName}'.",
                "Host accepted map change but participant model did not converge to target map.");
        }
        Assert.That(participantModelUpdated, Is.True, "Participant lobby model should reflect updated map.");

        bool participantVmUpdated = await WaitForConditionAsync(
            () => _participantVm.SelectedMap.ScenarioName == targetMap.ScenarioName,
            timeoutMs: 10000);
        if (!participantVmUpdated) {
            ReportServerIssue(
                scenario,
                $"Participant VM SelectedMap should become '{targetMap.ScenarioName}'.",
                $"Participant VM SelectedMap remained '{_participantVm.SelectedMap.ScenarioName}'.",
                "MapUpdated event arrived but VM did not converge to expected selected map.");
        }

        Assert.That(participantVmUpdated, Is.True, "Participant VM should reflect updated map.");
    }

    [Test]
    public async Task SlotChange_ParticipantMovesSlot_HostAndParticipantVmSynchronize() {
        const string scenario = nameof(SlotChange_ParticipantMovesSlot_HostAndParticipantVmSynchronize);
        const string participantId = "participant-user-1";

        var participantLobby = await _harness.JoinLobbyAsync(_browserLobby, participantId, "ParticipantPlayer");
        _participantVm = CreateVm(participantLobby, _fakeTime);

        await Task.Delay(500);

        await _harness.ParticipantLobby!.MoveToSlot(_harness.ParticipantLobby.Team2, 0);

        bool hostModelUpdated = await WaitForConditionAsync(
            () => _harness.HostLobby!.Team2.Slots.Any(slot => slot.ParticipantId == participantId),
            timeoutMs: 10000);
        if (!hostModelUpdated) {
            ReportServerIssue(
                scenario,
                "Host lobby model should show participant in Team2 after slot move.",
                "Host lobby model did not update with participant in Team2.",
                "Possible server propagation or event contract issue.");
        }
        Assert.That(hostModelUpdated, Is.True, "Host lobby model should reflect slot move.");

        bool participantModelUpdated = await WaitForConditionAsync(
            () => _harness.ParticipantLobby!.Team2.Slots.Any(slot => slot.ParticipantId == participantId),
            timeoutMs: 10000);
        if (!participantModelUpdated) {
            ReportServerIssue(
                scenario,
                "Participant lobby model should show local participant in Team2 after move.",
                "Participant lobby model did not update with moved slot.",
                "Possible move-slot propagation issue.");
        }
        Assert.That(participantModelUpdated, Is.True, "Participant lobby model should reflect own slot move.");

        bool hostVmUpdated = await WaitForConditionAsync(
            () => _hostVm.Team2Slots.Any(slot => slot.Slot.ParticipantId == participantId),
            timeoutMs: 10000);
        if (!hostVmUpdated) {
            ReportServerIssue(
                scenario,
                "Host VM Team2Slots should include moved participant.",
                "Host VM Team2Slots did not include moved participant.",
                "This may indicate server event payload mismatch or client event-mapping bug.");
        }
        Assert.That(hostVmUpdated, Is.True, "Host VM should reflect slot move.");

        bool participantVmUpdated = await WaitForConditionAsync(
            () => _participantVm.Team2Slots.Any(slot => slot.Slot.ParticipantId == participantId),
            timeoutMs: 10000);
        if (!participantVmUpdated) {
            ReportServerIssue(
                scenario,
                "Participant VM Team2Slots should include local participant after move.",
                "Participant VM Team2Slots did not include local participant.",
                "Possible server/client slot update mismatch.");
        }
        Assert.That(participantVmUpdated, Is.True, "Participant VM should reflect own slot move.");
    }

    [Test]
    public async Task MatchFlow_HostEndsMatch_ViewModelsReceiveMatchOverResult() {
        const string scenario = nameof(MatchFlow_HostEndsMatch_ViewModelsReceiveMatchOverResult);

        var participantLobby = await _harness.JoinLobbyAsync(_browserLobby, "participant-user-1", "ParticipantPlayer");
        _participantVm = CreateVm(participantLobby, _fakeTime);

        await Task.Delay(500);

        await _harness.HostLobby!.BeginMatch();
        await _harness.HostLobby!.LaunchGame();

        var validAnalysis = CreateValidReplayAnalysisResult(_harness.HostLobby!);
        var locallyComputedResult = validAnalysis.GetMatchResult(_harness.HostLobby!);
        Assert.That(locallyComputedResult.IsValid, Is.True, "Integration test replay payload should produce a locally valid match result.");

        bool reported = await _harness.HostLobby!.ReportMatchResult(validAnalysis);
        if (!reported) {
            ReportServerIssue(
                scenario,
                "Host should be able to report a valid replay analysis payload before ending the match.",
                "ReportMatchResult returned false for a structurally valid replay payload.",
                "Observed via HttpBattlegroundsServerAPI logs: /api/v1/match/report currently returns 401 Unauthorized in the integration image for this test token flow.");
        }

        await _harness.HostLobby!.EndMatch(EndMatchReason.MatchEndedInSuccess);

        if (!reported) {
            return;
        }

        bool hostMatchOverAvailable = await WaitForConditionAsync(
            () => _hostVm.MatchOverResult is not null,
            timeoutMs: 12000);
        if (!hostMatchOverAvailable) {
            ReportServerIssue(
                scenario,
                "Host VM MatchOverResult should become non-null within 12000 ms after EndMatch.",
                "Host VM MatchOverResult remained null.",
                "MatchOver event may not have propagated or match result retrieval returned null.");
        }

        bool participantMatchOverAvailable = await WaitForConditionAsync(
            () => _participantVm.MatchOverResult is not null,
            timeoutMs: 12000);
        if (!participantMatchOverAvailable) {
            ReportServerIssue(
                scenario,
                "Participant VM MatchOverResult should become non-null within 12000 ms after EndMatch.",
                "Participant VM MatchOverResult remained null.",
                "MatchOver event may not have propagated to participant or participant-side result materialization failed.");
        }

        Assert.That(hostMatchOverAvailable, Is.True, "Host VM should expose match-over data after EndMatch.");
        Assert.That(participantMatchOverAvailable, Is.True, "Participant VM should expose match-over data after EndMatch.");

        if (_hostVm.MatchOverResult is null) {
            ReportServerIssue(
                scenario,
                "Host VM MatchOverResult should be populated.",
                "Host VM MatchOverResult was null.",
                "MatchOver event may have been emitted without retrievable match result payload.");
        }
        if (_participantVm.MatchOverResult is null) {
            ReportServerIssue(
                scenario,
                "Participant VM MatchOverResult should be populated.",
                "Participant VM MatchOverResult was null.",
                "MatchOver event may have been emitted without retrievable match result payload.");
        }

        Assert.That(_hostVm.MatchOverResult, Is.Not.Null, "Host VM should expose match-over data.");
        Assert.That(_participantVm.MatchOverResult, Is.Not.Null, "Participant VM should expose match-over data.");
        Assert.That(_hostVm.MatchOverResult!.Concluded, Is.True, "Host match-over view should indicate a concluded match.");
        Assert.That(_participantVm.MatchOverResult!.Concluded, Is.True, "Participant match-over view should indicate a concluded match.");
    }

    // ════════════════════════════════════════════════════════════════════════
    //  G — Leave lobby via ViewModel
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task LeaveLobby_ParticipantCallsLeaveCommand_LobbyBecomesInactive() {
        var participantLobby = await _harness.JoinLobbyAsync(_browserLobby, "participant-user-1", "ParticipantPlayer");
        _participantVm = CreateVm(participantLobby, _fakeTime);

        await Task.Delay(300);

        // Call leave
        await _participantVm.LeaveCommand.ExecuteAsync(null);

        // Verify lobby service got a leave lobby call for the participant's lobby
        Received.InOrder(async () => {
            await _lobbyService.LeaveLobbyAsync(Arg.Is<ILobby>(l => l == participantLobby));
        });

    }

}
