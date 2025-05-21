namespace Battlegrounds.Models.Lobbies;

public sealed record Map(string Name, string Description, int MaxPlayers, string Preview, string ScenarioName);
