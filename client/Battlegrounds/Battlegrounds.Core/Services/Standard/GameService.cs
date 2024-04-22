using Battlegrounds.Core.Games;

namespace Battlegrounds.Core.Services.Standard;

public class GameService : IGameService {
    
    public IGame? FromName(string name) {
        return new CoH3();
    }

}
