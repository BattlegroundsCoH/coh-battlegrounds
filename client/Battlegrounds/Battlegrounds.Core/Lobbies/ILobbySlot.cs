using Battlegrounds.Core.Games;

namespace Battlegrounds.Core.Lobbies;

public interface ILobbySlot {

    bool IsVisible { get; }

    bool IsLocked { get; }

    ILobbyPlayer? Player { get; }

    AIDifficulty Difficulty { get; }

}
