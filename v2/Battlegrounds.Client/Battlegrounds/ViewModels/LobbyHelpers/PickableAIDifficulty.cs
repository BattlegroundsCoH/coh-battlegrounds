using Battlegrounds.Models.Playing;

namespace Battlegrounds.ViewModels.LobbyHelpers;

public sealed record PickableAIDifficulty(AIDifficulty Difficulty) {
    public string DisplayName => Difficulty switch { // Exhaustive checks when...
        AIDifficulty.EASY => "AI - Easy",
        AIDifficulty.NORMAL => "AI - Standard",
        AIDifficulty.HARD => "AI - Hard",
        AIDifficulty.EXPERT => "AI - Expert",
        AIDifficulty.HUMAN => "Select AI Difficulty",
        _ => throw new ArgumentOutOfRangeException(nameof(Difficulty), Difficulty, "Unknown AI difficulty")
    };
}
