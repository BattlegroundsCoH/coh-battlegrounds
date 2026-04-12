using System.ComponentModel;
using System.Threading.Channels;

using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services;
using Battlegrounds.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;

namespace Battlegrounds.Test.ViewModels;

/// <summary>
/// Unit tests for <see cref="LobbyViewModel"/>.
/// <para>
/// <b>Documented discrepancies between <c>MultiplayerLobby</c> and <c>SingleplayerLobby</c>:</b>
/// <list type="number">
///   <item>
///     <b><c>GetRealPlayersCount()</c></b> —
///     <c>MultiplayerLobby</c> returns <c>_participants.Count(x => x.IsAIParticipant)</c>
///     which counts <em>AI</em> participants, not real human players. <c>SingleplayerLobby</c>
///     hardcodes <c>1</c>. The ViewModel uses this in <c>StartGame</c> to decide the countdown duration.
///   </item>
///   <item>
///     <b><c>TeamUpdated</c> event <c>Arg</c> type</b> —
///     <c>MultiplayerLobby.MapAndApplyGrpcEvent</c> passes <c>teamId</c> (<c>int</c>) as the event
///     <c>Arg</c> for both <c>TeamUpdated</c> and <c>SlotUpdated</c> events received via gRPC.
///     <c>MultiplayerLobby.SetSlotFaction</c> also passes <c>int</c>.
///     However the ViewModel handler expects <c>TeamType</c> (enum).  When the <c>Arg</c> is <c>int</c>,
///     the pattern <c>lobbyEvent.Arg is TeamType</c> fails and <em>neither team is updated</em>.
///     See <see cref="TeamUpdated_Event_WithIntArg_DropsUpdate_Discrepancy"/>.
///   </item>
///   <item>
///     <b><c>TeamUpdated</c> event with <c>null</c> <c>Arg</c></b> —
///     <c>SingleplayerLobby.SetMap</c> writes <c>new LobbyEvent(LobbyEventType.TeamUpdated)</c>
///     (no <c>Arg</c>) when the map player count changes.  The ViewModel's
///     <c>lobbyEvent is null</c> guard checks the <em>event</em>, not its <c>Arg</c>, so it is
///     always <c>false</c> here and neither team is refreshed.
///     See <see cref="TeamUpdated_Event_WithNullArg_DropsUpdate_Discrepancy"/>.
///   </item>
///   <item>
///     <b><c>SingleplayerLobby.SetSlotFaction</c> is NOP</b> —
///     The ViewModel's <c>AddAIToSlot</c> calls <c>_lobby.SetSlotFaction</c> after setting AI
///     difficulty.  In singleplayer, this does nothing, so AI slots never receive a faction.
///   </item>
///   <item>
///     <b><c>SettingUpdated</c> event <c>Arg</c></b> —
///     Both lobby implementations write <c>LobbyEvent(LobbyEventType.SettingUpdated)</c> with <c>null</c>
///     <c>Arg</c> for local changes.  The ViewModel's targeted-update path
///     (<c>lobbyEvent.Arg is LobbySetting</c>) never matches for local changes, so it always
///     falls through to a <c>PropertyChanged</c> notification only.  Only gRPC-sourced events
///     from <c>MapAndApplyGrpcEvent</c> include the <c>LobbySetting</c> as <c>Arg</c>.
///   </item>
/// </list>
/// </para>
/// </summary>
[TestOf(typeof(LobbyViewModel))]
public sealed class LobbyViewModelTests {

    // ── Shared test infrastructure ───────────────────────────────────────────

    private static readonly Map DefaultMap = new("TestMap", "A test map", 4, "preview", "test_map_4p");

    private static Game CreateMockGame() {
        var game = Substitute.For<Game>();
        game.Id.Returns("CoH3");
        game.FactionIds.Returns(["british_africa", "germans"]);
        game.GetFactionAlliance("british_africa").Returns(FactionAlliance.Allies);
        game.GetFactionAlliance("germans").Returns(FactionAlliance.Axis);
        return game;
    }

