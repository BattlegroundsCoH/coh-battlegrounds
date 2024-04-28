namespace Battlegrounds.Core.Games.Gamemodes;

public interface IGamemode {

    string Id { get; }

    string Name { get; }

    IGamemodeOption[] Options { get; }

}
