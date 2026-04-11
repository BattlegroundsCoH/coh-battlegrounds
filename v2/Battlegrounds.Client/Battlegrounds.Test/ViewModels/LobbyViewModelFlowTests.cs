using System.ComponentModel;
using System.Threading.Channels;

using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Gamemodes;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Matches;
using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;
using Battlegrounds.Services;
using Battlegrounds.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;

namespace Battlegrounds.Test.ViewModels;

/// <summary>
/// Lobby flow tests that verify complete lobby lifecycles through the <see cref="LobbyViewModel"/>.
/// Covers singleplayer host flow, multiplayer host flow, and multiplayer participant (non-host) flow.
/// </summary>
[TestFixture]
public sealed class LobbyViewModelFlowTests {

    // ── Shared infrastructure ────────────────────────────────────────────────

    private static readonly Map DefaultMap = new("TestMap", "A test map", 4, "preview", "test_map_4p");

    private static Game CreateMockGame() {
        var game = Substitute.For<Game>();
        game.Id.Returns("CoH3");
        game.FactionIds.Returns(["british_africa", "germans"]);
        game.GetFactionAlliance("british_africa").Returns(FactionAlliance.Allies);
        game.GetFactionAlliance("germans").Returns(FactionAlliance.Axis);
        game.GetFactionName("british_africa").Returns("British");
        game.GetFactionName("germans").Returns("Germans");
        return game;
    }

