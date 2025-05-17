using Battlegrounds.Models.Playing;

namespace Battlegrounds.Test.Models.Playing;

public sealed class MockCoH3Game : ICoH3Game {
    public string MatchDataPath { get; set; } = string.Empty;
}
