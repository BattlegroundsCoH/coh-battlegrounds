using Battlegrounds.Models.Gamemodes;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

public interface IPlayService {
    
    Task<BuildGamemodeResult> BuildGamemode(ILobby lobby);
    
    Task<LaunchGameAppResult> LaunchGameApp(Game game);

}
