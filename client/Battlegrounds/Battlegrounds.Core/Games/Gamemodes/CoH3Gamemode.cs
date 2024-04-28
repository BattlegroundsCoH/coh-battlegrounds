namespace Battlegrounds.Core.Games.Gamemodes;

public class CoH3Gamemode(string id, string name, IGamemodeOption[] options) : IGamemode {

    public string Id => id;

    public string Name => name;

    public IGamemodeOption[] Options => options;

}
