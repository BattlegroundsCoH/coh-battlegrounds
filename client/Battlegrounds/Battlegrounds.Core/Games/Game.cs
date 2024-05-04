namespace Battlegrounds.Core.Games;

public abstract class Game(string name) : IGame {

    public string Name => name;

    public abstract string DefaultScenario { get; }

    public abstract (string, string)[] SkirmishSettings { get; }

}
