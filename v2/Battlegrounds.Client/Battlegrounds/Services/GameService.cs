using Battlegrounds.Models;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

public sealed class GameService(Configuration configuration) : IGameService {
    
    private readonly CoH3 _coh3 = new CoH3(configuration);

    public Game GetGame(string gameId) => gameId switch {
        CoH3.GameId => _coh3,
        _ => throw new NotImplementedException()
    };

    public T GetGame<T>() where T : Game {
        if (typeof(T) == typeof(CoH3)) {
            return GetGame(CoH3.GameId) as T ?? throw new Exception();
        }
        throw new NotImplementedException();
    }

}
