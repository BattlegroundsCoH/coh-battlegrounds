using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

public sealed class GameMapService : IGameMapService {

    public Task<Map> GetLatestMapAsync(string gameId) => Task.FromResult(new Map("(2p) Pachino Stalemate", "", 2, "pachino_2p_mm_handmade", "pachino_2p"));

    public Task<List<Map>> GetMapsForGame(string gameId) {
        if (gameId == CoH3.GameId) {
            return Task.FromResult(new List<Map>() {
                new Map("(2p) Pachino Stalemate", "", 2, "pachino_2p_mm_handmade", "pachino_2p"),
                new Map("(2p) Semois", "", 2, "semois_2p_mm_handmade", "semois_2p"),
            });
        }
        throw new NotImplementedException($"Game maps for {gameId} are not implemented.");
    }

    public Task<List<Map>> GetMapsForGame<T>() where T : Game => GetMapsForGame(typeof(T).Name);

}
