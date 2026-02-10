using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

public sealed class GameMapService : IGameMapService {

    private readonly Dictionary<string, Dictionary<string, Map>> _mapsByGame = new Dictionary<string, Dictionary<string, Map>> {
        { CoH3.GameId, new Dictionary<string, Map>() {
            { "pachino_2p_mm_handmade", new Map("(2p) Pachino Stalemate", "", 2, "pachino_2p_mm_handmade", "pachino_2p")  },
            { "semois_2p_mm_handmade", new Map("(2p) Semois", "", 2, "semois_2p_mm_handmade", "semois_2p") },
        }}
    };

    public Task<Map> GetLatestMapAsync(string gameId) => Task.FromResult(new Map("(2p) Pachino Stalemate", "", 2, "pachino_2p_mm_handmade", "pachino_2p"));

    public Map GetMapByScenarioName(Game game, string newValue) => _mapsByGame[game.Id][newValue];

    public Task<List<Map>> GetMapsForGame(string gameId) => Task.FromResult(_mapsByGame[gameId].Values.ToList());

    public Task<List<Map>> GetMapsForGame<T>() where T : Game => GetMapsForGame(typeof(T).Name);

}
