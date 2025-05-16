using Battlegrounds.Factories;
using Battlegrounds.Models.Gamemodes;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

public sealed class PlayService(CoH3ArchiverService coh3Archiver) : IPlayService {

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
            GamemodeSgaFileLocation = "dummy"
        };

    }

    public Task<LaunchGameAppResult> LaunchGameApp(Game game) {
        throw new NotImplementedException();
    }

}
