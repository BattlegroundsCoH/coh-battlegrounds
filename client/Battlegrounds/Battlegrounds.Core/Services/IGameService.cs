using Battlegrounds.Core.Games;

namespace Battlegrounds.Core.Services;

public interface IGameService {

    IGame? FromName(string name);

}
