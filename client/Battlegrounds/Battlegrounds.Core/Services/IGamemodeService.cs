using Battlegrounds.Core.Games;
using Battlegrounds.Core.Games.Gamemodes;

namespace Battlegrounds.Core.Services;

public interface IGamemodeService {

    IGamemode[] GetGamemodes(IGame game);

    IGamemode GetGamemode(IGame game, string gamemodeName);

}