    private static Team CreateTeam(TeamType type, Team.Slot[]? slots = null) {
        slots ??= [
            new Team.Slot(0, null, "", "", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ];
        return new Team(type, type.ToString(), slots);
    }

    /// <summary>
    /// Creates a mock <see cref="ILobby"/> with a controllable event channel.
    /// </summary>
    private static (ILobby lobby, Channel<LobbyEvent> events) CreateLobby(
        bool isHost = true,
        bool startEventLoop = false,
        Participant? localPlayer = null,
        Team? team1 = null,
        Team? team2 = null,
        HashSet<Participant>? participants = null) {

        var game = CreateMockGame();
        localPlayer ??= new Participant(0, "local-player-1", "Player One", false, false);
        team1 ??= CreateTeam(TeamType.Allies, [
            new Team.Slot(0, localPlayer.ParticipantId, "", "", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);
        team2 ??= CreateTeam(TeamType.Axis);
        participants ??= [localPlayer];

        var lobby = Substitute.For<ILobby>();
        lobby.Name.Returns("Test Lobby");
        lobby.IsHost.Returns(isHost);
        lobby.IsReady.Returns(false);
        lobby.Game.Returns(game);
        lobby.Map.Returns(DefaultMap);
        lobby.Team1.Returns(team1);
        lobby.Team2.Returns(team2);
        lobby.Participants.Returns(participants);
        lobby.Companies.Returns(new Dictionary<string, Company>());
        lobby.Settings.Returns(new List<LobbySetting>());
        lobby.GetLocalPlayerId().Returns(localPlayer.ParticipantId);
        lobby.GetLocalPlayerSlot().Returns((team1, 0));
        lobby.GetRealPlayersCount().Returns(participants.Count(p => !p.IsAIParticipant));

        var eventChannel = Channel.CreateUnbounded<LobbyEvent>();

        if (startEventLoop) {
            lobby.IsActive.Returns(true);
            Func<NSubstitute.Core.CallInfo, ValueTask<LobbyEvent?>> reader =
                _ => ReadNextAsync(eventChannel.Reader);
            lobby.GetNextEvent().Returns(reader);
        } else {
            lobby.IsActive.Returns(false);
            lobby.GetNextEvent().Returns(ValueTask.FromResult<LobbyEvent?>(null));
        }

        return (lobby, eventChannel);
    }

    private static async ValueTask<LobbyEvent?> ReadNextAsync(ChannelReader<LobbyEvent> reader) {
        try { return await reader.ReadAsync(); }
        catch (ChannelClosedException) { return null; }
    }

    private static IServiceProvider BuildServiceProvider(
        IPlayService? playService = null,
        IReplayService? replayService = null,
        IGameMapService? gameMapService = null,
        ICompanyService? companyService = null,
        IStatisticsService? statisticsService = null) {

        var services = new ServiceCollection();

        services.AddSingleton(Substitute.For<ILobbyService>());
        services.AddSingleton(playService ?? Substitute.For<IPlayService>());
        services.AddSingleton(replayService ?? Substitute.For<IReplayService>());
        services.AddSingleton(Substitute.For<IDialogService>());
        services.AddSingleton(Substitute.For<IUserService>());
        services.AddSingleton(Substitute.For<IBrowserService>());
        services.AddSingleton(Substitute.For<IGameService>());
        services.AddSingleton(Substitute.For<IUpdateService>());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        var gms = gameMapService ?? Substitute.For<IGameMapService>();
        gms.GetMapsForGame(Arg.Any<string>()).Returns(Task.FromResult(new List<Scenario>()));
        services.AddSingleton(gms);

        var cs = companyService ?? Substitute.For<ICompanyService>();
        cs.GetLocalCompaniesAsync().Returns(Task.FromResult<IEnumerable<Company>>([]));
        services.AddSingleton(cs);

        var ss = statisticsService ?? Substitute.For<IStatisticsService>();
        ss.IsLoaded.Returns(Task.CompletedTask);
        services.AddSingleton(ss);

        services.AddSingleton<UserViewModel>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<LoginViewModel>();
        services.AddSingleton<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }

    private static LobbyViewModel CreateVm(ILobby lobby, IServiceProvider sp)
        => new(lobby, sp, NullLogger<LobbyViewModel>.Instance);

    // ── Synchronization helpers ──────────────────────────────────────────────

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

    /// <summary>
    /// Pushes an event and waits until the ViewModel has processed it by observing
    /// the <c>CanStartMatch</c> property change that follows every event in the poll loop.
    /// </summary>
    private static async Task PushEventAndWait(
        LobbyViewModel vm, Channel<LobbyEvent> events, LobbyEvent lobbyEvent) {
        var processed = WaitForPropertyAsync(vm, nameof(vm.CanStartMatch));
        await events.Writer.WriteAsync(lobbyEvent);
        await processed;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  SINGLEPLAYER HOST FLOW
    // ══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task SingleplayerFlow_HostSetsUpLobbyAndReachesReadyState() {
        var localPlayer = new Participant(0, "local-player-1", "Player One", false, true);
        var aiPlayer = new Participant(1, "ai-1", "AI Easy", true, true);
        var team1 = new Team(TeamType.Allies, "Allies", [
            new Team.Slot(0, localPlayer.ParticipantId, "british_africa", "company-local", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);
        var team2 = new Team(TeamType.Axis, "Axis", [
            new Team.Slot(0, aiPlayer.ParticipantId, "germans", "company-ai", AIDifficulty.EASY, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);

        var (lobby, _) = CreateLobby(
            isHost: true,
            localPlayer: localPlayer,
            team1: team1,
            team2: team2,
            participants: [localPlayer, aiPlayer]);

        var vm = CreateVm(lobby, BuildServiceProvider());

        // With both teams having assigned companies, CanStartMatch should be true
        using (Assert.EnterMultipleScope()) {
            Assert.That(vm.IsHost, Is.True);
            Assert.That(vm.LobbyName, Is.EqualTo("Test Lobby"));
            Assert.That(vm.CanStartMatch, Is.True, "Both teams have occupied slots with companies");
        }
    }

    [Test]
    public async Task SingleplayerFlow_CannotStartWithoutCompanies() {
        var localPlayer = new Participant(0, "local-player-1", "Player One", false, true);
        var aiPlayer = new Participant(1, "ai-1", "AI Easy", true, true);
        var team1 = new Team(TeamType.Allies, "Allies", [
            new Team.Slot(0, localPlayer.ParticipantId, "british_africa", "", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);
        var team2 = new Team(TeamType.Axis, "Axis", [
            new Team.Slot(0, aiPlayer.ParticipantId, "germans", "", AIDifficulty.EASY, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);

        var (lobby, _) = CreateLobby(
            isHost: true,
            localPlayer: localPlayer,
            team1: team1,
            team2: team2,
            participants: [localPlayer, aiPlayer]);

        var vm = CreateVm(lobby, BuildServiceProvider());

        Assert.That(vm.CanStartMatch, Is.False, "No companies assigned, cannot start");
        Assert.That(vm.LobbyState, Is.EqualTo("Waiting for players to select companies and factions"));
    }

    [Test]
    public async Task SingleplayerFlow_MapChangeUpdatesViewModel() {
        var (lobby, events) = CreateLobby(isHost: true, startEventLoop: true);
        var vm = CreateVm(lobby, BuildServiceProvider());

        var newMap = new Map("Desert", "Arid terrain", 4, "desert_preview", "desert_4p");
        await PushEventAndWait(vm, events, new LobbyEvent(LobbyEventType.MapUpdated, newMap));
        events.Writer.Complete();

        Assert.That(vm.SelectedMap, Is.EqualTo(newMap));
    }

    [Test]
    public async Task SingleplayerFlow_SettingChange_RaisesPropertyChanged() {
        var (lobby, events) = CreateLobby(isHost: true, startEventLoop: true);
        var vm = CreateVm(lobby, BuildServiceProvider());

        var fired = new List<string>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName!);

        await PushEventAndWait(vm, events, new LobbyEvent(LobbyEventType.SettingUpdated));
        events.Writer.Complete();

        Assert.That(fired, Contains.Item(nameof(vm.SelectedSettings)));
    }

    [Test]
    public async Task SingleplayerFlow_ChatMessageAppearsInViewModel() {
        var (lobby, events) = CreateLobby(isHost: true, startEventLoop: true);
        var vm = CreateVm(lobby, BuildServiceProvider());

        var chatMsg = new ChatMessage("local-player-1", "Player One", ChatChannel.All, "Let's go!");
        await PushEventAndWait(vm, events, new LobbyEvent(LobbyEventType.ParticipantMessage, chatMsg));
        events.Writer.Complete();

        Assert.That(vm.ChatMessages, Has.Count.EqualTo(1));
        Assert.That(vm.ChatMessages[0].Message, Is.EqualTo("Let's go!"));
        Assert.That(vm.ChatMessages[0].IsSelf, Is.True);
    }

    [Test]
    public async Task SingleplayerFlow_EndMatchShowsResults() {
        var matchOverData = new MatchOverData { IsValid = true, IsVictory = true, MatchId = "m1" };

        var (lobby, events) = CreateLobby(isHost: true, startEventLoop: true);
        lobby.GetMatchResults().Returns(Task.FromResult<MatchOverData?>(matchOverData));

        var vm = CreateVm(lobby, BuildServiceProvider());

        await PushEventAndWait(vm, events, new LobbyEvent(LobbyEventType.MatchOver));

        // Give async void ShowMatchResults a moment to complete
        await Task.Delay(100);
        events.Writer.Complete();

        Assert.That(vm.MatchOverResult, Is.Not.Null);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  MULTIPLAYER HOST FLOW
    // ══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task MultiplayerHostFlow_InitialState_IsCorrect() {
        var host = new Participant(0, "host-player", "HostPlayer", false, true);
        var team1 = new Team(TeamType.Allies, "Allies", [
            new Team.Slot(0, host.ParticipantId, "british_africa", "", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);
        var team2 = CreateTeam(TeamType.Axis);

        var (lobby, _) = CreateLobby(
            isHost: true,
            localPlayer: host,
            team1: team1,
            team2: team2,
            participants: [host]);

        var vm = CreateVm(lobby, BuildServiceProvider());

        using (Assert.EnterMultipleScope()) {
            Assert.That(vm.IsHost, Is.True);
            Assert.That(vm.CanStartMatch, Is.False, "Only host in lobby, team2 empty");
            Assert.That(vm.LobbyState, Is.EqualTo("Waiting for players to select companies and factions"));
        }
    }

    [Test]
    public async Task MultiplayerHostFlow_BothTeamsReady_CanStart() {
        var host = new Participant(0, "host-player", "HostPlayer", false, true);
        var other = new Participant(1, "other-player", "OtherPlayer", false, true);
        var team1 = new Team(TeamType.Allies, "Allies", [
            new Team.Slot(0, host.ParticipantId, "british_africa", "host-company", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);
        var team2 = new Team(TeamType.Axis, "Axis", [
            new Team.Slot(0, other.ParticipantId, "germans", "other-company", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);

        var (lobby, _) = CreateLobby(
            isHost: true,
            localPlayer: host,
            team1: team1,
            team2: team2,
            participants: [host, other]);

        var vm = CreateVm(lobby, BuildServiceProvider());

        Assert.That(vm.CanStartMatch, Is.True);
    }

    [Test]
    public async Task MultiplayerHostFlow_TeamUpdatedEvent_RefreshesSlots() {
        var host = new Participant(0, "host-player", "HostPlayer", false, true);
        var team1 = new Team(TeamType.Allies, "Allies", [
            new Team.Slot(0, host.ParticipantId, "british_africa", "", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);
        var team2 = CreateTeam(TeamType.Axis);

        var (lobby, events) = CreateLobby(
            isHost: true,
            startEventLoop: true,
            localPlayer: host,
            team1: team1,
            team2: team2,
            participants: [host]);

        var vm = CreateVm(lobby, BuildServiceProvider());

        var fired = new List<string>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName!);

        // Simulate a team update event (as sent by lobby implementations with TeamType)
        await PushEventAndWait(vm, events, new LobbyEvent(LobbyEventType.TeamUpdated, TeamType.Allies));
        events.Writer.Complete();

        Assert.That(fired, Contains.Item(nameof(vm.Team1Slots)));
    }

    [Test]
    public async Task MultiplayerHostFlow_SendChat_InvokesLobby() {
        var (lobby, _) = CreateLobby(isHost: true);
        var vm = CreateVm(lobby, BuildServiceProvider());

        vm.ChatMessage = "Prepare yourselves!";
        await vm.SendMessageCommand.ExecuteAsync(null);

        await lobby.Received(1).SendMessage(ChatChannel.All, "Prepare yourselves!");
        Assert.That(vm.ChatMessage, Is.Empty);
    }

    [Test]
    public async Task MultiplayerHostFlow_SetMapCommand_InvokesLobby() {
        var (lobby, _) = CreateLobby(isHost: true);
        lobby.SetMap(Arg.Any<Map>()).Returns(Task.FromResult(true));

        var vm = CreateVm(lobby, BuildServiceProvider());

        var newMap = new Map("DesertMap", "Arid", 4, "desert", "desert_4p");
        await vm.SetMapCommand.ExecuteAsync(newMap);

        await lobby.Received(1).SetMap(newMap);
    }

    [Test]
    public async Task MultiplayerHostFlow_SetMapFails_ResetsSelectedMap() {
        var (lobby, _) = CreateLobby(isHost: true);
        lobby.SetMap(Arg.Any<Map>()).Returns(Task.FromResult(false));

        var vm = CreateVm(lobby, BuildServiceProvider());

        var badMap = new Map("TooSmall", "Tiny", 2, "tiny", "tiny_2p");
        await vm.SetMapCommand.ExecuteAsync(badMap);

        // Should reset to the lobby's map
        Assert.That(vm.SelectedMap, Is.EqualTo(DefaultMap));
    }

    [Test]
    public async Task MultiplayerHostFlow_TrayMessagesFlowThroughEvents() {
        var (lobby, events) = CreateLobby(isHost: true, startEventLoop: true);
        var vm = CreateVm(lobby, BuildServiceProvider());

        // Show tray message
        await PushEventAndWait(vm, events, new LobbyEvent(LobbyEventType.TrayMessage, "Uploading..."));
        Assert.That(vm.TrayMessage, Is.EqualTo("Uploading..."));

        // Hide tray message
        await PushEventAndWait(vm, events, new LobbyEvent(LobbyEventType.TrayMessageHide));
        events.Writer.Complete();

        Assert.That(vm.TrayMessage, Is.Empty);
    }

    [Test]
    public async Task MultiplayerHostFlow_ToggleReady_InvokesLobby() {
        var (lobby, _) = CreateLobby(isHost: true);
        var vm = CreateVm(lobby, BuildServiceProvider());

        await vm.ToggleReadyCommand.ExecuteAsync(null);

        await lobby.Received(1).MarkReady(true);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  MULTIPLAYER NON-HOST (PARTICIPANT) FLOW
    // ══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task ParticipantFlow_IsNotHost() {
        var participant = new Participant(1, "participant-1", "JoinedPlayer", false, false);
        var team1 = CreateTeam(TeamType.Allies);
        var team2 = new Team(TeamType.Axis, "Axis", [
            new Team.Slot(0, participant.ParticipantId, "germans", "", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);

        var (lobby, _) = CreateLobby(
            isHost: false,
            localPlayer: participant,
            team1: team1,
            team2: team2,
            participants: [participant]);
        lobby.GetLocalPlayerSlot().Returns((team2, 0));

        var vm = CreateVm(lobby, BuildServiceProvider());

        using (Assert.EnterMultipleScope()) {
            Assert.That(vm.IsHost, Is.False);
            Assert.That(vm.CanStartMatch, Is.False, "Non-host cannot start a match");
        }
    }

    [Test]
    public async Task ParticipantFlow_CannotStartMatch_EvenWhenTeamsReady() {
        var host = new Participant(0, "host-player", "HostPlayer", false, true);
        var participant = new Participant(1, "participant-1", "JoinedPlayer", false, true);
        var team1 = new Team(TeamType.Allies, "Allies", [
            new Team.Slot(0, host.ParticipantId, "british_africa", "host-co", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);
        var team2 = new Team(TeamType.Axis, "Axis", [
            new Team.Slot(0, participant.ParticipantId, "germans", "part-co", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);

        var (lobby, _) = CreateLobby(
            isHost: false,
            localPlayer: participant,
            team1: team1,
            team2: team2,
            participants: [host, participant]);
        lobby.GetLocalPlayerSlot().Returns((team2, 0));

        var vm = CreateVm(lobby, BuildServiceProvider());

        Assert.That(vm.CanStartMatch, Is.False);
    }

    [Test]
    public async Task ParticipantFlow_ReceivesChatFromOtherPlayers() {
        var participant = new Participant(1, "participant-1", "JoinedPlayer", false, false);
        var team2 = new Team(TeamType.Axis, "Axis", [
            new Team.Slot(0, participant.ParticipantId, "germans", "", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);

        var (lobby, events) = CreateLobby(
            isHost: false,
            startEventLoop: true,
            localPlayer: participant,
            team2: team2,
            participants: [participant]);
        lobby.GetLocalPlayerSlot().Returns((team2, 0));

        var vm = CreateVm(lobby, BuildServiceProvider());

        var hostMessage = new ChatMessage("host-player", "HostPlayer", ChatChannel.All, "Welcome!");
        await PushEventAndWait(vm, events, new LobbyEvent(LobbyEventType.ParticipantMessage, hostMessage));
        events.Writer.Complete();

        Assert.That(vm.ChatMessages, Has.Count.EqualTo(1));
        Assert.That(vm.ChatMessages[0].IsSelf, Is.False);
        Assert.That(vm.ChatMessages[0].Message, Is.EqualTo("Welcome!"));
    }

    [Test]
    public async Task ParticipantFlow_CanSendChat() {
        var participant = new Participant(1, "participant-1", "JoinedPlayer", false, false);
        var team2 = new Team(TeamType.Axis, "Axis", [
            new Team.Slot(0, participant.ParticipantId, "germans", "", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);

        var (lobby, _) = CreateLobby(
            isHost: false,
            localPlayer: participant,
            team2: team2,
            participants: [participant]);
        lobby.GetLocalPlayerSlot().Returns((team2, 0));

        var vm = CreateVm(lobby, BuildServiceProvider());

        vm.ChatMessage = "Hello host!";
        await vm.SendMessageCommand.ExecuteAsync(null);

        await lobby.Received(1).SendMessage(ChatChannel.All, "Hello host!");
    }

    [Test]
    public async Task ParticipantFlow_ToggleReady_InvokesMarkReady() {
        var participant = new Participant(1, "participant-1", "JoinedPlayer", false, false);
        var team2 = new Team(TeamType.Axis, "Axis", [
            new Team.Slot(0, participant.ParticipantId, "germans", "", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);

        var (lobby, _) = CreateLobby(
            isHost: false,
            localPlayer: participant,
            team2: team2,
            participants: [participant]);
        lobby.GetLocalPlayerSlot().Returns((team2, 0));

        var vm = CreateVm(lobby, BuildServiceProvider());

        await vm.ToggleReadyCommand.ExecuteAsync(null);

        await lobby.Received(1).MarkReady(true);
    }

    [Test]
    public async Task ParticipantFlow_ReceivesMapUpdate() {
        var participant = new Participant(1, "participant-1", "JoinedPlayer", false, false);
        var team2 = new Team(TeamType.Axis, "Axis", [
            new Team.Slot(0, participant.ParticipantId, "germans", "", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);

        var (lobby, events) = CreateLobby(
            isHost: false,
            startEventLoop: true,
            localPlayer: participant,
            team2: team2,
            participants: [participant]);
        lobby.GetLocalPlayerSlot().Returns((team2, 0));

        var vm = CreateVm(lobby, BuildServiceProvider());

        var newMap = new Map("HostChoseMap", "Desc", 4, "img", "host_map_4p");
        await PushEventAndWait(vm, events, new LobbyEvent(LobbyEventType.MapUpdated, newMap));
        events.Writer.Complete();

        Assert.That(vm.SelectedMap, Is.EqualTo(newMap));
    }

    [Test]
    public async Task ParticipantFlow_ReceivesTeamUpdate() {
        var participant = new Participant(1, "participant-1", "JoinedPlayer", false, false);
        var team2 = new Team(TeamType.Axis, "Axis", [
            new Team.Slot(0, participant.ParticipantId, "germans", "", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);

        var (lobby, events) = CreateLobby(
            isHost: false,
            startEventLoop: true,
            localPlayer: participant,
            team2: team2,
            participants: [participant]);
        lobby.GetLocalPlayerSlot().Returns((team2, 0));

        var vm = CreateVm(lobby, BuildServiceProvider());

        var fired = new List<string>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName!);

        await PushEventAndWait(vm, events, new LobbyEvent(LobbyEventType.TeamUpdated, TeamType.Axis));
        events.Writer.Complete();

        Assert.That(fired, Contains.Item(nameof(vm.Team2Slots)));
    }

    [Test]
    public async Task ParticipantFlow_ReceivesGameStartedEvent_LaunchesGame() {
        var participant = new Participant(1, "participant-1", "JoinedPlayer", false, false);
        var team2 = new Team(TeamType.Axis, "Axis", [
            new Team.Slot(0, participant.ParticipantId, "germans", "", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);

        var playService = Substitute.For<IPlayService>();
        playService.LaunchGameApp(Arg.Any<Game>()).Returns(Task.FromResult(new LaunchGameAppResult { Failed = true }));

        var (lobby, events) = CreateLobby(
            isHost: false,
            startEventLoop: true,
            localPlayer: participant,
            team2: team2,
            participants: [participant]);
        lobby.GetLocalPlayerSlot().Returns((team2, 0));

        var vm = CreateVm(lobby, BuildServiceProvider(playService: playService));

        await PushEventAndWait(vm, events, new LobbyEvent(LobbyEventType.GameStarted));
        events.Writer.Complete();

        // The participant should attempt to launch the game when receiving GameStarted
        await playService.Received(1).LaunchGameApp(Arg.Any<Game>());
    }

    [Test]
    public async Task ParticipantFlow_ReceivesMatchOver_ShowsResults() {
        var participant = new Participant(1, "participant-1", "JoinedPlayer", false, false);
        var team2 = new Team(TeamType.Axis, "Axis", [
            new Team.Slot(0, participant.ParticipantId, "germans", "", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);

        var matchData = new MatchOverData { IsValid = true, IsVictory = false, MatchId = "match-1" };

        var (lobby, events) = CreateLobby(
            isHost: false,
            startEventLoop: true,
            localPlayer: participant,
            team2: team2,
            participants: [participant]);
        lobby.GetLocalPlayerSlot().Returns((team2, 0));
        lobby.GetMatchResults().Returns(Task.FromResult<MatchOverData?>(matchData));

        var vm = CreateVm(lobby, BuildServiceProvider());

        await PushEventAndWait(vm, events, new LobbyEvent(LobbyEventType.MatchOver));
        await Task.Delay(100); // allow async void ShowMatchResults to complete
        events.Writer.Complete();

        Assert.That(vm.MatchOverResult, Is.Not.Null);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  COMMON FLOW SCENARIOS
    // ══════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task CommonFlow_MultipleMessagesAccumulate() {
        var (lobby, events) = CreateLobby(isHost: true, startEventLoop: true);
        var vm = CreateVm(lobby, BuildServiceProvider());

        await PushEventAndWait(vm, events, new LobbyEvent(LobbyEventType.ParticipantMessage,
            new ChatMessage("local-player-1", "Player One", ChatChannel.All, "First")));
        await PushEventAndWait(vm, events, new LobbyEvent(LobbyEventType.ParticipantMessage,
            new ChatMessage("other-player", "Other", ChatChannel.All, "Second")));
        await PushEventAndWait(vm, events, new LobbyEvent(LobbyEventType.ParticipantMessage,
            new ChatMessage("local-player-1", "Player One", ChatChannel.Team, "Third")));
        events.Writer.Complete();

        Assert.That(vm.ChatMessages, Has.Count.EqualTo(3));
        Assert.That(vm.ChatMessages[0].Message, Is.EqualTo("First"));
        Assert.That(vm.ChatMessages[1].IsSelf, Is.False);
        Assert.That(vm.ChatMessages[2].Message, Is.EqualTo("Third"));
    }

    [Test]
    public async Task CommonFlow_MapUpdatedDoesNotDuplicateForSameMap() {
        var (lobby, events) = CreateLobby(isHost: true, startEventLoop: true);
        var vm = CreateVm(lobby, BuildServiceProvider());

        var fired = new List<string>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName!);

        // Push same map as already selected
        await PushEventAndWait(vm, events, new LobbyEvent(LobbyEventType.MapUpdated, DefaultMap));
        events.Writer.Complete();

        Assert.That(fired, Has.No.Member(nameof(vm.SelectedMap)));
    }

    [Test]
    public async Task CommonFlow_SlotCompanyDownloadProgress_UpdatesSlot() {
        var (lobby, events) = CreateLobby(isHost: true, startEventLoop: true);
        var vm = CreateVm(lobby, BuildServiceProvider());

        await PushEventAndWait(vm, events, new LobbyEvent(LobbyEventType.SlotCompanyDownloadProgress, (0, 0, 0.75f)));
        events.Writer.Complete();

        var slot = vm.Team1Slots.FirstOrDefault(s => s.Slot.Index == 0);
        Assert.That(slot, Is.Not.Null);
        Assert.That(slot!.CompanyDownloadProgress, Is.EqualTo(0.75f));
    }

    [Test]
    public async Task CommonFlow_MatchOverWithNullResult_DoesNotCrash() {
        var (lobby, events) = CreateLobby(isHost: true, startEventLoop: true);
        lobby.GetMatchResults().Returns(Task.FromResult<MatchOverData?>(null));

        var vm = CreateVm(lobby, BuildServiceProvider());

        await PushEventAndWait(vm, events, new LobbyEvent(LobbyEventType.MatchOver));
        await Task.Delay(100);
        events.Writer.Complete();

        Assert.That(vm.MatchOverResult, Is.Null);
    }

    [Test]
    public async Task CommonFlow_LobbyState_ReflectsCanStartMatchTransition() {
        // Start with empty teams
        var (lobby, events) = CreateLobby(isHost: true, startEventLoop: true);
        var vm = CreateVm(lobby, BuildServiceProvider());

        Assert.That(vm.CanStartMatch, Is.False);
        Assert.That(vm.LobbyState, Is.EqualTo("Waiting for players to select companies and factions"));

        // Now simulate teams becoming ready by reconfiguring the mock
        var localPlayer = new Participant(0, "local-player-1", "Player One", false, true);
        var aiPlayer = new Participant(1, "ai-1", "AI", true, true);
        lobby.Team1.Returns(new Team(TeamType.Allies, "Allies", [
            new Team.Slot(0, localPlayer.ParticipantId, "british_africa", "co-1", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]));
        lobby.Team2.Returns(new Team(TeamType.Axis, "Axis", [
            new Team.Slot(0, aiPlayer.ParticipantId, "germans", "co-2", AIDifficulty.EASY, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]));

        // Push a team update to trigger SyncState
        await PushEventAndWait(vm, events, new LobbyEvent(LobbyEventType.TeamUpdated, TeamType.Allies));
        events.Writer.Complete();

        Assert.That(vm.CanStartMatch, Is.True);
        Assert.That(vm.LobbyState, Is.EqualTo("Ready to start the match"));
    }

    [Test]
    public async Task CommonFlow_CanStartMatch_RequiresHostRole() {
        var localPlayer = new Participant(0, "local-player-1", "Player One", false, true);
        var aiPlayer = new Participant(1, "ai-1", "AI", true, true);
        var team1 = new Team(TeamType.Allies, "Allies", [
            new Team.Slot(0, localPlayer.ParticipantId, "british_africa", "co-1", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);
        var team2 = new Team(TeamType.Axis, "Axis", [
            new Team.Slot(0, aiPlayer.ParticipantId, "germans", "co-2", AIDifficulty.EASY, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);

        // Non-host with fully ready teams still cannot start
        var (lobby, _) = CreateLobby(
            isHost: false,
            localPlayer: localPlayer,
            team1: team1,
            team2: team2,
            participants: [localPlayer, aiPlayer]);

        var vm = CreateVm(lobby, BuildServiceProvider());

        Assert.That(vm.CanStartMatch, Is.False, "Non-host should never be able to start");
    }

    [Test]
    public async Task CommonFlow_LeaveCommand_WhenInactive_DoesNotCallLobbyService() {
        // When lobby is already inactive, the leave command should short-circuit
        var (lobby, _) = CreateLobby(isHost: true, startEventLoop: false);
        var sp = BuildServiceProvider();
        var lobbyService = sp.GetRequiredService<ILobbyService>();
        var vm = CreateVm(lobby, sp);

        await vm.LeaveCommand.ExecuteAsync(null);

        await lobbyService.DidNotReceive().LeaveLobbyAsync(Arg.Any<ILobby>());
    }

}