    private static Team CreateTeam(TeamType type, string? localPlayerId = null) {
        var slots = new Team.Slot[2];
        for (int i = 0; i < slots.Length; i++) {
            string? pid = (i == 0 && localPlayerId is not null) ? localPlayerId : null;
            slots[i] = new Team.Slot(i, pid, "", "", AIDifficulty.HUMAN, false, false);
        }
        return new Team(type, type.ToString(), slots);
    }

    /// <summary>
    /// Builds a mock <see cref="ILobby"/>. The returned <see cref="Channel{LobbyEvent}"/>
    /// lets tests push events; complete the writer to stop the polling loop.
    /// </summary>
    private static (ILobby lobby, Channel<LobbyEvent> events) CreateLobby(
        bool isHost = true,
        bool startEventLoop = false) {

        var game = CreateMockGame();
        var localPlayer = new Participant(0, "local-player-1", "Player One", false, false);
        var team1 = CreateTeam(TeamType.Allies, localPlayer.ParticipantId);
        var team2 = CreateTeam(TeamType.Axis);

        var lobby = Substitute.For<ILobby>();
        lobby.Name.Returns("Test Lobby");
        lobby.IsHost.Returns(isHost);
        lobby.IsReady.Returns(false);
        lobby.Game.Returns(game);
        lobby.Map.Returns(DefaultMap);
        lobby.Team1.Returns(team1);
        lobby.Team2.Returns(team2);
        lobby.Participants.Returns(new HashSet<Participant> { localPlayer });
        lobby.Companies.Returns(new Dictionary<string, Company>());
        lobby.Settings.Returns(new List<LobbySetting>());
        lobby.GetLocalPlayerId().Returns(localPlayer.ParticipantId);
        lobby.GetLocalPlayerSlot().Returns((team1, 0));

        var eventChannel = Channel.CreateUnbounded<LobbyEvent>();

        if (startEventLoop) {
            lobby.IsActive.Returns(true);
            Func<NSubstitute.Core.CallInfo, ValueTask<LobbyEvent?>> readEvent =
                _ => ReadNextEventAsync(eventChannel.Reader);
            lobby.GetNextEvent().Returns(readEvent);
        } else {
            lobby.IsActive.Returns(false);
            lobby.GetNextEvent().Returns(ValueTask.FromResult<LobbyEvent?>(null));
        }

        return (lobby, eventChannel);
    }

    private static async ValueTask<LobbyEvent?> ReadNextEventAsync(ChannelReader<LobbyEvent> reader) {
        try { return await reader.ReadAsync(); }
        catch (ChannelClosedException) { return null; }
    }

