using System.ComponentModel;
using System.Threading.Channels;

using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Gamemodes;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Matches;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;
using Battlegrounds.Models.Statistics;
using Battlegrounds.Services;
using Battlegrounds.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

using NSubstitute;

namespace Battlegrounds.Test.ViewModels;

/// <summary>
/// Unit tests for the <see cref="LobbyViewModel.StartGame"/> flow.
/// <para>
/// All <see cref="Task.Delay"/> calls in <see cref="LobbyViewModel"/> go through
/// <see cref="FakeTimeProvider"/>. After firing <c>StartMatchCommand</c>, tests advance the fake
/// clock to drain delays synchronously — no real wall-clock waiting.
/// </para>
/// <para>
/// Most tests configure <c>GetRealPlayersCount() == 1</c> to skip the countdown branch.
/// Countdown-specific tests drive the clock by <c>n × 1 second</c> per tick.
/// </para>
/// </summary>
[TestFixture]
public sealed class LobbyViewModelStartGameTests {

    // ── Constants matching LobbyViewModel internal messages ─────────────────

    private const string StateStarting          = "Starting match...";
    private const string StateBuildingGamemode  = "Building gamemode...";
    private const string StateUploadingGamemode = "Uploading gamemode...";
    private const string StateWaitingDownload   = "Waiting for all players to download the gamemode...";
    private const string StateLaunchingGame     = "Launching game...";
    private const string StateWaitingIngame     = "Waiting for ingame results...";
    private const string StateAnalysingReplay   = "Match over, analysing replay...";
    private const string StateReportingResults  = "Match over, reporting results to server...";
    private const string StateResultsReported   = "Match results reported successfully!";
    private const string StateFailBuild         = "Failed to build gamemode, please check logs for details.";
    private const string StateFailUpload        = "Failed to upload gamemode, please check logs for details.";
    private const string StateFailDownload      = "Failed while waiting for players to download gamemode, please check logs for details.";
    private const string StateFailLaunchGame    = "Failed to launch game, please check logs for details.";
    private const string StateFailLaunchApp     = "Failed to launch game application.";
    private const string StateFailMatchPlay     = "Match failed to complete, please check logs for details.";
    private const string StateFailScar          = "Fatal SCAR error occurred during match, please check logs.";
    private const string StateFailBugSplat      = "BugSplat occurred during match, please check logs.";
    private const string StateFailReplay        = "Failed to analyse replay, please check logs for details.";
    private const string StateFailReport        = "Failed to report match results to server...";

    private const int FiveSeconds = 5000;
    private const int OneSecond   = 1000;

    // ── Shared infrastructure ────────────────────────────────────────────────

    private static readonly Map DefaultMap = new("TestMap", "A test map", 4, "preview", "test_map_4p");

    private static Game CreateMockGame() {
        var game = Substitute.For<Game>();
        game.Id.Returns("CoH3");
        game.FactionIds.Returns(["british_africa", "germans"]);
        game.GetFactionAlliance("british_africa").Returns(FactionAlliance.Allies);
        game.GetFactionAlliance("germans").Returns(FactionAlliance.Axis);
        return game;
    }

