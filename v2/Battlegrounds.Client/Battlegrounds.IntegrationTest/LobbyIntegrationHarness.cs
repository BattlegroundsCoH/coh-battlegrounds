using System.Diagnostics;

using Battlegrounds.Facades.API;
using Battlegrounds.Factories;
using Battlegrounds.Models;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Proto.Lobbies;
using Battlegrounds.Serializers;
using Battlegrounds.Services;
using Battlegrounds.Test;

using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Participant = Battlegrounds.Models.Lobbies.Participant;
using Team = Battlegrounds.Models.Lobbies.Team;

namespace Battlegrounds.IntegrationTest;

/// <summary>
/// Spins up two <see cref="MultiplayerLobby"/> instances (host + participant) connected to the
/// integration-test container. Each instance starts its <c>PollGrpcUpdates</c> background task
/// automatically; both are stopped and disposed in <see cref="DisposeAsync"/>.
/// </summary>
/// <remarks>
/// Authentication is disabled in the integration-test image. Identity is conveyed via
/// <c>x-test-user-id</c> and <c>x-test-user-name</c> gRPC metadata on the initial streaming call.
/// Subsequent calls from <see cref="MultiplayerLobby"/> carry <c>x-lobby-id</c> and
/// <c>x-participant-id</c> for server-side context resolution.
/// </remarks>
public sealed class LobbyIntegrationHarness : IAsyncDisposable {

    private readonly string _grpcAddress;
    private readonly string? _httpApiBaseUrl;
    private readonly GrpcChannel _channel;
    private readonly LobbyService.LobbyServiceClient _grpcClient;

    private CancellationTokenSource? _hostPollCts;
    private CancellationTokenSource? _participantPollCts;
    private Task? _hostPollTask;
    private Task? _participantPollTask;

    public MultiplayerLobby? HostLobby { get; private set; }
    public MultiplayerLobby? ParticipantLobby { get; private set; }

    public LobbyIntegrationHarness(string grpcAddress, string? httpApiBaseUrl = null) {
        _grpcAddress = grpcAddress;
        _httpApiBaseUrl = httpApiBaseUrl;
        _channel = GrpcChannel.ForAddress(_grpcAddress);
        _grpcClient = new LobbyService.LobbyServiceClient(_channel);
    }

    /// <summary>
    /// Creates a lobby as the host participant. The returned <see cref="MultiplayerLobby"/> has
    /// <c>IsHost = true</c> and its polling task is running.
    /// </summary>
    public async Task<MultiplayerLobby> CreateHostLobbyAsync(
        string userId, string userName, string lobbyName = "IntegrationTestLobby", string gameId = "CoH3") {

        var metadata = BuildTestMetadata(userId, userName);

        var stream = _grpcClient.HostLobby(
            new HostLobbyRequest { LobbyName = lobbyName, GameId = gameId },
            metadata);

        var services = BuildServiceProvider(userId, userName);
        var factory = new MultiplayerLobbyFactory(services);

        // Build an intercepted client so every subsequent unary RPC (PublishInitialState, SendMessage,
        // etc.) automatically carries x-test-user-id / x-test-user-name which the integration-test
        // server requires to resolve the caller's identity.
        var interceptedClient = new LobbyService.LobbyServiceClient(
            _channel.Intercept(new TestIdentityInterceptor(userId, userName)));

        var setup = BuildMinimalHostSetup(userId, userName, lobbyName, gameId, services);
        HostLobby = await factory.GetLobbyAsHost(interceptedClient, stream, setup);
        await HostLobby.PublishInitialState();

        _hostPollCts = new CancellationTokenSource();
        _hostPollTask = Task.Run(HostLobby.PollGrpcUpdates, _hostPollCts.Token);

        return HostLobby;
    }

    /// <summary>
    /// Joins an existing lobby as a non-host participant. The returned <see cref="MultiplayerLobby"/>
    /// has <c>IsHost = false</c> and its polling task is running.
    /// </summary>
    public async Task<MultiplayerLobby> JoinLobbyAsync(
        BrowserLobby browserLobby, string userId, string userName) {

        var metadata = BuildTestMetadata(userId, userName);

        var stream = _grpcClient.JoinLobby(
            new JoinLobbyRequest { LobbyId = browserLobby.Id },
            metadata);

        // Build a second channel/client scoped to the participant so metadata are isolated.
        // Use an intercepted invoker so every unary RPC from this participant also carries
        // x-test-user-id / x-test-user-name for the integration-test server.
        var participantChannel = GrpcChannel.ForAddress(_grpcAddress);
        var participantClient = new LobbyService.LobbyServiceClient(
            participantChannel.Intercept(new TestIdentityInterceptor(userId, userName)));

        var services = BuildServiceProvider(userId, userName);
        var factory = new MultiplayerLobbyFactory(services);

        ParticipantLobby = await factory.GetLobbyAsNonHost(browserLobby, participantClient, stream);

        _participantPollCts = new CancellationTokenSource();
        _participantPollTask = Task.Run(ParticipantLobby.PollGrpcUpdates, _participantPollCts.Token);

        return ParticipantLobby;
    }