    /// <summary>
    /// Builds an <see cref="IServiceProvider"/> that satisfies all transitive constructor
    /// dependencies of <see cref="LobbyViewModel"/>.
    /// </summary>
    private static IServiceProvider BuildServiceProvider() {
        var services = new ServiceCollection();

        // Leaf service substitutes
        services.AddSingleton(Substitute.For<ILobbyService>());
        services.AddSingleton(Substitute.For<IPlayService>());
        services.AddSingleton(Substitute.For<IReplayService>());
        services.AddSingleton(Substitute.For<IDialogService>());
        services.AddSingleton(Substitute.For<IUserService>());
        services.AddSingleton(Substitute.For<IBrowserService>());
        services.AddSingleton(Substitute.For<IGameService>());
        services.AddSingleton(Substitute.For<IUpdateService>());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        // These need default returns so the async-void SyncLobbyView doesn't crash
        var gameMapService = Substitute.For<IGameMapService>();
        gameMapService.GetMapsForGame(Arg.Any<string>()).Returns(Task.FromResult(new List<Scenario>()));
        services.AddSingleton(gameMapService);

        var companyService = Substitute.For<ICompanyService>();
        companyService.GetLocalCompaniesAsync().Returns(Task.FromResult<IEnumerable<Company>>([]));
        services.AddSingleton(companyService);

        var statisticsService = Substitute.For<IStatisticsService>();
        statisticsService.IsLoaded.Returns(Task.CompletedTask);
        services.AddSingleton(statisticsService);

        services.AddSingleton(TimeProvider.System);

        // Concrete sealed view-models needed by MainWindowViewModel
        services.AddSingleton<UserViewModel>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<LoginViewModel>();
        services.AddSingleton<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Creates a <see cref="LobbyViewModel"/>. All mocked async operations return
    /// already-completed tasks, so the <c>async void SyncLobbyView</c> chain runs
    /// synchronously during construction — no additional settling delay is required.
    /// </summary>
    private static Task<LobbyViewModel> CreateVmAsync(ILobby lobby, IServiceProvider sp)
        => Task.FromResult(new LobbyViewModel(lobby, sp, NullLogger<LobbyViewModel>.Instance));

    // ── Test synchronization helpers ─────────────────────────────────────────

    /// <summary>
    /// Polls <paramref name="condition"/> every <paramref name="pollMs"/> milliseconds until
    /// it returns <see langword="true"/> or <paramref name="timeoutMs"/> elapses.
    /// </summary>
    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 2000, int pollMs = 10) {
        int elapsed = 0;
        while (!condition()) {
            if (elapsed >= timeoutMs)
                Assert.Fail($"Condition not met within {timeoutMs} ms.");
            await Task.Delay(pollMs);
            elapsed += pollMs;
        }
    }