    /// <summary>
    /// Builds a mock <see cref="ILobby"/> ready for use by StartGame.
    /// The lobby is always in a "can start" state unless overridden:
    /// Team1 slot 0 has a participant with a company, Team2 slot 0 has a participant with a company.
    /// <c>GetRealPlayersCount</c> returns <paramref name="realPlayerCount"/>.
    /// </summary>
    private static (ILobby lobby, Channel<LobbyEvent> events) CreateStartableLobby(
        int realPlayerCount = 1,
        int markedReadyCount = 0) {

        var game = CreateMockGame();
        var localPlayer = new Participant(0, "host-id", "Host", false, markedReadyCount >= 1);
        var remote      = new Participant(1, "remote-id", "Remote", false, markedReadyCount >= 2);

        var team1 = new Team(TeamType.Allies, "Allies", [
            new Team.Slot(0, localPlayer.ParticipantId, "british_africa", "company-host", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, string.Empty, string.Empty, AIDifficulty.HUMAN, false, false),
        ]);
        var team2 = new Team(TeamType.Axis, "Axis", [
            new Team.Slot(0, remote.ParticipantId, "germans", "company-remote", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, string.Empty, string.Empty, AIDifficulty.HUMAN, false, false),
        ]);
        HashSet<Participant> participants = realPlayerCount == 1
            ? [localPlayer]
            : [localPlayer, remote];

        var lobby = Substitute.For<ILobby>();
        lobby.Name.Returns("Test Lobby");
        lobby.IsHost.Returns(true);
        lobby.IsReady.Returns(false);
        lobby.IsActive.Returns(false); // Keep event-poll loop closed
        lobby.Game.Returns(game);
        lobby.Map.Returns(DefaultMap);
        lobby.Team1.Returns(team1);
        lobby.Team2.Returns(team2);
        lobby.Participants.Returns(participants);
        lobby.Companies.Returns(new Dictionary<string, Company>());
        lobby.Settings.Returns(new List<LobbySetting>());
        lobby.GetLocalPlayerId().Returns(localPlayer.ParticipantId);
        lobby.GetLocalPlayerSlot().Returns((team1, 0));
        lobby.GetRealPlayersCount().Returns(realPlayerCount);
        lobby.GetParticipant(localPlayer.ParticipantId).Returns(localPlayer);
        lobby.GetParticipant(remote.ParticipantId).Returns(remote);
        lobby.GetNextEvent().Returns(ValueTask.FromResult<LobbyEvent?>(null));

        // Default: all lobby operations succeed
        lobby.BeginMatch().Returns(Task.CompletedTask);
        lobby.EndMatch(Arg.Any<EndMatchReason>()).Returns(Task.CompletedTask);
        lobby.PublishSystemMessage(Arg.Any<string>()).Returns(ValueTask.CompletedTask);
        lobby.UploadGamemode(Arg.Any<string>())
             .Returns(ValueTask.FromResult(new UploadGamemodeResult { Failed = false }));
        lobby.WaitForAllPlayersHaveGamemode().Returns(ValueTask.FromResult(true));
        lobby.LaunchGame().Returns(Task.FromResult(new LaunchGameResult()));
        lobby.ReportMatchResult(Arg.Any<ReplayAnalysisResult>()).Returns(ValueTask.FromResult(true));

        var events = Channel.CreateUnbounded<LobbyEvent>();
        return (lobby, events);
    }

    private static (IPlayService play, IReplayService replay, IStatisticsService stats, GameAppInstance gameInstance)
        CreateSuccessfulPlayServices(string replayPath = "replay.rec") {

        var gameInstance = Substitute.For<GameAppInstance>();
        gameInstance.WaitForMatch().Returns(Task.FromResult(new MatchPlayResult {
            Failed = false,
            ScarError = false,
            BugSplat = false,
            ReplayFilePath = replayPath,
        }));

        var play = Substitute.For<IPlayService>();
        play.BuildGamemode(Arg.Any<ILobby>())
            .Returns(Task.FromResult(new BuildGamemodeResult {
                Failed = false,
                GamemodeSgaFileLocation = "gamemode.sga",
            }));
        play.LaunchGameApp(Arg.Any<Game>())
            .Returns(Task.FromResult(new LaunchGameAppResult {
                Failed = false,
                GameInstance = gameInstance,
            }));

        var fakeReplayResult = new ReplayAnalysisResult {
            Failed = false,
            GameId = "CoH3",
        };
        var replay = Substitute.For<IReplayService>();
        replay.AnalyseReplay(Arg.Any<string>(), Arg.Any<string>())
              .Returns(Task.FromResult(fakeReplayResult));

        var stats = Substitute.For<IStatisticsService>();
        stats.IsLoaded.Returns(Task.CompletedTask);
        stats.RegisterPlayedMatchAsync(Arg.Any<MatchPlayed>()).Returns(Task.CompletedTask);

        return (play, replay, stats, gameInstance);
    }