    /// <summary>
    /// Drains <see cref="MultiplayerLobby.GetNextEvent"/> from <paramref name="lobby"/> until an
    /// event matching <paramref name="eventType"/> is found or <paramref name="timeoutMs"/> elapses.
    /// </summary>
    /// <exception cref="TimeoutException">Thrown when no matching event arrives within the timeout.</exception>
    public static async Task<LobbyEvent> WaitForEventAsync(
        MultiplayerLobby lobby, LobbyEventType eventType, int timeoutMs = 5000, string? scenario = null) {

        if (timeoutMs <= 0) {
            throw new ArgumentOutOfRangeException(nameof(timeoutMs), timeoutMs, "Timeout must be greater than zero.");
        }

        TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMs);
        long startedAt = Stopwatch.GetTimestamp();

        while (true) {
            TimeSpan remaining = timeout - Stopwatch.GetElapsedTime(startedAt);
            if (remaining <= TimeSpan.Zero) {
                break;
            }

            LobbyEvent? evt;
            try {
                evt = await lobby.GetNextEvent().AsTask().WaitAsync(remaining);
            } catch (TimeoutException ex) {
                throw new TimeoutException(BuildTimeoutMessage(eventType, timeoutMs, scenario), ex);
            }

            if (evt is null) {
                throw new TimeoutException(BuildTimeoutMessage(eventType, timeoutMs, scenario, "Lobby event stream closed before the expected event was observed."));
            }

            if (evt.EventType == eventType) {
                return evt;
            }
        }

