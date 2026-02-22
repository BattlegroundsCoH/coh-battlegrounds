using Battlegrounds.Factories;
using Battlegrounds.Models;
using Battlegrounds.Models.Gamemodes;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services.Infrastructure;

namespace Battlegrounds.Services.Playing;

/// <summary>
/// Provides services for building and launching game modes and applications for supported games.
/// </summary>
/// <remarks>This service currently supports operations for Company of Heroes 3. Attempting to use unsupported
/// games will result in a failure. The service is not thread-safe.</remarks>
/// <param name="coh3Archiver">The archiver service used to create and manage mod archives for Company of Heroes 3.</param>
/// <param name="configuration">The configuration settings that control game launch options and behavior.</param>
public sealed class PlayService(CoH3ArchiverService coh3Archiver, Configuration configuration) : IPlayService {

    public Task<BuildGamemodeResult> BuildGamemode(ILobby lobby) {

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

        if (!await coh3Archiver.CreateModArchiveAsync(coh3.ModProjectPath)) { 
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

    public async Task<LaunchGameAppResult> LaunchGameApp(Game game)
        => game switch {
            CoH3 coh3 => await LaunchCoH3GameApp(coh3),
            _ => (new LaunchGameAppResult() {
                Failed = true,
                ErrorMessage = "Game not supported."
            })
        };

    private async Task<LaunchGameAppResult> LaunchCoH3GameApp(CoH3 coh3) {

        List<string> args = [];
        if (configuration.GameDevMode) {
            args.Add("-dev");
        }

        if (configuration.GameDebugMode) {
            args.Add("-debug");
        }

        if (configuration.SkipMovies) {
            args.Add("-nomovies");
        }

        if (configuration.WindowedMode) {
            args.Add("-windowed");
        }

        GameAppInstance appInstance = new CoH3AppInstance(coh3);
        if (!await appInstance.Launch([..args])) {
            return new LaunchGameAppResult() {
                Failed = true,
                ErrorMessage = "Failed to launch game app."
            };
        }

        return new LaunchGameAppResult() {
            Failed = false,
            ErrorMessage = string.Empty,
            GameInstance = appInstance
        };

    }

}