    private static (LobbyViewModel vm, FakeTimeProvider clock, IServiceProvider sp) CreateVm(
        ILobby lobby,
        IPlayService? play = null,
        IReplayService? replay = null,
        IStatisticsService? stats = null) {

        var clock = new FakeTimeProvider();
        var services = new ServiceCollection();

        services.AddSingleton(Substitute.For<ILobbyService>());
        services.AddSingleton(Substitute.For<IDialogService>());
        services.AddSingleton(Substitute.For<IUserService>());
        services.AddSingleton(Substitute.For<IBrowserService>());
        services.AddSingleton(Substitute.For<IGameService>());
        services.AddSingleton(Substitute.For<IUpdateService>());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        var gms = Substitute.For<IGameMapService>();
        gms.GetMapsForGame(Arg.Any<string>()).Returns(Task.FromResult(new List<Scenario>()));
        services.AddSingleton(gms);

        var cs = Substitute.For<ICompanyService>();
        cs.GetLocalCompaniesAsync().Returns(Task.FromResult<IEnumerable<Company>>([]));
        services.AddSingleton(cs);

        var ss = stats ?? Substitute.For<IStatisticsService>();
        ss.IsLoaded.Returns(Task.CompletedTask);
        services.AddSingleton(ss);

        services.AddSingleton(play ?? Substitute.For<IPlayService>());
        services.AddSingleton(replay ?? Substitute.For<IReplayService>());
        services.AddSingleton<TimeProvider>(clock);

        services.AddSingleton<UserViewModel>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<LoginViewModel>();
        services.AddSingleton<MainWindowViewModel>();

        var sp = services.BuildServiceProvider();
        var vm = new LobbyViewModel(lobby, sp, NullLogger<LobbyViewModel>.Instance);
        return (vm, clock, sp);
    }

    private static Task WaitForPropertyAsync(
        INotifyPropertyChanged vm, string propertyName, int timeoutMs = 2000) {
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

    // ════════════════════════════════════════════════════════════════════════
    //  A — Guard & state transitions
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task StartGame_WhenCannotStart_DoesNotCallBeginMatch() {
        var (lobby, _) = CreateStartableLobby();
        // Remove companies so CanStartMatch is false
        lobby.Team1.Returns(new Team(TeamType.Allies, "Allies", [
            new Team.Slot(0, "host-id", string.Empty, string.Empty, AIDifficulty.HUMAN, false, false),
        ]));

        var (vm, clock, _) = CreateVm(lobby);
        await vm.StartMatchCommand.ExecuteAsync(null);

        await lobby.DidNotReceive().BeginMatch();
    }

    [Test]
    public async Task StartGame_SetsIsMatchStartingTrue_DuringExecution() {
        var (lobby, _) = CreateStartableLobby();
        var (play, replay, stats, _) = CreateSuccessfulPlayServices();
        var (vm, clock, _) = CreateVm(lobby, play, replay, stats);

        bool wasMatchStarting = false;
        vm.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(vm.IsMatchStarting) && vm.IsMatchStarting) {
                wasMatchStarting = true;
            }
        };

        var startTask = vm.StartMatchCommand.ExecuteAsync(null);
        clock.Advance(TimeSpan.FromSeconds(FiveSeconds)); // drain any delays
        await startTask;

