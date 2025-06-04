using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

public sealed class LobbyService(IUserService userService, IGameMapService mapService) : ILobbyService {

    private ReaderWriterLockSlim _activeLobbyLock = new();
    private ILobby? _activeLobby;

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
        var localUser = await userService.GetLocalUserAsync();
        var localUserParticipant = new Participant(0, localUser.UserId, localUser.UserDisplayName, false, true);
        var latestMap = await mapService.GetLatestMapAsync(game.Id);
        return new SingleplayerLobby(name, game, latestMap, localUserParticipant);
    }

    private Task<ILobby> CreateMultiplayerLobbyAsync(string name, string? password, Game game) {
        return Task.FromResult(new MultiplayerLobby() as ILobby);
    }

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
                throw new NotImplementedException("Leaving multiplayer lobbies is not implemented yet.");
            // TODO: Then also invoke Dispose on multiplayerLobby
            default:
                throw new InvalidOperationException("Unknown lobby type.");
        }
        ActiveLobby = null; // Clear the active lobby
    }

}
