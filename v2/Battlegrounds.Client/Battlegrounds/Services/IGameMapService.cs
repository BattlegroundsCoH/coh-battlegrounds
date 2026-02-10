using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

public interface IGameMapService {
    
    Task<Map> GetLatestMapAsync(string gameId);
    
    Map GetMapByScenarioName(Game game, string newValue);

    Task<List<Map>> GetMapsForGame(string gameId);

    Task<List<Map>> GetMapsForGame<T>() where T : Game;

}
