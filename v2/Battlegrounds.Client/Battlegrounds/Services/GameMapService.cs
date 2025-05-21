using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

public sealed class GameMapService : IGameMapService {

    public Task<Map> GetLatestMapAsync(string gameId) => Task.FromResult(new Map("(2p) Pachino Stalemate", "", 2, "", "2p_pachino_stalemate"));

    public Task<List<Map>> GetMapsForGame(string gameId) {
        if (gameId == CoH3.GameId) {
            return Task.FromResult(new List<Map>() {
                new Map("(2p) Pachino Stalemate", "", 2, "", "2p_pachino_stalemate"),
                new Map("(2p) Semois", "", 2, "", "2p_semois"),
            });
        }
        throw new NotImplementedException($"Game maps for {gameId} are not implemented.");
    }

    public Task<List<Map>> GetMapsForGame<T>() where T : Game => GetMapsForGame(typeof(T).Name);

}