    /// <summary>
    /// Returns a <see cref="Task"/> that completes the next time <paramref name="vm"/> raises
    /// <see cref="INotifyPropertyChanged.PropertyChanged"/> for <paramref name="propertyName"/>.
    /// Throws <see cref="TimeoutException"/> if no matching notification fires within
    /// <paramref name="timeoutMs"/> milliseconds.
    /// </summary>
    private static Task WaitForPropertyChangedAsync(
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

    // ── CanStartMatch ─────────────────────────────────────────────────────────

    [Test]
    public async Task CanStartMatch_IsFalse_WhenNotHost() {
        var (lobby, _) = CreateLobby(isHost: false);
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        Assert.That(vm.CanStartMatch, Is.False);
    }

    [Test]
    public async Task CanStartMatch_IsFalse_WhenNoSlotsHaveCompanies() {
        var (lobby, _) = CreateLobby(isHost: true);
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        Assert.That(vm.CanStartMatch, Is.False);
    }

    [Test]
    public async Task CanStartMatch_IsTrue_WhenBothTeamsHaveReadySlots() {
        var game = CreateMockGame();
        var localPlayer = new Participant(0, "local-player-1", "Player One", false, true);
        var aiPlayer = new Participant(1, "ai-1", "AI Player", true, true);

        var team1 = new Team(TeamType.Allies, "Allies", [
            new Team.Slot(0, localPlayer.ParticipantId, "british_africa", "company-1", AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);
        var team2 = new Team(TeamType.Axis, "Axis", [
            new Team.Slot(0, aiPlayer.ParticipantId, "germans", "company-2", AIDifficulty.EASY, false, false),
            new Team.Slot(1, null, "", "", AIDifficulty.HUMAN, false, false),
        ]);

        var lobby = Substitute.For<ILobby>();
        lobby.Name.Returns("Test");
        lobby.IsHost.Returns(true);
        lobby.IsReady.Returns(true);
        lobby.IsActive.Returns(false);
        lobby.Game.Returns(game);
        lobby.Map.Returns(DefaultMap);
        lobby.Team1.Returns(team1);
        lobby.Team2.Returns(team2);
        lobby.Participants.Returns(new HashSet<Participant> { localPlayer, aiPlayer });
        lobby.Companies.Returns(new Dictionary<string, Company>());
        lobby.Settings.Returns(new List<LobbySetting>());
        lobby.GetLocalPlayerId().Returns(localPlayer.ParticipantId);
        lobby.GetLocalPlayerSlot().Returns((team1, 0));
        lobby.GetNextEvent().Returns(ValueTask.FromResult<LobbyEvent?>(null));

        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        Assert.That(vm.CanStartMatch, Is.True);
    }

    // ── ChatMessage property ──────────────────────────────────────────────────

    [Test]
    public async Task ChatMessage_UnderLimit_IsSetVerbatim() {
        var (lobby, _) = CreateLobby();
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        vm.ChatMessage = "Hello world";

        Assert.That(vm.ChatMessage, Is.EqualTo("Hello world"));
    }

    [Test]
    public async Task ChatMessage_OverLimit_IsTruncatedToMaxLength() {
        var (lobby, _) = CreateLobby();
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        vm.ChatMessage = new string('a', 250);

        Assert.That(vm.ChatMessage, Has.Length.EqualTo(LobbyViewModel.MAX_CHAT_MESSAGE_LENGTH));
    }

    [Test]
    public async Task ChatMessage_OverLimit_AddsSystemWarning() {
        var (lobby, _) = CreateLobby();
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        vm.ChatMessage = new string('a', 250);

        Assert.That(vm.ChatMessages.Any(m =>
            m.IsSystemMessage && m.Message == LobbyViewModel.MAX_MESSAGE_LENGTH_REACHED), Is.True);
    }

    [Test]
    public async Task ChatMessage_OverLimitTwice_DoesNotDuplicateWarning() {
        var (lobby, _) = CreateLobby();
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        vm.ChatMessage = new string('a', 250);
        vm.ChatMessage = new string('b', 250);

        Assert.That(vm.ChatMessages.Count(m => m.IsSystemMessage), Is.EqualTo(1));
    }

    // ── PropertyChanged notifications ─────────────────────────────────────────

    [Test]
    public async Task LobbyState_RaisesPropertyChanged_WhenValueChanges() {
        var (lobby, _) = CreateLobby();
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        var fired = new List<string>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName!);

        vm.LobbyState = "New state";

        Assert.That(fired, Contains.Item(nameof(vm.LobbyState)));
    }

    [Test]
    public async Task LobbyState_DoesNotRaise_WhenValueUnchanged() {
        var (lobby, _) = CreateLobby();
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        vm.LobbyState = "Stable";
        var fired = new List<string>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName!);

        vm.LobbyState = "Stable"; // same value

        Assert.That(fired, Has.No.Member(nameof(vm.LobbyState)));
    }

    [Test]
    public async Task ChatMessage_RaisesPropertyChanged() {
        var (lobby, _) = CreateLobby();
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        var fired = new List<string>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName!);

        vm.ChatMessage = "test";

        Assert.That(fired, Contains.Item(nameof(vm.ChatMessage)));
    }

    [Test]
    public async Task TrayMessage_RaisesPropertyChanged() {
        var (lobby, _) = CreateLobby();
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        var fired = new List<string>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName!);

        vm.TrayMessage = "uploading...";