        throw new TimeoutException(BuildTimeoutMessage(eventType, timeoutMs, scenario));
    }

    /// <summary>
    /// Attempts to wait for an event and returns <see langword="null"/> on timeout instead of throwing.
    /// </summary>
    public static async Task<LobbyEvent?> TryWaitForEventAsync(
        MultiplayerLobby lobby, LobbyEventType eventType, int timeoutMs = 5000, string? scenario = null) {
        try {
            return await WaitForEventAsync(lobby, eventType, timeoutMs, scenario);
        } catch (TimeoutException) {
            return null;
        }
    }

    public async ValueTask DisposeAsync() {
        _hostPollCts?.Cancel();
        _participantPollCts?.Cancel();

        try {
            if (_hostPollTask is not null) await _hostPollTask.WaitAsync(TimeSpan.FromSeconds(3));
        } catch { /* expected on cancellation */ }

        try {
            if (_participantPollTask is not null) await _participantPollTask.WaitAsync(TimeSpan.FromSeconds(3));
        } catch { /* expected on cancellation */ }

        HostLobby?.Dispose();
        ParticipantLobby?.Dispose();
        await _channel.ShutdownAsync();
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private static Metadata BuildTestMetadata(string userId, string userName) =>
        new() {
            { "x-test-user-id", userId },
            { "x-test-user-name", userName },
        };

    private IServiceProvider BuildServiceProvider(string userId, string userName) {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        var userService = Substitute.For<IUserService>();
        string token = BuildTestJwt(userId);
        userService.GetLocalUserToken().Returns(token);
        userService.GetLocalUserTokenAsync().Returns(Task.FromResult(token));
        userService.GetLocalUserAsync().Returns(Task.FromResult<User?>(new User {
            UserId = userId,
            UserDisplayName = userName,
        }));
        services.AddSingleton(userService);

        var companyService = Substitute.For<ICompanyService>();
        companyService.GetLocalCompaniesAsync().Returns(Task.FromResult<IEnumerable<Company>>([]));
        services.AddSingleton(companyService);

        if (string.IsNullOrWhiteSpace(_httpApiBaseUrl)) {
            var serverAPI = Substitute.For<IBattlegroundsServerAPI>();
            services.AddSingleton(serverAPI);
        } else {
            var baseUri = new Uri(_httpApiBaseUrl);
            var config = new Configuration {
                BattlegroundsServerHost = $"{baseUri.Scheme}://{baseUri.Host}",
                BattlegroundsHttpServerPort = baseUri.Port,
            };

            var companyDeserializer = Substitute.For<ICompanyDeserializer>();
            var asyncHttpClient = new AsyncHttpClient(new HttpClient(), config, new TestLogger<AsyncHttpClient>());
            var serverAPI = new HttpBattlegroundsServerAPI(
                new TestLogger<HttpBattlegroundsServerAPI>(),
                asyncHttpClient,
                userService,
                companyDeserializer,
                config);

            services.AddSingleton(config);
            services.AddSingleton<ICompanyDeserializer>(companyDeserializer);
            services.AddSingleton<IAsyncHttpClient>(asyncHttpClient);
            services.AddSingleton<IBattlegroundsServerAPI>(serverAPI);
        }

        var mapService = Substitute.For<IGameMapService>();
        mapService.GetMapByScenarioName(Arg.Any<Game>(), Arg.Any<string>())
            .Returns(call => {
                string scenarioName = call.ArgAt<string>(1);
                int maxPlayers = scenarioName.StartsWith("2p", StringComparison.OrdinalIgnoreCase) ? 2 : 4;
                return new Scenario {
                    Name = (LocaleString)scenarioName,
                    Description = (LocaleString)$"Scenario {scenarioName}",
                    MaxPlayers = maxPlayers,
                    Preview = $"{scenarioName}_preview",
                    ScenarioName = scenarioName
                };
            });
        services.AddSingleton(mapService);

        var gameService = Substitute.For<IGameService>();
        var mockGame = Substitute.For<Game>();
        mockGame.Id.Returns("CoH3");
        gameService.GetGame(Arg.Any<string>()).Returns(mockGame);
        services.AddSingleton(gameService);

        return services.BuildServiceProvider();
    }

    private static LobbySetup BuildMinimalHostSetup(
        string userId, string userName, string lobbyName, string gameId, IServiceProvider services) {

        var mockGame = services.GetService(typeof(IGameService)) is IGameService gs
            ? gs.GetGame(gameId)
            : Substitute.For<Game>();

        var hostParticipant = new Participant(0, userId, userName, false, false);
        var team1 = new Team(TeamType.Allies, "Allies", [
            new Team.Slot(0, userId, string.Empty, string.Empty, AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, string.Empty, string.Empty, AIDifficulty.HUMAN, false, false),
            new Team.Slot(2, null, string.Empty, string.Empty, AIDifficulty.HUMAN, false, false),
            new Team.Slot(3, null, string.Empty, string.Empty, AIDifficulty.HUMAN, false, false),
        ]);
        var team2 = new Team(TeamType.Axis, "Axis", [
            new Team.Slot(0, null, string.Empty, string.Empty, AIDifficulty.HUMAN, false, false),
            new Team.Slot(1, null, string.Empty, string.Empty, AIDifficulty.HUMAN, false, false),
            new Team.Slot(2, null, string.Empty, string.Empty, AIDifficulty.HUMAN, false, false),
            new Team.Slot(3, null, string.Empty, string.Empty, AIDifficulty.HUMAN, false, false),
        ]);
        var defaultMap = new Map("2p_test", "Test Map", 2, "preview", "2p_test");

        return new LobbySetup {
            Name = lobbyName,
            Game = mockGame,
            Self = hostParticipant,
            Team1 = team1,
            Team2 = team2,
            Map = defaultMap,
            Settings = [],
            Participants = [hostParticipant],
        };
    }

    /// <summary>
    /// Builds an unsigned JWT (alg=none) whose <c>sub</c> claim is the given <paramref name="userId"/>.
    /// The test server has signature validation disabled; it only needs a well-formed JWT to extract
    /// the caller's identity from the <c>Authorization</c> header.
    /// </summary>
    private static string BuildTestJwt(string userId) {
        static string Base64UrlEncode(string json) {
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        var header  = Base64UrlEncode("{\"alg\":\"none\",\"typ\":\"JWT\"}");
        var payload = Base64UrlEncode($"{{\"sub\":\"{userId}\",\"exp\":9999999999}}");
        return $"{header}.{payload}.";
    }

    private static string BuildTimeoutMessage(LobbyEventType eventType, int timeoutMs, string? scenario, string? reason = null) {
        string scenarioInfo = string.IsNullOrWhiteSpace(scenario) ? string.Empty : $" Scenario: {scenario}.";
        string reasonInfo = string.IsNullOrWhiteSpace(reason) ? string.Empty : $" {reason}";
        return $"Timed out after {timeoutMs} ms waiting for {eventType} event.{scenarioInfo}{reasonInfo}";
    }
}

/// <summary>
/// gRPC client-side interceptor that appends <c>x-test-user-id</c> and <c>x-test-user-name</c>
/// headers to every RPC so the integration-test server can resolve caller identity for both
/// streaming and unary calls without a real JWT.
/// </summary>
file sealed class TestIdentityInterceptor(string userId, string userName) : Interceptor {

    private void AddHeaders(Metadata meta) {
        meta.Add("x-test-user-id", userId);
        meta.Add("x-test-user-name", userName);
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation) {

        var newMeta = context.Options.Headers ?? new Metadata();
        AddHeaders(newMeta);
        var newOptions = context.Options.WithHeaders(newMeta);
        var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, newOptions);
        return continuation(request, newContext);
    }

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
        TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
        AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation) {

        var newMeta = context.Options.Headers ?? new Metadata();
        AddHeaders(newMeta);
        var newOptions = context.Options.WithHeaders(newMeta);
        var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, newOptions);
        return continuation(request, newContext);
    }
}
