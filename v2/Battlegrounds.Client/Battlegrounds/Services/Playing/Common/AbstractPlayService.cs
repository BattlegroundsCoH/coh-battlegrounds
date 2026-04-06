using Battlegrounds.Factories;
using Battlegrounds.Models.Gamemodes;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services.Infrastructure;

namespace Battlegrounds.Services.Playing.Common;

/// <summary>
/// Provides an abstract base for services that build and launch game modes for supported games, such as Company of
/// Heroes 3.
/// </summary>
/// <remarks>This class defines the core contract for play services that support building game modes and launching
/// game applications. Implementations must provide logic for ensuring mod sources are available and for launching the
/// game. The class is intended to be extended for specific game support.</remarks>
/// <param name="coh3Archiver">The archiver service used to create and manage mod archives for Company of Heroes 3.</param>
public abstract class AbstractPlayService(CoH3ArchiverService coh3Archiver) : IPlayService {

    private readonly CoH3ArchiverService _coh3Archiver = coh3Archiver;

    // The currently active lobby for which a game mode is being built or launched.
    // This field is intended to be used by derived classes to access lobby information during the build and launch processes.
    protected ILobby? _activePlayLobby;

    /// <summary>
    /// Builds the gamemode configuration for the specified lobby asynchronously.
    /// </summary>
    /// <param name="lobby">The lobby for which to build the gamemode configuration. Must not be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the gamemode build result for the
    /// specified lobby.</returns>
    /// <exception cref="NotImplementedException">Thrown if the lobby's game type is not supported.</exception>
    public Task<BuildGamemodeResult> BuildGamemode(ILobby lobby) {

        _activePlayLobby = lobby;
        var targetGame = lobby.Game;

        if (targetGame is CoH3 coh3) {
            return BuildCoH3Gamemode(lobby, coh3);
        }

        throw new NotImplementedException();

    }

    private async Task<BuildGamemodeResult> BuildCoH3Gamemode(ILobby lobby, CoH3 coh3) {

        CoH3MatchDataBuilder matchDataBuilder = new(lobby, coh3);

        string matchDataLuaSource = await matchDataBuilder.BuildMatchData();

        if (!await matchDataBuilder.WriteMatchData(matchDataLuaSource)) {
            return new BuildGamemodeResult() {
                Failed = true,
                ErrorMessage = "Failed to write match data file."
            };
        }

        if (!await _coh3Archiver.CreateModArchiveAsync(coh3.ModProjectPath)) {
            return new BuildGamemodeResult() {
                Failed = true,
                ErrorMessage = "Failed to create mod archive."
            };
        }

        return new BuildGamemodeResult() {
            Failed = false,
            ErrorMessage = string.Empty,
            GamemodeSgaFileLocation = CoH3ArchiverService.ArchiveDestination,
            MatchId = matchDataBuilder.MatchId,
        };

    }

    public abstract Task EnsureModSourceIsAvailable();

    public abstract Task<LaunchGameAppResult> LaunchGameApp(Game game);

}
