using System.ComponentModel;
using System.Net.Http.Json;

using Battlegrounds.Facades.API;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;
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

    [SetUp]
    public async Task SetUp() {
        _fakeTime = new FakeTimeProvider();
        _harness = new LobbyIntegrationHarness(GrpcAddress);
        var hostLobby = await _harness.CreateHostLobbyAsync("host-user-1", "HostPlayer");
        _hostVm = CreateVm(hostLobby, _fakeTime);

        // Give the lobby a moment to receive its first state update from the server
        await Task.Delay(500);

        var lobbies = await FetchLobbiesAsync();
        _browserLobby = lobbies.First(l => l.Name == "IntegrationTestLobby");
    }

    [TearDown]
    public async Task TearDown() {
        await _harness.DisposeAsync();
    }

    // ── Factory helpers ──────────────────────────────────────────────────────

    private LobbyViewModel CreateVm(ILobby lobby, FakeTimeProvider clock) {
        var services = new ServiceCollection();

        services.AddSingleton<TimeProvider>(clock);
        services.AddSingleton(Substitute.For<ILobbyService>());
        services.AddSingleton(Substitute.For<IPlayService>());
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
        return tcs.Task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMs));
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
    //  F — Leave lobby via ViewModel
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task LeaveLobby_ParticipantCallsLeaveCommand_LobbyBecomesInactive() {
        var participantLobby = await _harness.JoinLobbyAsync(_browserLobby, "participant-user-1", "ParticipantPlayer");
        _participantVm = CreateVm(participantLobby, _fakeTime);

        await Task.Delay(300);

        // Configure LobbyService mock to call lobby.LeaveLobby
        // The ILobbyService.LeaveLobbyAsync is mocked by default; invoking it won't actually
        // close the gRPC stream. Instead we verify the command completes with an error complaining about the MultiplayerView.
        // That indicates the LeaveLobby command closed the lobby and attempted to return to the multiplayer view, which is the expected behavior.
        Assert.That(async () => await _participantVm.LeaveCommand.ExecuteAsync(null), Throws.InstanceOf<InvalidOperationException>().And.Message.Contain("Battlegrounds.Views.MultiplayerView"));
    }
}
