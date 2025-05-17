using Battlegrounds.Factories;
using Battlegrounds.Models;
using Battlegrounds.Models.Gamemodes;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

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
            GamemodeSgaFileLocation = "dummy",
            MatchId = matchDataBuilder.MatchId,
        };

    }

    public Task<LaunchGameAppResult> LaunchGameApp(Game game)
        => game switch {
            CoH3 coh3 => LaunchCoH3GameApp(coh3),
            _ => Task.FromResult(new LaunchGameAppResult() {
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
