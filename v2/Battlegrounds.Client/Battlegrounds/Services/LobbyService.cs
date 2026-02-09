using Battlegrounds.Facades.API;
using Battlegrounds.Factories;
using Battlegrounds.Models;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Proto.Lobbies;

using Grpc.Core;

using Microsoft.Extensions.Logging;

using HostLobbyRequest = Battlegrounds.Proto.Lobbies.HostLobbyRequest;

namespace Battlegrounds.Services;

/// <summary>
/// Provides functionality for managing game lobbies, including creating, leaving, and retrieving lobbies.
/// </summary>
/// <remarks>The <see cref="LobbyService"/> class is responsible for handling both singleplayer and multiplayer
/// lobbies. It ensures thread-safe access to the active lobby and interacts with various services such as user
/// management, game map retrieval, and server communication. Only one active lobby can exist at a time.</remarks>
/// <param name="userService"></param>
/// <param name="mapService"></param>
/// <param name="companyService"></param>
/// <param name="serverAPI"></param>
/// <param name="logger"></param>
public sealed class LobbyService(
    IUserService userService,
    ICompanyService companyService, 
    IBattlegroundsServerAPI serverAPI, 
    GrpcServerClientFactory clientFactory,
    LobbySetupFromConfigFactory lobbySetupFromConfigFactory,
    MultiplayerLobbyFactory multiplayerLobbyFactory,
    Configuration configuration,
    ILogger<LobbyService> logger) : ILobbyService {

    private readonly ILogger<LobbyService> _logger = logger;
    private readonly IUserService _userService = userService;
    private readonly ICompanyService _companyService = companyService;
    private readonly IBattlegroundsServerAPI _serverAPI = serverAPI;
    private readonly GrpcServerClientFactory _clientFactory = clientFactory;
    private readonly LobbySetupFromConfigFactory _lobbySetupFromConfigFactory = lobbySetupFromConfigFactory;
    private readonly MultiplayerLobbyFactory _multiplayerLobbyFactory = multiplayerLobbyFactory;
    private readonly Configuration _configuration = configuration;
    private readonly ReaderWriterLockSlim _activeLobbyLock = new();
    private ILobby? _activeLobby;

    /// <summary>
    /// Gets a value indicating whether there is an active lobby.
    /// </summary>
    /// <remarks>This property is thread-safe and ensures consistent access to the active lobby
    /// state.</remarks>
    public bool HasActiveLobby {
        get {
            _activeLobbyLock.EnterReadLock();
            try {
                return _activeLobby != null;
            } finally {
                _activeLobbyLock.ExitReadLock();
            }
        }
    }

    /// <summary>
    /// Gets the currently active lobby, or <see langword="null"/> if no lobby is active.
    /// </summary>
    /// <remarks>Access to this property is thread-safe. The value is protected by a read-write lock to ensure
    /// consistency during concurrent access.</remarks>
    public ILobby? ActiveLobby {
        get {
            _activeLobbyLock.EnterReadLock();
            try {
                return _activeLobby;
            } finally {
                _activeLobbyLock.ExitReadLock();
            }
        }
        private set {
            _activeLobbyLock.EnterWriteLock();
            try {
                _activeLobby = value;
            } finally {
                _activeLobbyLock.ExitWriteLock();
            }
        }
    }

    /// <summary>
    /// Creates a new lobby with the specified parameters.
    /// </summary>
    /// <remarks>This method creates either a multiplayer or single-player lobby based on the multiplayer
    /// parameter. If a multiplayer lobby is created, the password parameter determines whether the lobby is
    /// password-protected.</remarks>
    /// <param name="name">The name of the lobby. This value cannot be null or empty.</param>
    /// <param name="password">The optional password for the lobby. If null, the lobby will not be password-protected.</param>
    /// <param name="multiplayer">A value indicating whether the lobby is for multiplayer.  true to create a multiplayer lobby; otherwise, false
    /// to create a single-player lobby.</param>
    /// <param name="game">The game associated with the lobby. This value cannot be null.</param>
    /// <returns>An ILobby instance representing the newly created lobby.</returns>
    /// <exception cref="InvalidOperationException">Thrown if an active lobby already exists when attempting to create a new one.</exception>
    public async Task<ILobby> CreateLobbyAsync(string name, string? password, bool multiplayer, Game game) {
        if (HasActiveLobby) {
            throw new InvalidOperationException("Cannot create a new lobby while an active lobby exists.");
        }
        var lobby = multiplayer ? await CreateMultiplayerLobbyAsync(name, password, game)
            : await CreateSingleplayerLobbyAsync(name, game);
        _activeLobbyLock.EnterWriteLock();
        try {
            _activeLobby = lobby;
        } finally {
            _activeLobbyLock.ExitWriteLock();
        }
        return lobby;
    }

    private async Task<ILobby> CreateSingleplayerLobbyAsync(string name, Game game) {
        _logger.LogInformation("Creating singleplayer lobby with name: {LobbyName} for game: {GameId}", name, game.Id);
        var localUser = await _userService.GetLocalUserAsync() ?? throw new InvalidOperationException("Cannot create a singleplayer lobby without a local user.");
        var lobbySetup = await _lobbySetupFromConfigFactory.FromConfig(name, game, localUser);
        return new SingleplayerLobby(lobbySetup, _serverAPI, _companyService);
    }

    private async Task<ILobby> CreateMultiplayerLobbyAsync(string name, string? password, Game game) {
        _logger.LogInformation("Creating multiplayer lobby with name: {LobbyName} for game: {GameId}", name, game.Id);
        var localUser = _userService.GetLocalUserAsync().Result ?? throw new InvalidOperationException("Cannot create a multiplayer lobby without a local user.");

        try {

            var lobbySetup = await _lobbySetupFromConfigFactory.FromConfig(name, game, localUser);

            var client = _clientFactory.CreateClient(_configuration);

            var hostRequest = new HostLobbyRequest {
                LobbyName = name,
                Password = password ?? string.Empty,
                HostId = localUser.UserId,
                GameId = game.Id,
            };
            var headers = new Metadata {
                { "authorization", $"Bearer {_userService.GetLocalUserToken()}" }
            };

            var stream = client.HostLobby(hostRequest, headers);
            var lobby = await _multiplayerLobbyFactory.GetLobby(client, stream, lobbySetup);
            await lobby.PublishInitialState();

            return lobby;

        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to create gRPC client for multiplayer lobby creation.");
            throw new InvalidOperationException("Failed to create gRPC client for multiplayer lobby creation.", ex);
        }

    }

    /// <summary>
    /// Asynchronously leaves the specified lobby if it is the active lobby.
    /// </summary>
    /// <remarks>This method disposes of the specified lobby. The
    /// active lobby is cleared after successfully leaving the specified lobby.</remarks>
    /// <param name="lobby">The lobby to leave. Must be the currently active lobby.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException"/>
    public async Task LeaveLobbyAsync(ILobby lobby) {
        _activeLobbyLock.EnterReadLock();
        try {
            if (_activeLobby != lobby) {
                throw new InvalidOperationException("Cannot leave a lobby that is not the active lobby.");
            }
        } finally {
            _activeLobbyLock.ExitReadLock();
        }
        switch (lobby) {
            case SingleplayerLobby singleplayerLobby:
                singleplayerLobby.Dispose(); // Dispose the singleplayer lobby
                await Task.CompletedTask; // No action needed for singleplayer lobby
                break;
            case MultiplayerLobby multiplayerLobby:
                await multiplayerLobby.LeaveAsync(); // Leave the multiplayer lobby
                multiplayerLobby.Dispose(); // Dispose the multiplayer lobby
                break;
            default:
                throw new InvalidOperationException("Unknown lobby type.");
        }
        ActiveLobby = null; // Clear the active lobby
    }

    /// <summary>
    /// Retrieves a collection of available lobbies from the server.
    /// </summary>
    /// <remarks>This method communicates with the server to fetch the list of lobbies. Ensure that the server
    /// connection is properly configured before calling this method.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains an  IEnumerable{T} of BrowserLobby
    /// objects representing the available lobbies. If no lobbies are available, the collection will be empty.</returns>
    public async Task<IEnumerable<BrowserLobby>> GetLobbiesAsync() => await _serverAPI.GetLobbiesAsync();

    /// <summary>
    /// Asynchronously determines whether the server is available.
    /// </summary>
    /// <remarks>This method checks the availability of the server by delegating the call to the underlying
    /// server API. It is useful for determining whether operations that depend on the server can proceed.</remarks>
    /// <returns><see langword="true"/> if the server is available; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> IsServerAvailableAsync() => await _serverAPI.IsServerAvailableAsync();

}