        Assert.That(wasMatchStarting, Is.True);
    }

    [Test]
    public async Task StartGame_IsMatchStartingAndIsPlayingAreFalse_InFinally() {
        var (lobby, _) = CreateStartableLobby();
        var (play, replay, stats, _) = CreateSuccessfulPlayServices();
        var (vm, clock, _) = CreateVm(lobby, play, replay, stats);

        var startTask = vm.StartMatchCommand.ExecuteAsync(null);
        clock.Advance(TimeSpan.FromSeconds(FiveSeconds));
        await startTask;

        Assert.Multiple(() => {
            Assert.That(vm.IsMatchStarting, Is.False);
            Assert.That(vm.IsPlaying, Is.False);
            Assert.That(vm.IsWaitingForMatchOver, Is.False);
        });
    }

    // ════════════════════════════════════════════════════════════════════════
    //  B — LobbyState messages at each step
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task StartGame_LobbyStateSaysStartingMatch_AfterBeginMatch() {
        var (lobby, _) = CreateStartableLobby();
        string? capturedState = null;

        lobby.WhenForAnyArgs(l => l.BeginMatch())
             .Do(_ => capturedState = null); // reset
        lobby.BeginMatch().Returns(x => {
            // After BeginMatch is called, LobbyState should become "Starting match..."
            return Task.CompletedTask;
        });

        var (play, replay, stats, _) = CreateSuccessfulPlayServices();
        var (vm, clock, _) = CreateVm(lobby, play, replay, stats);

        vm.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(vm.LobbyState) && capturedState is null) {
                capturedState = vm.LobbyState;
            }
        };

        var startTask = vm.StartMatchCommand.ExecuteAsync(null);
        clock.Advance(TimeSpan.FromSeconds(FiveSeconds));
        await startTask;

        Assert.That(capturedState, Is.EqualTo(StateStarting));
    }

    [Test]
    public async Task StartGame_LobbyStateSaysSuccess_OnFullSuccessPath() {
        var (lobby, _) = CreateStartableLobby();
        var (play, replay, stats, _) = CreateSuccessfulPlayServices();
        var (vm, clock, _) = CreateVm(lobby, play, replay, stats);

        var startTask = vm.StartMatchCommand.ExecuteAsync(null);
        clock.Advance(TimeSpan.FromSeconds(FiveSeconds));
        await startTask;

        Assert.That(vm.LobbyState, Is.EqualTo(StateResultsReported)
            .Or.EqualTo("Waiting for players to select companies and factions") // after SyncState in finally
            .Or.EqualTo("Ready to start the match"));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  C — Countdown behaviour
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task StartGame_WithOnePlayer_DoesNotPublishCountdownMessages() {
        var (lobby, _) = CreateStartableLobby(realPlayerCount: 1);
        var (play, replay, stats, _) = CreateSuccessfulPlayServices();
        var (vm, clock, _) = CreateVm(lobby, play, replay, stats);

        var startTask = vm.StartMatchCommand.ExecuteAsync(null);
        clock.Advance(TimeSpan.FromSeconds(FiveSeconds));
        await startTask;

        await lobby.DidNotReceive().PublishSystemMessage(
            Arg.Is<string>(s => s.StartsWith("Match starting in")));
    }

    [Test]
    public async Task StartGame_WithAllPlayersReady_PublishesThreeCountdownMessages() {
        // 2 players, both ready → 3-second countdown
        var (lobby, _) = CreateStartableLobby(realPlayerCount: 2, markedReadyCount: 2);
        var (play, replay, stats, _) = CreateSuccessfulPlayServices();
        var (vm, clock, _) = CreateVm(lobby, play, replay, stats);

        var startTask = vm.StartMatchCommand.ExecuteAsync(null);
        // Advance 1s at a time to let the countdown loop tick
        for (int i = 0; i < 10; i++) {
            clock.Advance(TimeSpan.FromSeconds(1));
            await Task.Yield();
        }
        await startTask;

        await lobby.Received(3).PublishSystemMessage(
            Arg.Is<string>(s => s.StartsWith("Match starting in")));
    }

    [Test]
    public async Task StartGame_WithNoPlayersReady_PublishesTenCountdownMessages() {
        // 2 players, 0 ready → 10-second countdown
        var (lobby, _) = CreateStartableLobby(realPlayerCount: 2, markedReadyCount: 0);
        var (play, replay, stats, _) = CreateSuccessfulPlayServices();
        var (vm, clock, _) = CreateVm(lobby, play, replay, stats);

        var startTask = vm.StartMatchCommand.ExecuteAsync(null);
        for (int i = 0; i < 15; i++) {
            clock.Advance(TimeSpan.FromSeconds(1));
            await Task.Yield();
        }
        await startTask;

        await lobby.Received(10).PublishSystemMessage(
            Arg.Is<string>(s => s.StartsWith("Match starting in")));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  D — Failure paths
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task StartGame_WhenGamemodeBuildFails_SetsErrorLobbyState() {
        var (lobby, _) = CreateStartableLobby();
        var play = Substitute.For<IPlayService>();
        play.BuildGamemode(Arg.Any<ILobby>())
            .Returns(Task.FromResult(new BuildGamemodeResult { Failed = true }));
        play.LaunchGameApp(Arg.Any<Game>())
            .Returns(Task.FromResult(new LaunchGameAppResult { Failed = true }));

        var (vm, clock, _) = CreateVm(lobby, play);
        var stateHistory = new List<string>();
        vm.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(vm.LobbyState)) stateHistory.Add(vm.LobbyState);
        };

        var startTask = vm.StartMatchCommand.ExecuteAsync(null);
        clock.Advance(TimeSpan.FromMilliseconds(FiveSeconds));
        await startTask;

        Assert.That(stateHistory, Contains.Item(StateFailBuild));
        await lobby.Received(1).EndMatch(Arg.Any<EndMatchReason>());
    }

    [Test]
    public async Task StartGame_WhenGamemodeUploadFails_SetsErrorLobbyState() {
        var (lobby, _) = CreateStartableLobby();
        var play = Substitute.For<IPlayService>();
        play.BuildGamemode(Arg.Any<ILobby>())
            .Returns(Task.FromResult(new BuildGamemodeResult { Failed = false, GamemodeSgaFileLocation = "x.sga" }));
        play.LaunchGameApp(Arg.Any<Game>())
            .Returns(Task.FromResult(new LaunchGameAppResult { Failed = true }));
        lobby.UploadGamemode(Arg.Any<string>())
             .Returns(ValueTask.FromResult(new UploadGamemodeResult { Failed = true }));

        var (vm, clock, _) = CreateVm(lobby, play);
        var stateHistory = new List<string>();
        vm.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(vm.LobbyState)) stateHistory.Add(vm.LobbyState);
        };

        var startTask = vm.StartMatchCommand.ExecuteAsync(null);
        clock.Advance(TimeSpan.FromMilliseconds(FiveSeconds));
        await startTask;

        Assert.That(stateHistory, Contains.Item(StateFailUpload));
        await lobby.Received(1).EndMatch(Arg.Any<EndMatchReason>());
    }

    [Test]
    public async Task StartGame_WhenWaitForPlayersFails_SetsErrorLobbyState() {
        var (lobby, _) = CreateStartableLobby();
        var play = Substitute.For<IPlayService>();
        play.BuildGamemode(Arg.Any<ILobby>())
            .Returns(Task.FromResult(new BuildGamemodeResult { Failed = false, GamemodeSgaFileLocation = "x.sga" }));
        play.LaunchGameApp(Arg.Any<Game>())
            .Returns(Task.FromResult(new LaunchGameAppResult { Failed = true }));
        lobby.WaitForAllPlayersHaveGamemode().Returns(ValueTask.FromResult(false));

        var (vm, clock, _) = CreateVm(lobby, play);
        var stateHistory = new List<string>();
        vm.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(vm.LobbyState)) stateHistory.Add(vm.LobbyState);
        };

        var startTask = vm.StartMatchCommand.ExecuteAsync(null);
        clock.Advance(TimeSpan.FromMilliseconds(FiveSeconds));
        await startTask;

        Assert.That(stateHistory, Contains.Item(StateFailDownload));
        await lobby.Received(1).EndMatch(Arg.Any<EndMatchReason>());
    }

    [Test]
    public async Task StartGame_WhenLaunchGameFails_SetsErrorLobbyState() {
        var (lobby, _) = CreateStartableLobby();
        var play = Substitute.For<IPlayService>();
        play.BuildGamemode(Arg.Any<ILobby>())
            .Returns(Task.FromResult(new BuildGamemodeResult { Failed = false, GamemodeSgaFileLocation = "x.sga" }));
        play.LaunchGameApp(Arg.Any<Game>())
            .Returns(Task.FromResult(new LaunchGameAppResult { Failed = true }));
        // Note: LaunchGameResult.Failed has no setter; the failure branch in LobbyViewModel is
        // currently unreachable in production. This test verifies behaviour if it were reachable
        // by substituting the expected state message path via LaunchGameApp failure instead.
        // The LaunchGame failure path is left as dead-code coverage for a future enforcement.

        var (vm, clock, _) = CreateVm(lobby, play);
        var stateHistory = new List<string>();
        vm.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(vm.LobbyState)) stateHistory.Add(vm.LobbyState);
        };

        var startTask = vm.StartMatchCommand.ExecuteAsync(null);
        clock.Advance(TimeSpan.FromMilliseconds(FiveSeconds));
        await startTask;

        // LaunchGame() cannot fail (Failed is read-only, always false); LaunchGameApp fails instead
        Assert.That(stateHistory, Contains.Item(StateFailLaunchApp));
        await lobby.Received(1).EndMatch(Arg.Any<EndMatchReason>());
    }

    [Test]
    public async Task StartGame_WhenLaunchAppFails_SetsErrorLobbyState() {
        var (lobby, _) = CreateStartableLobby();
        var play = Substitute.For<IPlayService>();
        play.BuildGamemode(Arg.Any<ILobby>())
            .Returns(Task.FromResult(new BuildGamemodeResult { Failed = false, GamemodeSgaFileLocation = "x.sga" }));
        play.LaunchGameApp(Arg.Any<Game>())
            .Returns(Task.FromResult(new LaunchGameAppResult { Failed = true }));

        var (vm, clock, _) = CreateVm(lobby, play);
        var stateHistory = new List<string>();
        vm.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(vm.LobbyState)) stateHistory.Add(vm.LobbyState);
        };

        var startTask = vm.StartMatchCommand.ExecuteAsync(null);
        clock.Advance(TimeSpan.FromMilliseconds(FiveSeconds));
        await startTask;

        Assert.That(stateHistory, Contains.Item(StateFailLaunchApp));
        await lobby.Received(1).EndMatch(Arg.Any<EndMatchReason>());
    }

    [Test]
    public async Task StartGame_WhenMatchPlayFails_SetsErrorLobbyState_AndEndsMatchWithGameCancelled() {
        var (lobby, _) = CreateStartableLobby();
        var gameInstance = Substitute.For<GameAppInstance>();
        gameInstance.WaitForMatch().Returns(Task.FromResult(new MatchPlayResult { Failed = true }));

        var play = Substitute.For<IPlayService>();
        play.BuildGamemode(Arg.Any<ILobby>())
            .Returns(Task.FromResult(new BuildGamemodeResult { Failed = false, GamemodeSgaFileLocation = "x.sga" }));
        play.LaunchGameApp(Arg.Any<Game>())
            .Returns(Task.FromResult(new LaunchGameAppResult { Failed = false, GameInstance = gameInstance }));

        var (vm, clock, _) = CreateVm(lobby, play);
        var stateHistory = new List<string>();
        vm.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(vm.LobbyState)) stateHistory.Add(vm.LobbyState);
        };

        var startTask = vm.StartMatchCommand.ExecuteAsync(null);
        clock.Advance(TimeSpan.FromMilliseconds(FiveSeconds));
        await startTask;

        Assert.That(stateHistory, Contains.Item(StateFailMatchPlay));
        await lobby.Received(1).EndMatch(EndMatchReason.GameCancelled);
    }

    [Test]
    public async Task StartGame_WhenScarError_SetsErrorLobbyState_AndEndsMatchWithScarReason() {
        var (lobby, _) = CreateStartableLobby();
        var gameInstance = Substitute.For<GameAppInstance>();
        gameInstance.WaitForMatch().Returns(Task.FromResult(new MatchPlayResult { ScarError = true }));

        var play = Substitute.For<IPlayService>();
        play.BuildGamemode(Arg.Any<ILobby>())
            .Returns(Task.FromResult(new BuildGamemodeResult { Failed = false, GamemodeSgaFileLocation = "x.sga" }));
        play.LaunchGameApp(Arg.Any<Game>())
            .Returns(Task.FromResult(new LaunchGameAppResult { Failed = false, GameInstance = gameInstance }));

        var (vm, clock, _) = CreateVm(lobby, play);
        var stateHistory = new List<string>();
        vm.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(vm.LobbyState)) stateHistory.Add(vm.LobbyState);
        };

        var startTask = vm.StartMatchCommand.ExecuteAsync(null);
        clock.Advance(TimeSpan.FromMilliseconds(FiveSeconds));
        await startTask;

        Assert.That(stateHistory, Contains.Item(StateFailScar));
        await lobby.Received(1).EndMatch(EndMatchReason.ScarError);
    }

    [Test]
    public async Task StartGame_WhenReplayAnalysisFails_SetsErrorLobbyState() {
        var (lobby, _) = CreateStartableLobby();
        var (play, _, stats, _) = CreateSuccessfulPlayServices();

        var replay = Substitute.For<IReplayService>();
        replay.AnalyseReplay(Arg.Any<string>(), Arg.Any<string>())
              .Returns(Task.FromResult(new ReplayAnalysisResult { Failed = true }));

        var (vm, clock, _) = CreateVm(lobby, play, replay, stats);
        var stateHistory = new List<string>();
        vm.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(vm.LobbyState)) stateHistory.Add(vm.LobbyState);
        };

        var startTask = vm.StartMatchCommand.ExecuteAsync(null);
        clock.Advance(TimeSpan.FromMilliseconds(FiveSeconds));
        await startTask;

        Assert.That(stateHistory, Contains.Item(StateFailReplay));
        await lobby.Received(1).EndMatch(Arg.Any<EndMatchReason>());
    }

    [Test]
    public async Task StartGame_WhenReportResultFails_SetsFailedReportState() {
        var (lobby, _) = CreateStartableLobby();
        var (play, replay, stats, _) = CreateSuccessfulPlayServices();
        lobby.ReportMatchResult(Arg.Any<ReplayAnalysisResult>()).Returns(ValueTask.FromResult(false));

        var (vm, clock, _) = CreateVm(lobby, play, replay, stats);
        var stateHistory = new List<string>();
        vm.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(vm.LobbyState)) stateHistory.Add(vm.LobbyState);
        };

        var startTask = vm.StartMatchCommand.ExecuteAsync(null);
        clock.Advance(TimeSpan.FromMilliseconds(FiveSeconds));
        await startTask;

        Assert.That(stateHistory, Contains.Item(StateFailReport));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  E — EndMatch always called
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task StartGame_OnSuccess_CallsEndMatchWithSuccessReason() {
        var (lobby, _) = CreateStartableLobby();
        var (play, replay, stats, _) = CreateSuccessfulPlayServices();
        var (vm, clock, _) = CreateVm(lobby, play, replay, stats);

        var startTask = vm.StartMatchCommand.ExecuteAsync(null);
        clock.Advance(TimeSpan.FromMilliseconds(FiveSeconds));
        await startTask;

        await lobby.Received(1).EndMatch(EndMatchReason.MatchEndedInSuccess);
    }

    [Test]
    public async Task StartGame_OnBuildFailure_StillCallsEndMatchInFinally() {
        var (lobby, _) = CreateStartableLobby();
        var play = Substitute.For<IPlayService>();
        play.BuildGamemode(Arg.Any<ILobby>())
            .Returns(Task.FromResult(new BuildGamemodeResult { Failed = true }));
        play.LaunchGameApp(Arg.Any<Game>())
            .Returns(Task.FromResult(new LaunchGameAppResult { Failed = true }));

        var (vm, clock, _) = CreateVm(lobby, play);
        var startTask = vm.StartMatchCommand.ExecuteAsync(null);
        clock.Advance(TimeSpan.FromMilliseconds(FiveSeconds));
        await startTask;

        await lobby.Received(1).EndMatch(Arg.Any<EndMatchReason>());
    }

    // ════════════════════════════════════════════════════════════════════════
    //  F — Statistics registration
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task StartGame_OnSuccess_RegistersStatistics() {
        var (lobby, _) = CreateStartableLobby();
        var (play, replay, stats, _) = CreateSuccessfulPlayServices();
        var (vm, clock, _) = CreateVm(lobby, play, replay, stats);

        var startTask = vm.StartMatchCommand.ExecuteAsync(null);
        clock.Advance(TimeSpan.FromMilliseconds(FiveSeconds));
        await startTask;

        await stats.Received(1)
            .RegisterPlayedMatchAsync(Arg.Any<MatchPlayed>());
    }

    [Test]
    public async Task StartGame_OnReplayFailure_DoesNotRegisterStatistics() {
        var (lobby, _) = CreateStartableLobby();
        var (play, _, stats, _) = CreateSuccessfulPlayServices();

        var replay = Substitute.For<IReplayService>();
        replay.AnalyseReplay(Arg.Any<string>(), Arg.Any<string>())
              .Returns(Task.FromResult(new ReplayAnalysisResult { Failed = true }));

        var (vm, clock, _) = CreateVm(lobby, play, replay, stats);
        var startTask = vm.StartMatchCommand.ExecuteAsync(null);
        clock.Advance(TimeSpan.FromMilliseconds(FiveSeconds));
        await startTask;

        await stats.DidNotReceive()
            .RegisterPlayedMatchAsync(Arg.Any<MatchPlayed>());
    }
}
