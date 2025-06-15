namespace Battlegrounds.Models.Gamemodes;

public sealed class BuildGamemodeResult {

    public bool Failed { get; init; }

    public string ErrorMessage { get; init; } = string.Empty;

    public string GamemodeSgaFileLocation { get; init; } = string.Empty;

    public Guid MatchId { get; init; } = Guid.Empty;

}
