using Battlegrounds.Core.Lobbies;

namespace Battlegrounds.Core.Services;

public interface IMatchModBuilderService {

    Task<bool> BuildMatchGamemode(ILobby lobby);
    
    Task<Stream?> OpenReadGamemodeArchive(Guid guid);

}
