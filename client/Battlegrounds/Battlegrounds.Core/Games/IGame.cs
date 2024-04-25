namespace Battlegrounds.Core.Games;

public interface IGame {

    string Name { get; }

    string DefaultScenario { get; }

    (string, string)[] SkirmishSettings { get; }

}
