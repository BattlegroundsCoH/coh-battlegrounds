using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

public interface IGameService {

    Game GetGame(string gameId);

    T GetGame<T>() where T : Game;

    ICollection<Game> GetGames();

}