        Assert.That(fired, Contains.Item(nameof(vm.TrayMessage)));
    }

    [Test]
    public async Task TrayMessage_DoesNotRaise_WhenUnchanged() {
        var (lobby, _) = CreateLobby();
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        var fired = new List<string>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName!);

        vm.TrayMessage = string.Empty; // default is already empty

        Assert.That(fired, Has.No.Member(nameof(vm.TrayMessage)));
    }

    // ── SyncState ─────────────────────────────────────────────────────────────

    [Test]
    public async Task SyncState_SetsWaitingMessage_WhenCannotStart() {
        var (lobby, _) = CreateLobby();
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        Assert.That(vm.LobbyState, Is.EqualTo("Waiting for players to select companies and factions"));
    }

    // ── Event: ParticipantMessage ─────────────────────────────────────────────

    [Test]
    public async Task ParticipantMessage_Event_AddsToChatMessages() {
        var (lobby, events) = CreateLobby(startEventLoop: true);
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        var msg = new ChatMessage("local-player-1", "Player One", ChatChannel.All, "Hello!");
        var processed = WaitForPropertyChangedAsync(vm, nameof(vm.CanStartMatch));
        await events.Writer.WriteAsync(new LobbyEvent(LobbyEventType.ParticipantMessage, msg));
        await processed;
        events.Writer.Complete();

        Assert.That(vm.ChatMessages, Has.Count.EqualTo(1));
        Assert.That(vm.ChatMessages[0].Message, Is.EqualTo("Hello!"));
        Assert.That(vm.ChatMessages[0].IsSelf, Is.True);
    }

    [Test]
    public async Task ParticipantMessage_Event_FromOtherPlayer_IsNotSelf() {
        var (lobby, events) = CreateLobby(startEventLoop: true);
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        var msg = new ChatMessage("other-player-2", "Player Two", ChatChannel.All, "Hi there!");
        var processed = WaitForPropertyChangedAsync(vm, nameof(vm.CanStartMatch));
        await events.Writer.WriteAsync(new LobbyEvent(LobbyEventType.ParticipantMessage, msg));
        await processed;
        events.Writer.Complete();

        Assert.That(vm.ChatMessages, Has.Count.EqualTo(1));
        Assert.That(vm.ChatMessages[0].IsSelf, Is.False);
    }

    [Test]
    public async Task ParticipantMessage_Event_WithInvalidArg_IsIgnored() {
        var (lobby, events) = CreateLobby(startEventLoop: true);
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        var processed = WaitForPropertyChangedAsync(vm, nameof(vm.CanStartMatch));
        await events.Writer.WriteAsync(new LobbyEvent(LobbyEventType.ParticipantMessage, "not a ChatMessage"));
        await processed;
        events.Writer.Complete();

        Assert.That(vm.ChatMessages, Is.Empty);
    }

    // ── Event: TrayMessage / TrayMessageHide ──────────────────────────────────

    [Test]
    public async Task TrayMessage_Event_SetsTrayMessage() {
        var (lobby, events) = CreateLobby(startEventLoop: true);
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        var processed = WaitForPropertyChangedAsync(vm, nameof(vm.CanStartMatch));
        await events.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TrayMessage, "Uploading..."));
        await processed;
        events.Writer.Complete();

        Assert.That(vm.TrayMessage, Is.EqualTo("Uploading..."));
    }

    [Test]
    public async Task TrayMessage_Event_WithInvalidArg_IsIgnored() {
        var (lobby, events) = CreateLobby(startEventLoop: true);
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        var processed = WaitForPropertyChangedAsync(vm, nameof(vm.CanStartMatch));
        await events.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TrayMessage, 42));
        await processed;
        events.Writer.Complete();

        Assert.That(vm.TrayMessage, Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task TrayMessageHide_Event_ClearsTrayMessage() {
        var (lobby, events) = CreateLobby(startEventLoop: true);
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        var firstProcessed = WaitForPropertyChangedAsync(vm, nameof(vm.CanStartMatch));
        await events.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TrayMessage, "Something"));
        await firstProcessed;

        var secondProcessed = WaitForPropertyChangedAsync(vm, nameof(vm.CanStartMatch));
        await events.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TrayMessageHide));
        await secondProcessed;
        events.Writer.Complete();

        Assert.That(vm.TrayMessage, Is.EqualTo(string.Empty));
    }

    // ── Event: MapUpdated ─────────────────────────────────────────────────────

    [Test]
    public async Task MapUpdated_Event_UpdatesSelectedMap() {
        var (lobby, events) = CreateLobby(startEventLoop: true);
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        var newMap = new Map("New Map", "Desc", 4, "new_preview", "new_map_4p");
        var processed = WaitForPropertyChangedAsync(vm, nameof(vm.CanStartMatch));
        await events.Writer.WriteAsync(new LobbyEvent(LobbyEventType.MapUpdated, newMap));
        await processed;
        events.Writer.Complete();

        Assert.That(vm.SelectedMap, Is.EqualTo(newMap));
    }

    [Test]
    public async Task MapUpdated_Event_SameMap_DoesNothing() {
        var (lobby, events) = CreateLobby(startEventLoop: true);
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        var fired = new List<string>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName!);

        // Push the same map that is already selected
        var processed = WaitForPropertyChangedAsync(vm, nameof(vm.CanStartMatch));
        await events.Writer.WriteAsync(new LobbyEvent(LobbyEventType.MapUpdated, DefaultMap));
        await processed;
        events.Writer.Complete();

        Assert.That(fired, Has.No.Member(nameof(vm.SelectedMap)));
    }

    // ── Event: TeamUpdated ────────────────────────────────────────────────────

    [Test]
    public async Task TeamUpdated_Event_WithTeamType_UpdatesCorrectTeam() {
        var (lobby, events) = CreateLobby(startEventLoop: true);
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        var fired = new List<string>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName!);

        // This is how MultiplayerLobby.ToggleSlotLock, RemoveAI, SetSlotAIDifficulty send the event
        var processed = WaitForPropertyChangedAsync(vm, nameof(vm.CanStartMatch));
        await events.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TeamUpdated, TeamType.Allies));
        await processed;
        events.Writer.Complete();

        Assert.That(fired, Contains.Item(nameof(vm.Team1Slots)));
    }

    /// <summary>
    /// Documents discrepancy #2: <c>MultiplayerLobby.MapAndApplyGrpcEvent</c> passes an <c>int</c>
    /// teamId for <c>TeamUpdated</c> and <c>SlotUpdated</c> gRPC events, but the ViewModel expects
    /// <c>TeamType</c>.  Because <c>lobbyEvent.Arg is TeamType</c> fails for <c>int</c>, neither team is
    /// refreshed and the update is silently dropped.
    /// </summary>
    [Test]
    [NUnit.Framework.Category("Discrepancy")]
    public async Task TeamUpdated_Event_WithIntArg_DropsUpdate_Discrepancy() {
        var (lobby, events) = CreateLobby(startEventLoop: true);
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        var fired = new List<string>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName!);

        // MultiplayerLobby.MapAndApplyGrpcEvent sends int teamId, not TeamType
        var processed = WaitForPropertyChangedAsync(vm, nameof(vm.CanStartMatch));
        await events.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TeamUpdated, 0));
        await processed;
        events.Writer.Complete();

        // BUG: neither team is refreshed because the VM expects TeamType, not int
        Assert.That(fired, Has.No.Member(nameof(vm.Team1Slots)));
        Assert.That(fired, Has.No.Member(nameof(vm.Team2Slots)));
    }

    /// <summary>
    /// Documents discrepancy #3: <c>SingleplayerLobby.SetMap</c> writes
    /// <c>new LobbyEvent(LobbyEventType.TeamUpdated)</c> with no <c>Arg</c> when the map's
    /// player count changes.  The ViewModel checks <c>lobbyEvent is null</c> (the event object
    /// itself, not its <c>Arg</c>) which is always <c>false</c>, so neither team is refreshed.
    /// </summary>
    [Test]
    [NUnit.Framework.Category("Discrepancy")]
    public async Task TeamUpdated_Event_WithNullArg_DropsUpdate_Discrepancy() {
        var (lobby, events) = CreateLobby(startEventLoop: true);
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        var fired = new List<string>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName!);

        // SingleplayerLobby.SetMap writes TeamUpdated with no Arg
        var processed = WaitForPropertyChangedAsync(vm, nameof(vm.CanStartMatch));
        await events.Writer.WriteAsync(new LobbyEvent(LobbyEventType.TeamUpdated));
        await processed;
        events.Writer.Complete();

        // BUG: neither team is refreshed
        Assert.That(fired, Has.No.Member(nameof(vm.Team1Slots)));
        Assert.That(fired, Has.No.Member(nameof(vm.Team2Slots)));
    }

    // ── Event: SettingUpdated ─────────────────────────────────────────────────

    [Test]
    public async Task SettingUpdated_Event_WithLobbySetting_DoesTargetedUpdate() {
        var (lobby, events) = CreateLobby(startEventLoop: true);
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        // Pre-populate a setting via the lobby backing data (simulating SyncLobbySettings ran)
        var setting = new LobbySetting { Name = "test_setting", Type = LobbySettingType.Boolean, Value = 0 };
        lobby.Settings.Returns(new List<LobbySetting> { setting });

        // Force resync so the VM picks up the setting
        vm.LobbyState = "trigger resync"; // just to change something, settings are synced at construction

        var fired = new List<string>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName!);

        // This is how MapAndApplyGrpcEvent sends setting updates (with the LobbySetting as Arg)
        var processed = WaitForPropertyChangedAsync(vm, nameof(vm.CanStartMatch));
        await events.Writer.WriteAsync(new LobbyEvent(LobbyEventType.SettingUpdated, setting));
        await processed;
        events.Writer.Complete();

        Assert.That(fired, Contains.Item(nameof(vm.SelectedSettings)));
    }

    /// <summary>
    /// Documents discrepancy #5: both lobby implementations send <c>SettingUpdated</c> with
    /// <c>null</c> <c>Arg</c> for local changes.  The ViewModel's targeted-update path
    /// (<c>lobbyEvent.Arg is LobbySetting</c>) never fires; only <c>PropertyChanged</c> is raised.
    /// </summary>
    [Test]
    [NUnit.Framework.Category("Discrepancy")]
    public async Task SettingUpdated_Event_WithNullArg_OnlyFiresPropertyChanged() {
        var (lobby, events) = CreateLobby(startEventLoop: true);
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        var fired = new List<string>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName!);

        // Both SingleplayerLobby.SetSetting and MultiplayerLobby.SetSetting send this form
        var processed = WaitForPropertyChangedAsync(vm, nameof(vm.CanStartMatch));
        await events.Writer.WriteAsync(new LobbyEvent(LobbyEventType.SettingUpdated));
        await processed;
        events.Writer.Complete();

        // PropertyChanged still fires (so the UI re-reads existing wrappers), but no targeted swap
        Assert.That(fired, Contains.Item(nameof(vm.SelectedSettings)));
    }

    // ── Event: SlotCompanyDownloadProgress ────────────────────────────────────

    [Test]
    public async Task DownloadProgress_Event_UpdatesSlotVm() {
        var (lobby, events) = CreateLobby(startEventLoop: true);
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        var processed = WaitForPropertyChangedAsync(vm, nameof(vm.CanStartMatch));
        await events.Writer.WriteAsync(
            new LobbyEvent(LobbyEventType.SlotCompanyDownloadProgress, (0, 0, 0.5f)));
        await processed;
        events.Writer.Complete();

        var slot = vm.Team1Slots.FirstOrDefault(x => x.Slot.Index == 0);
        Assert.That(slot, Is.Not.Null);
        Assert.That(slot!.CompanyDownloadProgress, Is.EqualTo(0.5f));
    }

    [Test]
    public async Task DownloadProgress_WhenComplete_AutoHidesAfterDelay() {
        var (lobby, events) = CreateLobby(startEventLoop: true);
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        var processed = WaitForPropertyChangedAsync(vm, nameof(vm.CanStartMatch));
        await events.Writer.WriteAsync(
            new LobbyEvent(LobbyEventType.SlotCompanyDownloadProgress, (0, 0, 1.0f)));
        await processed;

        var slot = vm.Team1Slots.FirstOrDefault(x => x.Slot.Index == 0);
        Assert.That(slot, Is.Not.Null);
        Assert.That(slot!.CompanyDownloadProgress, Is.EqualTo(1.0f), "Should be 1.0 immediately after the event");

        // Poll until the 2-second production-code delay in HideDownloadProgressAfterDelay elapses
        await WaitUntilAsync(() => slot.CompanyDownloadProgress == 0f, timeoutMs: 3000);
        events.Writer.Complete();

        Assert.That(slot.CompanyDownloadProgress, Is.EqualTo(0f), "Should be reset to 0 after auto-hide delay");
    }

    // ── Event: MatchOver ──────────────────────────────────────────────────────

    [Test]
    public async Task MatchOver_Event_WhenGetMatchResultsReturnsNull_DoesNotSetOverlay() {
        var (lobby, events) = CreateLobby(startEventLoop: true);
        lobby.GetMatchResults().Returns(Task.FromResult<MatchOverData?>(null));
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        var processed = WaitForPropertyChangedAsync(vm, nameof(vm.CanStartMatch));
        await events.Writer.WriteAsync(new LobbyEvent(LobbyEventType.MatchOver));
        await processed;
        events.Writer.Complete();

        Assert.That(vm.MatchOverResult, Is.Null);
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    [Test]
    public async Task SendMessageCommand_SendsViaLobby() {
        var (lobby, _) = CreateLobby();
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        vm.ChatMessage = "Test message";
        await vm.SendMessageCommand.ExecuteAsync(null);

        await lobby.Received(1).SendMessage(ChatChannel.All, "Test message");
        Assert.That(vm.ChatMessage, Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task SendMessageCommand_EmptyMessage_IsIgnored() {
        var (lobby, _) = CreateLobby();
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        vm.ChatMessage = "   ";
        await vm.SendMessageCommand.ExecuteAsync(null);

        await lobby.DidNotReceive().SendMessage(Arg.Any<ChatChannel>(), Arg.Any<string>());
    }

    [Test]
    public async Task SendMessageCommand_OverLimitMessage_IsTruncatedBeforeSending() {
        var (lobby, _) = CreateLobby();
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        vm.ChatMessage = new string('x', 250); // already truncated to 180 by setter
        await vm.SendMessageCommand.ExecuteAsync(null);

        await lobby.Received(1).SendMessage(ChatChannel.All,
            Arg.Is<string>(s => s.Length == LobbyViewModel.MAX_CHAT_MESSAGE_LENGTH));
    }

    [Test]
    public async Task ToggleReadyCommand_CallsMarkReady() {
        var (lobby, _) = CreateLobby();
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        // IsReady is false by default → toggle should call MarkReady(true)
        await vm.ToggleReadyCommand.ExecuteAsync(null);

        await lobby.Received(1).MarkReady(true);
    }

    [Test]
    public async Task ToggleReadyCommand_RaisesIsReadyPropertyChanged() {
        var (lobby, _) = CreateLobby();
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        var fired = new List<string>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName!);

        await vm.ToggleReadyCommand.ExecuteAsync(null);

        Assert.That(fired, Contains.Item(nameof(vm.IsReady)));
    }

    // ── Initial state ─────────────────────────────────────────────────────────

    [Test]
    public async Task Constructor_SetsLobbyNameFromModel() {
        var (lobby, _) = CreateLobby();
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        Assert.That(vm.LobbyName, Is.EqualTo("Test Lobby"));
    }

    [Test]
    public async Task Constructor_SetsIsHostFromModel() {
        var (lobby, _) = CreateLobby(isHost: true);
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        Assert.That(vm.IsHost, Is.True);
    }

    [Test]
    public async Task Constructor_PopulatesTeamSlots() {
        var (lobby, _) = CreateLobby();
        var vm = await CreateVmAsync(lobby, BuildServiceProvider());

        Assert.That(vm.Team1Slots, Has.Count.EqualTo(2));
        Assert.That(vm.Team2Slots, Has.Count.EqualTo(2));
    }

}
