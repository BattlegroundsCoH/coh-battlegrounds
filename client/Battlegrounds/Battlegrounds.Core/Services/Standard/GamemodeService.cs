using Battlegrounds.Core.Games;
using Battlegrounds.Core.Games.Gamemodes;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Core.Services.Standard;

public class GamemodeService(ILogger<GamemodeService> logger) : IGamemodeService {
    
    private readonly ILogger<GamemodeService> _logger = logger;

    public IGamemode GetGamemode(IGame game, string gamemodeName) {
        return new CoH3Gamemode(gamemodeName, "Victory Points", []);
    }

    public IGamemode[] GetGamemodes(IGame game) {
        return [
            new CoH3Gamemode("victory_points", "Victory Points", [])];
    }

}
